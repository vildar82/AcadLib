namespace AcadLib.CommandLock.Data
{
    using System;
    using System.Collections.Generic;

    public class CaseInsensitiveDictionary : Dictionary<string, CommandLockInfo>
    {
        public CaseInsensitiveDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}