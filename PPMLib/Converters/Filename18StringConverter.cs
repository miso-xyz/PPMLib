//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

using System.ComponentModel;
using System.Globalization;

namespace PPMLib
{
	public class Filename18StringConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType.Equals(typeof(string)))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, sourceType);
			}
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			if (destinationType.Equals(typeof(string)))
			{
				return true;
			}
			else
			{
				return base.CanConvertTo(context, destinationType);
			}
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType.Equals(typeof(string)))
			{
				return ((Filename18)value).ToString();
			}
			else
			{
				return base.ConvertTo(context, culture, value, destinationType);
			}
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value.GetType().Equals(typeof(string)))
			{
				return new Filename18((value == null ? null : Convert.ToString(value)));
			}
			else
			{
				return base.ConvertFrom(context, culture, value);
			}
		}

		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] Attribute)
		{
			return TypeDescriptor.GetProperties(value);
		}
	}

}