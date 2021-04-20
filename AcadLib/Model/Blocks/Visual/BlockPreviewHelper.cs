namespace AcadLib.Blocks.Visual
{
    using System.Drawing;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Windows.Data;

    public static class BlockPreviewHelper
    {
        public static ImageSource GetPreview(BlockTableRecord btr)
        {
            return CMLContentSearchPreviews.GetBlockTRThumbnail(btr);
        }

        public static Icon GetPreviewIcon(BlockTableRecord btr)
        {
            var imgsrc = CMLContentSearchPreviews.GetBlockTRThumbnail(btr);
            var bitmap = (Bitmap)ImageSourceToGDI((BitmapSource)imgsrc);
            var iconPtr = bitmap.GetHicon();
            return Icon.FromHandle(iconPtr);
        }

        public static System.Drawing.Image GetPreviewImage(BlockTableRecord btr)
        {
            var imgsrc = CMLContentSearchPreviews.GetBlockTRThumbnail(btr);
            return ImageSourceToGDI((BitmapSource)imgsrc);
        }

        private static System.Drawing.Image ImageSourceToGDI(BitmapSource src)
        {
            using var ms = new MemoryStream();
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(src));
            encoder.Save(ms);
            ms.Flush();
            return System.Drawing.Image.FromStream(ms);
        }
    }
}