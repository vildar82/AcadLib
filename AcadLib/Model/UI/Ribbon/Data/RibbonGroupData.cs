namespace AcadLib.UI.Ribbon.Data
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using AutoCAD_PIK_Manager.Settings;
    using Elements;
    using JetBrains.Annotations;
    using NetLib;

    /// <summary>
    /// Набор вкладок ленты от одной группы пользователя
    /// </summary>
    public class RibbonGroupData
    {
        /// <summary>
        /// Вкладки
        /// </summary>
        public List<RibbonTabData> Tabs { get; set; }

        public List<RibbonItemData> FreeItems { get; set; }

        [NotNull]
        public static Type[] GetTypes()
        {
            return new[]
            {
                typeof(RibbonCommand), typeof(RibbonInsertBlock),
                typeof(RibbonVisualInsertBlock), typeof(RibbonVisualGroupInsertBlock), typeof(RibbonBreakPanel),
                typeof(RibbonSplit), typeof(RibbonToggle)
            };
        }

        [NotNull]
        public static string GetRibbonFile([NotNull] string userGroup)
        {
            return Path.Combine(PikSettings.LocalSettingsFolder, $@"Ribbon\{userGroup}\{userGroup}.ribbon");
        }

        public static string GetImagesFolder(string userGroup)
        {
            var ribbonFile = GetRibbonFile(userGroup);
            var ribbonDir = Path.GetDirectoryName(ribbonFile);
            return Path.Combine(ribbonDir, "Images");
        }

        public static RibbonGroupData Load([NotNull] string ribbonFile, Action<Exception> onException = null)
        {
            try
            {
                if (!File.Exists(ribbonFile))
                {
                    Logger.Log.Error($"Загрузка ленты. ribbonFile не найден {ribbonFile}.");
                    return null;
                }

                return ribbonFile.FromXml<RibbonGroupData>(GetTypes());
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                onException?.Invoke(ex);
                return null;
            }
        }

        public void Save([NotNull] string ribbonFile)
        {
            this.ToXml(ribbonFile, GetTypes());
        }

        public static void SaveImage(string imageSrcFile, string imageName, string userGroup)
        {
            var imageDir = GetImagesFolder(userGroup);
            var imageDestFile = Path.Combine(imageDir, imageName);
            var img = Image.FromFile(imageSrcFile);
            var resizeImg = NetLib.Images.ImageExt.ResizeImage(img, 64, 64);
            resizeImg.Save(imageDestFile, ImageFormat.Png);
        }

        public static void SaveImage(ImageSource imageSrc, string imageName, string userGroup)
        {
            var imageDir = GetImagesFolder(userGroup);
            var resizeImg = NetLib.Images.ImageExt.ResizedImage(imageSrc, 64, 64, 0);
            var file = Path.Combine(imageDir, imageName);
            var fi = new FileInfo(file);
            using (var fileStream = fi.Create())
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resizeImg));
                encoder.Save(fileStream);
            }
        }

        public static string GetImageName(string name)
        {
            if (name.EndsWith(".png"))
                return name;
            name = NetLib.IO.Path.GetValidFileName(name);
            return $"{name}.png";
        }

        public static ImageSource LoadImage(string userGroup, string itemName)
        {
            try
            {
                var imagesDir = GetImagesFolder(userGroup);
                var imageFile = Path.Combine(imagesDir, GetImageName(itemName));
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(imageFile, UriKind.Absolute);
                image.EndInit();
                return image;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex);
                return null;
            }
        }
    }
}
