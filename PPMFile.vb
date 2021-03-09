Imports PPMLib.Extensions
Imports System
Imports System.IO
Imports System.Linq

Namespace PPMLib
	Public Class PPMFile
		''' <summary>
		''' Read file as a flipnote
		''' </summary>
		''' <param name="path">Path to Flipnote</param>
		Public Sub LoadFrom(ByVal path As String) 'private static
			Parse(File.ReadAllBytes(path))
		End Sub

		''' <summary>
		''' Parse a flipnote's raw bytes
		''' </summary>
		''' <param name="bytes">Raw Flipnote Bytes</param>
		Public Sub Parse(ByVal bytes() As Byte)
			Dim br = New BinaryReader(New MemoryStream(bytes))
			If Not br.ReadChars(4).SequenceEqual(FileMagic) Then
				Throw New FileFormatException("Unexpected file format")
			End If
			AnimationDataSize = br.ReadUInt32()
			SoundDataSize = br.ReadUInt32()
			FrameCount = CUShort(br.ReadUInt16() + 1)
			FormatVersion = br.ReadUInt16()
			IsLocked = br.ReadUInt16() <> 0
			ThumbnailFrameIndex = br.ReadUInt16()
			Dim rootname As String = br.ReadWChars(11)
			Dim parentname As String = br.ReadWChars(11)
			Dim currentname As String = br.ReadWChars(11)
			Dim parentid As ULong = br.ReadUInt64()
			Dim currentid As ULong = br.ReadUInt64()
			ParentFilename = br.ReadPPMFilename()
			CurrentFilename = br.ReadPPMFilename()
			Dim rootid As ULong = br.ReadUInt64()
			RootAuthor = New PPMAuthor(rootname, rootid)
			ParentAuthor = New PPMAuthor(parentname, parentid)
			CurrentAuthor = New PPMAuthor(currentname, currentid)
			RootFileFragment = br.ReadPPMFileFragment()
			Timestamp = br.ReadPPMTimestamp()
			br.ReadUInt16() ' 0x9E
			Thumbnail = br.ReadPPMThumbnail()
			FrameOffsetTableSize = br.ReadUInt16()
			br.ReadUInt32() '0x6A2 - always 0
			AnimationFlags = br.ReadUInt16()
			Dim oCnt = FrameOffsetTableSize / 4.0 - 1
			_animationOffset = New UInteger(CInt(Math.Truncate(oCnt))){}
			Frames = New PPMLib.PPMFrame(FrameCount - 1){}
			For x = 0 To oCnt
				_animationOffset(x) = br.ReadUInt32()
			Next x
			Dim framePos0 As Long = br.BaseStream.Position
			Dim offset = framePos0 '&H6A8 + FrameOffsetTableSize
			For x = 0 To oCnt
				If offset + _animationOffset(x) = 4288480943 Then
					Throw New Exception("Data corrupted (possible memory pit?)")
				End If
				br.BaseStream.Seek(offset + _animationOffset(x), SeekOrigin.Begin)
				Frames(x) = br.ReadPPMFrame()
				Frames(x).AnimationIndex = Array.IndexOf(_animationOffset, _animationOffset(x))
				If x > 0 Then
					Frames(x).Overwrite(Frames(x - 1))
				End If
			Next x

			' Read Sound Data
			If SoundDataSize = 0 Then
				Return
			End If

			offset = &H6A0 + AnimationDataSize
			br.BaseStream.Seek(offset, SeekOrigin.Begin)
			SoundEffectFlags = New Byte(Frames.Length - 1){}
			Audio = New PPMAudio()
			For i As Integer = 0 To Frames.Length - 1
				SoundEffectFlags(i) = br.ReadByte()
			Next i
			offset += Frames.Length

			' make the next offset dividable by 4
			br.ReadBytes(CInt((4 - offset Mod 4) Mod 4))

			Audio.SoundData = New _SoundData()
			Audio.SoundHeader = New _SoundHeader()

			Audio.SoundHeader.BGMTrackSize = br.ReadUInt32()
			Audio.SoundHeader.SE1TrackSize = br.ReadUInt32()
			Audio.SoundHeader.SE2TrackSize = br.ReadUInt32()
			Audio.SoundHeader.SE3TrackSize = br.ReadUInt32()
			Audio.SoundHeader.CurrentFramespeed = CByte(8 - br.ReadByte())
			Audio.SoundHeader.RecordingBGMFramespeed = CByte(8 - br.ReadByte())

			' 
			Framerate = PPM_FRAMERATES(Audio.SoundHeader.CurrentFramespeed)
			BGMRate = PPM_FRAMERATES(Audio.SoundHeader.RecordingBGMFramespeed)
			br.ReadBytes(14)

			Audio.SoundData.RawBGM = br.ReadBytes(CInt(Audio.SoundHeader.BGMTrackSize))
			Audio.SoundData.RawSE1 = br.ReadBytes(CInt(Audio.SoundHeader.SE1TrackSize))
			Audio.SoundData.RawSE2 = br.ReadBytes(CInt(Audio.SoundHeader.SE2TrackSize))
			Audio.SoundData.RawSE3 = br.ReadBytes(CInt(Audio.SoundHeader.SE3TrackSize))

			' Read Signature (Will implement later)
			If br.BaseStream.Position = br.BaseStream.Length Then
				' file is RSA unsigned -> do something...
			Else
				' Next 0x80 bytes = RSA-1024 SHA-1 signature
				Signature = br.ReadBytes(&H80)
				Dim padding = br.ReadBytes(&H10)
				' Next 0x10 bytes are filled with 0
			End If

		End Sub

		Friend Shared ReadOnly FileMagic() As Char = { "P"c, "A"c, "R"c, "A"c }
		Private _animationOffset() As UInteger
		Private privateAnimationDataSize As UInteger
		Public Property AnimationDataSize() As UInteger
			Get
				Return privateAnimationDataSize
			End Get
			Private Set(ByVal value As UInteger)
				privateAnimationDataSize = value
			End Set
		End Property
		Private privateSoundDataSize As UInteger
		Public Property SoundDataSize() As UInteger
			Get
				Return privateSoundDataSize
			End Get
			Private Set(ByVal value As UInteger)
				privateSoundDataSize = value
			End Set
		End Property
		Private privateFrameCount As UShort
		Public Property FrameCount() As UShort
			Get
				Return privateFrameCount
			End Get
			Private Set(ByVal value As UShort)
				privateFrameCount = value
			End Set
		End Property
		Private privateFormatVersion As UShort
		Public Property FormatVersion() As UShort
			Get
				Return privateFormatVersion
			End Get
			Private Set(ByVal value As UShort)
				privateFormatVersion = value
			End Set
		End Property
		Private privateIsLocked As Boolean
		Public Property IsLocked() As Boolean
			Get
				Return privateIsLocked
			End Get
			Private Set(ByVal value As Boolean)
				privateIsLocked = value
			End Set
		End Property
		Private privateThumbnailFrameIndex As UShort
		Public Property ThumbnailFrameIndex() As UShort
			Get
				Return privateThumbnailFrameIndex
			End Get
			Private Set(ByVal value As UShort)
				privateThumbnailFrameIndex = value
			End Set
		End Property
		Private privateRootAuthor As PPMAuthor
		Public Property RootAuthor() As PPMAuthor
			Get
				Return privateRootAuthor
			End Get
			Private Set(ByVal value As PPMAuthor)
				privateRootAuthor = value
			End Set
		End Property
		Private privateParentAuthor As PPMAuthor
		Public Property ParentAuthor() As PPMAuthor
			Get
				Return privateParentAuthor
			End Get
			Private Set(ByVal value As PPMAuthor)
				privateParentAuthor = value
			End Set
		End Property
		Private privateCurrentAuthor As PPMAuthor
		Public Property CurrentAuthor() As PPMAuthor
			Get
				Return privateCurrentAuthor
			End Get
			Private Set(ByVal value As PPMAuthor)
				privateCurrentAuthor = value
			End Set
		End Property
		Private privateParentFilename As PPMFilename
		Public Property ParentFilename() As PPMFilename
			Get
				Return privateParentFilename
			End Get
			Private Set(ByVal value As PPMFilename)
				privateParentFilename = value
			End Set
		End Property
		Private privateCurrentFilename As PPMFilename
		Public Property CurrentFilename() As PPMFilename
			Get
				Return privateCurrentFilename
			End Get
			Private Set(ByVal value As PPMFilename)
				privateCurrentFilename = value
			End Set
		End Property
		Private privateRootFileFragment As PPMFileFragment
		Public Property RootFileFragment() As PPMFileFragment
			Get
				Return privateRootFileFragment
			End Get
			Private Set(ByVal value As PPMFileFragment)
				privateRootFileFragment = value
			End Set
		End Property
		Private privateTimestamp As PPMTimestamp
		Public Property Timestamp() As PPMTimestamp
			Get
				Return privateTimestamp
			End Get
			Private Set(ByVal value As PPMTimestamp)
				privateTimestamp = value
			End Set
		End Property
		Private privateThumbnail As PPMThumbnail
		Public Property Thumbnail() As PPMThumbnail
			Get
				Return privateThumbnail
			End Get
			Private Set(ByVal value As PPMThumbnail)
				privateThumbnail = value
			End Set
		End Property
		Private privateFrameOffsetTableSize As UShort
		Public Property FrameOffsetTableSize() As UShort
			Get
				Return privateFrameOffsetTableSize
			End Get
			Private Set(ByVal value As UShort)
				privateFrameOffsetTableSize = value
			End Set
		End Property
		Private privateAnimationFlags As UShort
		Public Property AnimationFlags() As UShort
			Get
				Return privateAnimationFlags
			End Get
			Private Set(ByVal value As UShort)
				privateAnimationFlags = value
			End Set
		End Property
		Private privateFrames As PPMFrame()
		Public Property Frames() As PPMFrame()
			Get
				Return privateFrames
			End Get
			Private Set(ByVal value As PPMFrame())
				privateFrames = value
			End Set
		End Property
		Public SoundEffectFlags() As Byte
		Private privateAudio As PPMAudio
		Public Property Audio() As PPMAudio
			Get
				Return privateAudio
			End Get
			Private Set(ByVal value As PPMAudio)
				privateAudio = value
			End Set
		End Property
		Public Signature() As Byte
		Private privateFramerate As Double
		Public Property Framerate() As Double
			Get
				Return privateFramerate
			End Get
			Private Set(ByVal value As Double)
				privateFramerate = value
			End Set
		End Property
		Private privateBGMRate As Double
		Public Property BGMRate() As Double
			Get
				Return privateBGMRate
			End Get
			Private Set(ByVal value As Double)
				privateBGMRate = value
			End Set
		End Property

		Public PPM_FRAMERATES() As Double = { 30.0, 0.5, 1.0, 2.0, 4.0, 6.0, 12.0, 20.0, 30.0 }

	End Class
End Namespace
