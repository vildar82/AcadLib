namespace AcadLib.CommandLock.UI
{
    using System.Collections.Generic;
    using Data;
    using JetBrains.Annotations;
    using NetLib.WPF;

    public class LockViewModel : BaseViewModel
    {
        public LockViewModel([NotNull] CommandLockInfo command)
        {
            Command = command;
            Message = command.Message;
            Buttons = new List<Button>();
            if (command.CanContinue)
            {
                ImageKey = "../../../Resources/notify.png";
                Title = "Предупреждение";
                Buttons.Add(new Button
                {
                    Name = "Продолжить",
                    Command = CreateCommand(() => DialogResult = true),
                    IsDefault = true
                });
            }
            else
            {
                ImageKey = "../../../Resources/stop.png";
                Title = "Команда заблокирована";
            }

            Buttons.Add(new Button
            {
                Name = "Выход",
                Command = CreateCommand(() => DialogResult = false),
                IsCancel = true
            });
        }

        public List<Button> Buttons { get; set; }

        public CommandLockInfo Command { get; }

        public string ImageKey { get; set; }

        public string Message { get; set; }

        public string Title { get; set; }
    }
}