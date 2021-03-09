Imports System

Namespace PPMLib
	Public Class PPMTimestamp
		Public Sub New(ByVal value As UInteger)
			Me.Value = value
		End Sub
		Public Value As UInteger

		Private Shared ReadOnly _2000_01_01 As New DateTime(2000, 1, 1)
		Public Function ToDateTime() As DateTime
			Return _2000_01_01.AddSeconds(Value)
		End Function
		Public Overrides Function ToString() As String
			Return ToDateTime().ToString()
		End Function
	End Class
End Namespace
