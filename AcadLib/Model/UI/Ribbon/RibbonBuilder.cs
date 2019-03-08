namespace AcadLib.UI.Ribbon
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.Private.Windows;
    using Autodesk.Windows;
    using Data;
    using Elements;
    using Files;
    using JetBrains.Annotations;
    using NetLib;
    using Options;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

    /// <summary>
    ///     Создает ленту
    /// </summary>
    public static class RibbonBuilder
    {
        private static readonly LocalFileData<RibbonOptions> ribbonOptions;
        private static bool isInitialized;
        private static RibbonControl ribbon;

        static RibbonBuilder()
        {
            // Загрузка настроек ленты
            ribbonOptions = FileDataExt.GetLocalFileData<RibbonOptions>("Ribbon", "RibbonOptions", false);
            ribbonOptions.TryLoad();
            if (ribbonOptions.Data == null)
            {
                ribbonOptions.Data = new RibbonOptions();
            }
            else
            {
                ribbonOptions.Data.Tabs = ribbonOptions.Data.Tabs.OrderBy(o => o.Index).ToList();
            }
        }

        public static void ChangeToggleState(string commandName)
        {
            SetToggleState(commandName, !GetToggleState(commandName));
        }

        public static void SetToggleState(string commandName, bool state)
        {
            if (commandName.IsNullOrEmpty()) return;
            ribbonOptions.Data.DictToggleState[commandName] = state;
        }

        public static bool GetToggleState(string commandName)
        {
            if (commandName.IsNullOrEmpty()) return false;
            ribbonOptions.Data.DictToggleState.TryGetValue(commandName, out var state);
            return state;
        }

        public static void InitRibbon()
        {
            if (ribbon != null || isInitialized)
                return;
            isInitialized = true;
            ComponentManager.ItemInitialized += ComponentManager_ItemInitialized;
        }

        internal static void CreateRibbon()
        {
            try
            {
                // Загрузка ленты по группам настроек
                CreateRibbon(LoadRibbonTabsFromGroups());
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CreateRibbon");
            }
        }

        public static List<(RibbonTabData, string)> LoadRibbonTabsFromGroups()
        {
            var groupsName = new List<string>
                { PikSettings.UserGroup, Commands.GroupCommon, PikSettings.AdditionalUserGroup };
            return groupsName.Where(w => !w.IsNullOrEmpty())
                .Select(s => RibbonGroupData.Load(RibbonGroupData.GetRibbonFile(s))?.Tabs.Select(t => (t, s)))
                .Where(w => w != null).SelectMany(s => s).ToList();
        }

        private static void CreateRibbon(List<(RibbonTabData, string)> tabsData)
        {
            try
            {
                if (ribbon == null)
                    ribbon = ComponentManager.Ribbon;
                ribbon.Tabs.CollectionChanged -= Tabs_CollectionChanged;

                foreach (var tabData in tabsData.OrderBy(o=> GetTabIndex(o.Item1.Name)))
                {
                    if (ribbon.FindTab(tabData.Item1.Name) != null)
                    {
                        Logger.Log.Info($"RibbonBuilder. Такая вкладка уже есть - {tabData.Item1.Name}");
                        continue;
                    }

                    var tabOpt = CreateTab(tabData.Item1, tabData.Item2);
                    var tab = (RibbonTab) tabOpt.Item;
                    if (tab == null || tabOpt.Items?.Any() != true)
                        continue;
                    AddItem(tabOpt.Index, tab, ribbon.Tabs);
                    tab.Panels.CollectionChanged += Panels_CollectionChanged;
                    tab.PropertyChanged += Tab_PropertyChanged;
                }

                var activeTab = (RibbonTab)ribbonOptions.Data.Tabs.FirstOrDefault(t => t.UID == ribbonOptions.Data.ActiveTab)?.Item;
                if (activeTab != null)
                {
                    ribbon.ActiveTab = activeTab;
                }

                ribbon.Tabs.CollectionChanged += Tabs_CollectionChanged;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CreateRibbon");
            }
        }

        [NotNull]
        private static ItemOptions CreateTab([NotNull] RibbonTabData tabData, string userGroup)
        {
            var tab = new RibbonTab
            {
                Title = tabData.Name,
                Name = tabData.Name,
                Id = tabData.Name,
                UID = tabData.Name
            };
            var tabOptions = GetItemOptions(tab, ribbonOptions.Data.Tabs);
            tab.IsVisible = tabOptions.IsVisible;
            tabOptions.Items = tabData.Panels?.Select(p => CreatePanel(p, tabOptions, userGroup)).Where(w => w != null)
                                   .OrderBy(o => o.Index).ToList() ?? new List<ItemOptions>();
            foreach (var panelOpt in tabOptions.Items.Where(w => w != null))
            {
                var panel = (RibbonPanel) panelOpt.Item;
                if (panel?.Source?.Items == null || panel.Source.Items.Count == 0)
                    continue;
                tab.Panels.Add(panel);
            }

            return tabOptions;
        }

        private static ItemOptions CreatePanel(RibbonPanelData panelData, [NotNull] ItemOptions tabOptions, string userGroup)
        {
            var name = panelData.Name.IsNullOrEmpty() ? "Главная" : panelData.Name;
            var panelSource = new RibbonPanelSource
            {
                Name = name,
                Id = name,
                Title = name,
                UID = name
            };
            foreach (var part in panelData.Items.Where(w => IsAccess(w.Access)).SplitParts(2))
            {
                var row = new RibbonRowPanel();
                foreach (var element in part)
                {
                    var item = GetItem(element, userGroup);
                    if (item == null)
                        continue;
                    row.Items.Add(item);
                }

                panelSource.Items.Add(row);
                panelSource.Items.Add(new RibbonRowBreak());
            }

            var panel = new RibbonPanel { Source = panelSource, UID = panelSource.UID };
            var panelOpt = GetItemOptions(panel, tabOptions.Items);
            panel.IsVisible = panelOpt.IsVisible;
            panel.PropertyChanged += Panel_PropertyChanged;
            return panelOpt;
        }

        private static RibbonItem GetItem(RibbonItemData element, string userGroup)
        {
            try
            {
                var image = GetImage(element, userGroup);
                RibbonItem item = null;
                var size = RibbonItemSize.Large;
                switch (element)
                {
                    case RibbonBreakPanel _:
                        item = new RibbonPanelBreak();
                        break;
                    case RibbonToggle ribbonToggle:
                        var t = new RibbonToggleButton();
                        item = t;
                        t.IsThreeState = false;
                        t.IsChecked = GetToggleState(ribbonToggle);
                        t.CommandHandler = ribbonToggle.GetCommand();
                        break;
                    case RibbonVisualGroupInsertBlock ribbonVisualGroupInsertBlock:
                        break;
                    case RibbonVisualInsertBlock ribbonVisualInsertBlock:
                        var vb = new RibbonButton();
                        vb.CommandHandler = ribbonVisualInsertBlock.GetCommand();
                        item = vb;
                        break;
                    case RibbonInsertBlock ribbonInsertBlock:
                        var ib = new RibbonButton();
                        ib.CommandHandler = ribbonInsertBlock.GetCommand();
                        item = ib;
                        break;
                    case RibbonSplit splitElem:
                        size = RibbonItemSize.Standard;
                        item = CreateSplitButton(splitElem, userGroup);
                        break;
                    case RibbonCommand command:
                        var b = new RibbonButton();
                        b.CommandHandler = command.GetCommand();
                        item = b;
                        break;
                }

                if (item == null)
                {
                    Logger.Log.Error(
                        $"Не создан элемент ленты из имя='{element.Name}', группа настроек='{userGroup}'.");
                    return null;
                }

                SetItemData(item, element, image, size);
                return item;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"Ошибка создание элемента ленты '{element?.Name}' '{userGroup}'");
                return null;
            }
        }

        private static bool GetToggleState(RibbonToggle ribbonToggle)
        {
            if (ribbonToggle.Command.IsNullOrEmpty()) return false;
            return ribbonOptions.Data.DictToggleState.TryGetValue(ribbonToggle.Command, out var state)
                ? state
                : ribbonToggle.IsChecked;
        }

        [NotNull]
        private static RibbonSplitButton CreateSplitButton([NotNull] RibbonSplit splitElem, string userGroup)
        {
            var splitB = new RibbonSplitButton
            {
                ListImageSize = RibbonImageSize.Large,
                ShowText = false
            };

            foreach (var elem in splitElem.Items.Where(w => IsAccess(w.Access)))
            {
                var item = GetItem(elem, userGroup);
                if (item == null) continue;
                splitB.Items.Add(item);
            }

            return splitB;
        }

        [NotNull]
        private static void SetItemData([NotNull] RibbonItem item, RibbonItemData itemData, ImageSource image, RibbonItemSize size)
        {
            item.Text = item.Name; // Текст рядом с кнопкой, если ShowText = true
            item.Name = item.Name; // Тест на всплявающем окошке (заголовов)
            item.Description = item.Description; // Описание на всплывающем окошке
            item.LargeImage = ResizeImage(image, 32);
            item.Image = ResizeImage(image, 16);
            item.ToolTip = GetToolTip(itemData, image);
            item.IsToolTipEnabled = true;
            item.ShowImage = image != null;
            item.ShowText = false;
            item.Size = size;
        }

        private static void AddItem<T>(int index, [NotNull] T item, [NotNull] IList<T> items)
            where T : IRibbonContentUid
        {
            if (index > items.Count)
            {
                index = ribbon.Tabs.Count;
            }
            else if (index < 0)
            {
                items.Add(item);
                return;
            }

            items.Insert(index, item);
        }

        private static void Application_SystemVariableChanged(object sender, [NotNull] SystemVariableChangedEventArgs e)
        {
            if (e.Name.Equals("WSCURRENT"))
                CreateRibbon();
        }

        private static void ComponentManager_ItemInitialized(object sender, RibbonItemEventArgs e)
        {
            ribbon = ComponentManager.Ribbon;
            if (ribbon == null)
                return;
            ComponentManager.ItemInitialized -= ComponentManager_ItemInitialized;
            CreateRibbon();
            Application.SystemVariableChanged += Application_SystemVariableChanged;
        }

        private static void Tab_PropertyChanged(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(RibbonTab.IsVisible):
                    var tab = (RibbonTab)sender;
                    var tabOpt = ribbonOptions.Data.Tabs.FirstOrDefault(t => t.UID == tab.UID);
                    if (tabOpt == null)
                        return;
                    tabOpt.IsVisible = tab.IsVisible;
                    SaveOptions();
                    break;
                case nameof(RibbonTab.IsActive):
                    SaveActiveTab();
                    break;
            }
        }

        public static void SaveActiveTab()
        {
            if (ribbon?.ActiveTab != null)
            {
                ribbonOptions.Data.ActiveTab = ribbon.ActiveTab.UID;
                ribbonOptions.TrySave();
            }
        }

        [NotNull]
        private static ItemOptions GetItemOptions<T>([NotNull] T item, [NotNull] List<ItemOptions> itemOptions)
            where T : IRibbonContentUid
        {
            var tabOption = itemOptions.FirstOrDefault(t => t.UID.Equals(item.UID));
            if (tabOption == null)
            {
                tabOption = new ItemOptions
                {
                    UID = item.UID,
                    Index = 0,
                    Item = item
                };
                itemOptions.Add(tabOption);
            }
            else
            {
                tabOption.Item = item;
            }

            return tabOption;
        }

        private static int GetTabIndex(string name)
        {
            return ribbonOptions.Data.Tabs.FirstOrDefault(t => t.UID == name)?.Index ?? 0;
        }

        [NotNull]
        private static RibbonToolTip GetToolTip([NotNull] RibbonItemData item, ImageSource image)
        {
            return new RibbonToolTip
            {
                Title = item.Name,
                Content = item.Description,
                IsHelpEnabled = false,
                Image = image,
            };
        }

        private static void Panel_PropertyChanged(object sender, [NotNull] PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsVisible")
            {
                var panel = (RibbonPanel)sender;
                var tab = panel.Tab;
                if (tab == null)
                    return;
                var tabOpt = ribbonOptions.Data.Tabs.FirstOrDefault(t => t.UID == tab.UID);
                if (tabOpt == null)
                    return;
                panel.IsVisible = panel.IsVisible;
                SaveOptions();
            }
        }

        private static void Panels_CollectionChanged(object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                var ribbonPanelCol = sender as RibbonPanelCollection;
                var tab = ribbonPanelCol?.FirstOrDefault()?.Tab;
                if (tab == null)
                    return;
                var tabOptions = ribbonOptions.Data.Tabs.FirstOrDefault(t => t.UID == tab.UID);
                if (tabOptions == null)
                    return;
                for (var index = 0; index < ribbonPanelCol.Count; index++)
                {
                    var panel = ribbonPanelCol[index];
                    var panelOpt = tabOptions.Items.FirstOrDefault(p => p.UID == panel.UID);
                    if (panelOpt != null)
                        panelOpt.Index = index;
                }

                SaveOptions();
            }
        }

        [CanBeNull]
        private static ImageSource ResizeImage([CanBeNull] ImageSource image, int size)
        {
            if (image is BitmapSource bmp)
                return new TransformedBitmap(bmp, new ScaleTransform(size / image.Width, size / image.Height));
            return image;
        }

        private static void SaveOptions()
        {
            ribbonOptions.Data.Tabs = ribbonOptions.Data.Tabs.Where(w => w.Item != null).OrderBy(o => o.Index).ToList();
            foreach (var tabOpt in ribbonOptions.Data.Tabs)
            {
                var tab = (RibbonTab)tabOpt.Item;
                if (tab == null)
                    continue;
                tabOpt.IsVisible = tab.IsVisible;
                foreach (var panelOpt in tabOpt.Items)
                {
                    var panel = (RibbonPanel)panelOpt.Item;
                    if (panel == null)
                        continue;
                    panelOpt.IsVisible = panel.IsVisible;
                }
            }

            Debug.WriteLine("RibbonBuilder SaveOptions");
            ribbonOptions.TrySave();
        }

        private static void Tabs_CollectionChanged(object sender, [NotNull] NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var tab in ribbonOptions.Data.Tabs)
                {
                    var index = ribbon.Tabs.IndexOf((RibbonTab)tab.Item);
                    if (index == -1)
                        continue;
                    tab.Index = index;
                }

                SaveOptions();
            }
        }

        public static ImageSource GetImage(RibbonItemData item, string userGroup)
        {
            var image = RibbonGroupData.LoadImage(userGroup, item.Name) ?? GetDefaultImage();
            if (item.IsTest)
            {
                image = AddTestWaterMark(image);
            }

            return image;
        }

        public static BitmapImage GetDefaultImage()
        {
            return new BitmapImage(new Uri("pack://application:,,,/Resources/unknown.png"));
        }

        private static ImageSource AddTestWaterMark([NotNull] ImageSource image)
        {
            var size = 64;
            var render = new RenderTargetBitmap(size,size, 96,96, PixelFormats.Pbgra32);
            var visual = new DrawingVisual();
            var context = visual.RenderOpen();
            context.DrawImage(image, new Rect(0, 0, size, size));
            var formatted_text = new FormattedText("Тест", CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Arial"),
                22, new SolidColorBrush(Colors.Red));
            formatted_text.SetFontWeight(FontWeights.Bold);
            context.DrawText(formatted_text, new Point(5, 40));
            context.Close();
            render.Render(visual);
            return render;
        }

        public static bool IsAccess([CanBeNull] List<string> accessLogins)
        {
            return accessLogins?.Any() != true ||
                   accessLogins.Contains(Environment.UserName, StringComparer.OrdinalIgnoreCase) ||
                   (AcadLib.General.IsBimUser && (accessLogins.Contains("BIM", StringComparer.OrdinalIgnoreCase) ||
                                                 accessLogins.Contains("БИМ", StringComparer.OrdinalIgnoreCase)));
        }
    }
}
