namespace AcadLib
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;
    using NetLib;
    using static Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Exception = Autodesk.AutoCAD.Runtime.Exception;

    /// <summary>
    /// Вспомогательные функции для работы с автокадом
    /// </summary>
    [PublicAPI]
    public static class AcadHelper
    {
        private static readonly int AcadId = Process.GetCurrentProcess().Id;

        public static void InvokeInMainThread(Action action)
        {
            if (IsMainThread())
                action();
            else
                Commands._dispatcher.Invoke(action);
        }

        public static bool IsMainThread()
        {
            return Thread.CurrentThread.ManagedThreadId == 1;
        }

        /// <summary>
        /// Текущий документ.
        /// </summary>
        /// <exception cref="InvalidOperationException">Если нет активного чертежа.</exception>
        [NotNull]
        public static Document Doc => DocumentManager.MdiActiveDocument ?? throw new InvalidOperationException();

        [NotNull]
        public static Database Db => HostApplicationServices.WorkingDatabase;

        /// <summary>
        /// Основной номер версии Автокада
        /// </summary>
        public static int VersionMajor => Application.Version.Major;

        public static string GetMajorAcadVersion(this string verStr)
        {
            var index = verStr.GetNthIndex('.', 2);
            return verStr.Substring(0, index);
        }

        public static void StartTransaction(this Document doc, Action<Transaction> action)
        {
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                action(t);
                t.Commit();
            }
        }

        public static T StartTransaction<T>(this Document doc, Func<Transaction,T> action)
        {
            using (doc.LockDocument())
            using (var t = doc.TransactionManager.StartTransaction())
            {
                var res = action(t);
                t.Commit();
                return res;
            }
        }

        /// <summary>
        /// Это русская версия AutoCAD ru-RU
        /// </summary>
        public static bool IsRussianAcad()
        {
            return SystemObjects.DynamicLinker.ProductLcid == 1049;
        }

        [CanBeNull]
        public static Document GetOpenedDocument(string file)
        {
            return DocumentManager.Cast<Document>().FirstOrDefault(d =>
                Path.GetFullPath(d.Name).Equals(Path.GetFullPath(file), StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Если пользователь нажал Esc для прерывания процесса
        /// </summary>
        public static bool UserBreak()
        {
            return HostApplicationServices.Current.UserBreak();
        }

        /// <summary>
        /// Сообщение в ком.строку. автокада
        /// </summary>
        public static void WriteLine(string msg)
        {
            try
            {
                Doc.Editor.WriteMessage($"\n{msg}");
            }
            catch
            {
                // Может не быть открытого чертежа и командной строки.
            }
        }

        public static void WriteToCommandLine(this string msg)
        {
            try
            {
                Doc.Editor.WriteMessage($"\n{msg}\n");
            }
            catch
            {
                // Может не быть открытого чертежа и командной строки.
            }
        }

        /// <summary>
        /// Id текущего процесса
        /// </summary>
        public static int GetCurrentAcadProcessId()
        {
            return AcadId;
        }

        /// <summary>
        /// Определение, что только один автокад запущен
        /// </summary>
        public static bool IsOneAcadRun()
        {
            try
            {
                return !Process.GetProcessesByName("acad").Where(IsValidAcadProcess).Skip(1).Any();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "IsOneAcadRun");
                return true;
            }
        }

        private static bool IsValidAcadProcess(Process process)
        {
            try
            {
                // На "липовом" процессе acad.exe - выскакивает исключение. Обнаружисоль в Новороссийске у Жуковой Юли/
                var unused = process.VirtualMemorySize64;
                if (process.NonpagedSystemMemorySize64 < 20000)
                {
                    return false;
                }

                var unused1 = process.MainWindowTitle;
                var modules = process.Modules.Count;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
