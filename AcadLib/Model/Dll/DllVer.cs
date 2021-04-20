namespace AcadLib
{
    using System.IO;
    using System.Text.RegularExpressions;

    public class DllVer
    {
        public DllVer(string fileDll, int ver)
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

        public static DllVer GetDllVer(string file)
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