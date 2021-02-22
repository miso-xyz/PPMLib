Imports System.Security.Cryptography
Imports System.Drawing.Imaging
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Windows.Media.Imaging
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions

Public Class PPMFile
    Public Shared ThumbnailPaletteHex As New List(Of String) From {"#FFFFFF", "#525252", "#FFFFFF", "#9C9C9C", "#FF4844", "#C8514F", "#FFADAC", "#00FF00", "#4840FF", "#514FB8", "#ADABFF", "#00FF00", "#B657B7", "#00FF00", "#00FF00", "#00FF00"}
    Public Shared ThumbnailPaletteGDI() As System.Drawing.Color = ThumbnailPaletteHex.Select(Function(hex) System.Drawing.ColorTranslator.FromHtml(hex)).ToArray()
    Private ppm As Byte()
    Private _animationOffset As UInteger()
    Sub New(ByVal path As String)
        ppm = File.ReadAllBytes(path)
        Load()
    End Sub
    Sub New(ByVal input As Byte())
        ppm = input
        Load()
    End Sub
    Public Function ParseBinary(ByVal target As String) As Integer
        If Not Regex.IsMatch(target, "^[01]+$") Then Throw New Exception("Invalid binary characters.")
        Return Convert.ToInt32(target, 2)
    End Function
    Private Function GetBytes(ByVal offset As Integer, ByVal count As Integer) As Byte()
        Dim b_ As Byte()
        Using a_ As New MemoryStream()
            a_.Write(ppm, offset, count)
            b_ = a_.ToArray
        End Using
        Return b_
    End Function
    Private Function ReadWChars(ByVal br As BinaryReader, ByVal count As Integer)
        Return Encoding.Unicode.GetString(br.ReadBytes(2 * count))
    End Function
    Public Sub Load()
        Using br As New BinaryReader(New MemoryStream(ppm))
            If br.ReadChars(4).Equals("PARA") Then
                Throw New FileFormatException("Unexpected file format.")
            End If
            _animationDataSize = br.ReadUInt32()
            SoundDataSize = br.ReadUInt32()
            TotalFrames = br.ReadUInt16()
            If br.ReadUInt16() <> &H24 Then
                Throw New FileFormatException("Wrong format version. It should be 0x24.")
            End If
            Editable = br.ReadUInt16()
            ThumbnailFrameIndex = br.ReadUInt16()
            RootAuthor = ReadWChars(br, 11)
            ParentAuthor = ReadWChars(br, 11)
            CurrentAuthor = ReadWChars(br, 11)
            ParentAuthorID = br.ReadBytes(8)
            CurrentAuthorID = br.ReadBytes(8)
            ParentFilename = Encoding.Default.GetString(br.ReadBytes(18))
            CurrentFilename = Encoding.Default.GetString(br.ReadBytes(18))
            RootAuthorID = br.ReadBytes(8)
            RootFilenameFragment = Encoding.Default.GetString(br.ReadBytes(8))
            Timestamp = br.ReadUInt32()
            br.ReadUInt16() 'Metadata._0x9E = 
            _rawThumbnail = br.ReadBytes(1536)
            FrameOffsetTableSize = br.ReadUInt16()
            br.ReadUInt32() '0x6A2 - always 0
            AnimationFlags = br.ReadUInt16()
            br.BaseStream.Position = &H6A8
            ReDim _animationOffset(OffsetTableSize / 4)
            ReDim _frames(TotalFrames)
            For x = 0 To OffsetTableSize / 4
                _animationOffset(x) = br.ReadUInt32()
            Next
            Dim framePos0 As Long = br.BaseStream.Position
            Dim offset = &H6A8 + OffsetTableSize
            Dim len = OffsetTableSize / 4
            For x = 0 To len
                br.BaseStream.Seek(offset + _animationOffset(x), SeekOrigin.Begin)
                Frames(x) = ReadPPMFrameData(br, &H6A8 + FrameOffsetTableSize)
                Frames(x).AnimationIndex = Array.IndexOf(_animationOffset, Frames(x).StreamPosition)
                If x > 0 Then
                    Frames(x).Overwrite(Frames(x - 1))
                End If
            Next
            Return
            '
            ' Sound Decoding (WIP)
            '
            If SoundDataSize = 0 Then
                Return
            End If
            br.ReadBytes(CInt(Math.Truncate((4 - offset Mod 4) Mod 4)))
            _bgmtracksize = br.ReadUInt32()
            _se1tracksize = br.ReadUInt32()
            _se2tracksize = br.ReadUInt32()
            _se3tracksize = br.ReadUInt32()
            _currentFrameSpeed = br.ReadByte()
            _bgmFrameSpeed = br.ReadByte()
            br.ReadBytes(14)
            BGMTrack = br.ReadBytes(CInt(_bgmtracksize))
            SE1Track = br.ReadBytes(CInt(_se1tracksize))
            SE2Track = br.ReadBytes(CInt(_se2tracksize))
            SE3Track = br.ReadBytes(CInt(_se3tracksize))
            If br.BaseStream.Position = br.BaseStream.Length Then
                ' unsigned
            Else
                ' signed
            End If
        End Using
    End Sub
    Private Function ReadPPMFrameData(ByVal br As BinaryReader, ByVal count As Integer)
        br.BaseStream.Position = count
        Dim frame As New PPMFrame
        frame._streamPosition = br.BaseStream.Position
        Try
            frame._firstByteHeader = br.ReadByte()
        Catch ex As EndOfStreamException
            If frame.StreamPosition = 4288480943 Then
                Throw New Exception("Data corrupted (possible memory pit?)")
            Else
                Throw New Exception("Given flipnote is broken")
            End If
        End Try
        If frame._firstByteHeader & ParseBinary("01100000") <> 0 Then
            frame._translateX = br.ReadSByte()
            frame._translateY = br.ReadSByte()
        End If

        frame.Layer1._lineEncoding = br.ReadBytes(48)
        frame.Layer2._lineEncoding = br.ReadBytes(48)

        Dim enc1 As String = ""
        For line = 0 To 191
            Dim _byte As Integer = frame.Layer1.LinesEncoding(line)
            Select Case CInt((_byte >> (line & &H3)) * 2)
                Case 0
                    enc1 += "0"
                Case 1
                    enc1 += "1"
                    PPMLineEncDealWith4Bytes(br, frame, 1, line)
                Case 2
                    enc1 += "2"
                    PPMLineEncDealWith4Bytes(br, frame, 1, line, True)
                Case 3
                    enc1 += "3"
                    PPMLineEncDealWithRawData(br, frame, 1, line)
            End Select
            Dim _byte_ As Integer = frame.Layer2.LinesEncoding(line)
            Select Case CInt((_byte_ >> (line & &H3)) * 2)
                Case 1
                    PPMLineEncDealWith4Bytes(br, frame, 2, line)
                Case 2
                    PPMLineEncDealWith4Bytes(br, frame, 2, line, True)
                Case 3
                    PPMLineEncDealWithRawData(br, frame, 2, line)
            End Select
        Next
        Return frame
    End Function

    Private Sub PPMLineEncDealWith4Bytes(ByVal br As BinaryReader, ByVal frame As PPMFrame, ByVal layer As Integer, ByVal line As Integer, Optional ByVal inv As Boolean = False)
        Dim y As Integer = 0
        If inv Then
            For x = 0 To 256
                If layer = 1 Then
                    frame.Layer1.Pixels(line, x) = True
                Else
                    frame.Layer2.Pixels(line, x) = True
                End If
            Next
        End If
        Dim b1 = br.ReadByte, b2 = br.ReadByte, b3 = br.ReadByte, b4 = br.ReadByte
        Dim bytes As UInteger = CUInt(b1 << 24) + (CUInt(b2 << 16)) + (CUInt(b3 << 8)) + b4
        Do While bytes <> 0
            If (bytes And &H80000000UI) <> 0 Then
                Dim pixels = br.ReadByte()
                For i As Integer = 0 To 7
                    If layer = 1 Then
                        frame.Layer1.Pixels(line, y) = ((pixels >> i) And 1) = 1
                        y += 1
                    Else
                        frame.Layer2.Pixels(line, y) = ((pixels >> i) And 1) = 1
                        y += 1
                    End If
                Next i
            Else
                y += 8
            End If
            bytes <<= 1
        Loop

    End Sub

    Private Sub PPMLineEncDealWithRawData(ByVal br As BinaryReader, ByVal frame As PPMFrame, ByVal layer As Integer, ByVal line As Integer)
        Dim y As Integer = 0
        For x = 0 To 32
            Dim val As Byte = br.ReadByte()
            For x_ = 0 To 8
                If layer = 1 Then
                    frame.Layer1.Pixels(line, y) = If(((val >> x_) & 1) = 1, True, False)
                Else
                    frame.Layer2.Pixels(line, y) = If(((val >> x_) & 1) = 1, True, False)
                End If
            Next
        Next
    End Sub

    Public Function ExportFrame(ByVal frameIndex As Integer) As WriteableBitmap
        Dim palette As New BitmapPalette(New List(Of Color) From {Frames(frameIndex).PaperColorToGDIColor(), PenColorToGDIColor(Frames(frameIndex).PaperColor, Frames(frameIndex).Layer1.PenColor), PenColorToGDIColor(Frames(frameIndex).PaperColor, Frames(frameIndex).Layer2.PenColor)}) ' From {Frames(frameIndex).PaperColor, Frames(frameIndex).Frame1Color, Frames(frameIndex).Frame2Color}
        Dim bmp As New WriteableBitmap(256, 192, 96, 96, System.Windows.Media.PixelFormats.Indexed2, palette)
        Dim pixels(64 * 192) As Byte
        For x = 0 To 256
            For y = 0 To 192
                If Frames(frameIndex).Layer2.Pixels(y, x) Then
                    Dim b = 256 * y + x
                    Dim p = 3 - b Mod 4
                    b \= 4
                    pixels(b) &= CByte(Not (&H11 << (2 * p)))
                    pixels(b) = pixels(b) Or CByte(&H10 << (2 - p))
                End If
                If Frames(frameIndex).Layer1.Pixels(y, x) Then
                    Dim b = 256 * y + x
                    Dim p = 3 - b Mod 4
                    b \= 4
                    pixels(b) &= CByte(Not (&H11 << (2 * p)))
                    pixels(b) = pixels(b) Or CByte(&H1 << (2 - p))
                End If
            Next
        Next
        bmp.WritePixels(New System.Windows.Int32Rect(0, 0, 256, 192), pixels, 64, 0)
        Return bmp
    End Function

    Public Function SignFlipnote(ByVal key As String)
        key.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace(System.Environment.NewLine, "")
        Dim rsa = CreateRsaProviderFromPrivateKey(key)
        Dim hash = New SHA1CryptoServiceProvider().ComputeHash(Encoding.Default.GetBytes(key))
        Return rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"))
    End Function

    Public Function VerifyKey(ByVal key As String) As Boolean
        key.Replace("-----BEGIN RSA PRIVATE KEY-----", "").Replace("-----END RSA PRIVATE KEY-----", "").Replace(System.Environment.NewLine, "")
        Using md5_ = MD5.Create()
            If Encoding.Default.GetString(md5_.ComputeHash(Encoding.Default.GetBytes(key))) <> "7a38d03a22c7e8b50f67028afafce3cb" Then
                Return False
            Else
                Return True
            End If
        End Using
    End Function

    Private Function CreateRsaProviderFromPrivateKey(ByVal key As String) As RSACryptoServiceProvider
        Dim privatekey_bytes As Byte() = Convert.FromBase64String(key)
        Dim rsa As New RSACryptoServiceProvider()
        Dim RSAparams As New RSAParameters()
        Using br As New BinaryReader(New MemoryStream(privatekey_bytes))
            Dim bt As Byte = 0
            Dim twobytes As UShort = br.ReadUInt16()
            Select Case twobytes
                Case &H8130
                    br.ReadByte()
                Case &H8230
                    br.ReadInt16()
                Case Else
                    Throw New Exception("Unexpected format! - Expected: 0x8130, 0x8230 - Received: " & twobytes)
            End Select
            twobytes = br.ReadUInt16()
            If twobytes <> &H102 Then
                Throw New Exception("Unexpected version!")
            End If
            bt = br.ReadByte
            If bt <> &H0 Then
                Throw New Exception("Unexpected format")
            End If
            RSAparams.Modulus = br.ReadBytes(GetIntegerSize(br))
            RSAparams.Exponent = br.ReadBytes(GetIntegerSize(br))
            RSAparams.D = br.ReadBytes(GetIntegerSize(br))
            RSAparams.P = br.ReadBytes(GetIntegerSize(br))
            RSAparams.Q = br.ReadBytes(GetIntegerSize(br))
            RSAparams.DP = br.ReadBytes(GetIntegerSize(br))
            RSAparams.DQ = br.ReadBytes(GetIntegerSize(br))
            RSAparams.InverseQ = br.ReadBytes(GetIntegerSize(br))
        End Using
        rsa.ImportParameters(RSAparams)
        Return rsa
    End Function

    Public Function GetIntegerSize(ByVal r As BinaryReader) As Integer
        Dim bt As Byte = r.ReadByte()
        Dim lowbyte As Byte = &H0
        Dim highbyte As Byte = &H0
        Dim count As Integer = 0
        If bt <> &H2 Then
            Return 0
        End If
        bt = r.ReadByte()
        Select Case bt
            Case &H81
                count = r.ReadByte
            Case &H82
                highbyte = r.ReadByte
                lowbyte = r.ReadByte
                count = BitConverter.ToInt32(New Byte() {lowbyte, highbyte, &H0, &H0}, 0)
            Case Else
                count = bt
        End Select
        While r.ReadByte = &H0
            count -= 1
        End While
        r.BaseStream.Seek(-1, SeekOrigin.Current)
        Return count
    End Function

    Public Function PlaybackSpeedToMS() As Integer
        Select Case 8 - PlaybackSpeed
            Case 1
                Return 2000
            Case 2
                Return 1000
            Case 3
                Return 500
            Case 4
                Return 250
            Case 5
                Return 166
            Case 6
                Return 83
            Case 7
                Return 50
            Case 8
                Return 33
            Case Else
                Return 33
        End Select
    End Function
    Public Function PenColorToGDIColor(ByVal paperColor As PaperColor, ByVal penColor As PenColor) As Color
        Select Case penColor
            Case penColor.Blue
                Return ColorTranslator.FromHtml("#0a39ff")
            Case penColor.Red
                Return ColorTranslator.FromHtml("#ff2a2a")
            Case penColor.Inverted
                If paperColor = paperColor.Black Then
                    Return Color.White
                Else
                    Return ColorTranslator.FromHtml("#0e0e0e")
                End If
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
    Private _parentFilename As String
    Private _currentFilename As String
    Private _rootFilenameFragment As String
    Private _timeStamp As UInt32
    Private _loop As Boolean
    Private _offsettablesize As UInt16
    Private _playbackSpeed As Integer
    Private _rawThumbnail As Byte()
    Private _frameOffsetTableSize As UInt16
    Private _animationFlags As UInt16
    Private _frames As PPMFrame()
    Private _bgmtracksize As UInt32
    Private _se1tracksize As UInt32
    Private _se2tracksize As UInt32
    Private _se3tracksize As UInt32
    Private _currentFrameSpeed As Byte
    Private _bgmFrameSpeed As Byte
    Private _rawBGM As Byte()
    Private _rawSE1 As Byte()
    Private _rawSE2 As Byte()
    Private _rawSE3 As Byte()
    Private _soundDataSize As UInt32
    Private _soundEffectFlags As Byte()
    Private _signature(&H80) As Byte
    Private _animationDataSize As UInt32

    Public ReadOnly Property Signature As Byte()
        Get
            Return _signature
        End Get
    End Property
    Public ReadOnly Property BGMTrackSize As UInt32
        Get
            Return _bgmtracksize
        End Get
    End Property
    Public ReadOnly Property SE1TrackSize As UInt32
        Get
            Return _se1tracksize
        End Get
    End Property
    Public ReadOnly Property SE2TrackSize As UInt32
        Get
            Return _se2tracksize
        End Get
    End Property
    Public ReadOnly Property SE3TrackSize As UInt32
        Get
            Return _se3tracksize
        End Get
    End Property
    Public Property SoundEffectFlags As Byte()
        Get
            Return _soundEffectFlags
        End Get
        Set(ByVal value As Byte())
            _soundEffectFlags = value
        End Set
    End Property

    Public Property SoundDataSize As UInt32
        Get
            Return _soundDataSize
        End Get
        Set(ByVal value As UInt32)
            _soundDataSize = value
        End Set
    End Property

    Public Property BGMTrack As Byte()
        Get
            Return _rawBGM
        End Get
        Set(ByVal value As Byte())
            _rawBGM = value
        End Set
    End Property
    Public Property SE1Track As Byte()
        Get
            Return _rawSE1
        End Get
        Set(ByVal value As Byte())
            _rawSE1 = value
        End Set
    End Property
    Public Property SE2Track As Byte()
        Get
            Return _rawSE2
        End Get
        Set(ByVal value As Byte())
            _rawSE2 = value
        End Set
    End Property
    Public Property SE3Track As Byte()
        Get
            Return _rawSE3
        End Get
        Set(ByVal value As Byte())
            _rawSE3 = value
        End Set
    End Property
    Public Property Frames As PPMFrame()
        Get
            Return _frames
        End Get
        Set(ByVal value As PPMFrame())
            _frames = value
        End Set
    End Property
    Public ReadOnly Property Thumbnail As Bitmap
        Get
            Dim palette = ThumbnailPaletteGDI
            Dim bmp = New System.Drawing.Bitmap(64, 48)
            Dim offset As Integer = 0
            For ty As Integer = 0 To 47 Step 8
                For tx As Integer = 0 To 63 Step 8
                    bmp.SetPixel(tx, ty, Color.Red)
                    For l As Integer = 0 To 7
                        For px As Integer = 0 To 7 Step 2
                            Dim x As Integer = tx + px
                            Dim y As Integer = ty + l
                            bmp.SetPixel(x, y, palette(_rawThumbnail(offset) And &HF))
                            bmp.SetPixel(x + 1, y, palette((_rawThumbnail(offset) >> 4) And &HF))
                            offset += 1
                        Next px
                    Next l
                Next tx
            Next ty
            ' bmp.Save("result.bmp")
            Return bmp
        End Get
    End Property

    Public Property AnimationFlags As UInt16
        Get
            Return _animationFlags
        End Get
        Set(ByVal value As UInt16)
            _animationFlags = value
        End Set
    End Property

    Public Property FrameOffsetTableSize As UInt16
        Get
            Return _frameOffsetTableSize
        End Get
        Set(ByVal value As UInt16)
            _frameOffsetTableSize = value
        End Set
    End Property

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
    Public Property ParentFilename As String
        Get
            Return _parentFilename
        End Get
        Set(ByVal value As String)
            _parentFilename = value
        End Set
    End Property
    Public Property CurrentFilename As String
        Get
            Return _currentFilename
        End Get
        Set(ByVal value As String)
            _currentFilename = value
        End Set
    End Property
    Public Property RootFilenameFragment As String
        Get
            Return _rootFilenameFragment
        End Get
        Set(ByVal value As String)
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
Public Enum LineEncoding
    SkipLine = 0
    CodedLine = 1
    InvertedCodedLine = 2
    RawLineData = 3
End Enum
'Private frameIndex As Integer = -1

Public Class PPMLayer
    Private _pen As PenColor
    Private _visibility As Boolean
    Private _layerData(192, 256) As Boolean
    Public _lineEncoding(48) As Byte
    Public Property LinesEncoding(ByVal lineIndex As Integer) As LineEncoding
        Get
            'MessageBox.Show("l " + _lineEncoding.Length.ToString() + " " + (lineIndex >> 2).ToString())
            Dim _byte As Integer = _lineEncoding(lineIndex >> 2)
            Dim pos As Integer = (lineIndex And &H3) * 2
            Return CType((_byte >> pos) And &H3, LineEncoding)
        End Get
        Set(ByVal value As LineEncoding)
            Dim o As Integer = lineIndex >> 2
            Dim pos As Integer = (lineIndex And &H3) * 2
            Dim b = _lineEncoding(o)
            b = b And CByte(Not (&H3 << pos))
            b = b Or CByte(value << pos)
            _lineEncoding(o) = b
        End Set
    End Property
#Region "Line-Related Functions"
    Public Function SetLineEncodingForWholeLayer(ByVal index As Integer) As LineEncoding
        Dim _0chks = 0, _1chks = 0
        For x = 0 To 32
            Dim c = 8 * index, n0 = 0, n1 = 0
            For x_ = 0 To 8
                If Pixels(index, c + x_) Then
                    n1 += 1
                Else
                    n0 += 1
                End If
            Next
            _0chks += If(n0 = 8, 1, 0)
            _1chks += If(n1 = 8, 1, 0)
        Next
        Select Case _0chks
            Case 32
                Return LineEncoding.SkipLine
            Case 0 AndAlso _1chks = 0
                Return LineEncoding.RawLineData
            Case Else
                Return If(_0chks > _1chks, LineEncoding.CodedLine, LineEncoding.InvertedCodedLine)
        End Select
    End Function
    Private Sub InsertLineInLayer(ByVal lineData As List(Of Byte), ByVal index As Integer, ByVal layerIndex As Integer)
        Dim chks As New List(Of Byte())
        Select Case LinesEncoding(index)
            Case 0
                Return
            Case 1, 2
                Dim flag As UInteger = 0
                For x = 0 To 32
                    Dim chunk As Byte = 0
                    For x_ = 0 To 8
                        If Pixels(index, 8 * x + x_) Then
                            chunk = chunk Or CByte(1 << x_)
                        End If
                    Next
                    If chunk <> If(LinesEncoding(index) = 1, &H0, &HFF) Then
                        flag = flag Or (1UI << (31 - x))
                        chks.Add(New Byte() {chunk})
                    End If
                Next
                lineData.Add(CByte((flag And &HFF000000UI) >> 24))
                lineData.Add(CByte((flag And &HFF0000UI) >> 16))
                lineData.Add(CByte((flag And &HFF00UI) >> 8))
                lineData.Add(CByte(flag And &HFFUI))
                lineData.AddRange(chks)
                Return
            Case 3
                For x = 0 To 32
                    Dim chunk As Byte = 0
                    For x_ = 0 To 8
                        If Pixels(index, 8 * x + x_) Then
                            chunk = chunk Or CByte(1 << x_)
                        End If
                    Next
                    chks.Add(New Byte() {chunk})
                Next
        End Select
    End Sub
#End Region
    Public Property Visible As Boolean
        Get
            Return _visibility
        End Get
        Set(ByVal value As Boolean)
            _visibility = value
        End Set
    End Property
    Public Property Pixels(ByVal x As Integer, ByVal y As Integer) As Boolean
        Get
            Return _layerData(x, y)
        End Get
        Set(ByVal value As Boolean)
            _layerData(x, y) = value
        End Set
    End Property
    Public Property PenColor As PenColor
        Get
            Return _pen
        End Get
        Set(ByVal value As PenColor)
            _pen = value
        End Set
    End Property
End Class
Public Class PPMFrame

    Private _layer1 As New PPMLayer
    Private _layer2 As New PPMLayer
    Private _paperColor As PaperColor
    Private _frame As Bitmap = New Bitmap(256, 192)
    Private _animationIndex As Integer
    Public _streamPosition As Long
    Public _firstByteHeader As Byte
    Public _translateX As Integer
    Public _translateY As Integer
    Public Sub Overwrite(ByVal frame As PPMFrame)
        If _firstByteHeader & &H10000000 <> 0 Then
            Return
        End If
        For y = 0 To 192
            If y - _translateY < 0 Then
                Continue For
            ElseIf y - _translateY >= 192 Then
                Exit For
            End If
            For x = 0 To 256
                If x - _translateX < 0 Then
                    Continue For
                ElseIf x - _translateX >= 256 Then
                    Exit For
                End If
                Layer1.Pixels(x, y) = Layer1.Pixels(x, y) Xor frame.Layer1.Pixels(x - _translateX, y - _translateY)
                Layer2.Pixels(x, y) = Layer2.Pixels(x, y) Xor frame.Layer2.Pixels(x - _translateX, y - _translateY)
            Next
        Next
    End Sub
    Public Function PaperColorToGDIColor()
        Select Case PaperColor
            Case PaperColor.Black
                Return ColorTranslator.FromHtml("#0e0e0e")
            Case PaperColor.White
                Return Color.White
            Case Else
                Throw New Exception("Unknown Paper Color! - Expected: Black, White - Received: " & PaperColor)
        End Select
    End Function
    Public ReadOnly Property Layer1 As PPMLayer
        Get
            Return _layer1
        End Get
    End Property
    Public ReadOnly Property Layer2 As PPMLayer
        Get
            Return _layer2
        End Get
    End Property
    Public Property StreamPosition As Long
        Get
            Return _streamPosition
        End Get
        Set(ByVal value As Long)
            _streamPosition = value
        End Set
    End Property

    Public Property AnimationIndex As Integer
        Get
            Return _animationIndex
        End Get
        Set(ByVal value As Integer)
            _animationIndex = value
        End Set
    End Property

    Public Function ToBitmap() As Bitmap
        _frame = New Bitmap(256, 192)
        For y = 0 To 191
            For x = 0 To 255
                ' _frame.SetPixel(x,y)
                If Layer2.Pixels(y, x) Then
                    _frame.SetPixel(x, y, Color.Red)
                Else
                    If Layer1.Pixels(y, x) Then
                        _frame.SetPixel(x, y, Color.Blue)
                    Else
                        _frame.SetPixel(x, y, If(PaperColor = PaperColor.White, Color.White, Color.Black))
                    End If
                End If
            Next
        Next
        Return _frame
    End Function

    Public Property PaperColor As PaperColor
        Get
            Return _paperColor
        End Get
        Set(ByVal value As PaperColor)
            _paperColor = value
        End Set
    End Property
End Class
