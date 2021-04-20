namespace AcadLib.Blocks.CommonBlocks
{
    using Autodesk.AutoCAD.DatabaseServices;

    public class Leader : BlockBase
    {
        public const string BlockName = "Обозначение_Выноска_ПИК";
        public const string ParamName = "ОБОЗНАЧЕНИЕ";

        public Leader(BlockReference blRef, string blName)
            : base(blRef, blName)
        {
            Name = GetPropValue<string>(ParamName);
        }

        public string Name { get; set; }

        public void SetName(string value)
        {
            FillPropValue(ParamName, value);
        }
    }
}