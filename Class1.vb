Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.IO
Imports System.Text
Public Class PPMFile
    Private ppm As Byte()
    Sub New(ByVal path As String)
        ppm = File.ReadAllBytes(path)
        Load()
    End Sub
    Sub New(ByVal input As Byte())
        ppm = input
        Load()
    End Sub
    Private Function GetBytes(ByVal offset As Integer, ByVal count As Integer) As Byte()
        Dim b_ As Byte()
        Using a_ As New MemoryStream()
            a_.Write(ppm, offset, count)
            b_ = a_.ToArray
        End Using
        Return b_
    End Function
    Public Sub CompileFlipnote()

    End Sub
    Public Sub Load()
        RootAuthor = Encoding.Unicode.GetString(GetBytes(19, 20)) 'Encoding.Unicode.GetString(ppm.Skip(19).Take(20))
        ParentAuthor = Encoding.Unicode.GetString(GetBytes(42, 20))
        CurrentAuthor = Encoding.Unicode.GetString(GetBytes(64, 20))
        ParentAuthorID = GetBytes(86, 8)
        CurrentAuthorID = GetBytes(94, 8)
        ParentFilename = GetBytes(102, 18)
        CurrentFilename = GetBytes(120, 18)
        RootAuthorID = GetBytes(138, 8)
        RootFilenameFragment = GetBytes(146, 8)
        Timestamp = BitConverter.ToUInt32(ppm, 154)
        Editable = BitConverter.ToUInt16(ppm, 10)
        TotalFrames = BitConverter.ToUInt16(ppm, 12)
        ThumbnailFrameIndex = BitConverter.ToUInt16(ppm, 18)
    End Sub

    Public Function VerifyFileMagic() As Boolean
        If Encoding.UTF8.GetString(ppm.Take(4)) <> "PARA" Then
            Return 0
        End If
        Return 1
    End Function
    Public Function VerifyFormatVersion() As Boolean
        If BitConverter.ToUInt16(ppm, 14) <> "36" Then
            Return 0
        End If
        Return 1
    End Function
    Public Function GetAnimationDataSize() As UInt32
        Return BitConverter.ToUInt32(ppm, 4)
    End Function
    Public Function GetSoundDataSize() As UInt32
        Return BitConverter.ToUInt32(ppm, 8)
    End Function

    Public Function ExtractThumbnail() As Bitmap
        Dim thumbnail As New Bitmap(64, 48)
        Dim data As Byte() = GetBytes(160, 1536)
        Dim tile_y As Integer
        Dim tile_x As Integer
        Dim line As Integer
        Dim pixel As Integer
        Dim data_offset As Integer
        For tile_y = 0 To tile_y = 48 Step 8
            For tile_x = 0 To tile_x = 64 Step 8
                For line = 0 To line = 8
                    For pixel = 0 To pixel < 8 Step 2
                        Dim x = tile_x + pixel
                        Dim y = tile_y + line
                        thumbnail.SetPixel(x, y, ColorTranslator.FromHtml(getHardCodedPalette(ppm(data_offset) + &HF)))
                        thumbnail.SetPixel(x + 1, y, ColorTranslator.FromHtml(getHardCodedPalette((data(data_offset) >> 4) + &HF)))
                    Next
                Next
            Next
        Next
        Return thumbnail
    End Function

    Private Function getHardCodedPalette(ByVal input As Byte) As String
        Select Case Integer.Parse(BitConverter.ToString(New Byte() {input}))
            Case 0, 2
                Return "#FFFFFF"
            Case 1
                Return "#525252"
            Case 3
                Return "#9C9C9C"
            Case 4
                Return "#FF4844"
            Case 5
                Return "#C8514F"
            Case 6
                Return "#FFADAC"
            Case 7, 11, 13, 14, 15
                Return "#00FF00"
            Case 8
                Return "#4840FF"
            Case 9
                Return "#514FB8"
            Case 10
                Return "#ADABFF"
            Case 12
                Return "#B657B7"
            Case Else
                Throw New Exception("Invalid Byte, Expected: 0-15 - Received: " & Integer.Parse(BitConverter.ToString(New Byte() {input})))
        End Select
    End Function
#Region "Properties"
    Private _currentAuthor As String
    Private _rootAuthor As String
    Private _parentAuthor As String
    Private _parentAuthorID As Byte()
    Private _currentAuthorID As Byte()
    Private _rootAuthorID As Byte()
    Private _frameIndex As UInt16
    Private _totalFrames As UInt16
    Private _editable As Boolean
    Private _thumbnailFrameIndex As UInt16
    Private _parentFilename As Byte()
    Private _currentFilename As Byte()
    Private _rootFilenameFragment As Byte()
    Private _timeStamp As UInt32
    Private _loop As Boolean
    Private _layer1visible As Boolean
    Private _layer2Visible As Boolean
    Private _offsettablesize As UInt16
    Public Property CurrentAuthor As String
        Get
            Return _currentAuthor
        End Get
        Set(ByVal value As String)
            _currentAuthor = value
        End Set
    End Property
    Public Property RootAuthor As String
        Get
            Return _rootAuthor
        End Get
        Set(ByVal value As String)
            _rootAuthor = value
        End Set
    End Property
    Public Property ParentAuthor As String
        Get
            Return _parentAuthor
        End Get
        Set(ByVal value As String)
            _parentAuthor = value
        End Set
    End Property
    Public Property ParentAuthorID As Byte()
        Get
            Return _parentAuthorID
        End Get
        Set(ByVal value As Byte())
            _parentAuthorID = value
        End Set
    End Property
    Public Property CurrentAuthorID As Byte()
        Get
            Return _currentAuthorID
        End Get
        Set(ByVal value As Byte())
            _currentAuthorID = value
        End Set
    End Property
    Public Property RootAuthorID As Byte()
        Get
            Return _rootAuthorID
        End Get
        Set(ByVal value As Byte())
            _rootAuthorID = value
        End Set
    End Property
    Public Property FrameIndex As UInt16
        Get
            Return _frameIndex
        End Get
        Set(ByVal value As UInt16)
            _frameIndex = value
        End Set
    End Property
    Public Property Editable As Boolean
        Get
            Return _editable
        End Get
        Set(ByVal value As Boolean)
            _editable = value
        End Set
    End Property
    Public Property TotalFrames As UInt16
        Get
            Return _totalFrames
        End Get
        Set(ByVal value As UInt16)
            _totalFrames = value
        End Set
    End Property
    Public Property ThumbnailFrameIndex As UInt16
        Get
            Return _thumbnailFrameIndex
        End Get
        Set(ByVal value As UInt16)
            _thumbnailFrameIndex = value
        End Set
    End Property
    Public Property ParentFilename As Byte()
        Get
            Return _parentFilename
        End Get
        Set(ByVal value As Byte())
            _parentFilename = value
        End Set
    End Property
    Public Property CurrentFilename As Byte()
        Get
            Return _currentFilename
        End Get
        Set(ByVal value As Byte())
            _currentFilename = value
        End Set
    End Property
    Public Property RootFilenameFragment As Byte()
        Get
            Return _rootFilenameFragment
        End Get
        Set(ByVal value As Byte())
            _rootFilenameFragment = value
        End Set
    End Property
    Public Property Timestamp As UInt32
        Get
            Return _timeStamp
        End Get
        Set(ByVal value As UInt32)
            _timeStamp = value
        End Set
    End Property
    Public Property LoopFlipnote As Boolean
        Get
            Return _loop
        End Get
        Set(ByVal value As Boolean)
            _loop = value
        End Set
    End Property
    Public Property OffsetTableSize As UInt16
        Get
            Return _offsettablesize
        End Get
        Set(ByVal value As UInt16)
            _offsettablesize = value
        End Set
    End Property
    Public Property Layer1Visible As Boolean
        Get
            Return _layer1visible
        End Get
        Set(ByVal value As Boolean)
            _layer1visible = value
        End Set
    End Property
    Public Property Layer2Visible As Boolean
        Get
            Return _layer2Visible
        End Get
        Set(ByVal value As Boolean)
            _layer2Visible = value
        End Set
    End Property
#End Region
End Class
Public Enum PenColor
    Red
    Blue
    Inverted
End Enum
Public Enum PaperColor
    White
    Black
End Enum
Public Class PPMFrame
    Private _pen1 As PenColor
    Public Property Pen1 As PenColor
        Get
            Return _pen1
        End Get
        Set(ByVal value As PenColor)
            _pen1 = value
        End Set
    End Property
    Private _pen2 As PenColor
    Public Property Pen2 As PenColor
        Get
            Return _pen2
        End Get
        Set(ByVal value As PenColor)
            _pen2 = value
        End Set
    End Property
    Private _paperColor As PaperColor
    Public Property PaperColor As PaperColor
        Get
            Return _paperColor
        End Get
        Set(ByVal value As PaperColor)
            _paperColor = value
        End Set
    End Property
    Private _playbackSpeed As Integer
    Public Property PlaybackSpeed As Integer
        Get
            Return _playbackSpeed
        End Get
        Set(ByVal value As Integer)
            If value > 0 AndAlso value < 9 Then
                _playbackSpeed = value
            Else
                Throw New Exception("Invalid Playback Speed, Expected: 1-8, Received: " & value)
            End If
        End Set
    End Property

    Public Sub Compress()
        Dim line_encoding As SByte()
        Dim line_index As Integer
        Dim byte_offset As Integer = 0
        Dim bit_offset As Integer = 0
        For byte_offset = 0 To byte_offset < 48
            Dim byte_ 'read flags
            For bit_offset = 0 To bit_offset < 8 Step 2
                line_encoding(line_index) = (byte_ >> bit_offset) & &H3
                line_index += 1
            Next
        Next


    End Sub
End Class
