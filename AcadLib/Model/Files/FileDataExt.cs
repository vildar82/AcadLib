namespace AcadLib.Files
{
    using IO;
    using JetBrains.Annotations;
    using NetLib;

    [PublicAPI]
    public static class FileDataExt
    {
        /// <summary>
        /// Данные хранимые локально у пользователя в формате xml/json.
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="plugin">Имя плагина</param>
        /// <param name="name">Имя файла без расширения</param>
        /// <param name="xmlOrJson">true - xml, false - json</param>
        /// <returns>Данные</returns>
        [NotNull]
        public static LocalFileData<T> GetLocalFileData<T>([NotNull] string plugin, [NotNull] string name, bool xmlOrJson)
            where T : class, new()
        {
            var ext = xmlOrJson ? ".xml" : ".json";
            var localFile = Path.GetUserPluginFile(plugin, name + ext);
            return new LocalFileData<T>(localFile, xmlOrJson);
        }

        /// <summary>
        /// Данные хранимые на сервере в папке Shared в формате json.
        /// </summary>
        /// <typeparam name="T">Тип данных</typeparam>
        /// <param name="plugin">Имя плагина</param>
        /// <param name="name">Имя файла без расширения</param>
        /// <param name="xmlOrJson">true - xml, false - json</param>
        /// <returns>Данные</returns>
        [NotNull]
        public static FileData<T> GetSharedFileData<T>([NotNull] string plugin, [NotNull] string name, bool xmlOrJson)
            where T : class, new()
        {
            var ext = xmlOrJson ? ".xml" : ".json";
            var serverFile = Path.GetSharedFile(plugin, name + ext);
            var localFile = Path.GetUserPluginFile(plugin, name + ext);
            return new FileData<T>(serverFile, localFile, xmlOrJson);
        }
    }
}