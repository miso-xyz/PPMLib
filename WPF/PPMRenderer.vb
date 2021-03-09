Imports System
Imports System.Collections.Generic
Imports System.Windows.Media
Imports System.Windows.Media.Imaging

Namespace PPMLib.WPF
	Public Module PPMRenderer
		''' <summary>
		''' Color pallete used when rendering the thumbnail
		''' </summary>
		Public ReadOnly ThumbnailPalette As New List(Of Color) From {Color.FromRgb(&HFF,&HFF,&HFF), Color.FromRgb(&H52,&H52,&H52), Color.FromRgb(&HFF,&HFF,&HFF), Color.FromRgb(&H9C,&H9C,&H9C), Color.FromRgb(&HFF,&H48,&H44), Color.FromRgb(&HC8,&H51,&H4F), Color.FromRgb(&HFF,&HAD,&HAC), Color.FromRgb(&H0,&HFF,&H0), Color.FromRgb(&H48,&H40,&HFF), Color.FromRgb(&H51,&H4F,&HB8), Color.FromRgb(&HAD,&HAB,&HFF), Color.FromRgb(&H0,&HFF,&H0), Color.FromRgb(&HB6,&H57,&HB7), Color.FromRgb(&H0,&HFF,&H0), Color.FromRgb(&H0,&HFF,&H0), Color.FromRgb(&H0,&HFF,&H0)}

		''' <summary>
		''' Converts the flipnote's thumbnail to Bitmap
		''' </summary>
		''' <param name="buffer">Raw Thumbnail Bytes</param>
		Public Function GetThumbnailBitmap(ByVal buffer() As Byte) As WriteableBitmap
			If buffer.Length <> 1536 Then
				Throw New ArgumentException("Wrong thumbnail buffer size")
			End If

			Dim bytes((32 * 48) - 1) As Byte

			Dim offset As Integer = 0
			For ty As Integer = 0 To 47 Step 8
				For tx As Integer = 0 To 31 Step 4
					For l As Integer = 0 To 7
						Dim line As Integer = (ty + l) << 5
						For px As Integer = 0 To 3
							' Need to reverse nibbles :
							bytes(line + tx + px) = CByte(((buffer(offset) And &HF) << 4) Or ((buffer(offset) And &HF0) >> 4))
							offset += 1
						Next px
					Next l
				Next tx
			Next ty

			Dim palette = New BitmapPalette(ThumbnailPalette)
			' Directly set bitmap's 4-bit palette instead of using 32-bit colors 
			Dim bmp = New WriteableBitmap(64, 48, 96, 96, PixelFormats.Indexed4, palette)
			bmp.WritePixels(New System.Windows.Int32Rect(0, 0, 64, 48), bytes, 32, 0)
			Return bmp
		End Function

		''' <summary>
		''' All colors available for frames
		''' </summary>
		Public ReadOnly FramePalette As New List(Of Color) From {Color.FromRgb(&He,&He,&He), Color.FromRgb(&HFF,&HFF,&HFF), Color.FromRgb(&HFF,&H0,&H0), Color.FromRgb(&H0,&H0,&HFF)}

		''' <summary>
		''' Get the pen color of the choosen layer
		''' </summary>
		''' <param name="pc">Pen Color (of the layer)</param>
		''' <param name="paper">Paper Color (of the frame)</param>
		''' <returns>Color</returns>
		Private Function GetLayerColor(ByVal pc As PenColor, ByVal paper As PaperColor) As Color
			If pc = PenColor.Inverted Then
				Return FramePalette(1 - CInt(paper))
			End If
			Return FramePalette(CInt(pc))
		End Function

		''' <summary>
		''' Renders the given frame to a WritableBitmap
		''' </summary>
		''' <param name="frame">Frame Data</param>
		''' <returns>Rendered Frame</returns>
		Public Function GetFrameBitmap(ByVal frame As PPMFrame) As WriteableBitmap
			Dim palette = New BitmapPalette(New List(Of Color) From {FramePalette(CInt(frame.PaperColor)), GetLayerColor(frame.Layer1.PenColor,frame.PaperColor), GetLayerColor(frame.Layer2.PenColor,frame.PaperColor)})
			Dim bmp = New WriteableBitmap(256, 192, 96, 96, PixelFormats.Indexed2, palette)

			Dim stride As Integer = 64
			Dim pixels((64 * 192) - 1) As Byte
			For x As Integer = 0 To 255
				For y As Integer = 0 To 191
					If frame.Layer1(y, x) Then
						Dim b As Integer = 256 * y + x
						Dim p As Integer = 3 - b Mod 4
						b \= 4
						pixels(b) = pixels(b) And CByte(Not (&B11 << (2 * p)))
						pixels(b) = pixels(b) Or CByte(&B10 << (2 * p))
					End If
				Next y
			Next x
			For x As Integer = 0 To 255
				For y As Integer = 0 To 191
					If frame.Layer2(y, x) Then
						Dim b As Integer = 256 * y + x
						Dim p As Integer = 3 - b Mod 4
						b \= 4
						pixels(b) = pixels(b) And CByte(Not (&B11 << (2 * p)))
						pixels(b) = pixels(b) Or CByte(&B01 << (2 * p))
					End If
				Next y
			Next x
			bmp.WritePixels(New System.Windows.Int32Rect(0, 0, 256, 192), pixels, stride, 0)
			Return bmp
		End Function

	End Module
End Namespace
