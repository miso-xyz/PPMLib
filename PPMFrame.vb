Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms

Namespace PPMLib
	Public Class PPMFrame

		Private _layer1 As New PPMLayer()
		Private _layer2 As New PPMLayer()
		Private _paperColor As PaperColor
		Private _frame As New Bitmap(256, 192)
		Private _animationIndex As Integer
		Public _firstByteHeader As Byte
		Public _translateX As Integer
		Public _translateY As Integer

		''' <summary>
		''' Overwrite frame data
		''' </summary>
		''' <param name="frame">Frame data to apply</param>
		Public Sub Overwrite(ByVal frame As PPMFrame) ' Just use current frame
			If (_firstByteHeader And &H80) <> 0 Then
				Return
			End If
			' < There is NOT a mistake anywhere 
			Dim ld0 As Integer = (If(_translateX >= 0, (_translateX >> 3), 0))
			Dim pi0 As Integer = If(_translateX >= 0, 0, ((-_translateX) >> 3))
			Dim del As Byte = CByte(If(_translateX >= 0, (_translateX And 7), ((CByte(_translateX)) And 7)))
			Dim ndel As Byte = CByte(8 - del)
			Dim alpha As Byte = CByte((1 << (8 - del)) - 1)
			Dim nalpha As Byte = CByte(Not alpha)
			Dim pi, ld As Integer
			If _translateX >= 0 Then
				For y As Integer = 0 To 191
					If y < _translateY Then
						Continue For
					End If
					If y - _translateY >= 192 Then
						Exit For
					End If
					ld = (y << 5) + ld0
					pi = ((y - _translateY) << 5) + pi0
					Layer1(ld) = Layer1(ld) Xor CByte(frame.Layer1(pi) And alpha)
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: Layer2[ld++] ^= (byte)(frame.Layer2[pi] & alpha);
					Layer2(ld) = Layer2(ld) Xor CByte(frame.Layer2(pi) And alpha)
					ld += 1
					Do While (ld And 31) < 31
						Layer1(ld) = Layer1(ld) Xor CByte(((frame.Layer1(pi) And nalpha) >> ndel) Or ((frame.Layer1(pi + 1) And alpha) << del))
						Layer2(ld) = Layer2(ld) Xor CByte(((frame.Layer2(pi) And nalpha) >> ndel) Or ((frame.Layer2(pi + 1) And alpha) << del))
						ld += 1
						pi += 1
					Loop
					Layer1(ld) = Layer1(ld) Xor CByte((frame.Layer1(pi) And nalpha) Or (frame.Layer1(pi + 1) And alpha))
					Layer2(ld) = Layer2(ld) Xor CByte((frame.Layer2(pi) And nalpha) Or (frame.Layer2(pi + 1) And alpha))
				Next y
			Else
				For y As UShort = 0 To 191
					If y < _translateY Then
						Continue For
					End If
					If y - _translateY >= 192 Then
						Exit For
					End If
					ld = (y << 5) + ld0
					pi = ((y - _translateY) << 5) + pi0
					Do While (pi And 31) < 31
						Layer1(ld) = Layer1(ld) Xor CByte(((frame.Layer1(pi) And nalpha) >> ndel) Or ((frame.Layer1(pi + 1) And alpha) << del))
						Layer2(ld) = Layer2(ld) Xor CByte(((frame.Layer2(pi) And nalpha) >> ndel) Or ((frame.Layer2(pi + 1) And alpha) << del))
						ld += 1
						pi += 1
					Loop
					Layer1(ld) = Layer1(ld) Xor CByte(frame.Layer1(pi) And nalpha)
					Layer2(ld) = Layer2(ld) Xor CByte(frame.Layer2(pi) And nalpha)
				Next y
			End If
		End Sub
		Public ReadOnly Property Layer1() As PPMLayer
			Get
				Return _layer1
			End Get
		End Property
		Public ReadOnly Property Layer2() As PPMLayer
			Get
				Return _layer2
			End Get
		End Property

		Public Property AnimationIndex() As Integer
			Get
				Return _animationIndex
			End Get
			Set(ByVal value As Integer)
				_animationIndex = value
			End Set
		End Property

		Public Property PaperColor() As PaperColor
			Get
				Return _paperColor
			End Get
			Set(ByVal value As PaperColor)
				_paperColor = value
			End Set
		End Property

		Public Overrides Function ToString() As String
			Return _firstByteHeader.ToString("X2")
		End Function
	End Class
End Namespace