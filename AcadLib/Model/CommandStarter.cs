namespace AcadLib
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Windows;
    using Autodesk.AutoCAD.ApplicationServices;
    using Autodesk.AutoCAD.Runtime;
    using CommandLock;
    using Errors;
    using JetBrains.Annotations;
    using Statistic;
    using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
    using Exception = System.Exception;

    [PublicAPI]
    public class CommandStart
    {
        public CommandStart()
        {
        }

        public CommandStart(string commandName, Assembly asm)
        {
            CommandName = commandName;
            Assembly = asm;
        }

        public static string CurrentCommand { get; set; }

        public string CommandName { get; set; }

        public string Plugin { get; set; }

        public string Doc { get; set; }

        public Assembly Assembly { get; set; }

        public static void StartLisp(string commandName, string file)
        {
            Logger.Log.StartLisp(commandName, file);
            PluginStatisticsHelper.PluginStart(new CommandStart(commandName, null) { Doc = file, Plugin = "Lisp" });
        }

        public static void Start(string commandName, Action<Document> action)
        {
            MethodBase caller = null;
            try
            {
                caller = new StackTrace().GetFrame(1).GetMethod();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CommandStart - StackTrace");
            }

            StartCommand(action, caller, commandName);
        }

        /// <summary>
        /// Оболочка для старта команды - try-catch, log, inspectoe.clear-show, commandcounter
        /// Условие использования: отключить оптимизацию кода (Параметры проекта -> Сборка) - т.к. используется StackTrace
        /// </summary>
        /// <param name="action">Код выполнения команды</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Start(Action<Document> action)
        {
            MethodBase caller = null;
            try
            {
                caller = new StackTrace().GetFrame(1).GetMethod();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CommandStart - StackTrace");
            }

            StartCommand(action, caller, null);
        }

        public static void StartWoStat(Action<Document> action)
        {
            MethodBase caller = null;
            try
            {
                caller = new StackTrace().GetFrame(1).GetMethod();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CommandStart - StackTrace");
            }

            StartCommand(action, caller, null, true);
        }

        public static void Start(Action<Document> action, Version minAcadVersion)
        {
            if (Application.Version < minAcadVersion)
            {
                MessageBox.Show(
                    $"Команда не работает в данной версии автокада. \nМинимальная требуемая версия {minAcadVersion}.");
                return;
            }

            MethodBase caller = null;
            try
            {
                caller = new StackTrace().GetFrame(1).GetMethod();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CommandStart - StackTrace");
            }

            StartCommand(action, caller, null);
        }

        [NotNull]
        internal static CommandStart GetCallerCommand([CanBeNull] MethodBase caller, [CanBeNull] string commandName = null)
        {
            Assembly assm = null;
            try
            {
                CurrentCommand = commandName ?? GetCallerCommandName(caller);
                assm = caller?.DeclaringType?.Assembly;
            }
            catch
            {
                //
            }

            var com = new CommandStart
            {
                CommandName = CurrentCommand,
                Assembly = assm,
                Plugin = assm?.GetName().Name,
                Doc = Application.DocumentManager.MdiActiveDocument?.Name
            };
            return com;
        }

        private static void StartCommand(Action<Document> action, MethodBase caller, string commandName, bool woStatistic = false)
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;
            CommandStart commandStart = null;
            try
            {
                commandStart = GetCallerCommand(caller, commandName);
                if (!woStatistic)
                {
                    Logger.Log.StartCommand(commandStart);
                    Logger.Log.Info($"Document={doc.Name}");
                    PluginStatisticsHelper.PluginStart(commandStart);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "CommandStart");
            }

            try
            {
                // Проверка блокировки команды
                if (commandStart != null && !CommandLockService.CanStartCommand(commandStart.CommandName))
                {
                    Logger.Log.Info($"Команда заблокирована - {commandStart.CommandName}");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Проверка блокировки команды");
            }

            try
            {
                Inspector.Clear();
                action(doc);
            }
            catch (OperationCanceledException ex)
            {
                if (!doc.IsDisposed)
                    doc.Editor.WriteMessage(ex.Message);
            }
            catch (Exceptions.ErrorException error)
            {
                Inspector.AddError(error.Error);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, CurrentCommand);
                Inspector.AddError($"Ошибка в программе. {ex.Message}", System.Drawing.SystemIcons.Error);

                if (!doc.IsDisposed)
                    doc.Editor.WriteMessage(ex.Message);
            }

            Inspector.Show();
        }

        [NotNull]
        private static string GetCallerCommandName([CanBeNull] MethodBase caller)
        {
            if (caller == null)
                return "nullCallerMethod!?";
            var atrCom = (CommandMethodAttribute)caller.GetCustomAttribute(typeof(CommandMethodAttribute));
            return atrCom?.GlobalName ?? caller.Name;
        }
    }
}