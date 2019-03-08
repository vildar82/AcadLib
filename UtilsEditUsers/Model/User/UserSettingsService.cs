namespace UtilsEditUsers.Model.User
{
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Windows.Media.Imaging;
    using JetBrains.Annotations;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using UsersEditor;

    [PublicAPI]
    public static class UserSettingsService
    {
        public static BitmapImage LoadHomePikImage(string login, string domain)
        {
            var client = new WebClient { Credentials = new NetworkCredential("sys.bim", "GBRGhjtrn<bv!") };
            var req = $"https://home.pik.ru/api/v1.0/Employee/byLogin?login={domain}%5C{login}";
            var resId = client.DownloadString(req);
            var bytes = Encoding.Default.GetBytes(resId);
            resId = Encoding.UTF8.GetString(bytes);
            if (JsonConvert.DeserializeObject<JArray>(resId).FirstOrDefault() is JObject objId)
            {
                var id = objId["id"].ToString();
                var resImage = client.DownloadData($"https://home.pik.ru/api/v1.0/Employee/{id}/photo?renditionId=1");
                return LoadImage(resImage);
            }

            return null;
        }

        public static void UsersEditor()
        {
            var usersVM = new UsersEditorVM();
            var usersView = new UsersEditorView(usersVM);
            usersView.Show();
        }

        private static BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;
            var image = new BitmapImage();
            using (var mem = new MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }

            image.Freeze();
            return image;
        }
    }
}