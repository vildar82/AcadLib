namespace AcadLib.CommandLock.Data
{
    /// <summary>
    /// Данные блокировко команд
    /// </summary>
    public class CommandLocks
    {
        /// <summary>
        /// Список заблокированных команд
        /// </summary>
        public CaseInsensitiveDictionary Locks { get; set; } = new CaseInsensitiveDictionary();
    }
}