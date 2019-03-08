namespace AcadLib.UI.StatusBar
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using AutoCAD_PIK_Manager;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.Windows;
    using JetBrains.Annotations;
    using NetLib;
    using Statistic;
    using View;
    using Commands = AcadLib.Commands;

    /// <summary>
    /// Статусная строка
    /// </summary>
    [PublicAPI]
    public static class StatusBarEx
    {
        /// <summary>
        /// Добавление панели с выпадающим списком значений.
        /// </summary>
        /// <param name="value">текщее знасчение</param>
        /// <param name="values">Значения</param>
        /// <param name="toolTip">Описание</param>
        /// <param name="selectValue">Действие при выборе значения</param>
        /// <param name="showMenu">Показ меню - текущее значение</param>
        /// <param name="minWidth"></param>
        /// <param name="maxWidth"></param>
        [NotNull]
        public static Pane AddMenuPane(
            string value,
            List<string> values,
            string toolTip,
            Action<string> selectValue,
            Func<string> showMenu,
            int minWidth = 0,
            int maxWidth = 0)
        {
            var pane = new Pane { Text = value, Style = PaneStyles.PopUp | PaneStyles.Normal, ToolTipText = toolTip };
            pane.MouseDown += (o, e) => { new StatusBarMenu(showMenu(), values, selectValue).Show(); };
            pane.Visible = false;
            Application.StatusBar.Panes.Insert(0, pane);
            if (minWidth != 0)
                pane.MinimumWidth = minWidth;
            if (maxWidth != 0)
                pane.MinimumWidth = maxWidth;
            pane.Visible = true;
            Application.StatusBar.Update();
            return pane;
        }

        public static Pane AddPane(
            string name,
            string toolTip,
            [CanBeNull] Action<Pane, StatusBarMouseDownEventArgs> onClick = null,
            int minWith = 20,
            int maxWith = 200,
            [CanBeNull] Icon icon = null,
            PaneStyles style = PaneStyles.Normal)
        {
            var pane = new Pane
            {
                Text = name,
                ToolTipText = toolTip,
                Visible = false,
                MinimumWidth = minWith,
                MaximumWidth = maxWith,
                Style = style
            };
            if (icon != null)
                pane.Icon = icon;
            pane.MouseDown += (s, e) => onClick?.Invoke(pane, e);
            Application.StatusBar.Panes.Insert(0, pane);
            pane.Visible = true;
            Application.StatusBar.Update();
            return pane;
        }

        public static void AddPaneUserGroup()
        {
            var name = PikSettings.UserGroup;
            if (!PikSettings.AdditionalUserGroup.IsNullOrEmpty())
                name += $", {PikSettings.AdditionalUserGroup}";
            AddPane(name,
                $"{GetGroupVersionInfo(PikSettings.Versions)}",
                (p, e) =>
                {
                    try
                    {
                        p.ToolTipText = GetGroupVersionInfo(Update.GetVersions());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, "AddPaneUserGroup ToolTipText=GetGroupVersionInfo");
                    }

                    AcadHelper.Doc.SendStringToExecute($"{nameof(Commands.PIK_CheckUpdates)} ", true, false, true);
                    AcadHelper.Doc.SendStringToExecute($"{nameof(Commands.PIK_UserSettings)} ", true, false, true);
                });
        }

        [NotNull]
        private static string GetGroupVersionInfo([NotNull] List<GroupInfo> groupInfos)
        {
            return $"{groupInfos.JoinToString(GetGroupInfo, "\n")}";

            string GetGroupInfo(GroupInfo groupInfo)
            {
                var info = $"{groupInfo.GroupName}: вер. {groupInfo.VersionLocal} ({groupInfo.VersionLocalDate:dd.MM.yy HH:mm})";
                if (groupInfo.UpdateRequired)
                {
                    info += $", на сервере {groupInfo.VersionServer} ({groupInfo.VersionServerDate:dd.MM.yy HH:mm})";
                }

                if (CheckUpdates.NeedNotify(groupInfo.UpdateDescription, out var descResult))
                {
                    info += $" '{descResult.Truncate(75)}'";
                }

                if (groupInfo.GroupName == Commands.GroupCommon)
                {
                    info += ". Alt+стрелка, предыдущее выделение.";
                }

                return info;
            }
        }
    }
}
