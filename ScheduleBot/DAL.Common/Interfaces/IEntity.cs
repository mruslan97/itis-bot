namespace DAL.Common
{
    /// <summary>
    /// Базовая сущность
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Идентификатор сущности
        /// </summary>
        long Id { get; set; }
    }
}
