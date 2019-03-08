namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using JetBrains.Annotations;

    /// <summary>
    /// Automates saving/changing/restoring system variables
    /// </summary>
    [PublicAPI]
    public class ManagedSystemVariable : IDisposable
    {
        private string name;
        private object oldval;

        public ManagedSystemVariable([NotNull] string name, object value)
            : this(name)
        {
            Application.SetSystemVariable(name, value);
        }

        public ManagedSystemVariable([NotNull] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(name);
            this.name = name;
            oldval = Application.GetSystemVariable(name);
        }

        public void Dispose()
        {
            if (oldval != null)
            {
                var temp = oldval;
                oldval = null;
                Application.SetSystemVariable(name, temp);
            }
        }
    }
}