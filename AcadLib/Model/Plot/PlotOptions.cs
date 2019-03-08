namespace AcadLib.Plot
{
    using System;
    using JetBrains.Annotations;

    [PublicAPI]
    [Serializable]
    public class PlotOptions
    {
        /// <summary>
        /// [Category("Печать")]
        /// [DisplayName("Печать по умолчанию:")]
        /// [Description("При вызове команды установить опцию поумолчанию печати из текущего чертежа или выбор папки.")]
        /// </summary>
        public bool DefaultPlotCurOrFolder { get; set; }

        /// <summary>
        /// [Category("Фильтр")]
        /// [DisplayName("Фильтр по названию вкладок:")]
        /// [Description("Печатать только вкладки соответствующим заданной строке поиска. Через | можно складывать условия ИЛИ.")]
        /// </summary>
        public string FilterByNames { get; set; }

        /// <summary>
        /// [Category("Фильтр")]
        /// [DisplayName("Фильтр по номерам вкладок:")]
        /// [Description("Печатать только указанные номера вкладок. Номера через запятую и/или тире. Отрицательные числа считаются с конца вкладок.\n\r Например: 16--4 печать с 16 листа до 4 с конца; -1--3 печать трех последних листов.")] 
        /// </summary>
        public string FilterByNumbers { get; set; }

        /// <summary>
        /// [Category("Фильтр")]
        /// [DisplayName("Использовать фильтр?")]
        /// [Description("Включение и отключение фильтров.")]
        /// </summary>
        public bool FilterState { get; set; }

        /// <summary>
        /// [Category("Печать")]
        /// [DisplayName("PDF файл")]
        /// [Description("Создавать один общий файл pdf или для каждого чертежа dwg отдельно.")]
        /// </summary>
        public bool OnePdfOrEachDwg { get; set; } = true;

        /// <summary>
        /// [Category("Печать")]
        /// [DisplayName("С подпапками")]
        /// [Description("Если выбрана печать всей папки, то включать все файлы в подпапках удовлетворяющие фильтру.")]
        /// </summary>
        public bool IncludeSubdirs { get; set; }

        /// <summary>
        /// [Category("Печать")]
        /// [DisplayName("Сортировка по")]
        /// [Description("Сортировка листов - по порядку вкладок в чертеже или по алфавиту.")]
        /// </summary>
        public bool SortTabOrName { get; set; } = true;

        /// <summary>
        /// Вставлять бланк
        /// </summary>
        public bool BlankOn { get; set; }

        /// <summary>
        /// Номер листа для бланка
        /// </summary>
        public int BlankPageNumber { get; set; } = 3;
    }
}