Imports System

Namespace PPMLib.Extensions

	''' <summary>
	''' AdpcmDecoder heavily based on https://github.com/jaames/flipnote.js
	''' thank you jaames
	''' </summary>
	Public Class AdpcmDecoder
		Private Property step_index() As Integer

		Private Property Flipnote() As PPMFile

		''' <summary>
		''' Initialize Audio Decoder
		''' </summary>
		''' <param name="input">The Flipnote File as input</param>
		Public Sub New(ByVal input As PPMFile)
			Me.step_index = 0
			Me.Flipnote = input
		End Sub

		Private IndexTable() As Integer = { -1, -1, -1, -1, 2, 4, 6, 8, -1, -1, -1, -1, 2, 4, 6, 8}

		Private ADPCM_STEP_TABLE() As Integer = { 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88, 97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658, 724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767, 0 }

		''' <summary>
		''' Get decoded audio track for a given track
		''' </summary>
		''' <param name="track">Which type of Audio Track to decode</param>
		''' <returns>Signed 16-Bit PCM audio</returns>
		Private Function Decode(ByVal track As PPMAudioTrack) As Short()
			Dim sounds As _SoundData = Flipnote.Audio.SoundData
			Dim src() As Byte = Nothing
			Select Case track
				Case PPMAudioTrack.BGM
					src = sounds.RawBGM
				Case PPMAudioTrack.SE1
					src = sounds.RawSE1
				Case PPMAudioTrack.SE2
					src = sounds.RawSE2
				Case PPMAudioTrack.SE3
					src = sounds.RawSE3
				Case Else
					src = sounds.RawBGM
			End Select
			Dim srcSize = src.Length
			Dim dst = New Short((srcSize * 2) - 1){}
			Dim srcPtr = 0
			Dim dstPtr = 0
			Dim sample = 0
			step_index = 0
			Dim predictor = 0
			Dim lowNibble = True

			Do While srcPtr < srcSize
				' switch between high and low nibble each loop iteration
				' increments srcPtr after every high nibble
				If lowNibble Then
					sample = src(srcPtr) And &HF
				Else
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: sample = src[srcPtr++] >> 4;
					sample = src(srcPtr) >> 4
					srcPtr += 1
				End If
				lowNibble = Not lowNibble
				Dim [step] = ADPCM_STEP_TABLE(step_index)
				Dim diff = [step] >> 3

				If (sample And 1) <> 0 Then
					diff += [step] >> 2
				End If
				If (sample And 2) <> 0 Then
					diff += [step] >> 1
				End If
				If (sample And 4) <> 0 Then
					diff += [step]
				End If
				If (sample And 8) <> 0 Then
					diff = -diff
				End If
				predictor += diff
				predictor = Utils.NumClamp(predictor, -32768, 32767)

				step_index += IndexTable(sample)
				step_index = Utils.NumClamp(step_index, 0, 88)
'INSTANT VB WARNING: An assignment within expression was extracted from the following statement:
'ORIGINAL LINE: dst[dstPtr++] = (short)predictor;
				dst(dstPtr) = CShort(predictor)
				dstPtr += 1

			Loop

			Return dst
		End Function

		''' <summary>
		''' Get decoded audio track for the BGM using a specified samplerate. 
		''' could probably work with different samplerates but i don't know why you'd try
		''' </summary>
		''' <param name="dstFreq"></param>
		''' <param name="track">The type of Audio Track</param>
		''' <returns>Signed 16-Bit PCM audio</returns>
		Public Function getAudioTrackPcm(ByVal dstFreq As Integer, ByVal track As PPMAudioTrack) As Short()
			Dim srcPcm = Decode(track)
			Dim srcFreq = 8192
			Dim soundspeed As Double = Flipnote.BGMRate
			Dim framerate As Double = Flipnote.Framerate

			If track = PPMAudioTrack.BGM Then


				Dim bgmAdjust = (1.0 / soundspeed) / (1.0 / framerate)
				srcFreq = (CInt(Math.Truncate(srcFreq * bgmAdjust)))




			End If
			If CInt(srcFreq) <> dstFreq Then
				Return pcmResampleNearestNeighbour(srcPcm, srcFreq, dstFreq)
			End If
			Return srcPcm

		End Function

		''' <summary>
		''' Mixes two tracks together at the given offset
		''' </summary>
		''' <param name="src">The Audio to add</param>
		''' <param name="dst">The output</param>
		''' <param name="dstOffset">The Offset</param>
		''' <returns>Signed 16-bit PCM audio</returns>
		Private Function pcmAudioMix(ByVal src() As Short, ByVal dst() As Short, Optional ByVal dstOffset As Integer = 0) As Short()
			Dim srcSize = src.Length
			Dim dstSize = dst.Length

			For i As Integer = 0 To srcSize - 1
				If dstOffset + i > dstSize Then
					Exit For
				End If
				'half src volume
				Dim samp As Integer = 0
				Try
					samp = dst(dstOffset + i) + (src(i) \ 2)
					dst(dstOffset + i) = CShort(Utils.NumClamp(samp, -32768, 32767))
				Catch e As Exception

				End Try


			Next i
			Return dst
		End Function


		''' <summary>
		''' Get the full mixed audio for the Flipnote, using the specified samplerate
		''' </summary>
		''' <param name="flip">The Flipnote</param>
		''' <param name="dstFreq">16384 is recommended</param>
		''' <returns>Signed 16-bit PCM audio</returns>
		Public Function getAudioMasterPcm(ByVal dstFreq As Integer) As Short()
			Dim dstSize = CInt(Math.Truncate(Math.Ceiling(timeGetNoteDuration(Flipnote.FrameCount, Flipnote.Framerate) * dstFreq)))
			Dim master = New Short(dstSize){}
			Dim hasBgm = Flipnote.Audio.SoundHeader.BGMTrackSize > 0
			Dim hasSe1 = Flipnote.Audio.SoundHeader.SE1TrackSize > 0
			Dim hasSe2 = Flipnote.Audio.SoundHeader.SE2TrackSize > 0
			Dim hasSe3 = Flipnote.Audio.SoundHeader.SE3TrackSize > 0

			' Mix background music
			If hasBgm Then
				Dim bgmPcm = getAudioTrackPcm(dstFreq, PPMAudioTrack.BGM)
				master = pcmAudioMix(bgmPcm, master, 0)
			End If

			If hasSe1 OrElse hasSe2 OrElse hasSe3 Then
				Dim samplesPerFrame = dstFreq / Flipnote.Framerate
				Dim se1Pcm = If(hasSe1, getAudioTrackPcm(dstFreq, PPMAudioTrack.SE1), Nothing)
				Dim se2Pcm = If(hasSe1, getAudioTrackPcm(dstFreq, PPMAudioTrack.SE2), Nothing)
				Dim se3Pcm = If(hasSe1, getAudioTrackPcm(dstFreq, PPMAudioTrack.SE3), Nothing)
				Dim seFlags = Flipnote.SoundEffectFlags
				For i As Integer = 0 To Flipnote.FrameCount - 1
					Dim seOffset = CInt(Math.Truncate(Math.Ceiling(i * samplesPerFrame)))
					Dim flag = seFlags(i)
					If hasSe1 AndAlso flag = 1 Then
						master = pcmAudioMix(se1Pcm, master, seOffset)
					End If
					If hasSe2 AndAlso flag = 2 Then
						master = pcmAudioMix(se2Pcm, master, seOffset)
					End If
					If hasSe3 AndAlso flag = 4 Then
						master = pcmAudioMix(se3Pcm, master, seOffset)
					End If
				Next i
			End If


			Return master
		End Function

		''' <summary>
		''' Returns the duration of a Flipnote
		''' </summary>
		''' <param name="frameCount"></param>
		''' <param name="framerate"></param>
		''' <returns></returns>
		Public Function timeGetNoteDuration(ByVal frameCount As Integer, ByVal framerate As Double) As Double
			Return ((frameCount * 100) * (1 / framerate)) / 100
		End Function

		''' <summary>
		''' Return the sample at the specified position
		''' </summary>
		''' <param name="src">source audio</param>
		''' <param name="srcSize">the size of the source</param>
		''' <param name="srcPtr">the position</param>
		''' <returns></returns>
		Private Function pcmGetSample(ByVal src() As Short, ByVal srcSize As Integer, ByVal srcPtr As Integer) As Short
			If srcPtr < 0 OrElse srcPtr >= srcSize Then
				Return 0
			End If
			Return src(srcPtr)
		End Function

		''' <summary>
		''' Zero-order hold (nearest neighbour) audio interpolation.
		''' Credit to SimonTime for the original C version.
		''' </summary>
		''' <param name="src"></param>
		''' <param name="srcFreq"></param>
		''' <param name="dstFreq"></param>
		''' <returns>Resampled Signed 16-bit PCM audio</returns>
		Private Function pcmResampleNearestNeighbour(ByVal src() As Short, ByVal srcFreq As Double, ByVal dstFreq As Integer) As Short()
			Dim srcLength = src.Length
			Dim srcDuration = srcLength / srcFreq
			Dim dstLength = srcDuration * dstFreq
			Dim dst = New Short(CInt(Math.Truncate(dstLength)) - 1){}
			Dim adjFreq = srcFreq / dstFreq
			For dstPtr = 0 To dstLength - 1
				dst(dstPtr) = pcmGetSample(src, srcLength, CInt(Math.Truncate(Math.Floor(CDbl(dstPtr * adjFreq)))))
			Next dstPtr
			Return dst
		End Function

		''' <summary>
		''' Unused Linear interpolation
		''' </summary>
		''' <param name="src"></param>
		''' <param name="srcFreq"></param>
		''' <param name="dstFreq"></param>
		''' <returns>Resampled Signed 16-bit PCM audio</returns>
		Private Function pcmResampleLinear(ByVal src() As Short, ByVal srcFreq As Double, ByVal dstFreq As Integer) As Short()
			Dim srcLength = src.Length
			Dim srcDuration = srcLength / srcFreq
			Dim dstLength = srcDuration * dstFreq
			Dim dst = New Short(CInt(Math.Truncate(dstLength)) - 1){}
			Dim adjFreq = srcFreq / dstFreq

			Dim adj As Integer = 0
			Dim srcPtr As Integer = 0
			Dim weight As Integer = 0

			For dstPtr As Integer = 0 To dstLength - 1
				adj = CInt(Math.Truncate(dstPtr * adjFreq))
				srcPtr = CInt(Math.Truncate(Math.Floor(CDbl(adj))))
				weight = adj Mod 1
				dst(dstPtr) = CShort((1 - weight) * pcmGetSample(src, srcLength, srcPtr) + weight * pcmGetSample(src, srcLength, srcPtr + 1))
			Next dstPtr
			Return dst
		End Function
	End Class
End Namespace
