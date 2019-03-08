namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using NetLib;

    public static class SystemVariableExt
    {
        public static void SetSystemVariable(this string name, object value)
        {
            Application.SetSystemVariable(name, value);
        }

        public static void SetSystemVariableTry(this string name, object value)
        {
            try
            {
                Application.SetSystemVariable(name, value);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"SetSystemVariableTry name={name}, value={value}");
            }
        }

        public static object GetSystemVariable(this string name)
        {
            return Application.GetSystemVariable(name);
        }

        public static T GetSystemVariable<T>(this string name)
        {
            return Application.GetSystemVariable(name).GetValue<T>();
        }
    }
}