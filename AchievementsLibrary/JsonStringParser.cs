using System.Text;

namespace AchievementsLibrary 
{
    /// <summary>
    /// Вспомогательный класс: парсер JSON через рекурсивный спуск (строго проверяет структуру)
    /// </summary>
    internal class JsonStringParser
    {
        private readonly string _text;
        private int _pos;

        /// <summary>
        /// Конструктор, который создаёт парсер для указанного текста JSON
        /// </summary>
        /// <param name="text">Текст JSON</param>
        public JsonStringParser(string text)
        {
            _text = text;
            _pos = 0;
        }

        /// <summary>
        /// Признак, что мы дошли до конца текста
        /// </summary>
        public bool EndOfText => _pos >= _text.Length;

        /// <summary>
        /// Парсит текущее значение (объект, массив, строка, число, bool или null)
        /// </summary>
        /// <returns>JsonValue</returns>
        public JsonValue ParseValue()
        {
            SkipSpaces();
            if (EndOfText)
            {
                throw new FormatException("Ожидалось значение, но найден конец строки.");
            }

            char c = _text[_pos];
            switch (c)
            {
                case '{': return ParseObject();
                case '[': return ParseArray();
                case '"': return ParseString();
                case 't':
                case 'f': return ParseBoolean();
                case 'n': return ParseNull();
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return ParseNumber();
                default:
                    throw new FormatException($"Неизвестный символ '{c}' при разборе значения");
            }
        }

        /// <summary>
        /// Парсит объект { ... }
        /// </summary>
        private JsonValue ParseObject()
        {
            if (_text[_pos] != '{')
            {
                throw new FormatException("ParseObject: ожидается '{'");
            }

            _pos++;
            SkipSpaces();

            JsonValue obj = JsonValue.NewObject();
            Dictionary<string, JsonValue> dict = obj.AsObject();

            // Проверяем пустой объект
            if (!EndOfText && _text[_pos] == '}')
            {
                _pos++;
                return obj; // пустой объект
            }

            // иначе парсим пары "ключ": значение
            while (true)
            {
                SkipSpaces();
                // ключ - строка
                if (_text[_pos] != '"')
                {
                    throw new FormatException("ParseObject: ожидается строка-ключ в двойных кавычках.");
                }

                JsonValue keyVal = ParseString();
                if (keyVal.Type != JsonValueType.String)
                {
                    throw new FormatException("ParseObject: ключ должен быть строкой.");
                }

                string key = keyVal.AsString();

                SkipSpaces();
                if (EndOfText || _text[_pos] != ':')
                {
                    throw new FormatException("ParseObject: ожидается ':' после ключа.");
                }

                _pos++;

                SkipSpaces();
                JsonValue val = ParseValue();

                dict[key] = val;

                SkipSpaces();
                if (EndOfText)
                {
                    throw new FormatException("ParseObject: не закрыт объект '}'");
                }

                if (_text[_pos] == '}')
                {
                    _pos++;
                    break;
                }

                if (_text[_pos] == ',')
                {
                    _pos++;
                    continue;
                }

                throw new FormatException($"ParseObject: ожидается ',' или '}}', а найден символ '{_text[_pos]}'");
            }

            return obj;
        }

        /// <summary>
        /// Парсит массив [ ... ]
        /// </summary>
        private JsonValue ParseArray()
        {
            if (_text[_pos] != '[')
            {
                throw new FormatException("ParseArray: ожидается '['");
            }

            _pos++;
            SkipSpaces();

            JsonValue arr = JsonValue.NewArray();
            List<JsonValue> list = arr.AsArray();

            if (!EndOfText && _text[_pos] == ']')
            {
                _pos++;
                return arr; // пустой массив
            }

            while (true)
            {
                SkipSpaces();
                JsonValue val = ParseValue();
                list.Add(val);

                SkipSpaces();
                if (EndOfText)
                {
                    throw new FormatException("ParseArray: не закрыт массив ']'");
                }

                if (_text[_pos] == ']')
                {
                    _pos++;
                    break;
                }

                if (_text[_pos] == ',')
                {
                    _pos++;
                    continue;
                }

                throw new FormatException($"ParseArray: ожидается ',' или ']', а найден символ '{_text[_pos]}'");
            }

            return arr;
        }

        /// <summary>
        /// Парсит строку в кавычках "..." (с учётом простых escape‐последовательностей)
        /// </summary>
        private JsonValue ParseString()
        {
            if (_text[_pos] != '"')
            {
                throw new FormatException("ParseString: ожидается '\"'");
            }

            _pos++;

            StringBuilder sb = new StringBuilder();
            while (!EndOfText)
            {
                char c = _text[_pos];
                _pos++;
                if (c == '"')
                {
                    return JsonValue.NewString(sb.ToString());
                }

                if (c == '\\')
                {
                    if (EndOfText)
                    {
                        throw new FormatException("Обрыв после символа '\\'");
                    }

                    char esc = _text[_pos];
                    _pos++;
                    switch (esc)
                    {
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '"':
                            sb.Append('"');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'u':
                            if (_pos + 4 > _text.Length)
                            {
                                throw new FormatException("Недостаточно символов для \\uXXXX");
                            }

                            string hex = _text.Substring(_pos, 4);
                            if (!ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ushort code))
                            {
                                throw new FormatException($"Неверный \\u код: {hex}");
                            }

                            sb.Append((char)code);
                            _pos += 4;
                            break;
                        default:
                            sb.Append(esc);
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }

            throw new FormatException("Строка не закрыта кавычкой.");
        }

        /// <summary>
        /// Парсит литерал true/false
        /// </summary>
        private JsonValue ParseBoolean()
        {
            if (Match("true"))
            {
                return JsonValue.NewBoolean(true);
            }

            if (Match("false"))
            {
                return JsonValue.NewBoolean(false);
            }

            throw new FormatException("Ожидался литерал true/false");
        }

        /// <summary>
        /// Парсит литерал null
        /// </summary>
        private JsonValue ParseNull()
        {
            if (Match("null"))
            {
                return JsonValue.NewNull();
            }

            throw new FormatException("Ожидался литерал null");
        }

        /// <summary>
        /// Парсит число (целое или с точкой)
        /// </summary>
        private JsonValue ParseNumber()
        {
            int start = _pos;
            bool hasDot = false;

            if (_text[_pos] == '-')
            {
                _pos++;
            }

            while (!EndOfText && (char.IsDigit(_text[_pos]) || _text[_pos] == '.'))
            {
                if (_text[_pos] == '.')
                {
                    if (hasDot)
                    {
                        throw new FormatException("Число содержит более одной точки");
                    }

                    hasDot = true;
                }

                _pos++;
            }

            string numStr = _text.Substring(start, _pos - start);
            if (!double.TryParse(numStr, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out double value))
            {
                throw new FormatException($"Не удалось разобрать число '{numStr}'");
            }

            return JsonValue.NewNumber(value);
        }

        /// <summary>
        /// Пропускает пробелы и управляющие символы
        /// </summary>
        public void SkipSpaces()
        {
            while (!EndOfText && char.IsWhiteSpace(_text[_pos]))
            {
                _pos++;
            }
        }

        /// <summary>
        /// Проверяет, совпадает ли следующий участок текста с указанной подстрокой
        /// Если совпадает - сдвигает позицию, иначе false
        /// </summary>
        private bool Match(string s)
        {
            if (_pos + s.Length > _text.Length)
            {
                return false;
            }

            if (_text.Substring(_pos, s.Length) == s)
            {
                _pos += s.Length;
                return true;
            }

            return false;
        }
    }
}