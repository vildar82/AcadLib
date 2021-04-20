namespace AcadLib
{
    using System;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using JetBrains.Annotations;

    /// <summary>
    /// Automates saving/changing/restoring system variables
    /// </summary>
    public class ManagedSystemVariable : IDisposable
    {
        private string name;
        private object? oldVal;

        public ManagedSystemVariable(string name, object value)
            : this(name)
        {
            Application.SetSystemVariable(name, value);
        }

        public ManagedSystemVariable(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(name);
            this.name = name;
            oldVal = Application.GetSystemVariable(name);
        }

        public void Dispose()
        {
            if (oldVal != null)
            {
                var temp = oldVal;
                oldVal = null;
                Application.SetSystemVariable(name, temp);
            }
        }
    }
}