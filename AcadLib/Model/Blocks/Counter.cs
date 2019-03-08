namespace AcadLib.Blocks
{
    using System.Collections.Generic;
    using System.Text;
    using JetBrains.Annotations;

    public static class Counter
    {
        private static Dictionary<string, int> _counter;

        static Counter()
        {
            Clear();
        }

        public static void Clear()
        {
            _counter = new Dictionary<string, int>();
        }

        public static void AddCount([NotNull] string key)
        {
            if (_counter.ContainsKey(key))
            {
                _counter[key]++;
            }
            else
            {
                _counter.Add(key, 1);
            }
        }

        public static int GetCount([NotNull] string key)
        {
            _counter.TryGetValue(key, out var count);
            return count;
        }

        [NotNull]
        public static string Report()
        {
            var report = new StringBuilder("Обработано блоков:");
            foreach (var counter in _counter)
            {
                report.AppendLine($"\n{counter.Key} - {counter.Value} блоков.");
            }

            return report.ToString();
        }
    }
}