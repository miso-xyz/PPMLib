Imports System.Collections.Generic

Namespace PPMLib
	Public Class PPMLayer
		Private _pen As PenColor
		Private _visibility As Boolean
		Friend _layerData((32 * 192) - 1) As Byte
		Friend _linesEncoding(47) As Byte
		Public Function LinesEncoding(ByVal lineIndex As Integer) As LineEncoding
			Return CType((_linesEncoding(lineIndex >> 2) >> ((lineIndex And &H3) << 1)) And &H3, LineEncoding)
		End Function

		''' <summary>
		''' Apply a line encoding value to a line of pixels
		''' </summary>
		''' <param name="lineIndex">Line Index</param>
		''' <param name="value">Line Encoding to apply onto the line</param>
		Public Sub setLinesEncoding(ByVal lineIndex As Integer, ByVal value As LineEncoding)
			Dim o As Integer = lineIndex >> 2
			Dim pos As Integer = (lineIndex And &H3) * 2
			Dim b = _linesEncoding(o)
			b = CByte(b And CByte(Not (&H3 << pos)))
			b = CByte(b Or CByte(CInt(value) << pos))
			_linesEncoding(o) = b
		End Sub

		#Region "Line-Related Functions"
		''' <summary>
		''' Set the line encoding for the whole layer
		''' </summary>
		''' <param name="index">Line Index</param>
		''' <returns>New encoding type for the line</returns>
		Public Function SetLineEncodingForWholeLayer(ByVal index As Integer) As LineEncoding ' Set?
			Dim _0chks = 0
			Dim _1chks = 0
			For x = 0 To 32
				Dim c = 8 * index
				Dim n0 = 0
				Dim n1 = 0
				For x_ = 0 To 8
					If Me(index, c + x_) Then
						n1 += 1
					Else
						n0 += 1
					End If
				Next x_
				_0chks += If(n0 = 8, 1, 0)
				_1chks += If(n1 = 8, 1, 0)
			Next x
			If _0chks = 32 Then
				Return LineEncoding.SkipLine
			ElseIf _0chks = (If(_1chks = 0, -1, 0)) Then
				Return LineEncoding.RawLineData
			Else
				Return (If(_0chks > _1chks, LineEncoding.CodedLine, LineEncoding.InvertedCodedLine))
			End If
		End Function

		''' <summary>
		''' Insert a line in current layer
		''' </summary>
		''' <param name="lineData">Line Data</param>
		''' <param name="index">Index Of Line</param>
		Private Sub InsertLineInLayer(ByVal lineData As List(Of Byte), ByVal index As Integer)
			Dim chks As New List(Of Byte)()
			Select Case LinesEncoding(index)
				Case 0
						Return
				Case CType(1, LineEncoding), CType(2, LineEncoding)
						Dim flag As UInteger = 0
						For x = 0 To 32
							Dim chunk As Byte = 0
							For x_ = 0 To 8
								If Me(index, 8 * x + x_) Then
									chunk = CByte(chunk Or CByte(1 << x_))
								End If
							Next x_
							If chunk <> (If(LinesEncoding(index) = CType(1, PPMLib.LineEncoding), &H0, &HFF)) Then
								flag = flag Or (1UI << (31 - x))
								chks.Add(chunk)
							End If
						Next x
						lineData.Add(CByte((flag And &HFF000000UI) >> 24))
						lineData.Add(CByte((flag And &HFF0000UI) >> 16))
						lineData.Add(CByte((flag And &HFF00UI) >> 8))
						lineData.Add(CByte(flag And &HFFUI))
						lineData.AddRange(chks)
						Return
				Case CType(3, LineEncoding)
						For x = 0 To 32
							Dim chunk As Byte = 0
							For x_ = 0 To 8
								If Me(index, 8 * x + x_) Then
									chunk = CByte(chunk Or CByte(1 << x_))
								End If
							Next x_
							chks.Add(chunk)
						Next x
						Exit Select
			End Select
		End Sub
		#End Region

		''' <summary>
		''' Set the visibility of the layer
		''' </summary>
		Public Property Visible() As Boolean
			Get
				Return _visibility
			End Get
			Set(ByVal value As Boolean)
				_visibility = value
			End Set
		End Property

		Default Public Property Item(ByVal p As Integer) As Byte
			Get
				Return _layerData(p)
			End Get
			Set(ByVal value As Byte)
				_layerData(p) = value
			End Set
		End Property

		Default Public Property Item(ByVal y As Integer, ByVal x As Integer) As Boolean
			Get
				Dim p As Integer = 256 * y + x
				Return (_layerData(p >> 3) And (CByte(1 << (p And 7)))) <> 0
			End Get
			Set(ByVal value As Boolean)
				Dim p As Integer = 256 * y + x
				_layerData(p >> 3) = _layerData(p >> 3) And CByte(Not (1 << (p And &H7)))
				_layerData(p >> 3) = _layerData(p >> 3) Or CByte((If(value, 1, 0)) << (p And &H7))
			End Set
		End Property
		Public Property PenColor() As PenColor
	End Class
End Namespace