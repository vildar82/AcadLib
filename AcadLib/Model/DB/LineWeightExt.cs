namespace AcadLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.AutoCAD.DatabaseServices;
    using JetBrains.Annotations;

    [PublicAPI]
    public static class LineWeightExt
    {
        private static readonly List<double> lws100 = new List<double>
        {
            0,
            5,
            9,
            13,
            15,
            18,
            20,
            25,
            30,
            35,
            40,
            50,
            53,
            60,
            70,
            80,
            90,
            100,
            106,
            120,
            140,
            158,
            200,
            211
        };

        public static LineWeight GetLineWeightFromMM(double mm)
        {
            if (mm < 0)
            {
                if (mm < -3)
                    return LineWeight.ByLineWeightDefault;
                return NetLib.MathExt.IsEqual(mm, -2, 0.1) ? LineWeight.ByBlock : LineWeight.ByLayer;
            }

            var mm100 = mm * 100;
            var lwsLast = lws100.Last();
            if (mm100 > lwsLast)
                return (LineWeight)lwsLast;
            return (LineWeight)lws100.OrderBy(o => Math.Abs(o - mm100)).First();
        }
    }
}