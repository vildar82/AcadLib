namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using IO;
    using JetBrains.Annotations;
    using MongoDblib.UsersData.Data;
    using NetLib;

    [PublicAPI]
    public static class UserInfo
    {
        static UserInfo()
        {
            var noConnectMongoDb = false;
            try
            {
                var task = Task.Run(() =>
                {
                    try
                    {
                        using var adUtils = new NetLib.AD.ADUtils();
                        UserGroupsAd = adUtils.GetCurrentUserGroups(out var fioAd);
                        IsProductUser = UserGroupsAd.Any(g => g == "000883_Департамент продукта");
                        FioAD = fioAd;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log.Error(ex, "adUtils");
                    }
                });
                task.Wait(100);
                if (!task.IsCompleted)
                    task.Wait(3000);

                if (!task.IsCompleted)
                {
                    Logger.Log.Error("UserInfo Constructor - нет в доступа к ADUtils или прошло более 3000.");
                }

                task = Task.Run(() =>
                {
                    UserData = new MongoDblib.UsersData.DbUserData().GetCurrentUser();
                });
                task.Wait(100);
                if (!task.IsCompleted)
                    task.Wait(3000);

                if (!task.IsCompleted)
                {
                    noConnectMongoDb = true;
                    Logger.Log.Error("UserInfo Constructor - нет в доступа к MongoDb или прошло более 3000.");
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "adUtils and mongo load user info");
            }

            if (UserData == null || UserGroupsAd == null)
            {
                try
                {
                    LoadBackup();
                }
                catch
                {
                    //
                }
            }

            if (UserData == null && !noConnectMongoDb)
            {
                try
                {
                    ShowUserProfileRegister();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "ShowUserProfileRegister");
                }
            }

            SaveBackup();
        }

        public static string? FioAD { get; set; }

        public static UserData? UserData { get; set; }

        public static List<string>? UserGroupsAd { get; set; }

        public static bool IsProductUser { get; set; }

        public static void ShowUserProfileRegister()
        {
            MongoDblib.UsersData.UserDataRegUI.ShowUserProfileRegister(FioAD, string.Empty, "AutoCAD");
        }

        public static List<Division> GetDivisions()
        {
            return new MongoDblib.UsersData.DbUserData().GetDivisions();
        }

        public static List<SubDivision> GetSubDivisions()
        {
            return new MongoDblib.UsersData.DbUserData().GetSubDivisions();
        }

        private static void SaveBackup()
        {
            try
            {
                var user = new UserInfoData
                {
                    UserData = UserData,
                    FioAD = FioAD,
                    UserGroupsAd = UserGroupsAd,
                };

                var file = GetFile();
                user.Serialize(file);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "UserInfo SaveBackup");
            }
        }

        private static void LoadBackup()
        {
            try
            {
                var file = GetFile();
                var user = file.Deserialize<UserInfoData>();
                FioAD ??= user.FioAD;
                UserData ??= user.UserData;
                UserGroupsAd ??= user.UserGroupsAd;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "UserInfo LoadBackup");
            }
        }

        [NotNull]
        private static string GetFile()
        {
            return Path.GetUserPluginFile("UserInfo", "UserInfo");
        }
    }
}