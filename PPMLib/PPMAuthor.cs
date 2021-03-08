using System.Collections.Generic;
using System.Text;

namespace PPMLib
{
    public class PPMAuthor
    {
        public PPMAuthor(string name, ulong id)
        {
            _Name = name;
            Id = id;
        }

        private string _Name;
        public string Name { get => ToUnicode(); }
        public ulong Id { get; }

        public override string ToString() => $"{Name} ({Id.ToString("X8")})";
        // public override string ToString() => Name + " " + string.Join(" ", Encoding.BigEndianUnicode.GetBytes(Name).Select(t => t.ToString("X2")));
        // ^-- this line is for debugging --^

        // As stated here: https://github.com/Sudomemo/Sudofont
        internal static readonly Dictionary<string, string> CharTable = new Dictionary<string, string>
        {
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x00}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xB6}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x01}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xB7}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x02}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xCD}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x03}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xCE}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x04}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xC1}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x05}), Encoding.BigEndianUnicode.GetString(new byte[]{0x24,0xC7}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x06}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0x95}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x07}), Encoding.BigEndianUnicode.GetString(new byte[]{0x23,0xF0}) },
            // vvv emojis not working vvv
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x08}), Encoding.BigEndianUnicode.GetString(new byte[]{0x01,0xF6,0x03}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x09}), Encoding.BigEndianUnicode.GetString(new byte[]{0x01,0xF6,0x20}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0A}), Encoding.BigEndianUnicode.GetString(new byte[]{0x01,0xF6,0x14}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0B}), Encoding.BigEndianUnicode.GetString(new byte[]{0x01,0xF6,0x11}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0C}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x00}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0D}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x01}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0E}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x14}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x0F}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0xC4}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x10}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0x57}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x11}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0x53}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x12}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0x09}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x13}), Encoding.BigEndianUnicode.GetString(new byte[]{0x01,0xF4,0xF1})  },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x15}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x60}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x16}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x66}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x17}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x65}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x18}), Encoding.BigEndianUnicode.GetString(new byte[]{0x26,0x63}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x19}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0xA1}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x1A}), Encoding.BigEndianUnicode.GetString(new byte[]{0x2B,0x05}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x1B}), Encoding.BigEndianUnicode.GetString(new byte[]{0x2B,0x06}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x1C}), Encoding.BigEndianUnicode.GetString(new byte[]{0x2B,0x07}) },
            { Encoding.BigEndianUnicode.GetString(new byte[]{0xE0,0x28}), Encoding.BigEndianUnicode.GetString(new byte[]{0x27,0x15}) },
        };

        private string ToUnicode()
        {
            string str = _Name;
            // not the final implementation
            // it's just to test if things are working
            foreach (var entry in CharTable)
                str = str.Replace(entry.Key, entry.Value);
            return str;
        }

    }
}
