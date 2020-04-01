using AutoCAD_PIK_Manager;
using NCalc;

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

        public static void SetSystemVariable(this object value, string name)
        {
            try
            {
                Application.SetSystemVariable(name, value);
                return;
            }
            catch (Exception ex)
            {
            }

            if (value is long l)
            {
                try
                {
                    Application.SetSystemVariable(name, (int)l);
                }
                catch
                {
                }
            }

            Env.SetEnv(name, value.ToString());
        }

        public static void SetSystemVariableTry(this string name, object value)
        {
            try
            {
                SetSystemVariable(name, value);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, $"SetSystemVariableTry name={name}, value={value}");
            }
        }

        public static object GetSystemVariable(this string name)
        {
            try
            {
                return Application.GetSystemVariable(name);
            }
            catch
            {
                return Env.GetEnv(name);
            }
        }

        public static object GetSystemVariableTry(this string name)
        {
            try
            {
                return GetSystemVariable(name);
            }
            catch(Exception ex)
            {
                Logger.Log.Error(ex, $"GetSystemVariableTry name={name}");
            }

            return null;
        }

        public static T GetSystemVariable<T>(this string name)
        {
            return GetSystemVariable(name).GetValue<T>();
        }
    }
}