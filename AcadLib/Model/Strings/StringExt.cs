namespace AcadLib.Strings
{
    using System.Text.RegularExpressions;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class StringExt
    {
        [NotNull]
        public static string GetValidDbSymbolName([NotNull] this string name)
        {
            return GetValidDbSymbolName(name, ".");
        }

        public static string GetValidDbSymbolName([NotNull] this string name, string replacement)
        {
            // string testString = "<>/?\";:*|,='";
            if (name.IsNullOrEmpty())
                return name;

            var pattern = new Regex("[<>/?\";:*|,='`]");
            var res = pattern.Replace(name, replacement);
            res = res.Replace("\\", replacement);
            SymbolUtilityServices.ValidateSymbolName(res, false);
            return res;
        }

        public static bool IsValidDbSymbolName(this string input)
        {
            try
            {
                SymbolUtilityServices.ValidateSymbolName(input, false);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
