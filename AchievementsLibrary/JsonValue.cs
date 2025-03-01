namespace AchievementsLibrary
{
    /// <summary>
    /// Тип значения JSON: объект, массив, строка, число, bool, null
    /// </summary>
    public enum JsonValueType
    {
        Object,
        Array,
        String,
        Number,
        Boolean,
        Null
    }

    /// <summary>
    /// Простая модель узла JSON, а также статический метод <see cref="Parse(string)"/>.
    /// Реализовано через рекурсивный спуск для проверки валидности
    /// </summary>
    public class JsonValue
    {
        public JsonValueType Type { get; private set; }

        // Храним значение (в зависимости от Type)
        private readonly Dictionary<string, JsonValue>? _objectValue;
        private readonly List<JsonValue>? _arrayValue;
        private string? _stringValue;
        private double? _numberValue;
        private bool? _boolValue;

        #region Конструкторы-фабрики
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="t">Тип значения Json</param>
        private JsonValue(JsonValueType t)
        {
            Type = t;
            if (t == JsonValueType.Object)
            {
                _objectValue = new Dictionary<string, JsonValue>();
            }

            if (t == JsonValueType.Array)
            {
                _arrayValue = new List<JsonValue>();
            }

            if (t == JsonValueType.String)
            {
                _stringValue = string.Empty;
            }
        }
        /// <summary>
        /// Создаёт новый JsonValue типа Object 
        /// </summary>
        public static JsonValue NewObject()
        {
            return new JsonValue(JsonValueType.Object);
        }

        /// <summary>
        /// Создаёт новый JsonValue типа Array 
        /// </summary>
        public static JsonValue NewArray()
        {
            return new JsonValue(JsonValueType.Array);
        }

        /// <summary>
        /// Создаёт новый JsonValue типа String
        /// </summary>
        public static JsonValue NewString(string s)
        {
            JsonValue jv = new JsonValue(JsonValueType.String) { _stringValue = s };
            return jv;
        }
        /// <summary>
        /// Создаёт новый JsonValue типа Number
        /// </summary>
        public static JsonValue NewNumber(double d)
        {
            JsonValue jv = new JsonValue(JsonValueType.Number) { _numberValue = d };
            return jv;
        }
        /// <summary>
        /// Создаёт новый JsonValue типа Boolean
        /// </summary>
        public static JsonValue NewBoolean(bool b)
        {
            JsonValue jv = new JsonValue(JsonValueType.Boolean) { _boolValue = b };
            return jv;
        }
        /// <summary>
        /// Создаёт новый JsonValue типа Null
        /// </summary>
        public static JsonValue NewNull()
        {
            return new JsonValue(JsonValueType.Null);
        }
        #endregion

        #region Доступ
        /// <summary>
        /// Возвращает словарь, если данный узел является объектом; иначе выбрасывает исключение
        /// </summary>
        public Dictionary<string, JsonValue> AsObject()
        {
            if (Type != JsonValueType.Object)
            {
                throw new InvalidOperationException("Not an object");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed
            return _objectValue!;
        }
        
        /// <summary>
        /// Возвращает список JsonValue, если данный узел является массивом; иначе выбрасывает исключение
        /// </summary>
        public List<JsonValue> AsArray()
        {
            if (Type != JsonValueType.Array)
            {
                throw new InvalidOperationException("Not an array");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed
            return _arrayValue!;
        }
        
        /// <summary>
        /// Возвращает строковое содержимое, если данный узел является строкой; иначе исключение
        /// </summary>
        public string AsString()
        {
            if (Type != JsonValueType.String)
            {
                throw new InvalidOperationException("Not a string");
            }

            // ReSharper disable once NullableWarningSuppressionIsUsed
            return _stringValue!;
        }

        /// <summary>
        /// Преобразует текущее значение в читаемую строку
        /// </summary>
        public override string ToString()
        {
            return Type switch
            {
                JsonValueType.String  => $"\"{_stringValue}\"",
                JsonValueType.Number  => _numberValue?.ToString() ?? "0",
                JsonValueType.Boolean => _boolValue?.ToString() ?? "",
                JsonValueType.Null    => "null",
                JsonValueType.Object  => "[Object]",
                JsonValueType.Array   => "[Array]",
                _ => "???"
            };
        }

        /// <summary>
        /// Преобразует текущее значение в строку (для поля Achievement), игнорируя объекты и массивы
        /// </summary>
        public string ToStringValue()
        {
            return Type switch
            {
                JsonValueType.String  => _stringValue ?? "",
                JsonValueType.Number  => _numberValue?.ToString() ?? "0",
                JsonValueType.Boolean => _boolValue?.ToString() ?? "false",
                JsonValueType.Null    => "",
                _ => "" // Для объектов/массивов пустая строка
            };
        }
        #endregion

        #region Parse
        /// <summary>
        /// Разбирает JSON-строку целиком, возвращая корневой JsonValue (или бросает FormatException)
        /// </summary>
        /// <param name="text">Текст JSON</param>
        /// <returns>Результирующий JsonValue</returns>
        public static JsonValue Parse(string text)
        {
            JsonStringParser parser = new JsonStringParser(text);
            JsonValue value = parser.ParseValue();
            parser.SkipSpaces();
            if (!parser.EndOfText)
            {
                throw new FormatException("Лишние данные после окончания JSON.");
            }

            return value;
        }
        #endregion
    }
}