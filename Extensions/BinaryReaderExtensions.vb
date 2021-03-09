Imports System.IO
Imports System.Text

Namespace PPMLib.Extensions
	Friend Module BinaryReaderExtensions
		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadWChars(ByVal br As BinaryReader, ByVal count As Integer) As String
			Return Encoding.Unicode.GetString(br.ReadBytes(2 * count))
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadPPMFilename(ByVal br As BinaryReader) As PPMFilename
			Return New PPMFilename(br.ReadBytes(18))
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadPPMFileFragment(ByVal br As BinaryReader) As PPMFileFragment
			Return New PPMFileFragment(br.ReadBytes(8))
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadPPMTimestamp(ByVal br As BinaryReader) As PPMTimestamp
			Return New PPMTimestamp(br.ReadUInt32())
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadPPMThumbnail(ByVal br As BinaryReader) As PPMThumbnail
			Return New PPMThumbnail(br.ReadBytes(1536))
		End Function

		<System.Runtime.CompilerServices.Extension> _
		Public Function ReadPPMFrame(ByVal br As BinaryReader) As PPMFrame
			Dim frame As New PPMFrame()
			frame._firstByteHeader = br.ReadByte()
			If (frame._firstByteHeader And &H60) <> 0 Then
				frame._translateX = br.ReadSByte()
				frame._translateY = br.ReadSByte()
			End If

			frame.PaperColor = CType(frame._firstByteHeader Mod 2, PaperColor)
			frame.Layer1.PenColor = CType((frame._firstByteHeader >> 1) And 3, PenColor)
			frame.Layer2.PenColor = CType((frame._firstByteHeader >> 3) And 3, PenColor)

			frame.Layer1._linesEncoding = br.ReadBytes(&H30)
			frame.Layer2._linesEncoding = br.ReadBytes(&H30)

			Dim y As Integer = 0
			Dim yy As Integer
			Do While y < 192
				yy = y << 5
				Select Case frame.Layer1.LinesEncoding(y)
					Case 0
					Case CType(1, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer1._layerData(yy + x) = &H0
						Next x
						Dim b1 As Byte = br.ReadByte(), b2 As Byte = br.ReadByte(), b3 As Byte = br.ReadByte(), b4 As Byte = br.ReadByte()
						Dim bytes As UInteger = (CUInt(b1 << 24)) + (CUInt(b2 << 16)) + (CUInt(b3 << 8)) + b4
						Do While bytes <> 0
							If (bytes And &H80000000UI) <> 0 Then
								frame.Layer1._layerData(yy) = br.ReadByte()
							End If
							bytes <<= 1
							yy += 1
						Loop
					Case CType(2, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer1._layerData(yy + x) = &HFF
						Next x
						b1 = br.ReadByte()
						b2 = br.ReadByte()
						b3 = br.ReadByte()
						b4 = br.ReadByte()
						bytes = (CUInt(b1 << 24)) + (CUInt(b2 << 16)) + (CUInt(b3 << 8)) + b4
						Do While bytes <> 0
							If (bytes And &H80000000UI) <> 0 Then
								frame.Layer1._layerData(yy) = br.ReadByte()
							End If
							bytes <<= 1
							yy += 1
						Loop
					Case CType(3, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer1._layerData(yy + x) = br.ReadByte()
						Next x
				End Select
				y += 1
			Loop
			y = 0
			Dim yy As Integer
			Do While y < 192
				yy = y << 5
				Select Case frame.Layer2.LinesEncoding(y)
					Case 0
					Case CType(1, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer2._layerData(yy + x) = &H0
						Next x
						Dim b1 As Byte = br.ReadByte(), b2 As Byte = br.ReadByte(), b3 As Byte = br.ReadByte(), b4 As Byte = br.ReadByte()
						Dim bytes As UInteger = (CUInt(b1 << 24)) + (CUInt(b2 << 16)) + (CUInt(b3 << 8)) + b4
						Do While bytes <> 0
							If (bytes And &H80000000UI) <> 0 Then
								frame.Layer2._layerData(yy) = br.ReadByte()
							End If
							bytes <<= 1
							yy += 1
						Loop
					Case CType(2, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer2._layerData(yy + x) = &HFF
						Next x
						b1 = br.ReadByte()
						b2 = br.ReadByte()
						b3 = br.ReadByte()
						b4 = br.ReadByte()
						bytes = (CUInt(b1 << 24)) + (CUInt(b2 << 16)) + (CUInt(b3 << 8)) + b4
						Do While bytes <> 0
							If (bytes And &H80000000UI) <> 0 Then
								frame.Layer2._layerData(yy) = br.ReadByte()
							End If
							bytes <<= 1
							yy += 1
						Loop
					Case CType(3, LineEncoding)
						For x As Integer = 0 To 31
							frame.Layer2._layerData(yy + x) = br.ReadByte()
						Next x
				End Select
				y += 1
			Loop
			Return frame
		End Function
	End Module
End Namespace
