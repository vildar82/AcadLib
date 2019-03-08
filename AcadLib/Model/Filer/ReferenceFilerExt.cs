namespace AcadLib.Filer
{
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public static class ReferenceFilerExt
    {
        /// <summary>
        /// Поиск ссылок на объект
        /// </summary>
        /// <param name="dbo">Объект</param>
        /// <returns>Объект содержащий найденные ссылки</returns>
        [NotNull]
        public static ReferenceFilerResult GetReferences([NotNull] this DBObject dbo)
        {
            var filer = new ReferenceFiler();
            dbo.DwgOut(filer);
            return new ReferenceFilerResult
            {
                HardOwnershipIds = filer.HardOwnershipIds,
                HardPointerIds = filer.HardPointerIds,
                SoftOwnershipIds = filer.SoftOwnershipIds,
                SoftPointerIds = filer.SoftPointerIds
            };
        }
    }
}