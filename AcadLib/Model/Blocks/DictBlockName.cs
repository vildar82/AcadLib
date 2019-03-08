namespace AcadLib.Blocks
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public class DictBlockName : IDisposable
    {
        private Dictionary<ObjectId, string> dict = new Dictionary<ObjectId, string>();

        [NotNull]
        public string GetName([NotNull] BlockReference blRef)
        {
            if (!dict.TryGetValue(blRef.DynamicBlockTableRecord, out var blName))
            {
                blName = blRef.GetEffectiveName();
                dict[blRef.DynamicBlockTableRecord] = blName;
            }

            return blName;
        }

        public void Dispose()
        {
            dict = new Dictionary<ObjectId, string>();
        }
    }
}