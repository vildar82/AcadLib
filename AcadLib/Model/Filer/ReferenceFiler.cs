namespace AcadLib.Filer
{
    using System;
    using System.Collections.Generic;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.Geometry;
    using JetBrains.Annotations;

    [PublicAPI]
    public class ReferenceFiler : DwgFiler
    {
        // member data
        public List<ObjectId> SoftPointerIds { get; } = new List<ObjectId>();

        public List<ObjectId> HardPointerIds { get; } = new List<ObjectId>();

        public List<ObjectId> SoftOwnershipIds { get; } = new List<ObjectId>();

        public List<ObjectId> HardOwnershipIds { get; } = new List<ObjectId>();

        public override Autodesk.AutoCAD.Runtime.ErrorStatus FilerStatus
        {
            get => Autodesk.AutoCAD.Runtime.ErrorStatus.OK;
            set { }
        }

        public override FilerType FilerType => FilerType.IdFiler;

        public override long Position => 0;

        public override void ResetFilerStatus()
        {
        }

        public override void Seek(long offset, int method)
        {
        }

        public override IntPtr ReadAddress()
        {
            return new IntPtr();
        }

        [NotNull]
        public override byte[] ReadBinaryChunk()
        {
            return new byte[0];
        }

        public override bool ReadBoolean()
        {
            return false;
        }

        public override byte ReadByte()
        {
            return 0;
        }

        public override void ReadBytes(byte[] value)
        {
        }

        public override double ReadDouble()
        {
            return 0.0;
        }

        public override Handle ReadHandle()
        {
            return new Handle();
        }

        public override short ReadInt16()
        {
            return 0;
        }

        public override int ReadInt32()
        {
            return 0;
        }

        public override long ReadInt64()
        {
            return 0;
        }

        public override Point2d ReadPoint2d()
        {
            return new Point2d();
        }

        public override Point3d ReadPoint3d()
        {
            return new Point3d();
        }

        public override Scale3d ReadScale3d()
        {
            return new Scale3d();
        }

        [NotNull]
        public override string ReadString()
        {
            return string.Empty;
        }

        public override ushort ReadUInt16()
        {
            return 0;
        }

        public override uint ReadUInt32()
        {
            return 0;
        }

        public override ulong ReadUInt64()
        {
            return 0;
        }

        public override Vector2d ReadVector2d()
        {
            return new Vector2d();
        }

        public override Vector3d ReadVector3d()
        {
            return new Vector3d();
        }

        public override ObjectId ReadHardOwnershipId()
        {
            return new ObjectId();
        }

        public override ObjectId ReadHardPointerId()
        {
            return new ObjectId();
        }

        public override ObjectId ReadSoftOwnershipId()
        {
            return new ObjectId();
        }

        public override ObjectId ReadSoftPointerId()
        {
            return new ObjectId();
        }

        public override void WriteAddress(IntPtr value)
        {
        }

        public override void WriteBinaryChunk(byte[] chunk)
        {
        }

        public override void WriteBoolean(bool value)
        {
        }

        public override void WriteByte(byte value)
        {
        }

        public override void WriteBytes(byte[] value)
        {
        }

        public override void WriteDouble(double value)
        {
        }

        public override void WriteHandle(Handle handle)
        {
        }

        public override void WriteInt16(short value)
        {
        }

        public override void WriteInt32(int value)
        {
        }

        public override void WriteInt64(long value)
        {
        }

        public override void WritePoint2d(Point2d value)
        {
        }

        public override void WritePoint3d(Point3d value)
        {
        }

        public override void WriteScale3d(Scale3d value)
        {
        }

        public override void WriteString(string value)
        {
        }

        public override void WriteUInt16(ushort value)
        {
        }

        public override void WriteUInt32(uint value)
        {
        }

        public override void WriteUInt64(ulong value)
        {
        }

        public override void WriteVector2d(Vector2d value)
        {
        }

        public override void WriteVector3d(Vector3d value)
        {
        }

        public override void WriteHardOwnershipId(ObjectId value)
        {
            if (!value.IsNull)
                HardOwnershipIds.Add(value);
        }

        public override void WriteHardPointerId(ObjectId value)
        {
            if (!value.IsNull)
                HardPointerIds.Add(value);
        }

        public override void WriteSoftOwnershipId(ObjectId value)
        {
            if (!value.IsNull)
                SoftOwnershipIds.Add(value);
        }

        public override void WriteSoftPointerId(ObjectId value)
        {
            if (!value.IsNull)
                SoftPointerIds.Add(value);
        }

        public void Reset()
        {
            SoftPointerIds.Clear();
            HardPointerIds.Clear();
            SoftOwnershipIds.Clear();
            HardOwnershipIds.Clear();
        }
    }
}