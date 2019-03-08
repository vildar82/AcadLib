namespace AcadLib.Blocks.Visual
{
    using System.Collections.Generic;

    public class VisualGroup
    {
        public string Name { get; set; }

        public List<IVisualBlock> Blocks { get; set; }
    }
}