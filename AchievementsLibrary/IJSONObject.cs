namespace AchievementsLibrary
{
    /// <summary>
    /// Интерфейс для объектов, представляемых в JSON
    /// Предоставляет методы для перечисления полей и доступа к ним по строковому имени
    /// </summary>
    public interface IJsonObject
    {
        /// <summary>
        /// Возвращает список названий полей, ожидаемых в JSON
        /// </summary>
        IEnumerable<string> GetAllFields();

        /// <summary>
        /// Возвращает значение поля как строку (или null, если такого поля нет)
        /// </summary>
        /// <param name="fieldName">Имя поля, как в JSON</param>
        /// <returns>Строковое представление значения поля, либо null</returns>
        string? GetField(string fieldName);

        /// <summary>
        /// Устанавливает значение поля (строка из JSON). 
        /// Если поле отсутствует, можно сгенерировать KeyNotFoundException 
        /// или просто игнорировать (в зависимости от логики).
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="value">Значение поля</param>
        void SetField(string fieldName, string value);
    }
}