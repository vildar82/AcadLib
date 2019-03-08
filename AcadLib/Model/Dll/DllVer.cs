namespace AcadLib
{
    using System.IO;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;

    [PublicAPI]
    public class DllVer
    {
        public DllVer([NotNull] string fileDll, int ver)
        {
            Dll = fileDll;
            Ver = ver;
            FileWoVer = Path.GetFileNameWithoutExtension(fileDll);
            if (ver != 0)
            {
                FileWoVer = FileWoVer.Substring(0, FileWoVer.LastIndexOf('_'));
            }
        }

        public string Dll { get; set; }

        public string FileWoVer { get; set; }

        public int Ver { get; set; }

        [CanBeNull]
        public static DllVer GetDllVer([NotNull] string file)
        {
            DllVer dllVer;
            var match = Regex.Match(file, @"(_v(\d{4}).dll)$");
            if (match.Success && int.TryParse(match.Groups[2].Value, out var ver))
            {
                dllVer = new DllVer(file, ver);
            }
            else
            {
                dllVer = new DllVer(file, 0);
            }

            return dllVer;
        }
    }
}