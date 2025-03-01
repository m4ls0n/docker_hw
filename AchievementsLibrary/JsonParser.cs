using System.Text;

namespace AchievementsLibrary
{
    /// <summary>
    /// Статический класс для чтения/записи JSON из/в потоки 
    /// и для парсинга массива "achievements": [ ... ] в список Achievement
    /// </summary>
    public static class JsonParser
    {
        /// <summary>
        /// Читает JSON из Console.In целиком, затем парсит
        /// </summary>
        /// <returns>Список достижений</returns>
        public static List<Achievement> ReadAchievementsFromConsole()
        {
            string input = ReadAll(Console.In);
            return ParseAchievements(input);
        }

        /// <summary>
        /// Читает JSON из указанного файла, затем парсит
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <returns>Список достижений</returns>
        public static List<Achievement> ReadAchievementsFromFile(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Файл {path} не найден.");
            }

            string input = File.ReadAllText(path, Encoding.UTF8);
            return ParseAchievements(input);
        }

        /// <summary>
        /// Пишет список достижений как JSON в Console.Out, формируя структуру
        /// { "achievements": [ { ... }, { ... } ] }.
        /// </summary>
        /// <param name="achievements">Список достижений для сериализации</param>
        public static void WriteAchievementsToConsole(IReadOnlyList<Achievement> achievements)
        {
            TextWriter writer = Console.Out;
            writer.WriteLine("{");
            writer.WriteLine("  \"achievements\": [");

            for (int i = 0; i < achievements.Count; i++)
            {
                Achievement a = achievements[i];
                writer.WriteLine("    {");
                
                IEnumerable<string> fields = a.GetAllFields();
                List<string> nonNullFields = new();
                foreach (string f in fields)
                {
                    a.GetField(f);
                    nonNullFields.Add(f);
                }

                for (int fIndex = 0; fIndex < nonNullFields.Count; fIndex++)
                {
                    string fieldName = nonNullFields[fIndex];
                    string fieldVal  = a.GetField(fieldName);
                    
                    // Простейший вариант: проверяем bool (True/False), иначе строка
                    bool isBool = fieldVal == "True" || fieldVal == "False";
                    if (isBool)
                    {
                        fieldVal = fieldVal.ToLower();
                    }

                    writer.Write("      ");
                    writer.Write("\"" + EscapeString(fieldName) + "\": ");
                    if (isBool)
                    {
                        writer.Write(fieldVal);
                    }
                    else
                    {
                        writer.Write("\"" + EscapeString(fieldVal) + "\""); // как строку
                    }

                    if (fIndex < nonNullFields.Count - 1)
                    {
                        writer.Write(",");
                    }

                    writer.WriteLine();
                }
                writer.Write("    }");
                if (i < achievements.Count - 1)
                {
                    writer.Write(",");
                }

                writer.WriteLine();
            }

            writer.WriteLine("  ]");
            writer.WriteLine("}");
        }

        #region Основной метод парсинга
        /// <summary>
        /// Разбирает JSON‐строку, ищет массив "achievements": [ ... ] и внутри создаёт Achievement
        /// </summary>
        /// <param name="json">Вся строка JSON</param>
        /// <returns>Список Achievement</returns>
        private static List<Achievement> ParseAchievements(string json)
        {
            JsonValue? root = JsonValue.Parse(json);
            if (root == null || root.Type != JsonValueType.Object)
            {
                throw new FormatException("Неверный формат JSON— корневой элемент должен быть объектом.");
            }

            Dictionary<string, JsonValue> dict = root.AsObject();
            if (!dict.ContainsKey("achievements") || dict["achievements"].Type != JsonValueType.Array)
            {
                throw new FormatException("Не найден необходимый массив 'achievements'.");
            }

            List<JsonValue> arr = dict["achievements"].AsArray();
            List<Achievement> results = new List<Achievement>();

            foreach (JsonValue item in arr)
            {
                if (item.Type != JsonValueType.Object)
                {
                    throw new FormatException("Еще один элемент! Ожидаем объект.");
                }

                Dictionary<string, JsonValue> obj = item.AsObject();
                Achievement achievement = new Achievement();

                // Переход через поля, получение данных
                foreach (KeyValuePair<string, JsonValue> kvp in obj)
                {
                    try
                    {
                        string valStr = kvp.Value.ToStringValue();
                        achievement.SetField(kvp.Key, valStr);
                    }
                    catch (KeyNotFoundException)
                    {
                        // Игнорируем поля, которых нет в классе.
                    }
                }
                results.Add(achievement);
            }

            return results;
        }
        #endregion

        #region Вспомогательные методы
        /// <summary>
        /// Считывает весь текст из указанного TextReader (до конца).
        /// </summary>
        private static string ReadAll(TextReader reader)
        {
            StringBuilder sb = new();
            while (reader.ReadLine() is { } line)
            {
                sb.AppendLine(line);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// Экранирует символы \ и " в строке для корректной записи в JSON.
        /// </summary>
        private static string EscapeString(string s)
        {
            // Упрощённое экранирование
            return s
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"");
        }
        #endregion
    }
}