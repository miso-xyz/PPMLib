//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace PPMLib
{
	public enum LineEncoding
	{
		SkipLine = 0,
		CodedLine = 1,
		InvertedCodedLine = 2,
		RawLineData = 3
	}
}