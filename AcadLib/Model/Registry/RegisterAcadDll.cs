namespace AcadLib
{
    using System;
    using System.Reflection;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;
    using Microsoft.Win32;

    [PublicAPI]
    public static class RegisterAcadDll
    {
        private const string sAppName = "AcadLib";

        /// <summary>
        /// Controls how and when the .NET assembly is loaded.
        /// </summary>
        [Flags]
        public enum LOADCTRLS
        {
            DetectProxy = 1,
            Startup = 2,
            Command = 4,
            Request = 8,
            NotLoad = 16,
            Transparently = 32
        }

        /// <summary>
        /// Регистрация в реестре автозагрузки текущей сборки в текущем автокаде.
        /// </summary>
        /// <param name="loadctrls">Управление загрузкой сборки</param>
        /// <param name="UserOrMachine">True - User (HKCU); False - Local machine (HKLM)</param>
        /// <returns></returns>
        public static bool Registration(LOADCTRLS loadctrls = LOADCTRLS.Command | LOADCTRLS.Request, bool UserOrMachine = true)
        {
            try
            {
                var sProdKey = UserOrMachine
                    ? HostApplicationServices.Current.UserRegistryProductRootKey
                    : HostApplicationServices.Current.MachineRegistryProductRootKey;
                var curAssembly = Commands.AcadLibAssembly;

                using (var regAcadProdKey = UserOrMachine
                    ? Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sProdKey)
                    : Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sProdKey))
                {
                    if (regAcadProdKey == null)
                        throw new InvalidOperationException();
                    using (var regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true))
                    {
                        if (regAcadAppKey == null)
                            throw new InvalidOperationException();

                        // Check to see if the "MyApp" key exists
                        var subKeys = regAcadAppKey.GetSubKeyNames();
                        foreach (var subKey in subKeys)
                        {
                            if (subKey.Equals(sAppName))
                            {
                                return true;
                            }
                        }

                        // Register the application
                        using (var regAppAddInKey = regAcadAppKey.CreateSubKey(sAppName))
                        {
                            if (regAppAddInKey == null)
                                throw new InvalidOperationException();
                            var desc = curAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description;
                            if (desc == string.Empty)
                                desc = sAppName;
                            regAppAddInKey.SetValue("DESCRIPTION", desc, RegistryValueKind.String);
                            regAppAddInKey.SetValue("LOADCTRLS", loadctrls, RegistryValueKind.DWord);
                            regAppAddInKey.SetValue("LOADER", curAssembly.Location, RegistryValueKind.String);
                            regAppAddInKey.SetValue("MANAGED", 1, RegistryValueKind.DWord);

                            // Запись раздела Commands
                            SetCommands(regAppAddInKey, curAssembly);

                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Удаление записи из реестра, автозагрузки текущего приложения из текущей версии автокада.
        /// </summary>
        public static void Unregistration()
        {
            // Get the AutoCAD Applications key
            var sProdKey = HostApplicationServices.Current.UserRegistryProductRootKey;

            // HKCU
            DeleteApp(sProdKey, sAppName, true);

            // HKLM
            DeleteApp(sProdKey, sAppName, false);
        }

        private static void DeleteApp([NotNull] string sProdKey, string appName, bool UserOrMachine)
        {
            using (var regAcadProdKey = UserOrMachine
                ? Microsoft.Win32.Registry.CurrentUser.OpenSubKey(sProdKey)
                : Microsoft.Win32.Registry.LocalMachine.OpenSubKey(sProdKey))
            {
                if (regAcadProdKey == null)
                    return;
                using (var regAcadAppKey = regAcadProdKey.OpenSubKey("Applications", true))
                {
                    if (regAcadAppKey == null)
                        throw new InvalidOperationException();

                    // Delete the key for the application
                    try
                    {
                        regAcadAppKey.DeleteSubKeyTree(appName);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }
        }

        private static void SetCommands(Microsoft.Win32.RegistryKey regAppAddInKey, [NotNull] Assembly curAssembly)
        {
            // Создание раздела Commands в переданной ветке реестра и создание записей команд в этом разделе.
            // Команды определяются по атрибутам переданной сборки, в которой должен быть определен атрибут класса команд
            // из которого получаются методы с атрибутами CommandMethod.
            using (regAppAddInKey = regAppAddInKey.CreateSubKey("Commands"))
            {
                if (regAppAddInKey == null)
                    return;
                var attClass = curAssembly.GetCustomAttribute<CommandClassAttribute>();
                var members = attClass.Type.GetMembers();
                foreach (var member in members)
                {
                    if (member.MemberType == MemberTypes.Method)
                    {
                        var att = member.GetCustomAttribute<CommandMethodAttribute>();
                        if (att != null)
                            regAppAddInKey.SetValue(att.GlobalName, att.GlobalName);
                    }
                }
            }
        }
    }
}