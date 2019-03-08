namespace AcadLib.Filer
{
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;

    public class ReferenceFilerResult
    {
        public List<ObjectId> SoftPointerIds { get; set; }

        public List<ObjectId> HardPointerIds { get; set; }

        public List<ObjectId> SoftOwnershipIds { get; set; }

        public List<ObjectId> HardOwnershipIds { get; set; }
    }
}