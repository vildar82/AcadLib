namespace AcadLib.Errors
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using UI;

    public interface IError : IComparable<IError>, IEquatable<IError>
    {
        Extents3d Extents { get; set; }

        bool HasEntity { get; set; }

        bool CanShow { get; }

        Icon Icon { get; set; }

        ErrorStatus Status { get; set; }

        ObjectId IdEnt { get; set; }

        string Message { get; }

        string Group { get; }

        string ShortMsg { get; }

        object Tag { get; set; }

        Matrix3d Trans { get; set; }

        List<Entity> Visuals { get; set; }

        List<ErrorAddButton> AddButtons { get; set; }

        Color Background { get; set; }

        void AdditionToMessage(string addMsg);
        IError GetCopy();
        void Show();
    }
}