namespace AcadLib.UI.Ribbon.Editor
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using AcadLib.UI.Ribbon.Data;
    using AutoCAD_PIK_Manager.Settings;
    using Data;
    using Elements;
    using JetBrains.Annotations;
    using MahApps.Metro.IconPacks;
    using NetLib;
    using NetLib.WPF;
    using NetLib.WPF.Data;
    using ReactiveUI;

    public class RibbonVM : BaseViewModel
    {
        internal static RibbonVM ribbonVm;
        internal static string userGroup;

        public RibbonVM()
        {
            ribbonVm = this;
            UserGroups = new List<string> { PikSettings.UserGroup, Commands.GroupCommon };
            if (!PikSettings.AdditionalUserGroup.IsNullOrEmpty())
                UserGroups.Add(PikSettings.AdditionalUserGroup);
            UserGroup = PikSettings.UserGroup;
            BlockFiles = BlockFile.GetFiles();
            this.WhenAnyValue(v => v.UserGroup).Subscribe(s =>
            {
                userGroup = UserGroup;
                RibbonGroup = LoadRibbonGroup(UserGroup);
                Tabs = new ObservableCollection<RibbonTabDataVM>(RibbonGroup.Tabs?.Select(t=> new RibbonTabDataVM
                {
                    Name = t.Name,
                    Panels = new ObservableCollection<RibbonPanelDataVM>(t.Panels.Select(p => new RibbonPanelDataVM
                    {
                        Name = p.Name,
                        Items = new ObservableCollection<RibbonItemDataVM>(p.Items?.Select(GetItemVM) ?? new List<RibbonItemDataVM>())
                    }))
                }));
                FreeItems = new ObservableCollection<RibbonItemDataVM>(
                    RibbonGroup.FreeItems?.Select(GetItemVM) ?? new List<RibbonItemDataVM>());
            });

            var iconConverter = new NetLib.WPF.Converters.PackIconImageSourceConverter();
            iconConverter.Convert(PackIconMaterialKind.FormatListBulleted, null, null,
                CultureInfo.CurrentCulture);
            Save = new RelayCommand(SaveExec);
            SelectImage = new RelayCommand(SelectImageExec);
            DeleteSelectedItem = new RelayCommand(DeleteSelectedItemExec);
            DeletePanel = new RelayCommand<RibbonPanelDataVM>(DeletePanelExec, e => ShowMessage(e.Message));
            NewPanel = new RelayCommand(() => SelectedTab.Panels.Add(new RibbonPanelDataVM {Name = "Панель"}));
            AddTab = new RelayCommand(() => Tabs.Add(new RibbonTabDataVM { Name = "Вкладка" + Tabs.Count }));
            AddCommandItem = new RelayCommand(() =>
            {
                FreeItems.Add(new RibbonCommandVM(new RibbonCommand())
                {
                    Image = iconConverter.Convert(PackIconFontAwesomeKind.TerminalSolid, null, null,null) as ImageSource
                });
            });
            AddSplitItem = new RelayCommand(() =>
            {
                FreeItems.Add(new RibbonSplitVM(new RibbonSplit())
                {
                    Image = iconConverter.Convert(PackIconMaterialKind.FormatListBulleted, null, null,null) as ImageSource
                });
            });
            AddInsertBlockItem = new RelayCommand(() =>
            {
                FreeItems.Add(new RibbonInsertBlockVM(new RibbonInsertBlock(), BlockFiles)
                {
                    Image = iconConverter.Convert(PackIconFontAwesomeKind.ObjectGroupRegular, null, null,null) as ImageSource
                });
            });
            AddVisualInsertBlockItem = new RelayCommand(() =>
            {
                FreeItems.Add(new RibbonVisualInsertBlockVM(new RibbonVisualInsertBlock(), BlockFiles)
                {
                    Image = iconConverter.Convert(PackIconFontAwesomeKind.WindowsBrands, null, null,null) as ImageSource
                });
            });
            AddToggleItem = new RelayCommand(() =>
            {
                FreeItems.Add(new RibbonToggleVM(new RibbonToggle())
                {
                    Image = iconConverter.Convert(PackIconMaterialKind.Check, null, null,null) as ImageSource
                });
            });
        }

        public List<string> UserGroups { get; set; }

        public string UserGroup { get; set; }

        public RibbonGroupData RibbonGroup { get; set; }

        public ObservableCollection<RibbonTabDataVM> Tabs { get; set; }

        [NotNull]
        public ObservableCollection<RibbonItemDataVM> FreeItems { get; set; } = new ObservableCollection<RibbonItemDataVM>();

        public RelayCommand Save { get; set; }

        public RibbonItemDataVM SelectedItem { get; set; }

        public RelayCommand SelectImage { get; set; }

        public RelayCommand DeleteSelectedItem { get; set; }

        public List<BlockFile> BlockFiles { get; set; }

        public RibbonTabDataVM SelectedTab { get; set; }

        public RelayCommand<RibbonPanelDataVM> DeletePanel { get; set; }

        public RelayCommand NewPanel { get; set; }

        public RelayCommand AddCommandItem { get; set; }
        public RelayCommand AddSplitItem { get; set; }
        public RelayCommand AddInsertBlockItem { get; set; }
        public RelayCommand AddVisualInsertBlockItem { get; set; }
        public RelayCommand AddToggleItem { get; set; }
        public RelayCommand AddTab { get; set; }

        private static void SaveRibbonGroup(RibbonGroupData ribbonGroup, string userGroup)
        {
            var ribbonFile = RibbonGroupData.GetRibbonFile(userGroup);
            ribbonGroup?.Save(ribbonFile);
        }

        private static RibbonGroupData LoadRibbonGroup(string userGroup)
        {
            if (userGroup.IsNullOrEmpty())
                return null;
            var ribbonFile = RibbonGroupData.GetRibbonFile(userGroup);
            if (File.Exists(ribbonFile))
            {
                return RibbonGroupData.Load(ribbonFile, e => throw e);
            }

            var ribbonGroup = new RibbonGroupData
            {
                Tabs = new List<RibbonTabData>
                {
                    new RibbonTabData
                    {
                        Name = userGroup,
                        Panels = new List<RibbonPanelData>()
                    }
                }
            };
            return ribbonGroup;
        }

        private void SaveExec()
        {
            // Сохранить текущую группу настроек
            try
            {
                if (RibbonGroup != null)
                {
                    ClearImages();
                    RibbonGroup.Tabs = Tabs?.Select(t => new RibbonTabData
                    {
                        Name = t.Name,
                        Panels = t.Panels.Select(p => new RibbonPanelData
                        {
                            Name = p.Name,
                            Items = p.Items.Select(s => s.GetItem()).ToList()
                        }).ToList()
                    }).ToList();
                    RibbonGroup.FreeItems = FreeItems.Select(s => s.GetItem()).ToList();
                    SaveRibbonGroup(RibbonGroup, UserGroup);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearImages()
        {
            var imageDir = RibbonGroupData.GetImagesFolder(UserGroup);
            NetLib.IO.Path.DeleteDir(imageDir);
        }

        private void SelectImageExec()
        {
            try
            {
                var dlg = new OpenFileDialog { Title = "Выбор картинки", Multiselect = false };
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(dlg.FileName, UriKind.Absolute);
                image.EndInit();
                SelectedItem.Image = image;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public RibbonItemDataVM GetItemVM(RibbonItemData item)
        {
            RibbonItemDataVM itemVm;
            switch (item)
            {
                case RibbonBreakPanel ribbonBreak:
                    itemVm = new RibbonBreakVM(ribbonBreak);
                    break;
                case RibbonToggle ribbonToggle:
                    itemVm = new RibbonToggleVM(ribbonToggle);
                    break;
                case RibbonCommand ribbonCommand:
                    itemVm = new RibbonCommandVM(ribbonCommand);
                    break;
                case RibbonVisualGroupInsertBlock ribbonVisualGroupInsertBlock:
                    itemVm = new RibbonVisualGroupInsertBlockVM(ribbonVisualGroupInsertBlock, BlockFiles);
                    break;
                case RibbonVisualInsertBlock ribbonVisualInsertBlock:
                    itemVm = new RibbonVisualInsertBlockVM(ribbonVisualInsertBlock, BlockFiles);
                    break;
                case RibbonInsertBlock ribbonInsertBlock:
                    itemVm = new RibbonInsertBlockVM(ribbonInsertBlock, BlockFiles);
                    break;
                case RibbonSplit ribbonSplit:
                    itemVm = new RibbonSplitVM(ribbonSplit);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }

            itemVm.Name = item.Name;
            itemVm.Access = new ObservableCollection<AccessItem>(item.Access?.Select(s => new AccessItem
            {
                Access = s
            }) ?? new List<AccessItem>());
            itemVm.Description = item.Description;
            itemVm.IsTest = item.IsTest;
            itemVm.Image = RibbonGroupData.LoadImage(userGroup, item.Name);
            return itemVm;
        }

        private void DeleteSelectedItemExec()
        {
            var isFind = false;
            var item = SelectedItem;
            foreach (var tab in Tabs)
            {
                foreach (var panel in tab.Panels)
                {
                    if (panel.Items.Contains(item))
                    {
                        isFind = true;
                        panel.Items.Remove(item);
                    }
                }
            }

            if (isFind)
                FreeItems.Add(item);
            else
                FreeItems.Remove(item);
        }

        private void DeletePanelExec(RibbonPanelDataVM panel)
        {
            foreach (var item in panel.Items)
            {
                FreeItems.Add(item);
            }

            SelectedTab.Panels.Remove(panel);
        }
    }
}
