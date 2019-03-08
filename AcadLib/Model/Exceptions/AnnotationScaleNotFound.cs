using System;

namespace AcadLib.Exceptions
{
    public class AnnotationScaleNotFound : Exception
    {
        public AnnotationScaleNotFound(string msg) : base(msg)
        {
        }
    }
}
