//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PPMLib
{
	public class Filename18
	{
		public Filename18(byte[] bytes)
		{
			if (bytes.Length != 18)
			{
				throw new ArgumentException("Wrong buffer size length");
			}
			Array.Copy(bytes, _bytes, 17);
		}

		public Filename18(string fn)
		{
			if (fn.Length != 24)
			{
				throw new ArgumentException("Wrong filename length");
			}
			if (!Regex.IsMatch(fn, "[0-9,A-F]{6}_[0-9,A-F]{13}_\\d{3}"))
			{
				throw new FormatException("Incorrect filename");
			}
			for (var i = 0; i <= 2; i++)
			{
				_bytes[i] = Convert.ToByte("" + fn[2 * i].ToString() + fn[2 * i + 1].ToString(), 16);
			}
			var j = 7;
			for (var i = 3; i <= 15; i++)
			{
				_bytes[i] = (byte)Microsoft.VisualBasic.Strings.Asc(fn[j]);
				j += 1;
			}
			j = Convert.ToUInt16(fn.Substring(21));
			_bytes[16] = (byte)(j & 0xFF);
			j >>= 8;
			_bytes[17] = (byte)j;
		}

		public override string ToString()
		{
			// Converts to FFFFFF_NNNNNNNNNNNNN_000 string
			string str = "";
			for (var i = 0; i <= 2; i++)
			{
				str += _bytes[i].ToString("X2");
			}
			str += "_";
			for (var i = 3; i <= 15; i++)
			{
				str += Microsoft.VisualBasic.Strings.Chr(_bytes[i]).ToString();
			}
			str += "_";
			var cnt = (_bytes[17] << 4) | _bytes[16];
			str += cnt.ToString("000");
			return str;
		}

#region Properties
		private byte[] _bytes = new byte[18];
#endregion

		public byte[] Bytes
		{
			get
			{
				return _bytes;
			}
			set
			{
				_bytes = value;
			}
		}

	}

}