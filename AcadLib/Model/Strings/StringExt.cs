using System.Text.RegularExpressions;
using Autodesk.AutoCAD.DatabaseServices;
using JetBrains.Annotations;

namespace AcadLib.Strings
{
    [PublicAPI]
    public static class StringExt
    {
        [NotNull]
        public static string GetValidDbSymbolName([NotNull] this string name)
        {
            // string testString = "<>/?\";:*|,='";
            var pattern = new Regex("[<>/?\";:*|,=']");
            var res = pattern.Replace(name, ".");
            res = res.Replace('\\', '.');
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