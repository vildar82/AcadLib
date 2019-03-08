using NetLib;

namespace AcadLib.PaletteCommands
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Media;
    using AcadLib.UI.PaletteCommands;
    using AcadLib.UI.PaletteCommands.UI;
    using AcadLib.UI.Ribbon;
    using AcadLib.UI.Ribbon.Elements;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.Windows;
    using JetBrains.Annotations;
    using Layers;
    using Properties;
    using Brush = System.Windows.Media.Brush;

    [PublicAPI]
    public class PaletteSetCommands : PaletteSet
    {
        internal static UserGroupPalette _paletteSets;

        public PaletteSetCommands(
            string paletteName,
            Guid paletteGuid,
            string commandStartPalette,
            List<IPaletteCommand> commandsAddin)
            : base(paletteName, commandStartPalette, paletteGuid)
        {
            CommandsAddin = commandsAddin;
            Icon = Resources.pik;
            LoadPalettes();
            PaletteAddContextMenu += PaletteSetCommands_PaletteAddContextMenu;
        }

        /// <summary>
        /// Команды переданные из сборки данного раздела
        /// </summary>
        public List<IPaletteCommand> CommandsAddin { get; set; }

        /// <summary>
        /// Данные для палитры
        /// </summary>
        private List<PaletteModel> Models { get; set; }

        public static void Init()
        {
            SetTrayPalette();
        }

        public static double GetButtonWidth()
        {
            if (Settings.Default.PaletteStyle == 1)
            {
                var wb = NetLib.MathExt.Interpolate(8, 55, 25, 180, Settings.Default.PaletteFontSize);
                return wb < Settings.Default.PaletteImageSize ? Settings.Default.PaletteImageSize : wb;
            }

            return Settings.Default.PaletteImageSize * 1.08;
        }

        public static double GetFontSize()
        {
            return Settings.Default.PaletteFontSize * 2.5;
        }

        /// <summary>
        /// Подготовка для определения палитры ПИК.
        /// Добавление значка ПИК в трей для запуска палитры.
        /// </summary>
        [Obsolete("Построение палитры инструментов происходит из настроек Ribbon")]
        public static void InitPalette(
            List<IPaletteCommand> commands,
            string commandStartPalette,
            string paletteName,
            Guid paletteGuid)
        {
        }

        /// <summary>
        /// Создание палитры и показ
        /// </summary>
        public static void Start()
        {
            try
            {
                if (_paletteSets == null)
                {
                    _paletteSets = LoadPaletteGroup();
                }

                if (_paletteSets.Palette == null)
                {
                    _paletteSets.Palette = new PaletteSetCommands(_paletteSets.Name, _paletteSets.Guid,
                        _paletteSets.CommandStartPalette, _paletteSets.Commands);
                }

                _paletteSets.Palette.Visible = true;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PaletteSetCommands.Start().");
            }

            try
            {
                // Построение ленты (бывает автоматом не создается при старте)
                RibbonBuilder.CreateRibbon();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Start CreateRibbon.");
            }
        }

        private static UserGroupPalette LoadPaletteGroup()
        {
            return new UserGroupPalette
            {
                Guid = new Guid("DFE87B3D-78A0-47A3-84C6-3E66909C65C8"),
                Name = "Палитра инструментов ПИК",
                CommandStartPalette = nameof(Commands.PIK_StartPalette),
                Commands = LoadCommands(),
            };
        }

        private static List<IPaletteCommand> LoadCommands()
        {
            var commands = new List<IPaletteCommand>();
            try
            {
                foreach (var group in RibbonBuilder.LoadRibbonTabsFromGroups())
                {
                    foreach (var panel in @group.Item1.Panels)
                    {
                        var panelName = panel.Name;
                        if (panelName.IsNullOrEmpty())
                            panelName = "Главная";
                        foreach (var item in panel.Items)
                        {
                            try
                            {
                                var com = GetCommand(item, panelName, group.Item2);
                                if (com != null)
                                    commands.Add(com);
                            }
                            catch
                            {
                                // Пофигу
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
            }

            return commands;
        }

        private static IPaletteCommand GetCommand(RibbonItemData item, string panel, string userGroup)
        {
            if (!RibbonBuilder.IsAccess(item.Access))
                return null;
            IPaletteCommand com = null;
            switch (item)
            {
                case RibbonBreakPanel ribbonBreakPanel:
                    break;
                case RibbonToggle ribbonToggle:
                    var toggle = new ToggleButton();
                    toggle.IsChecked = ribbonToggle.IsChecked;
                    toggle.CommandName = ribbonToggle.Command;
                    com = toggle;
                    break;
                case RibbonCommand ribbonCommand:
                    var c = new PaletteCommand();
                    c.CommandName = ribbonCommand.Command;
                    com = c;
                    break;
                case RibbonVisualGroupInsertBlock ribbonVisualGroupInsertBlock:
                    break;
                case RibbonVisualInsertBlock ribbonVisualInsertBlock:
                    var vb = new PaletteVisualInsertBlocks();
                    vb.file = ribbonVisualInsertBlock.File;
                    vb.filter = s => Regex.IsMatch(s, ribbonVisualInsertBlock.Filter);
                    vb.explode = ribbonVisualInsertBlock.Explode;
                    vb.Layer = new LayerInfo(ribbonVisualInsertBlock.Layer);
                    com = vb;
                    break;
                case RibbonInsertBlock ribbonInsertBlock:
                    var ib = new PaletteInsertBlock();
                    ib.file = ribbonInsertBlock.File;
                    ib.blName = ribbonInsertBlock.BlockName;
                    ib.explode = ribbonInsertBlock.Explode;
                    ib.Layer = new LayerInfo(ribbonInsertBlock.Layer);
                    ib.props = ribbonInsertBlock.Properties;
                    com = ib;
                    break;
                case RibbonSplit ribbonSplit:
                    var coms = ribbonSplit.Items.Select(s => GetCommand(s, panel, userGroup))
                        .Where(w => w != null).ToList();
                    var split = new SplitCommand(coms);
                    com = split;
                    break;
            }

            if (com != null)
            {
                com.Name = item.Name;
                com.Description = item.Description;
                com.Image = RibbonBuilder.GetImage(item, userGroup);
                com.Access = item.Access;
                com.Command = item.GetCommand();
                com.IsTest = item.IsTest;
                com.Group = panel;
            }

            return com;
        }

        private static void PikTray_MouseDown()
        {
            Start();
        }

        private static void SetTrayPalette()
        {
            // Добавление иконки в трей
            try
            {
                var p = new Pane
                {
                    ToolTipText = "Палитра инструментов ПИК",
                    Icon = Icon.FromHandle(Resources.logo.GetHicon())
                };
                p.MouseDown += (o, e) => PikTray_MouseDown();
                p.Visible = false;
                Application.StatusBar.Panes.Insert(0, p);
                p.Visible = true;
                Application.StatusBar.Update();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "PaletteSetCommands.SetTrayPalette().");
            }
        }

        private void CheckTheme()
        {
            var isDarkTheme = (short)Autodesk.AutoCAD.ApplicationServices.Core.Application.GetSystemVariable("COLORTHEME") == 0;
            Brush colorBkg = isDarkTheme
                ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 92, 92, 92))
                : System.Windows.Media.Brushes.White;
            Models.ForEach(m => m.Background = colorBkg);
        }

        private void LoadPalettes()
        {
            Models = new List<PaletteModel>();

            // Группировка команд
            // const string groupCommon = Commands.GroupCommon;
            // var commonCommands = Commands.CommandsPalette;
            var groupCommands = CommandsAddin.GroupBy(c => c.Group).OrderBy(g => g.Key);
            foreach (var group in groupCommands)
            {
                // if (group.Key.Equals(groupCommon, StringComparison.OrdinalIgnoreCase))
                // {
                //    commonCommands.AddRange(group);
                // }
                // else
                {
                    var model = new PaletteModel(group.GroupBy(g => g.Name).Select(s => s.First()));
                    if (model.PaletteCommands.Any())
                    {
                        var commControl = new UI.CommandsControl { DataContext = model };
                        var name = group.Key;
                        if (string.IsNullOrEmpty(name))
                            name = "Главная";
                        AddVisual(name, commControl);
                        Models.Add(model);
                    }
                }
            }

            //// Общие команды для всех отделов определенные в этой сборке
            // var modelCommon = new PaletteModel(commonCommands.GroupBy(g => g.Name).Select(s => s.First()).ToList(),
            //    versionPalette);
            // var controlCommon = new UI.CommandsControl { DataContext = modelCommon };
            // AddVisual(groupCommon, controlCommon);
            // Models.Add(modelCommon);
            Settings.Default.PropertyChanged += (o, e) =>
            {
                double bw;
                switch (e.PropertyName)
                {
                    case "PaletteImageSize":
                    case "PaletteStyle":
                        bw = GetButtonWidth();
                        Models.ForEach(m => m.ButtonWidth = bw);
                        break;

                    case "PaletteFontSize":
                        var fontMaxH = GetFontSize();
                        bw = GetButtonWidth();
                        Models.ForEach(m =>
                        {
                            m.FontMaxHeight = fontMaxH;
                            m.ButtonWidth = bw;
                        });
                        break;
                }
            };
        }

        private void PaletteSetCommands_PaletteAddContextMenu(object sender, [NotNull] PaletteAddContextMenuEventArgs e)
        {
            var mi = new MenuItem("Параметры отображения");
            mi.Click += (co, ce) =>
            {
                var paletteoptView = new PaletteOptionsView(new PaletteOptionsViewModel(Models));
                paletteoptView.ShowDialog();
            };
            e.MenuItems.Add(mi);
        }
    }
}
