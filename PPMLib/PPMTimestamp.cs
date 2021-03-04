using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PPMLib
{
    public class PPMTimestamp
    {
        public PPMTimestamp(uint value)
        {
            Value = value;
        }
        public uint Value;

        private static readonly DateTime _2000_01_01 = new DateTime(2000, 1, 1);
        public DateTime ToDateTime() => _2000_01_01.AddSeconds(Value);
        public override string ToString() => ToDateTime().ToString();
    }
}
