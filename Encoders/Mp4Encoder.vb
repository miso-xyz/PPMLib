Imports FFMpegCore
Imports FFMpegCore.Enums
Imports PPMLib.Extensions
Imports PPMLib.Winforms
Imports System
Imports System.IO
Imports System.Linq

Namespace PPMLib.Encoders
	Public Class Mp4Encoder
		Private Property Flipnote() As PPMFile

		Public Sub New(ByVal flipnote As PPMFile)
			Me.Flipnote = flipnote
		End Sub

		Public Function EncodeMp4() As Byte()
			Return Encode()
		End Function

		Public Function EncodeMp4(ByVal path As String) As Byte()
			Return Encode(path)
		End Function

		Public Function EncodeMp4(ByVal scale As Integer) As Byte()
			Return Encode("out", scale)
		End Function

		Public Function EncodeMp4(ByVal path As String, ByVal scale As Integer) As Byte()
			Return Encode(path, scale)
		End Function

		Private Function Encode(Optional ByVal path As String = "out", Optional ByVal scale As Integer = 1) As Byte()
			Try
				If Not Directory.Exists("temp") Then
					Directory.CreateDirectory("temp")
				Else
					Cleanup()
				End If

				For i As Integer = 0 To Flipnote.FrameCount - 1
					PPMRenderer.GetFrameBitmap(Flipnote.Frames(i)).Save($"temp/frame_{i}.png")
				Next i
				Dim frames = Directory.EnumerateFiles("temp").ToArray()


				File.WriteAllBytes("temp/audio.wav", Flipnote.Audio.GetWavBGM(Flipnote))

				If Not Directory.Exists(path) Then
					Directory.CreateDirectory(path)
				End If




				Utils.NumericalSort(frames)

				Dim a = FFMpegArguments.FromConcatInput(frames, Function(options) options.WithFramerate(Flipnote.Framerate)).AddFileInput("temp/audio.wav", False).OutputToFile($"{path}/{Flipnote.CurrentFilename}.mp4", True, Sub(o)
							o.Resize(256 * scale, 192 * scale).WithVideoCodec(VideoCodec.LibX264).ForcePixelFormat("yuv420p").ForceFormat("mp4")
				End Sub)

				a.ProcessAsynchronously().Wait()

				Cleanup()

				Return File.ReadAllBytes($"{path}/{Flipnote.CurrentFilename}.mp4")


			Catch e As Exception
				Cleanup()
				Return Nothing
			End Try
		End Function

		Private Sub Cleanup()
			If Not Directory.Exists("temp") Then
				Return
			End If
			Dim files = Directory.EnumerateFiles("temp")

			files.ToList().ForEach(Sub(file)
				Try
					File.Delete(file)
				Catch e As Exception
					' idk yet
				End Try

			End Sub)
			Directory.Delete("temp")
		End Sub
	End Class
End Namespace
