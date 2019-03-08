// ReSharper disable once CheckNamespace
namespace AcadLib.XData
{
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public interface IDboDataSave : IExtDataSave, ITypedDataValues
    {
        string PluginName { get; set; }

        DBObject GetDBObject();
    }
}