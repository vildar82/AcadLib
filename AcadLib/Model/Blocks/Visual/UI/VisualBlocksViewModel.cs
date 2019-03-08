namespace AcadLib.Blocks.Visual.UI
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive;
    using System.Windows;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using ReactiveUI;

    public class VisualBlocksViewModel : BaseViewModel
    {
        public VisualBlocksViewModel()
        {
        }

        public VisualBlocksViewModel([NotNull] List<IVisualBlock> visuals)
        {
            Groups = visuals.GroupBy(g => g.Group).Select(s => new VisualGroup { Name = s.Key, Blocks = s.ToList() }).ToList();
            Insert = CreateCommand<IVisualBlock>(OnInsertExecute);
            VisibleSeparator = Groups.Count > 1 ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool Explode { get; set; }

        public List<VisualGroup> Groups { get; set; }

        public ReactiveCommand<IVisualBlock, Unit> Insert { get; set; }

        public bool IsHideWindow { get; set; } = true;

        public Visibility VisibleSeparator { get; set; }

        private void OnInsertExecute(IVisualBlock block)
        {
            var doc = AcadHelper.Doc;
            using (doc.LockDocument())
            {
                if (IsHideWindow)
                {
                    HideMe();
                    VisualInsertBlock.Insert(block, Explode);
                }
                else
                {
                    using (HideWindow())
                    {
                        VisualInsertBlock.Insert(block, Explode);
                    }
                }
            }
        }
    }
}