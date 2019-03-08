namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class RegAppHelper
    {
        public static void RegApp([NotNull] this Database db, string regAppName)
        {
            using (var rat = (RegAppTable) db.RegAppTableId.Open(OpenMode.ForRead, false))
            {
                if (rat.Has(regAppName))
                    return;
            }

            using (var rat = (RegAppTable) db.RegAppTableId.Open(OpenMode.ForWrite, false))
            {
                using (var ratr = new RegAppTableRecord())
                {
                    ratr.Name = regAppName;
                    rat.Add(ratr);
                }
            }
        }

        public static void RegAppPIK([NotNull] this Database db)
        {
            RegApp(db, General.Company);
        }

        public static IEnumerable<string> GetRegApps(this Database db)
        {
            using (var t = db.TransactionManager.StartTransaction())
            {
                var rat = db.RegAppTableId.GetObjectT<RegAppTable>();
                foreach (var id in rat)
                {
                    var regApp = id.GetObject<RegAppTableRecord>();
                    if (regApp == null)
                        continue;
                    yield return regApp.Name;
                }

                t.Commit();
            }
        }

        public static void CleanRegApps(this Database db)
        {
            var cleanApps = new Dictionary<string, (Regex, int)>
            {
                { "^GENIUS_", (null, 0) },
                { "^LGR_", (null, 0) },
                { "^GEVID", (null, 0) },
                { @"^\$", (null, 0) },
                { "^SIT", (null, 0) },
                { "^VAZ", (null, 0) },
                { "^PLOT", (null, 0) },
                { "AUDIT_", (null, 0) },
                { "^MULTI_SUITE_", (null, 0) },
                { "^BE_TEXT_", (null, 0) },
                { "^CK_TEXT_", (null, 0) },
                { "^AHEAD_", (null, 0) },
                { "^BACK_", (null, 0) },
                { "^CD-C_SEZ_", (null, 0) },
                { "^CD-C_ELEMENTS_", (null, 0) },
            };
            foreach (var key in cleanApps.Keys.ToList())
            {
                cleanApps[key] = (new Regex(key), 0);
            }

            using (var t = db.TransactionManager.StartTransaction())
            {
                var rat = db.RegAppTableId.GetObjectT<RegAppTable>();
                foreach (var id in rat)
                {
                    var regApp = id.GetObject<RegAppTableRecord>();
                    if (regApp == null)
                        continue;
                    var cleanApp = cleanApps.FirstOrDefault(r => r.Value.Item1.IsMatch(regApp.Name));
                    if (cleanApp.Key != null)
                    {
                        regApp = regApp.UpgradeOpenTr();
                        regApp.Erase();
                        var item = cleanApps[cleanApp.Key];
                        item.Item2 += 1;
                        cleanApps[cleanApp.Key] = item;
                    }
                }

                t.Commit();
            }

            var erasedAppsCount = cleanApps.Where(w => w.Value.Item2 > 0).OrderByDescending(o => o.Value.Item2)
                .JoinToString(s => $"{s.Key} - {s.Value.Item2}", Environment.NewLine);
            if (!erasedAppsCount.IsNullOrEmpty())
            {
                $"Удалено зарегистрированных приложений:\nсоответствие имени приложения - кол. соответствий\n{erasedAppsCount}"
                    .WriteToCommandLine();
            }
        }
    }
}
