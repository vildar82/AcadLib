using AcadLib.UI.Ribbon.Data;
using AcadLib.User.UI;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

namespace AcadLib.UI.Ribbon.Editor.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Windows.Media;
    using Elements;
    using NetLib.WPF;

    public abstract class RibbonItemDataVM : BaseModel
    {
        public RibbonItemDataVM(RibbonItemData item)
        {
            Name = item.Name;
            Description = item.Description;
            IsTest = item.IsTest;
            Access = new ObservableCollection<AccessItem>(item.Access?.Select(s => new AccessItem
            {
                Access = s
            }) ?? new List<AccessItem>());
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTest { get; set; }

        public ImageSource Image { get; set; }

        /// <summary>
        /// Доступ - имена групп и логины.
        /// </summary>
        public ObservableCollection<AccessItem> Access { get; set; }

        public abstract RibbonItemData GetItem();

        protected virtual void FillItem(RibbonItemData item)
        {
            item.Name = Name;
            item.Access = Access?.Select(s => s.Access).ToList();
            item.Description = Description;
            item.IsTest = IsTest;
            var imgName = RibbonGroupData.GetImageName(item.Name);
            RibbonGroupData.SaveImage(Image, imgName, RibbonVM.userGroup);
        }
    }
}
