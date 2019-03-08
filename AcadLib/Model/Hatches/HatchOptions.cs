using System;
using NetLib;

namespace AcadLib.Hatches
{
    using Autodesk.AutoCAD.Colors;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    public class HatchOptions : EntityOptions
    {
        public HatchOptions()
        {
        }

        public HatchOptions([NotNull] Hatch h) : base (h)
        {
            PatternName = h.PatternName;
            PatternType = h.PatternType;
            PatternScale = h.PatternScale;
            PatternAngle = h.PatternAngle;
            BackgroundColor = h.BackgroundColor;
            HatchStyle = h.HatchStyle;
        }

        public string PatternName { get; set; }

        public HatchPatternType PatternType { get; set; }

        public double? PatternScale { get; set; }

        public double? PatternAngle { get; set; }

        public Color BackgroundColor { get; set; }

        public HatchStyle? HatchStyle { get; set; }

        public override void SetOptions(Entity ent)
        {
            base.SetOptions(ent);
            if (!(ent is Hatch h)) return;
            if (PatternScale != null && PatternScale.Value > 0 && Math.Abs(PatternScale.Value - h.PatternScale) > 0.001)
            {
                h.PatternScale = PatternScale.Value;
            }

            if (!PatternName.IsNullOrEmpty() && !PatternName.EqualsIgnoreCase(h.PatternName))
            {
                h.SetHatchPattern(PatternType, PatternName);
            }

            if (PatternAngle != null && PatternAngle.Value > 0 && Math.Abs(PatternAngle.Value - h.PatternAngle) > 0.001)
            {
                h.PatternAngle = PatternAngle.Value;
            }

            if (HatchStyle != null && HatchStyle != h.HatchStyle)
            {
                h.HatchStyle = HatchStyle.Value;
            }

            h.EvaluateHatch(true);
            h.BackgroundColor = BackgroundColor ?? Autodesk.AutoCAD.Colors.Color.FromColorIndex(ColorMethod.None, 257);
        }
    }
}