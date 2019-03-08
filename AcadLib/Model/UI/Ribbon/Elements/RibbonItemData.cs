namespace AcadLib.UI.Ribbon.Elements
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Xml.Serialization;
    using JetBrains.Annotations;
    using NetLib.WPF;
    using Newtonsoft.Json;

    public abstract class RibbonItemData : BaseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsTest { get; set; }

        /// <summary>
        /// Доступ - имена групп и логины.
        /// </summary>
        public List<string> Access { get; set; }

        public abstract ICommand GetCommand();
    }
}
