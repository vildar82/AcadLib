// ReSharper disable once CheckNamespace
namespace AcadLib
{
    using JetBrains.Annotations;
    using BF = System.Reflection.BindingFlags;

    [PublicAPI]
    public static class LateBinding
    {
        [NotNull]
        public static object CreateInstance(string appName)
        {
            return System.Activator.CreateInstance(System.Type.GetTypeFromProgID(appName));
        }

        public static object Get([NotNull] this object obj, string propName, params object[] parameter)
        {
            return obj.GetType().InvokeMember(propName, BF.GetProperty, null, obj, parameter);
        }

        public static object GetInstance(string appName)
        {
            return System.Runtime.InteropServices.Marshal.GetActiveObject(appName);
        }

        public static object GetOrCreateInstance(string appName)
        {
            try
            {
                return GetInstance(appName);
            }
            catch
            {
                return CreateInstance(appName);
            }
        }

        public static object Invoke([NotNull] this object obj, string methName, params object[] parameter)
        {
            return obj.GetType().InvokeMember(methName, BF.InvokeMethod, null, obj, parameter);
        }

        public static void ReleaseInstance([NotNull] this object obj)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        }

        public static void Set([NotNull] this object obj, string propName, params object[] parameter)
        {
            obj.GetType().InvokeMember(propName, BF.SetProperty, null, obj, parameter);
        }
    }
}