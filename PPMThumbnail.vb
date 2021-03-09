Imports System

Namespace PPMLib
	Public Class PPMThumbnail
		Public Sub New(ByVal bytes() As Byte)
			Buffer = bytes
		End Sub

		Private _Buffer() As Byte
		Public Property Buffer() As Byte()
			Get
				Return _Buffer
			End Get
			Set(ByVal value As Byte())
				If value.Length <> 1536 Then
					Throw New ArgumentException("Thumbnail buffer must be 1536 bytes long")
				End If
				_Buffer = value
			End Set
		End Property
	End Class
End Namespace
