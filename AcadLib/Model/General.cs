namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoCAD_PIK_Manager;
    using AutoCAD_PIK_Manager.Settings;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Runtime;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class General
    {
        public const string Company = CompanyInfo.NameEngShort;
        public const string UserGroupAR = "АР";
        public const string UserGroupEO = "ЭО";
        public const string UserGroupGBKTO = "ЖБК-ТО";
        public const string UserGroupGP = "ГП";
        public const string UserGroupGPTest = "ГП_Тест";
        public const string UserGroupKRMN = "КР-МН";
        public const string UserGroupKRSB = "КР-СБ";
        public const string UserGroupKRSBGK = "КР-СБ-ГК";
        public const string UserGroupOV = "ОВ";
        public const string UserGroupSS = "СС";
        public const string UserGroupVK = "ВК";
        public static readonly RXClass ClassAttDef;
        public static readonly RXClass ClassBlRef;
        public static readonly RXClass ClassDBDic;
        public static readonly RXClass ClassDbTextRX;
        public static readonly RXClass ClassDimension;
        public static readonly RXClass ClassHatch;
        public static readonly RXClass ClassMLeaderRX;
        public static readonly RXClass ClassMTextRX;
        public static readonly RXClass ClassPolyline;
        public static readonly RXClass ClassRecord;
        public static readonly RXClass ClassRegion;
        public static readonly RXClass ClassVport;

        private static readonly List<string> bimUsers = new List<string>
        {
            "PrudnikovVS",
            "vrublevskiyba",
            "khisyametdinovvt",
            "arslanovti",
            "karadzhayanra"
        };

        static General()
        {
            try
            {
                IsBimUser = bimUsers.Any(u => u.EqualsIgnoreCase(Environment.UserName)) || IsBimUserByUserData();
                ClassAttDef = RXObject.GetClass(typeof(AttributeDefinition));
                ClassBlRef = RXObject.GetClass(typeof(BlockReference));
                ClassDBDic = RXObject.GetClass(typeof(DBDictionary));
                ClassDbTextRX = RXObject.GetClass(typeof(DBText));
                ClassDimension = RXObject.GetClass(typeof(Dimension));
                ClassHatch = RXObject.GetClass(typeof(Hatch));
                ClassMLeaderRX = RXObject.GetClass(typeof(MLeader));
                ClassMTextRX = RXObject.GetClass(typeof(MText));
                ClassPolyline = RXObject.GetClass(typeof(Polyline));
                ClassRecord = RXObject.GetClass(typeof(Xrecord));
                ClassRegion = RXObject.GetClass(typeof(Region));
                ClassVport = RXObject.GetClass(typeof(Viewport));
            }
            catch
            {
                //
            }
        }

        public static bool IsRemoteUser()
        {
            return Environment.MachineName.StartsWith("V-70");
        }

        /// <summary>
        ///     BIM-manager - отдел поддержки пользователей
        /// </summary>
        public static bool IsBimUser { get; set; }

        public static bool IsCadManager()
        {
            return Environment.UserName.Equals(PikSettings.PikFileSettings.LoginCADManager,
                StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBimUserByUserData()
        {
            try
            {
                var isDeveloper = UserInfo.UserData.SubDivision?.EqualsIgnoreCase("ИРА") == true;
                var isBIM = UserInfo.UserData.SubDivision?.EqualsIgnoreCase("BIM") == true;
                return isBIM || isDeveloper;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        ///     Символы строковые
        /// </summary>
        [PublicAPI]
        public static class Symbols
        {
            /// <summary>
            ///     Кубическая степень- ³
            /// </summary>
            public const string Cubic = "³";

            /// <summary>
            ///     Градус - °
            /// </summary>
            public const string Degree = "°";

            /// <summary>
            ///     Диаметр ⌀
            /// </summary>
            public const string Diam = "⌀";

            /// <summary>
            ///     Квадратная степень- ²
            /// </summary>
            public const string Square = "²";
        }
    }
}
