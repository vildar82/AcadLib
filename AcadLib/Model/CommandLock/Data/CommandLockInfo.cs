namespace AcadLib.CommandLock.Data
{
    /// <summary>
    /// Описание блокировки команды
    /// </summary>
    public class CommandLockInfo
    {
        /// <summary>
        /// Можно ли продолжить выполнение команды.
        /// </summary>
        public bool CanContinue { get; set; }

        /// <summary>
        /// Блокировка включена
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Сообщение о блокировке
        /// </summary>
        public string Message { get; set; }
    }
}