namespace AcadLib.Filer
{
    using Autodesk.AutoCAD.DatabaseServices;

    public static class ReferenceFilerExt
    {
        /// <summary>
        /// Поиск ссылок на объект
        /// </summary>
        /// <param name="dbo">Объект</param>
        /// <returns>Объект содержащий найденные ссылки</returns>
        public static ReferenceFilerResult GetReferences(this DBObject dbo)
        {
            var filer = new ReferenceFiler();
            dbo.DwgOut(filer);
            return new ReferenceFilerResult
            {
                HardOwnershipIds = filer.HardOwnershipIds,
                HardPointerIds = filer.HardPointerIds,
                SoftOwnershipIds = filer.SoftOwnershipIds,
                SoftPointerIds = filer.SoftPointerIds,
            };
        }
    }
}