namespace AcadLib.Statistic
{
    using Autodesk.Civil.ApplicationServices;

    public static class CivilTest
    {
        private static bool? isCivil;

        public static bool IsCivil()
        {
            if (isCivil == null)
            {
                try
                {
                    var unused = CivilApplication.ActiveProduct;
                    isCivil = true;
                }
                catch
                {
                    isCivil = false;
                }
            }

            return isCivil.Value;
        }
    }
}