Imports System
Imports System.Runtime.InteropServices

Namespace PPMLib.Extensions
	<StructLayout(LayoutKind.Sequential, Pack := 1)>
	Public Class WavePcmFormat
		' ChunkID          Contains the letters "RIFF" in ASCII form 
		<MarshalAs(UnmanagedType.ByValArray, SizeConst := 4)>
		Private chunkID() As Char = { "R"c, "I"c, "F"c, "F"c }

		' ChunkSize        36 + SubChunk2Size 
		<MarshalAs(UnmanagedType.U4, SizeConst := 4)>
		Private chunkSize As UInteger = 0

		' Format           The "WAVE" format name 
		<MarshalAs(UnmanagedType.ByValArray, SizeConst := 4)>
		Private format() As Char = { "W"c, "A"c, "V"c, "E"c }

		' Subchunk1ID      Contains the letters "fmt " 
		<MarshalAs(UnmanagedType.ByValArray, SizeConst := 4)>
		Private subchunk1ID() As Char = { "f"c, "m"c, "t"c, " "c }

		' Subchunk1Size    16 for PCM 
		<MarshalAs(UnmanagedType.U4, SizeConst := 4)>
		Private subchunk1Size As UInteger = 16

		' AudioFormat      PCM = 1 (i.e. Linear quantization) 
		<MarshalAs(UnmanagedType.U2, SizeConst := 2)>
		Private audioFormat As UShort = 1

		' NumChannels      Mono = 1, Stereo = 2, etc. 
'INSTANT VB NOTE: The field numChannels was renamed since Visual Basic does not allow fields to have the same name as other class members:
		<MarshalAs(UnmanagedType.U2, SizeConst := 2)>
		Private numChannels_Conflict As UShort = 1
		Public Property NumChannels() As UShort
			Get
				Return numChannels_Conflict
			End Get
			Set(ByVal value As UShort)
				numChannels_Conflict = value
			End Set
		End Property

		' SampleRate       8000, 44100, etc. 
'INSTANT VB NOTE: The field sampleRate was renamed since Visual Basic does not allow fields to have the same name as other class members:
		<MarshalAs(UnmanagedType.U4, SizeConst := 4)>
		Private sampleRate_Conflict As UInteger = 44100
		Public Property SampleRate() As UInteger
			Get
				Return sampleRate_Conflict
			End Get
			Set(ByVal value As UInteger)
				sampleRate_Conflict = value
			End Set
		End Property

		' ByteRate         == SampleRate * NumChannels * BitsPerSample/8 
		<MarshalAs(UnmanagedType.U4, SizeConst := 4)>
		Private byteRate As UInteger = 0

		' BlockAlign       == NumChannels * BitsPerSample/8 
		<MarshalAs(UnmanagedType.U2, SizeConst := 2)>
		Private blockAlign As UShort = 0

		' BitsPerSample    8 bits = 8, 16 bits = 16, etc. 
'INSTANT VB NOTE: The field bitsPerSample was renamed since Visual Basic does not allow fields to have the same name as other class members:
		<MarshalAs(UnmanagedType.U2, SizeConst := 2)>
		Private bitsPerSample_Conflict As UShort = 8
		Public Property BitsPerSample() As UShort
			Get
				Return bitsPerSample_Conflict
			End Get
			Set(ByVal value As UShort)
				bitsPerSample_Conflict = value
			End Set
		End Property

		' Subchunk2ID      Contains the letters "data" 
		<MarshalAs(UnmanagedType.ByValArray, SizeConst := 4)>
		Private subchunk2ID() As Char = { "d"c, "a"c, "t"c, "a"c }

		' Subchunk2Size    == NumSamples * NumChannels * BitsPerSample/8 
		<MarshalAs(UnmanagedType.U4, SizeConst := 4)>
		Private subchunk2Size As UInteger = 0

		' Data             The actual sound data. 
		Public Property Data() As Byte(-1)

		Public Sub New(ByVal data() As Byte, Optional ByVal numChannels As UShort = 1, Optional ByVal sampleRate As UInteger = 8192, Optional ByVal bitsPerSample As UShort = 16)
			Me.Data = data
			Me.NumChannels = numChannels
			Me.SampleRate = sampleRate
			Me.BitsPerSample = bitsPerSample
		End Sub

		Private Sub CalculateSizes()
			subchunk2Size = CUInt(Data.Length)
			blockAlign = CUShort(NumChannels * BitsPerSample \ 8)
			byteRate = SampleRate * NumChannels * BitsPerSample \ 8
			chunkSize = 36 + subchunk2Size
		End Sub

		Public Function ToBytesArray() As Byte()
			CalculateSizes()
			Dim headerSize As Integer = Marshal.SizeOf(Me)
			Dim headerPtr As IntPtr = Marshal.AllocHGlobal(headerSize)
			Marshal.StructureToPtr(Me, headerPtr, False)
			Dim rawData((headerSize + Data.Length) - 1) As Byte
			Marshal.Copy(headerPtr, rawData, 0, headerSize)
			Marshal.FreeHGlobal(headerPtr)
			Array.Copy(Data, 0, rawData, 44, Data.Length)
			Return rawData
		End Function
	End Class
End Namespace
