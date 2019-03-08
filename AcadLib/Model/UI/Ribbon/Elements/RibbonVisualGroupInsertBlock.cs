namespace AcadLib.UI.Ribbon.Elements
{
    using System.Collections.Generic;

    public class RibbonVisualGroupInsertBlock : RibbonInsertBlock
    {
        public List<FilterGroup> Groups { get; set; }
    }

    public class FilterGroup
    {
        public string Match { get; set; }
        public string GroupName { get; set; }
    }
}