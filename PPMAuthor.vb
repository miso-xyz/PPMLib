Imports System.Collections.Generic
Imports System.Text

Namespace PPMLib
	Public Class PPMAuthor

		Public Sub New(ByVal name As String, ByVal id As ULong)
			_Name = name
			Me.Id = id
		End Sub

		Private _Name As String
		Public ReadOnly Property Name() As String
			Get
				Return ToUnicode()
			End Get
		End Property
		Public ReadOnly Property Id() As ULong

		Public Overrides Function ToString() As String
			Return $"{Name} ({Id.ToString("X8")})"
		End Function
		' public override string ToString() => Name + " " + string.Join(" ", Encoding.BigEndianUnicode.GetBytes(Name).Select(t => t.ToString("X2")));
		' ^-- this line is for debugging --^

		' As stated here: https://github.com/Sudomemo/Sudofont
		Friend Shared ReadOnly CharTable As New Dictionary(Of String, String) From {
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H0}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HB6})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H1}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HB7})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H2}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HCD})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H3}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HCE})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H4}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HC1})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H5}), Encoding.BigEndianUnicode.GetString(New Byte(){&H24,&HC7})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H6}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&H95})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H7}), Encoding.BigEndianUnicode.GetString(New Byte(){&H23,&HF0})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H8}), Encoding.BigEndianUnicode.GetString(New Byte(){&H1,&HF6,&H3})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H9}), Encoding.BigEndianUnicode.GetString(New Byte(){&H1,&HF6,&H20})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HA}), Encoding.BigEndianUnicode.GetString(New Byte(){&H1,&HF6,&H14})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HB}), Encoding.BigEndianUnicode.GetString(New Byte(){&H1,&HF6,&H11})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HC}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H0})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HD}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H1})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HE}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H14})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&HF}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&HC4})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H10}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&H57})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H11}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&H53})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H12}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&H9})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H13}), Encoding.BigEndianUnicode.GetString(New Byte(){&H1,&HF4,&HF1})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H15}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H60})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H16}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H66})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H17}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H65})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H18}), Encoding.BigEndianUnicode.GetString(New Byte(){&H26,&H63})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H19}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&HA1})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H1A}), Encoding.BigEndianUnicode.GetString(New Byte(){&H2B,&H5})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H1B}), Encoding.BigEndianUnicode.GetString(New Byte(){&H2B,&H6})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H1C}), Encoding.BigEndianUnicode.GetString(New Byte(){&H2B,&H7})
			},
			{
				Encoding.BigEndianUnicode.GetString(New Byte(){&HE0,&H28}), Encoding.BigEndianUnicode.GetString(New Byte(){&H27,&H15})
			}
		}

		Private Function ToUnicode() As String
			Dim str As String = _Name
			' not the final implementation
			' it's just to test if things are working
			For Each entry In CharTable
				str = str.Replace(entry.Key, entry.Value)
			Next entry
			Return str
		End Function

	End Class
End Namespace
