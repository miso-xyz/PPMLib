Imports NAudio.Wave
Imports PPMLib.Extensions
Imports System
Imports System.IO

Namespace PPMLib
	Public Class PPMAudio

		Public Property SoundHeader() As _SoundHeader
		Public Property SoundData() As _SoundData


		Public Sub New()

		End Sub

		''' <summary>
		''' Returns the fully mixed audio of the Flipnote, Including its Sound Effects.
		''' Returns Null if no audio exists.
		''' </summary>
		''' <param name="flip"></param>
		''' <returns>Signed 16-bit PCM audio</returns>
		Public Function GetWavBGM(ByVal flip As PPMFile) As Byte()
			' start decoding
			Dim encoder As New AdpcmDecoder(flip)
			Dim decoded = encoder.getAudioMasterPcm(32768)
			If decoded.Length > 0 Then
				Dim output(decoded.Length - 1) As Byte

				' thank you https://github.com/meemo
				For i As Integer = 0 To decoded.Length - 1 Step 2
					Try
						output(i) = CByte(decoded(i + 1) And &Hff)
						output(i + 1) = CByte(decoded(i) >> 8)
					Catch e As Exception

					End Try

				Next i



				Dim provider = New RawSourceWaveStream(New MemoryStream(output), New WaveFormat(32768 \ 2, 16, 1))
				Dim a = New MemoryStream()
				WaveFileWriter.WriteWavFileToStream(a, provider)
				Return a.ToArray()
			End If
			Return Nothing


		End Function

	End Class

	Public Class _SoundHeader
		Public BGMTrackSize As UInteger
		Public SE1TrackSize As UInteger
		Public SE2TrackSize As UInteger
		Public SE3TrackSize As UInteger
		Public CurrentFramespeed As Byte
		Public RecordingBGMFramespeed As Byte
	End Class

	Public Class _SoundData
		Public RawBGM() As Byte
		Public RawSE1() As Byte
		Public RawSE2() As Byte
		Public RawSE3() As Byte
	End Class
End Namespace
