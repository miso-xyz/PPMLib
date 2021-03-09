Imports System

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Runtime.InteropServices

Namespace PPMLib.Winforms
	Public Module PPMRenderer
		''' <summary>
		''' Color pallete used when rendering the thumbnail
		''' </summary>
'INSTANT VB TODO TASK: There is no VB equivalent to 'unchecked' in this context:
'ORIGINAL LINE: public static readonly Color[] ThumbnailPalette = { Color.FromArgb(unchecked((int)&HFFFFFFFF)), Color.FromArgb(unchecked((int)&HFF525252)), Color.FromArgb(unchecked((int)&HFFFFFFFF)), Color.FromArgb(unchecked((int)&HFF9C9C9C)), Color.FromArgb(unchecked((int)&HFFFF4844)), Color.FromArgb(unchecked((int)&HFFC8514F)), Color.FromArgb(unchecked((int)&HFFFFADAC)), Color.FromArgb(unchecked((int)&HFF00FF00)), Color.FromArgb(unchecked((int)&HFF4840FF)), Color.FromArgb(unchecked((int)&HFF514FB8)), Color.FromArgb(unchecked((int)&HFFADABFF)), Color.FromArgb(unchecked((int)&HFF00FF00)), Color.FromArgb(unchecked((int)&HFFB657B7)), Color.FromArgb(unchecked((int)&HFF00FF00)), Color.FromArgb(unchecked((int)&HFF00FF00)), Color.FromArgb(unchecked((int)&HFF00FF00)) };
		Public ReadOnly ThumbnailPalette() As Color = { Color.FromArgb(CInt(&HFFFFFFFF)), Color.FromArgb(CInt(&HFF525252)), Color.FromArgb(CInt(&HFFFFFFFF)), Color.FromArgb(CInt(&HFF9C9C9C)), Color.FromArgb(CInt(&HFFFF4844)), Color.FromArgb(CInt(&HFFC8514F)), Color.FromArgb(CInt(&HFFFFADAC)), Color.FromArgb(CInt(&HFF00FF00)), Color.FromArgb(CInt(&HFF4840FF)), Color.FromArgb(CInt(&HFF514FB8)), Color.FromArgb(CInt(&HFFADABFF)), Color.FromArgb(CInt(&HFF00FF00)), Color.FromArgb(CInt(&HFFB657B7)), Color.FromArgb(CInt(&HFF00FF00)), Color.FromArgb(CInt(&HFF00FF00)), Color.FromArgb(CInt(&HFF00FF00)) }

		''' <summary>
		''' Converts the flipnote's thumbnail to Bitmap
		''' </summary>
		''' <param name="buffer">Raw Thumbnail Bytes</param>
		Public Function GetThumbnailBitmap(ByVal buffer() As Byte) As Bitmap
			If buffer.Length <> 1536 Then
				Throw New ArgumentException("Wrong thumbnail buffer size")
			End If
			' Directly set bitmap's 4-bit palette instead of using 32-bit colors 
			Dim bmp = New Bitmap(64, 48, PixelFormat.Format4bppIndexed)
			Dim palette = bmp.Palette
			Dim entries = palette.Entries
			For i = 0 To 15
				entries(i) = ThumbnailPalette(i)
			Next i
			bmp.Palette = palette

			Dim rect = New Rectangle(0, 0, 64, 48)
			Dim bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat)

			Dim bytes((32 * 48)) As Byte
			Dim IPtr = bmpData.Scan0
			Marshal.Copy(IPtr, bytes, 0, 32 * 48)

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

			Marshal.Copy(bytes, 0, IPtr, 32 * 48)
			bmp.UnlockBits(bmpData)
			Return bmp
		End Function

		''' <summary>
		''' All colors available for frames
		''' </summary>
'INSTANT VB TODO TASK: There is no VB equivalent to 'unchecked' in this context:
'ORIGINAL LINE: public static readonly Color[] FramePalette = { Color.FromArgb(unchecked((int)&HFF000000)), Color.FromArgb(unchecked((int)&HFFFFFFFF)), Color.FromArgb(unchecked((int)&HFFFF0000)), Color.FromArgb(unchecked((int)&HFF0000FF)) };
		Public ReadOnly FramePalette() As Color = { Color.FromArgb(CInt(&HFF000000)), Color.FromArgb(CInt(&HFFFFFFFF)), Color.FromArgb(CInt(&HFFFF0000)), Color.FromArgb(CInt(&HFF0000FF)) }

		''' <summary>
		''' Renders the given frame to a Bitmap
		''' </summary>
		''' <param name="frame">Frame Data</param>
		''' <returns>Rendered Frame</returns>
		Public Function GetFrameBitmap(ByVal frame As PPMFrame) As Bitmap
			Dim bmp = New Bitmap(256, 192, PixelFormat.Format8bppIndexed)
			Dim palette = bmp.Palette
			Dim entries = palette.Entries
			For i = 0 To 3
				entries(i) = FramePalette(i)
			Next i
			bmp.Palette = palette

			Dim rect = New Rectangle(0, 0, 256, 192)
			Dim bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat)

			Dim bytes((256 * 192)) As Byte
			Dim IPtr = bmpData.Scan0
			Marshal.Copy(IPtr, bytes, 0, 256 * 192)
			For y = 0 To 191
				For x = 0 To 255
					If frame.Layer1(y, x) Then
						If frame.Layer1.PenColor <> PenColor.Inverted Then
							bytes(256 * y + x) = CByte(frame.Layer1.PenColor)
						Else
							bytes(256 * y + x) = CByte(1 - CInt(frame.PaperColor))
						End If
					Else
						If frame.Layer2(y, x) Then
							If frame.Layer2.PenColor <> PenColor.Inverted Then
								bytes(256 * y + x) = CByte(frame.Layer2.PenColor)
							Else
								bytes(256 * y + x) = CByte(1 - CInt(frame.PaperColor))
							End If
						Else
							bytes(256 * y + x) = CByte(frame.PaperColor)
						End If
					End If
				Next x
			Next y
			Marshal.Copy(bytes, 0, IPtr, 256 * 192)
			bmp.UnlockBits(bmpData)
			Return bmp
		End Function
	End Module

End Namespace