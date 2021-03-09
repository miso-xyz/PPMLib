Imports System
Imports System.Text.RegularExpressions

Namespace PPMLib
	Public Class PPMFilename
		''' <summary>
		''' Set the filename of the flipnote
		''' </summary>
		''' <param name="bytes">New filename</param>
		Public Sub New(ByVal bytes() As Byte)
			Buffer = bytes
		End Sub

		''' <summary>
		''' Set the filename of the flipnote
		''' </summary>
		''' <param name="fn">New filename</param>
		Public Sub New(ByVal fn As String)
			If fn.Length <> 24 Then
				Throw New ArgumentException("Wrong filename string length. It should be 24 characters long")
			End If
			If Not Regex.IsMatch(fn, "[0-9,A-F]{6}_[0-9,A-F]{13}_\d{3}") Then
				Throw New FormatException("Incorrect filename")
			End If
			Buffer = New Byte(17){}
			For i As Integer = 0 To 2
				Buffer(i) = Convert.ToByte("" & AscW(fn.Chars(2 * i)) + fn.Chars(2 * i + 1), 16)
			Next i
			Dim i As Integer = 3
			Dim j As Integer = 7
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: for (int i = 3, j = 7; i < 16; Buffer[i++] = (byte)fn[j++])
			Do While i < 16

				Buffer(i) = AscW(fn.Chars(j))
j += 1
i += 1
			Loop
			Dim b As UShort = Convert.ToUInt16(fn.Substring(21))
			Buffer(16) = CByte(b)
			b >>= 8
			Buffer(17) = CByte(b)
		End Sub

		Private _Buffer() As Byte
		Public Property Buffer() As Byte()
			Get
				Return _Buffer
			End Get
			Set(ByVal value As Byte())
				If value.Length <> 18 Then
					Throw New ArgumentException("Wrong filename buffer size. It should be 18 bytes long")
				End If
				_Buffer = value
			End Set
		End Property

		Public Overrides Function ToString() As String
			Dim result = ""
			Dim i As Integer = 0
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: for (int i = 0; i < 3; result += Buffer[i++].ToString("X2"))
			Do While i < 3

				result &= Buffer(i).ToString("X2")
i += 1
			Loop
			result &= "_"
			i = 3
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: for (int i = 3; i < 16; result += Convert.ToChar(Buffer[i++]))
			Do While i < 16

				result &= Convert.ToChar(Buffer(i))
i += 1
			Loop
			Return result & "_" & ((Buffer(17) << 4) Or Buffer(16)).ToString("000")
		End Function
	End Class
End Namespace
