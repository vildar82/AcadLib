namespace AcadLib.Errors.UI
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows;
    using JetBrains.Annotations;
    using NetLib;

    public class ErrorModelList : ErrorModelBase
    {
        public ErrorModelList([NotNull] List<IError> sameErrors)
            : base(sameErrors.First())
        {
            VisibilityCount = Visibility.Visible;
            firstErr = sameErrors.First();
            Message = firstErr.Group;
            Header = new ErrorModelOne(firstErr, null)
            {
                AddButtons = null,
                MarginHeader = new Thickness(1),
                Parent = this,
                ShowCount = true,
                Message = firstErr.Group
            };
            SameErrors = new ObservableCollection<ErrorModelBase>(
                sameErrors.Select(s => new ErrorModelOne(s, this)));
        }

        [Reactive]
        public ObservableCollection<ErrorModelBase> SameErrors { get; set; }

        public ErrorModelOne Header { get; set; }

        [Reactive]
        public bool IsExpanded { get; set; }

        protected override void OnShowExecute()
        {
            base.OnShowExecute();
            IsExpanded = !IsExpanded;
        }
    }
}
