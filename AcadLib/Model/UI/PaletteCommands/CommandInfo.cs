// ReSharper disable once CheckNamespace
namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    [PublicAPI]
    [Serializable]
    public class CommandInfo
    {
        public CommandInfo()
        {
        }

        public CommandInfo(string name)
        {
            CommandName = name;
        }

        /// <summary>
        /// Имя команды
        /// </summary>
        public string CommandName { get; set; }

        /// <summary>
        /// Список времени запуска
        /// </summary>
        public List<DateTime> DatesStart { get; set; }

        public void StartCommand()
        {
            if (DatesStart == null)
                DatesStart = new List<DateTime>();
            DatesStart.Add(DateTime.Now);
        }
    }
}