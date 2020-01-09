using System;
using AutoMapper;

namespace AcadLib.Errors
{
    public class UsingInspector : IDisposable
    {
        public UsingInspector()
        {
            Inspector.Clear();
        }

        public void Dispose()
        {
            Inspector.Show();
        }
    }
}