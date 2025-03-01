namespace AchievementsLibrary
{
    /// <summary>
    /// Класс-обёртка над коллекцией Achievement
    /// Хранит список категорий, а также их вложенные достижения
    /// Управляет фильтрацией, сортировкой и прочим состоянием коллекции
    /// </summary>
    public class AchievementsManager
    {
        /// <summary>
        /// Все объекты (и категории, и обычные достижения)
        /// </summary>
        private List<Achievement> _all = new();

        /// <summary>
        /// Список категорий (isCategory == true)
        /// </summary>
        public List<Achievement> Categories { get; private set; } = new();
        
        /// <summary>
        /// Ключ: Id категории; значение: список достижений, где achievement.Category == Id
        /// </summary>
        private readonly Dictionary<string, List<Achievement>> _achievementsByCategory = new();

        /// <summary>
        /// Какое поле было последним отфильтровано
        /// </summary>
        public string LastFilterField    { get; private set; } = "";
        /// <summary>
        /// Тип фильтра: "Подстрока" или "Массив объектов"
        /// </summary>
        public string LastFilterType     { get; private set; } = "";
        /// <summary>
        /// Критерий фильтра (подстрока или перечисление значений)
        /// </summary>
        public string LastFilterCriteria { get; private set; } = "";
        /// <summary>
        /// Какое поле было последним отсортировано
        /// </summary>
        public string LastSortField      { get; private set; } = "";
        /// <summary>
        /// Направление сортировки: "asc" или "desc"
        /// </summary>
        public string LastSortDirection  { get; private set; } = "";

        /// <summary>
        /// Позволяет внешнему коду читать текущий список всех достижений (включая категории).
        /// </summary>
        public IReadOnlyList<Achievement> All => _all;
        
        /// <summary>
        /// Загружает (заменяет) коллекцию достижений новым списком и перестраивает внутренние структуры (категории)
        /// Сбрасывает сведения о прошлой фильтрации/сортировке
        /// </summary>
        /// <param name="achievements">Новый набор Achievement</param>
        public void Load(IEnumerable<Achievement> achievements)
        {
            _all = achievements.ToList();
            BuildStructure();
            
            // При загрузке нового файла сбрасываем все "Last..." поля
            LastFilterField = "";
            LastFilterType = "";
            LastFilterCriteria = "";
            LastSortField = "";
            LastSortDirection = "";
        }

        /// <summary>
        /// Перестраивает списки категорий и карту достижений, используемые для TUI
        /// </summary>
        private void BuildStructure()
        {
            
            Categories = _all.Where(a => a.IsCategory).ToList();
            _achievementsByCategory.Clear();
            List<Achievement> onlyAchievements = _all.Where(a => !a.IsCategory).ToList();
            foreach (Achievement ach in onlyAchievements)
            {
                string categoryId = ach.Category;
                if (string.IsNullOrEmpty(categoryId))
                {
                    continue;
                }

                if (!_achievementsByCategory.ContainsKey(categoryId))
                {
                    _achievementsByCategory [categoryId] = new List<Achievement>();
                }

                _achievementsByCategory [categoryId].Add(ach);
            }
        }

        /// <summary>
        /// Возвращает список достижений, принадлежащих данной категории (по Id)
        /// Если нет таких, возвращается пустой список
        /// </summary>
        /// <param name="categoryId">Id категории (например, "A_CATEGORY_DANCER")</param>
        /// <returns>Список достижений</returns>
        public List<Achievement> GetAchievementsForCategory(string categoryId)
        {
            if (_achievementsByCategory.TryGetValue(categoryId, out List<Achievement>? list))
            {
                return list;
            }

            return new List<Achievement>();
        }

        /// <summary>
        /// Фильтрация по подстроке value в поле fieldName (строчное сравнение)
        /// Применяется ко всем _all
        /// Запоминает сведения о последнем фильтре (LastFilter...)
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="value">Подстрока</param>
        public void FilterBySubstring(string fieldName, string value)
        {
            _all = _all
                .Where(a =>
                {
                    string f = a.GetField(fieldName);

                    return f.ToLower().Contains(value.ToLower());
                })
                .ToList();

            BuildStructure();
            
            // Запоминаем, что за фильтрация
            LastFilterField = fieldName;
            LastFilterType = "Подстрока";
            LastFilterCriteria = value.ToLowerInvariant();
        }
        
        /// <summary>
        /// Фильтрация по массиву значений. Если поле category, а пользователь ввёл ["A_CATEGORY_CS", "A_CATEGORY_EXILE"],
        /// оставим только те объекты, у которых значение поля совпадает с одним из массива
        /// Запоминает сведения о последнем фильтре (LastFilter...)
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Список значений</param>
        public void FilterByValues(string fieldName, List<string> values)
        {
            HashSet<string> valSet = [..values.Select(v => v.ToLowerInvariant())];

            _all = _all.Where(a =>
            {
                string val = a.GetField(fieldName);

                return valSet.Contains(val.ToLowerInvariant());
            }).ToList();

            BuildStructure();

            // Запоминаем, что за фильтрация
            LastFilterField = fieldName;
            LastFilterType = "Массив объектов";
            LastFilterCriteria = string.Join(", ", values);
        }
        
        /// <summary>
        /// Сортирует текущий список _all по указанному полю (строковое сравнение)
        /// ascending = true -> по возрастанию, false -> по убыванию
        /// Запоминает сведения о последней сортировке
        /// </summary>
        /// <param name="fieldName">Имя поля (например, "label" или "category")</param>
        /// <param name="ascending">True для возрастания (asc), False для убывания (desc)</param>
        public void Sort(string fieldName, bool ascending)
        {
            _all = ascending ? _all.OrderBy(a => a.GetField(fieldName)).ToList() : _all.OrderByDescending(a => a.GetField(fieldName)).ToList();

            BuildStructure();
            
            // Запомним, что за сортировка
            LastSortField = fieldName;
            LastSortDirection = ascending ? "asc" : "desc";
        }
        /// <summary>
        /// Присвоение глобальных процентов (из Steam Web API) по имени достижения (id)
        /// </summary>
        /// <param name="steamData">Словарь: ключ = id достижения, значение = процент игроков</param>
        public void UpdateGlobalPercentage(Dictionary<string, double> steamData)
        {
            for (int i = 0; i < _all.Count; i++)
            {
                Achievement temp = _all[i];
                if (steamData.TryGetValue(temp.Id, out double pct))
                {
                    temp.GlobalPercent = pct;
                    _all[i] = temp;
                }
            }
            
            BuildStructure();
        }
    }
}