/*
 * Малапура Артемий Андреевич
 * Проект №1 по c# в 3 модуле
 * Вариант 10
 * БПИ-247-2
 */


/*
 * === ЛОГИКА РАБОТЫ ПРОГРАММЫ ===
 * 1) ВВОД ДАННЫХ:
 * Позволяет считать JSON либо напрямую из консоли, либо из файла по заданному пути.
 * Полученные данные парсятся, загружаются в менеджер (AchievementsManager),
 * где хранятся в виде списка категорий и достижений.
 * 
 * 2) ФИЛЬТРАЦИЯ ДАННЫХ:
 * Два режима фильтра:
 * 1. По подстроке (пользователь вводит часть текста, и остаются объекты, в чьём выбранном поле встречается эта подстрока).
 * 2. По массиву значений (пользователь задаёт несколько точных значений, и остаются объекты, в чьём поле одно из этих значений).
 * 3. Результаты фильтрации сохраняются в менеджере, а количество оставшихся объектов отображается пользователю.
 *
 * 3) СОРТИРОВКА ДАННЫХ:
 * Пользователь указывает строковое поле и направление (asc/desc).
 * Вся текущая коллекция сортируется по этому полю.
 * Информация (какое поле, какое направление) запоминается для последующего вывода.
 *
 * 4) TUI:
 * Отображает группы достижений и достижения в них в виде карточек (если isCategory=false, то TUI не отображается).
 * Для каждой категории пользователь может нажать Enter, чтобы развернуть/свернуть список входящих в неё достижений.
 * Навигация выполняется стрелками вверх/вниз, а Tab возвращает в меню.
 *
 * 5) ДОПОЛНИТЕЛЬНАЯ ЗАДАЧА (STEAM WEB API):
 * Пользователь вводит AppID игры.
 * Программа делает публичный GET-запрос к Steam Web API по адресу:
 * https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?gameid=<AppID>
 * Ответ (JSON) парсится, и для каждого достижения, чьё имя (Id) совпадает с полем "name"
 * из ответа Steam, присваивается процент игроков, получивших это достижение.
 *
 * 6) ВЫВОД ДАННЫХ:
 * Программа показывает сведения о последней фильтрации и сортировке (если были).
 * Далее пользователь выбирает, куда выводить (в консоль или в файл).
 * Формируется JSON формата:
 * {
     "achievements": [
       { ... },
       { ... }
     ]
   }
 * и записывается в выбранный источник.
 *
 * 7) ВЫХОД:
 * Завершает работу программы.
 */



using System.Text;
using AchievementsLibrary;

namespace AchievementsConsoleApp
{
    /// <summary>
    /// Главный класс консольного приложения, содержащий метод Main и логику меню
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Менеджер, содержащий все загруженные данные (категории и достижения)
        /// </summary>
        private static readonly AchievementsManager Manager = new AchievementsManager();
        
        /// <summary>
        /// Точка входа в программу. Показывает меню и обрабатывает пользовательский ввод
        /// </summary>
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8; // чтобы поддерживать весь русский текст (кириллицу) корректно
            bool exit = false;

            while (!exit)
            {
                ShowMenu();
                string choice = Console.ReadLine() ?? "";
                switch (choice)
                {
                    case "1": // ввод
                        InputData();
                        break;
                    case "2": // фильтрация
                        DoFilter();
                        break;
                    case "3": // сортировка
                        DoSort();
                        break;
                    case "4": // TUI
                        ShowTui();
                        break;
                    case "5": // Доп.задача: Steam Web API
                        SteamWebApiTask();
                        break;
                    case "6": // вывод
                        OutputData();
                        break;
                    case "7":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }

            Console.WriteLine("Завершение работы.");
        }

        /// <summary>
        /// Выводит текстовое меню главных действий программы
        /// </summary>
        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("==== М Е Н Ю ====");
            Console.WriteLine("1. Ввести данные (консоль/файл)");
            Console.WriteLine("2. Отфильтровать данные");
            Console.WriteLine("3. Отсортировать данные");
            Console.WriteLine("4. TUI: Просмотр групп достижений");
            Console.WriteLine("5. [Доп. Steam Web API]");
            Console.WriteLine("6. Вывести данные (консоль/файл)");
            Console.WriteLine("7. Выход");
            Console.Write("Ваш выбор: ");
        }

        #region 1) Ввести данные
        /// <summary>
        /// Пункт меню для ввода данных: либо из консоли (вставляя JSON), либо из файла по пути
        /// Обрабатывает исключения, связанные с чтением и парсингом
        /// </summary>
        private static void InputData()
        {
            Console.WriteLine("1) Из консоли");
            Console.WriteLine("2) Из файла");
            Console.Write("Выберите: ");
            string c = Console.ReadLine() ?? "";

            try
            {
                if (c == "1")
                {
                    Console.WriteLine("Вставьте JSON, затем нажмите Ctrl+Z (Windows) или Ctrl+D (Linux/macOS):");

                    List<Achievement> list = JsonParser.ReadAchievementsFromConsole();
                    Manager.Load(list);
                    Console.WriteLine($"Загружено объектов: {Manager.Categories.Count} категорий + достижения");
                }
                else if (c == "2")
                {
                    Console.Write("Введите путь к файлу: ");
                    string path = Console.ReadLine() ?? "";
                    List<Achievement> list = JsonParser.ReadAchievementsFromFile(path);
                    Manager.Load(list);
                    Console.WriteLine($"Загружено объектов: {Manager.Categories.Count} категорий + достижения");
                }
                else
                {
                    Console.WriteLine("Неизвестный выбор.");
                }
            }
            catch (FormatException fex)
            {
                Console.WriteLine("Ошибка: JSON невалиден по стандарту RFC 8259!");
                Console.WriteLine($"Сообщение: {fex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при вводе данных: " + ex.Message);
            }
        }
        #endregion

        #region 2) Фильтрация
        /// <summary>
        /// Пункт меню для выполнения двух типов фильтрации:
        /// 1) По подстроке
        /// 2) По массиву значений
        /// Пользователь выбирает тип, вводит поле и критерии, затем вызывается соответствующий метод из AchievementsManager
        /// </summary>
        private static void DoFilter()
        {
            if (Manager.All.Count == 0)
            {
                Console.WriteLine("Сначала загрузите данные (п.1).");
                return;
            }
            
            Console.WriteLine("Выберите тип фильтрации:");
            Console.WriteLine("1) По подстроке (например, найти 'moon' в поле 'label')");
            Console.WriteLine("2) По массиву значений (например, ['A_CATEGORY_CS','A_CATEGORY_EVERAFTER'] для поля 'category')");
            Console.Write("Ваш выбор: ");
            string filterChoice = Console.ReadLine() ?? "";
            if (filterChoice != "1" && filterChoice != "2")
            {
                Console.WriteLine("Неизвестный выбор (1 или 2). Фильтрация не выполнена.");
                return;
            }
            // Для удобства покажем доступные поля (строковые)
            Console.WriteLine("Доступные строковые поля для фильтрации:");
            Console.WriteLine("\"id\", \"category\", \"iconUnlocked\", \"label\", " +
                          "\"descriptionunlocked\", \"singleDescription\", \"isHidden\", \"isCategory\", \"validateOnStorefront\")");

            // Просим поле до тех пор, пока не будет корректно
            string fieldName;
            while (true)
            {
                Console.Write("Введите имя поля: ");
                fieldName = (Console.ReadLine() ?? "").Trim();
                if (IsValidStringField(fieldName))
                {
                    break;
                }

                Console.WriteLine("Некорректное поле, повторите ввод.");
            }
            switch (filterChoice)
            {
                case "1":
                    {
                        // Фильтрация по подстроке
                        Console.Write("Введите подстроку: ");
                        string substring = Console.ReadLine() ?? "";
                        Manager.FilterBySubstring(fieldName, substring);

                        // Выводим информацию о фильтрации
                        Console.WriteLine($"Выполнена фильтрация по подстроке.\nПоле: {fieldName}, подстрока: \"{substring}\".");
                        Console.WriteLine($"Теперь в коллекции: {Manager.All.Count} объектов.");
                        break;
                    }
                case "2":
                    {
                        // Фильтрация по массиву значений
                        Console.WriteLine("Введите значения, которые нужно оставить (через точку с запятой)");
                        Console.Write("Значения: ");
                        string line = Console.ReadLine() ?? "";

                        // Разделим по ";" (чтобы корректно считывались предложения, где есть запятая и иные знаки препинания)
                        // и уберём пробелы
                        string[] parts = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
                        List<string> values = new List<string>();
                        foreach (string p in parts)
                        {
                            values.Add(p.Trim());
                        }

                        Manager.FilterByValues(fieldName, values);

                        // Выводим информацию о фильтрации
                        Console.WriteLine($"Выполнена фильтрация по массиву значений.\nПоле: {fieldName}, значения: [{string.Join("; ", values)}].");
                        Console.WriteLine($"Теперь в коллекции: {Manager.All.Count} объектов.");
                        break;
                    }
            }
        }
        /// <summary>
        /// Проверяет, является ли указанная строка допустимым именем поля (строкового)
        /// </summary>
        /// <param name="f">Имя поля</param>
        /// <returns>True, если поле допустимо; False в противном случае</returns>
        private static bool IsValidStringField(string f)
        {
            HashSet<string> valid = new HashSet<string>(StringComparer.Ordinal)
                { "id", "category", "iconUnlocked", "label", "descriptionunlocked", 
                    "singleDescription", "isHidden", "isCategory", "validateOnStorefront" };
            return valid.Contains(f);
        }
        #endregion

        #region 3) Сортировка
        /// <summary>
        /// Пункт меню для сортировки: спрашивает поле (строковое) и направление (asc/desc)
        /// </summary>
        private static void DoSort()
        {
            if (Manager.All.Count == 0)
            {
                Console.WriteLine("Сначала загрузите данные (п.1).");
                return;
            }
            Console.WriteLine("Доступные строковые поля для сортировки:");
            Console.WriteLine("\"id\", \"category\", \"iconUnlocked\", \"label\", " +
                              "\"descriptionunlocked\", \"singleDescription\", \"isHidden\", " +
                              "\"isCategory\", \"validateOnStorefront\")");
            
            string field;
            while (true)
            {
                Console.Write("Введите поле для сортировки: ");
                field = (Console.ReadLine() ?? "").Trim();
                if (IsValidStringField(field))
                {
                    break;
                }

                Console.WriteLine("Некорректное поле, повторите ввод.");
            }
            
            bool ascending;
            while (true)
            {
                Console.Write("Введите направление (asc/desc): ");
                string dir = (Console.ReadLine() ?? "").ToLowerInvariant();
                if (dir == "asc")
                {
                    ascending = true;
                    break;
                }
                else if (dir == "desc")
                {
                    ascending = false;
                    break;
                }
                else
                {
                    Console.WriteLine("Некорректное направление. Попробуйте ещё раз (asc/desc).");
                }
            }
            Manager.Sort(field, ascending);

            // Выводим информацию о сортировке
            Console.WriteLine($"Сортировка выполнена. Поле: {field}, Направление: {(ascending ? "asc" : "desc")}.");
        }
        #endregion

        #region 4) TUI (Просмотр групп достижений)
        /// <summary>
        /// Текстовый пользовательский интерфейс (TUI) просмотра категорий и достижений.
        /// </summary>
        private static void ShowTui()
        {
            List<Achievement> categories = Manager.Categories;
            if (categories.Count == 0)
            {
                Console.WriteLine("Нет ни одной категории (isCategory=true). Сначала загрузите данные.");
                return;
            }

            // Массив флагов: развернута ли категория?
            bool[] expanded = new bool[categories.Count];
            int currentIndex = 0;

            bool done = false;
            while (!done)
            {
                Console.Clear();
                Console.WriteLine("===== Просмотр групп достижений (TUI) =====");
                Console.WriteLine("Up/Down: переключить группу, Enter: раскрыть/свернуть, Tab: выйти в главное меню");
                Console.WriteLine();

                // Отрисовка категорий
                for (int i = 0; i < categories.Count; i++)
                {
                    bool isSelected = i == currentIndex;
                    DrawCategory(categories[i], isSelected, expanded[i]);
                }

                // Считываем клавишу
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentIndex > 0)
                        {
                            currentIndex--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if (currentIndex < categories.Count - 1)
                        {
                            currentIndex++;
                        }

                        break;
                    case ConsoleKey.Enter:
                        expanded[currentIndex] = !expanded[currentIndex];
                        break;
                    case ConsoleKey.Tab:
                        done = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Рисует одну «карточку» категории с подписью (Label).
        /// Если развернута – выводим список достижений
        /// Если isSelected = true, рисуем «тень» под карточкой
        /// </summary>
        /// <param name="cat">Объект категории</param>
        /// <param name="isSelected">Выбрана ли эта категория</param>
        /// <param name="isExpanded">Развернута ли эта категория</param>
        private static void DrawCategory(Achievement cat, bool isSelected, bool isExpanded)
        {
            // 1) Верхняя граница рамки
            Console.WriteLine("┌───────────────────────────────────────────┐");
            // 2) Название категории (Label)
            string labelLine = "│ " + cat.Label;
            Console.WriteLine(FixWidth(labelLine, 45));

            // 3) Нижняя граница рамки
            Console.WriteLine("└───────────────────────────────────────────┘");

            // Если развернута – выводим достижения
            if (isExpanded)
            {
                // Список достижений, у которых Category == cat.Id
                List<Achievement> achs = Manager.GetAchievementsForCategory(cat.Id);
                if (achs.Count == 0)
                {
                    Console.WriteLine("   (Нет достижений в этой группе)");
                }
                else
                {
                    // Верхняя граница меньшей рамки
                    Console.WriteLine("   ╔═══════════════════════════════════════╗");
                    foreach (Achievement a in achs)
                    {
                        string info = a.Label;
                        if (a.GlobalPercent >= 0)
                        {
                            info += $" [{a.GlobalPercent:F2}% из Steam]";
                        }

                        // Перечислим их Label построчно
                        Console.WriteLine("   ║ " + info);
                    }
                    // Нижняя граница меньшей рамки
                    Console.WriteLine("   ╚═══════════════════════════════════════╝");
                }
            }

            // Если категория выделена – под ней рисуем «тень»
            if (isSelected)
            {
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("██████████████████████████████████████████");
                Console.ResetColor();
            }
            else
            {
                // Пустая строка, отделяющая карточки
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Подгоняет строку под заданную ширину, обрезая или дополняя пробелами
        /// </summary>
        /// <param name="line">Исходная строка</param>
        /// <param name="totalWidth">Желаемая ширина</param>
        /// <returns>Строка фиксированной ширины</returns>
        private static string FixWidth(string line, int totalWidth)
        {
            // line может быть короче или длиннее
            if (line.Length > totalWidth)
            {
                return line.Substring(0, totalWidth);
            }

            return line + new string(' ', totalWidth - line.Length);
        }
        #endregion
        
        #region 5) Дополнительная задача: Steam Web API
        /// <summary>
        /// Пункт меню для вызова Steam Web API, 
        /// получения процента получивших достижение и обновления этих процентов в AchievementsManager
        /// </summary>
        private static void SteamWebApiTask()
        {
            if (Manager.All.Count == 0)
            {
                Console.WriteLine("Сначала загрузите данные!");
                return;
            }

            Console.Write("Введите AppID для Steam (для игры CultistSimulator AppId: 718670):");
            string appId = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(appId))
            {
                Console.WriteLine("AppID не задан, отмена.");
                return;
            }

            try
            {
                // Выполним запрос
                Dictionary<string, double> result = GetSteamAchievementPercentages(appId).Result; // синхронный вызов
                // result - это Dictionary<string,double>, где key=название достижения, value=процент
                // Обновляем в менеджере
                Manager.UpdateGlobalPercentage(result);
                Console.WriteLine("Проценты обновлены. Можете посмотреть в TUI (вложенные достижения).");
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine("Ошибка сети или HTTP при обращении к Steam API: " + httpEx.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при обращении к Steam API: " + ex.Message);
            }
        }

        /// <summary>
        /// Делаем запрос к https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?gameid=AppID
        /// Парсим JSON, возвращаем словарь achievementName -> percentage
        /// </summary>
        /// <param name="appId">ID игры в Steam</param>
        /// <returns>Словарь: ключ = имя достижения, значение = процент игроков</returns>
        private static async Task<Dictionary<string,double>> GetSteamAchievementPercentages(string appId)
        {
            string url = $"https://api.steampowered.com/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/?gameid={appId}";
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            string content = await response.Content.ReadAsStringAsync();
            
            JsonValue root = JsonValue.Parse(content);

            Dictionary<string, double> result = new(StringComparer.OrdinalIgnoreCase);

            if (root.Type != JsonValueType.Object)
            {
                return result;
            }

            Dictionary<string, JsonValue> rootObj = root.AsObject();
            if (!rootObj.TryGetValue("achievementpercentages", out JsonValue? ap))
            {
                return result;
            }

            if (ap.Type != JsonValueType.Object)
            {
                return result;
            }

            Dictionary<string, JsonValue> apObj = ap.AsObject();
            if (!apObj.TryGetValue("achievements", out JsonValue? arrVal))
            {
                return result;
            }

            if (arrVal.Type != JsonValueType.Array)
            {
                return result;
            }

            List<JsonValue> arr = arrVal.AsArray();
            foreach (JsonValue item in arr)
            {
                if (item.Type != JsonValueType.Object)
                {
                    continue;
                }

                Dictionary<string, JsonValue> o = item.AsObject();
                if (!o.ContainsKey("name") || !o.ContainsKey("percent"))
                {
                    continue;
                }

                string achName = o["name"].ToStringValue();
                string percentStr = o["percent"].ToStringValue();
                if (double.TryParse(percentStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.CurrentCulture, out double prc))
                {
                    result[achName] = prc;
                }
            }

            return result;
        }
        #endregion

        #region 6) Вывести данные
        /// <summary>
        /// Пункт меню для вывода текущих данных в консоль или в файл
        /// Также выводит информацию о последней фильтрации и сортировке
        /// </summary>
        private static void OutputData()
        {
            if (Manager.All.Count==0)
            {
                Console.WriteLine("Нет данных для вывода.");
                return;
            }
            
            // Информация о фильтрации
            if (!string.IsNullOrEmpty(Manager.LastFilterType))
            {
                Console.WriteLine("=== Информация о последней фильтрации ===");
                Console.WriteLine($" Тип: {Manager.LastFilterType}");
                Console.WriteLine($" Поле: {Manager.LastFilterField}");
                Console.WriteLine($" Критерий: {Manager.LastFilterCriteria}");
            }
            
            // Информация о сортировке
            if (!string.IsNullOrEmpty(Manager.LastSortField))
            {
                Console.WriteLine("=== Информация о последней сортировке ===");
                Console.WriteLine($"Поле: {Manager.LastSortField}");
                Console.WriteLine($"Направление: {Manager.LastSortDirection}");
            }
            
            Console.WriteLine("1) В консоль");
            Console.WriteLine("2) В файл");
            string c = Console.ReadLine() ?? "";

            try
            {
                if (c == "1")
                {
                    JsonParser.WriteAchievementsToConsole(Manager.All);
                }
                else if (c == "2")
                {
                    Console.Write("Введите путь к файлу: ");
                    string path = Console.ReadLine() ?? "";
                    try
                    {
                        using FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
                        using StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);

                        TextWriter backup = Console.Out;
                        Console.SetOut(sw);
                        JsonParser.WriteAchievementsToConsole(GatherAll());
                        Console.SetOut(backup);
                        Console.WriteLine($"Данные сохранены в файл: {path}");
                    }
                    catch (FileNotFoundException fnfEx)
                    {
                        Console.WriteLine("Файл не найден (возможно, неверный путь): " + fnfEx.Message);
                    }
                    catch (UnauthorizedAccessException uaEx)
                    {
                        Console.WriteLine("Нет прав на создание/запись в файл: " + uaEx.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Неизвестный вариант.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при выводе: " + ex.Message);
            }
        }

        /// <summary>
        /// Собирает все объекты (и категории, и достижения) в один список для сериализации
        /// </summary>
        /// <returns>Сформированный список категорий и их достижений</returns>
        private static List<Achievement> GatherAll()
        {
            List<Achievement> final = new List<Achievement>();
            final.AddRange(Manager.Categories); // все категории
            
            // Все достижения, которые привязаны к этим категориям
            foreach (Achievement cat in Manager.Categories)
            {
                List<Achievement> achs = Manager.GetAchievementsForCategory(cat.Id);
                final.AddRange(achs);
            }

            return final;
        }
        #endregion
    }
}