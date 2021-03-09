Imports System
Imports System.Text.RegularExpressions

Namespace PPMLib.Extensions
	Public Module Utils
		<System.Runtime.CompilerServices.Extension> _
		Public Function Clamp(Of T As IComparable(Of T))(ByVal val As T, ByVal min As T, ByVal max As T) As T
			If val.CompareTo(min) < 0 Then
				Return min
			ElseIf val.CompareTo(max) > 0 Then
				Return max
			Else
				Return val
			End If
		End Function

		Public Function NumClamp(ByVal n As Integer, ByVal l As Integer, ByVal h As Integer) As Integer
			If n < l Then
				Return l
			End If
			If n > h Then
				Return h
			End If
			Return n
		End Function

		Public Sub NumericalSort(ByVal ar() As String)
			Dim rgx As New Regex("([^0-9]*)([0-9]+)")
			Array.Sort(ar, Function(a, b)
				Dim ma = rgx.Matches(a)
				Dim mb = rgx.Matches(b)
				For i As Integer = 0 To ma.Count - 1
					Dim ret As Integer = ma(i).Groups(1).Value.CompareTo(mb(i).Groups(1).Value)
					If ret <> 0 Then
						Return ret
					End If

					ret = Integer.Parse(ma(i).Groups(2).Value) - Integer.Parse(mb(i).Groups(2).Value)
					If ret <> 0 Then
						Return ret
					End If
				Next i

				Return 0
			End Function)
		End Sub
	End Module
End Namespace
