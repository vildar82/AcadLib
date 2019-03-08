namespace AcadLib.UI.Hotkeys
{
    using System;
    using System.Collections.Generic;
    using System.Reactive;
    using System.Reactive.Linq;
    using Demo;
    using GlobalHooks;
    using JetBrains.Annotations;

    public static class HotKeyService
    {
        [NotNull]
        private static Dictionary<string, Action> hotKeys = new Dictionary<string, Action>();
        private static GlobalKeyHook keyHook;
        private static IDisposable obs;

        public static void AddHotKey(ModifyKey modifyKey, Keys key, Action action)
        {
            if (keyHook == null) Init();
            var hotKey = GetKeyString(modifyKey, key);
            if (hotKeys.ContainsKey(hotKey))
            {
                Logger.Log.Error($"Такой HotKey уже есть - {hotKey}.");
                return;
            }

            hotKeys.Add(hotKey, action);
        }

        private static string GetKeyString(ModifyKey modifyKey, Keys key)
        {
            return $"{modifyKey}+{key}";
        }

        private static void Init()
        {
            keyHook = new GlobalKeyHook();
            var events = keyHook.Events();
            obs = events.KeyUp.Merge(events.KeyDown).Buffer(TimeSpan.FromMilliseconds(400), 2)
                .Where(x => x.Count == 2)
                .Select(x => GetKeyString(GetModifier(x[0]), (Keys) x[1].EventArgs.KeyCode))
                .Where(w => hotKeys.ContainsKey(w))
                .ObserveOnDispatcher()
                .Subscribe(s => { hotKeys[s]?.Invoke(); });
        }

        private static ModifyKey GetModifier(EventPattern<GlobalKeyEventArgs> e)
        {
            if (e.EventArgs.Alt != ModifierKeySide.None)
                return ModifyKey.Alt;
            if (e.EventArgs.Shift != ModifierKeySide.None)
                return ModifyKey.Shift;
            if (e.EventArgs.Control != ModifierKeySide.None)
                return ModifyKey.Control;
            return ModifyKey.None;
        }
    }
}
