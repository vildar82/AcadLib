using System.Reactive;

namespace AcadLib.User.UI
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using AutoCAD_PIK_Manager.Settings;
    using AutoCAD_PIK_Manager.User;
    using NetLib;
    using NetLib.WPF;
    using ReactiveUI;

    public class UserSettingsVM : BaseViewModel
    {
        public UserSettingsVM(AutocadUser user, UserSettings userSettings)
        {
            User = user;
            UserSettings = userSettings;
            Groups = LoadGroups();
            if (user != null && !user.Group.IsNullOrEmpty())
            {
                FillGroup(user.Group, user.AdditionalGroup);
            }
            else if (!PikSettings.UserGroup.IsNullOrEmpty())
            {
                FillGroup(PikSettings.UserGroup, PikSettings.AdditionalUserGroup);
            }
            else
            {
                Group = Groups[0];
            }

            PreviewUpdate = user?.PreviewUpdate ?? false;
            Disabled = user?.Disabled ?? false;
            Ok = CreateCommand(OkExec);
            DeleteExtraGroup = CreateCommand(() => ExtraGroup = null);
            this.WhenAnyValue(v => v.Group).Subscribe(s => UpdateExtraGroups());
        }

        /// <summary>
        /// Данные пользователя из базы
        /// </summary>
        public AutocadUser User { get; set; }

        /// <summary>
        /// Настройки пользователя по плагинам
        /// </summary>
        public UserSettings UserSettings { get; }

        /// <summary>
        /// Группы настроек
        /// </summary>
        public List<UserGroup> Groups { get; set; }

        /// <summary>
        /// Выбранная группа настроек
        /// </summary>
        public UserGroup Group { get; set; }

        /// <summary>
        /// Список для дополнительной группы настроек
        /// </summary>
        public List<UserGroup> ExtraGroups { get; set; }

        /// <summary>
        /// Выбранная дополнительная группа настроек
        /// </summary>
        public UserGroup ExtraGroup { get; set; }

        /// <summary>
        /// Отключены настроек
        /// </summary>
        public bool Disabled { get; set; }

        /// <summary>
        /// Предварительные обновления
        /// </summary>
        public bool PreviewUpdate { get; set; }

        public ReactiveCommand<Unit, Unit> Ok { get; set; }

        public ReactiveCommand<Unit, Unit> DeleteExtraGroup { get; set; }

        private static UserGroup FindGroup(List<UserGroup> groups, string name)
        {
            return groups.FirstOrDefault(g => g.Name == name);
        }

        private List<UserGroup> LoadGroups()
        {
            var fileGroups = Path.Combine(PikSettings.ServerSettingsFolder, $@"{Commands.GroupCommon}\Dll\groups.json");
            var groups = fileGroups.Deserialize<Dictionary<string, string>>();
            return groups.Select(s => new UserGroup
            {
                Name = s.Key,
                Description = s.Value
            }).OrderBy(o => o.Name).ToList();
        }

        private void FillGroup(string userGroup, string additioinalGroup)
        {
            var group = userGroup;
            Group = FindGroup(Groups, group);
            if (Group == null)
                return;
            if (!additioinalGroup.IsNullOrEmpty())
            {
                ExtraGroup = FindGroup(Groups, additioinalGroup);
            }
        }

        private void UpdateExtraGroups()
        {
            var groups = Groups.ToList();
            if (Group != null)
                groups.Remove(Group);
            ExtraGroups = groups;
        }

        private void OkExec()
        {
            if (User == null)
            {
                User = new AutocadUser
                {
                    FIO = UserInfo.FioAD,
                    Login = Environment.UserName.ToLower()
                };
            }

            if (Group != null)
            {
                User.Group = Group.Name;
                User.AdditionalGroup = ExtraGroup?.Name;
            }

            User.Disabled = Disabled;
            User.PreviewUpdate = PreviewUpdate;
            DialogResult = true;
        }
    }
}
