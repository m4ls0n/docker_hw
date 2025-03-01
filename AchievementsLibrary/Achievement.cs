namespace AchievementsLibrary
{
    /// <summary>
    /// Структура для представления JSON-объекта: 
    /// Либо категория (IsCategory = true),
    /// Либо обычное достижение (IsCategory = false).
    /// </summary>
    public struct Achievement : IJsonObject
    {
        /// <summary>
        /// Свойства для полей
        /// </summary>
        public string Id { get; private set; } = string.Empty;
        public string Label { get; private set; } = string.Empty;
        private string DescriptionUnlocked { get; set; } = string.Empty;
        public bool IsCategory { get; private set; }
        public string Category { get; private set; } = string.Empty;
        private bool IsHidden { get; set; }
        private string IconUnlocked { get; set; } = string.Empty;
        private bool SingleDescription { get; set; }
        private bool ValidateOnStorefront { get; set; }
        public double GlobalPercent { get; set; } = -1;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        public Achievement() { }

        /// <summary>
        /// Возвращает список имён полей, ожидаемых в JSON (в нижнем регистре).
        /// </summary>
        public IEnumerable<string> GetAllFields()
        {
            return new List<string>
            {
                "id",
                "label",
                "descriptionunlocked",
                "iscategory",
                "category",
                "ishidden",
                "iconunlocked",
                "singledescription",
                "validateonstorefront"
            };
        }

        /// <summary>
        /// Возвращает значение поля (как строку) по имени (в нижнем регистре)
        /// Если поля нет, возвращается null
        /// </summary>
        /// <param name="fieldName">Имя поля (строчное, в нижнем регистре)</param>
        /// <returns>Значение поля, либо null</returns>
        public string GetField(string fieldName)
        {
            switch (fieldName.ToLowerInvariant())
            {
                case "id": return Id;
                case "label": return Label;
                case "descriptionunlocked": return DescriptionUnlocked;
                case "iscategory": return IsCategory.ToString();
                case "category": return Category;
                case "ishidden": return IsHidden.ToString();
                case "iconunlocked": return IconUnlocked;
                case "singledescription": return SingleDescription.ToString();
                case "validateonstorefront": return ValidateOnStorefront.ToString();
                default:
                    // ReSharper disable once NullableWarningSuppressionIsUsed
                    return null!;
            }
        }

        /// <summary>
        /// Устанавливает значение поля (как строку) по имени (в нижнем регистре)
        /// При несоответствии типов может бросать FormatException
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="value">Значение поля</param>
        public void SetField(string fieldName, string value)
        {
            switch (fieldName.ToLowerInvariant())
            {
                case "id":
                    Id = value;
                    break;
                case "label":
                    Label = value;
                    break;
                case "descriptionunlocked":
                    DescriptionUnlocked = value;
                    break;
                case "iscategory":
                    if (bool.TryParse(value, out bool cat))
                    {
                        IsCategory = cat;
                    }
                    else
                    {
                        throw new FormatException($"Не удалось интерпретировать 'isCategory' как bool: {value}");
                    }

                    break;
                case "category":
                    Category = value;
                    break;
                case "ishidden":
                    if (bool.TryParse(value, out bool hid))
                    {
                        IsHidden = hid;
                    }
                    else
                    {
                        throw new FormatException($"Не удалось интерпретировать 'isHidden' как bool: {value}");
                    }

                    break;
                case "iconunlocked":
                    IconUnlocked = value;
                    break;
                case "singledescription":
                    if (bool.TryParse(value, out bool sdr))
                    {
                        SingleDescription = sdr;
                    }
                    else
                    {
                        throw new FormatException($"Не удалось интерпретировать 'singleDescription' как bool: {value}");
                    }

                    break;
                case "validateonstorefront":
                    if (bool.TryParse(value, out bool crd))
                    {
                        ValidateOnStorefront = crd;
                    }
                    else
                    {
                        throw new FormatException($"Не удалось интерпретировать 'validateonstorefront' как bool: {value}");
                    }

                    break;
            }
        }
    }
}