using System.Reflection;
using System.Runtime.InteropServices;
using AcadLib;
using Autodesk.AutoCAD.Runtime;

[assembly: AssemblyTitle("AcadLib")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("AcadLib")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("2008ae5d-550a-4478-a9b5-297832058377")] // Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: CommandClass(typeof(Commands))]
[assembly: ExtensionApplication(typeof(Commands))]