namespace AcadLib.Field
{
    using System;
    using System.Runtime.InteropServices;
    using Autodesk.AutoCAD.ApplicationServices.Core;
    using Autodesk.AutoCAD.DatabaseServices;
    using Autodesk.AutoCAD.EditorInput;
    using JetBrains.Annotations;

    /// <summary>
    /// PInvoke acdbEvaluateFields - обновление полей - с 2013 - 2017
    /// </summary>
    [PublicAPI]
    public static class UpdateField
    {
        /// <summary>
        /// Обновление полей в объекте.
        /// Можно передать id блока - все поля в блоке обновятся
        /// </summary>
        /// <param name="id"></param>
        public static int Update(ObjectId id)
        {
            return AcdbEvaluateFields(ref id, 16);
        }

        public static void UpdateInSelected()
        {
            // Обновление полей в блоке
            var doc = Application.DocumentManager.MdiActiveDocument;
            var ed = doc.Editor;
            var sel = ed.Select("Выбер объектов для обновления полей: ");
            foreach (var item in sel)
            {
                var id = item;
                AcdbEvaluateFields(ref id, 16);
            }
        }

        private static int AcdbEvaluateFields(ref ObjectId id, int context)
        {
            switch (Application.Version.Major)
            {
                case 23:
                    return AcdbEvaluateFields23x64(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero);

                case 21:
                    return IntPtr.Size == 8
                        ? AcdbEvaluateFields21x64(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero)
                        : AcdbEvaluateFields21x32(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero);

                case 20:
                    return IntPtr.Size == 8
                        ? AcdbEvaluateFields20x64(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero)
                        : AcdbEvaluateFields20x32(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero);

                case 19:
                    return IntPtr.Size == 8
                        ? AcdbEvaluateFields19x64(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero)
                        : AcdbEvaluateFields19x32(ref id, 16, IntPtr.Zero, IntPtr.Zero, 1, IntPtr.Zero, IntPtr.Zero);
            }

            return 0;
        }

        // AutoCAD 2013 и 2014 x86
        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@HPB_WPAVAcDbDatabase@@W4EvalFields@AcFd@@PAH4@Z")]
        private static extern int AcdbEvaluateFields19x32(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2013 и 2014 x64
        [DllImport("acdb19.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@HPEB_WPEAVAcDbDatabase@@W4EvalFields@AcFd@@PEAH4@Z")]
        private static extern int AcdbEvaluateFields19x64(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2015 и 2016 x86
        [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@HPB_WPAVAcDbDatabase@@W4EvalFields@AcFd@@PAH4@Z")]
        private static extern int AcdbEvaluateFields20x32(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2015 и 2016 x64
        [DllImport("acdb20.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@HPEB_WPEAVAcDbDatabase@@W4EvalFields@AcFd@@PEAH4@Z")]
        private static extern int AcdbEvaluateFields20x64(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2017x86
        [DllImport("acdb21.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@ABVAcDbObjectId@@HPB_WPAVAcDbDatabase@@W4EvalFields@AcFd@@PAH4@Z")]
        private static extern int AcdbEvaluateFields21x32(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2017 x64
        [DllImport("acdb21.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@HPEB_WPEAVAcDbDatabase@@W4EvalFields@AcFd@@PEAH4@Z")]
        private static extern int AcdbEvaluateFields21x64(
            ref ObjectId id,
            int context,
            IntPtr pszPropName,
            IntPtr db,
            int eval,
            IntPtr i1,
            IntPtr i2);

        // AutoCAD 2019 x64
        [DllImport("acdb23.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, ExactSpelling = true,
            EntryPoint =
                "?acdbEvaluateFields@@YA?AW4ErrorStatus@Acad@@AEBVAcDbObjectId@@HPEB_WPEAVAcDbDatabase@@W4EvalFields@AcFd@@PEAH4@Z")]
        private static extern int AcdbEvaluateFields23x64(
                ref ObjectId id,
                int context,
                IntPtr pszPropName,
                IntPtr db,
                int eval,
                IntPtr i1,
                IntPtr i2);
    }
}
