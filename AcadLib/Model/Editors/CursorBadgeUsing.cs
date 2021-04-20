namespace AcadLib.Editors
{
    using System;
    using Autodesk.AutoCAD.EditorInput;
    using Autodesk.AutoCAD.GraphicsInterface;

    /// <summary>
    /// Повесить значек на курсор
    /// </summary>
    public class CursorBadgeUsing : IDisposable
    {
        private CursorBadgeUtilities cbu;
        private ImageBGRA32 image;

        /// <summary>
        /// Добавление значка с приоритетом 1 означает, что
        /// он не будет виден при операциях выбора. Изменение
        /// этого значения в 3 достаточно, чтобы и при операциях
        /// выбора был виден наш значок (большее значение
        /// еще увеличит шансы на то, что он останется видимым)
        /// </summary>
        /// <param name="image">Иконка</param>
        /// <param name="priority">Приоритет</param>
        public CursorBadgeUsing(ImageBGRA32 image, int priority = 1)
        {
            this.image = image;
            cbu = new CursorBadgeUtilities();
            cbu.AddSupplementalCursorImage(image, priority);
        }

        public void Dispose()
        {
            cbu.RemoveSupplementalCursorImage(image);
        }
    }
}
