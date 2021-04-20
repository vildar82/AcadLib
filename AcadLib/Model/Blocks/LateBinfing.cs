namespace AcadLib
{
    using BF = System.Reflection.BindingFlags;

    public static class LateBinding
    {
        public static object CreateInstance(string appName)
        {
            return System.Activator.CreateInstance(System.Type.GetTypeFromProgID(appName));
        }

        public static object Get(this object obj, string propName, params object[] parameter)
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

        public static object Invoke(this object obj, string methName, params object[] parameter)
        {
            return obj.GetType().InvokeMember(methName, BF.InvokeMethod, null, obj, parameter);
        }

        public static void ReleaseInstance(this object obj)
        {
            System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        }

        public static void Set(this object obj, string propName, params object[] parameter)
        {
            obj.GetType().InvokeMember(propName, BF.SetProperty, null, obj, parameter);
        }
    }
}