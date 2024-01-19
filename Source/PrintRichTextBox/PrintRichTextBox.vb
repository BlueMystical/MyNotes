Imports System.Windows.Forms
Imports System.Drawing
Imports System.Runtime.InteropServices
Imports System.Drawing.Printing
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Drawing.Imaging
Imports System.IO

Public Module PrintRichTextBox
	'   private components

	'      constants
	Private Const WM_USER As Int32 = &H400&
	Private Const EM_FORMATRANGE As Int32 = WM_USER + 57, _
		EM_GETSCROLLPOS As Integer = WM_USER + 221, _
		EM_SETSCROLLPOS As Integer = WM_USER + 222
	Private Const EM_GETCHARFORMAT As Integer = WM_USER + 58, _
		EM_SETCHARFORMAT As Integer = WM_USER + 68
   Private Const EM_GETPARAFORMAT As Integer = WM_USER + 61, _
		EM_SETPARAFORMAT As Integer = WM_USER + 71, _
		EM_SETYPOGRAPHYOPTIONS As Integer = WM_USER + 202
	Private Const TO_ADVANCEDTYPOGRAPHY As Integer = 1
   Private Const PFM_NUMBERING As Integer = &H20, PFM_NUMBERINGSTYLE As Integer = &H2000, _
		PFM_NUMBERINGTAB As Integer = &H4000, PFM_NUMBERINGSTART As Integer = &H8000
	Private Const PFM_ALLNUMBERING = _
		PFM_NUMBERING Or PFM_NUMBERINGSTART Or PFM_NUMBERINGSTYLE Or PFM_NUMBERINGTAB
	Private Const PFM_ALIGNMENT As Integer = &H8
   Private Const CFM_SUBORSUPERSCRIPT As Integer = CFE_SUBSCRIPT Or CFE_SUPERSCRIPT
   Private Const CFE_SUBSCRIPT As Integer = &H10000, _
		CFE_SUPERSCRIPT As Integer = &H20000 'superscript and subscript are mutually exclusive
	Private Const SCF_SELECTION As Integer = 1
	Private USHORT_MASK As Integer = &HFFFF, STYLE_FACTOR As Integer = &H100
   Private Const MAX_TAB_STOPS As Integer = 32
	Private Const WM_SETREDRAW As Integer = &HB
	Private Const MM_ISOTROPIC As Integer = 7 'keep 1:1 aspect ratio
	Private Const MM_ANISOTROPIC As Integer = 8 'adjust x and y separately
	Private Const HMM_PER_INCH As Integer = 2540 'himetrics per inch
	Private Const TWIPS_PER_INCH As Integer = 1440 'twips per inch

	Private Const ProtectedTextSyntax As String = "(^|[^\\])(\\\\)*\\protect[^0]", _
		HiddenTextSyntax As String = "(^|[^\\])(\\\\)*\\v[^0]"


	'      variables
	Private _RichTextBox As System.Windows.Forms.RichTextBox
	Private _TextLength As Integer
	Private _PrintDocument As PrintDocument
	Private _CurrentPage As Integer
	Private _PageIndexes() As Integer, _PageCount As Integer
	Private _PagesToPrint() As Integer
	Private _AreSettingWidth As Boolean = False

	Private ProtectedTextChecker As Regex = _
			New Regex(ProtectedTextSyntax, _
				RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.ExplicitCapture), _
		HiddenTextChecker As Regex = _
			New Regex(HiddenTextSyntax, _
				RegexOptions.IgnoreCase Or RegexOptions.Compiled Or RegexOptions.ExplicitCapture)


	'      enums

	'         options for call to EmfToWmfBits()
	<Flags()> _
	Private Enum EmfToWmfBitsFlags
		EmfToWmfBitsFlagsDefault = &H0
		EmfToWmfBitsFlagsEmbedEmf = &H1
		EmfToWmfBitsFlagsIncludePlaceable = &H2 'place header into WMF so that it's placeable
		EmfToWmfBitsFlagsNoXORClip = &H4 'no clipping
	End Enum



	'      structures

	<StructLayout(LayoutKind.Sequential)> _
	Private Structure Rect
		Public left As Int32
		Public top As Int32
		Public right As Int32
		Public bottom As Int32
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Private Structure CharRange
		Public cpMin As Int32
		Public cpMax As Int32
	End Structure

	<StructLayout(LayoutKind.Sequential)> _
	Private Structure FormatRangeStructure
		Public hdc As IntPtr
		Public hdcTarget As IntPtr
		Public rc As Rect
		Public rcPage As Rect
		Public chrg As CharRange
	End Structure

	'      classes

	<StructLayout(LayoutKind.Sequential)> _
	Private Class CHARFORMAT2
		Public cbSize As Integer = Marshal.SizeOf(GetType(CHARFORMAT2))
		Public dwMask As UInt32
		Public dwEffects As UInt32
		Public yHeight As Int32
		Public yOffset As Int32
		Public crTextColor As Int32
		Public bCharSet As Byte
		Public bPitchAndFamily As Byte
		<MarshalAs(UnmanagedType.ByValArray, SizeConst:=32)> _
		Public szFaceName() As Char
		Public wWeight As UInt16
		Public sSpacing As UInt16
		Public crBackColor As Integer ' Color.ToArgb() -> int
		Public lcid As Integer
		Public dwReserved As Integer
		Public sStyle As Int16
		Public wKerning As Int16
		Public bUnderlineType As Byte
		Public bAnimation As Byte
		Public bRevAuthor As Byte
		Public bReserved1 As Byte
	End Class

   <StructLayout(LayoutKind.Sequential)> _
   Public Class PARAFORMAT2
      Public cbSize As Integer = Marshal.SizeOf(GetType(PARAFORMAT2))
      Public dwMask As UInteger
      Public wNumbering As UShort = CType(RTBListStyle.Numbers, UShort)
      Public wEffects As Short = 0
      Public dxStartIndent As Integer
      Public dxRightIndent As Integer
      Public dxOffset As Integer
      Public wAlignment As UShort
      Public cTabCount As Short
      <MarshalAs(UnmanagedType.ByValArray, SizeConst:=MAX_TAB_STOPS)> _
      Public rgxTabs As Integer()
      Public dySpaceBefore As Integer
      Public dySpaceAfter As Integer
      Public dyLineSpacing As Integer
      Public sStyle As Short
      Public bLineSpacingRule As Byte
      Public bOutlineLevel As Byte
      Public wShadingWeight As Short
      Public wShadingStyle As Short
      Public wNumberingStart As UShort = 1US
      Public wNumberingStyle As UShort = _
			CType(RTBNumberingSyntax.Period * STYLE_FACTOR, UShort)
      Public wNumberingTab As UShort = 0US
      Public wBorderSpace As Short
      Public wBorderWidth As Short
      Public wBorders As Short
   End Class



	'      DLL calls

	'         for printing
	<DllImport("user32.dll")> _
	Private Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Int32, _
		ByVal wParam As Int32, ByVal lParam As IntPtr) As Int32
	End Function

	'         for scrolling
	<DllImport("user32.dll")> _
	Private Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Int32, _
		ByVal wParam As Int32, ByRef lParam As Point) As Int32
	End Function
	<DllImport("user32.dll", EntryPoint:="GetScrollInfo")> _
	Private Function GetScrollInfo(ByVal hwnd As IntPtr, _
			ByVal nBar As Integer, ByRef lpsi As ScrollInfo) _
		As <MarshalAs(UnmanagedType.Bool)> Boolean
	End Function

	'         for list formatting
   <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
   Private Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As Integer, _
      <[In], Out, MarshalAs(UnmanagedType.LPStruct)> ByVal lParam As PARAFORMAT2) As IntPtr
   End Function

	'         for superscript and subscript
   <DllImport("User32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)> _
   Private Function SendMessage(hWnd As IntPtr, msg As Integer, wParam As Integer, _
      <[In], Out, MarshalAs(UnmanagedType.LPStruct)> ByVal lParam As CHARFORMAT2) As IntPtr
   End Function

	'         for auto-redraw setting and alignment
	<DllImport("user32.dll")> _
	Private Function SendMessage(ByVal hWnd As IntPtr, ByVal msg As Int32, _
		ByVal wParam As Integer, ByVal lParam As Integer) As Int32
	End Function

	'         for image insertion
	<DllImportAttribute("gdiplus.dll")> _
	Private Function GdipEmfToWmfBits(ByVal mfHandle As IntPtr, ByVal _bufferSize As UInteger, ByVal _buffer() As Byte, ByVal _mappingMode As Integer, ByVal _flags As EmfToWmfBitsFlags) As UInteger
	End Function
	<DllImport("gdi32.dll")> _
	Private Function DeleteEnhMetaFile(ByVal hemf As IntPtr) As Boolean
	End Function


	'      private procedures

	'         printout procedures

	Private Sub RightMarginFromPrinterWidth( _
		RichTextBox As RichTextBox, PageSettings As PageSettings)
	'   set .RightMargin property of RichTextBox to width of printer in PageSettings
	Dim PageWidth As Integer
	'   get page width in 1/100's of inches 
	With PageSettings
		PageWidth = .Bounds.Width - .Margins.Left - .Margins.Right
	End With
	'   set rich-textbox's .RightMargin property
	With RichTextBox
		.RightMargin = CType(PageWidth * .CreateGraphics.DpiX / 100R, Integer)
	End With
	End Sub

	Private Sub PrinterWidthFromRightMargin( _
		RichTextBox As RichTextBox, PageSettings As PageSettings)
	'   set printer width in PageSettings to .RightMargain property of RichTextBox
	Dim PageWidth As Integer
	'   get rich-textbox's .RightMargin property in 1/100's of inches 
	With RichTextBox
		PageWidth = CType(.GetMaximumWidth() * 100R / .CreateGraphics.DpiX, Integer)
	End With
	'   set left- and right-hand margin of printer page
	With PageSettings
		Dim Difference As Integer = .Bounds.Width - PageWidth
		.Margins.Left = Difference \ 2 : .Margins.Right = Difference - .Margins.Left
	End With
	End Sub

	Private Function FormatRange(ByVal Graphics As Graphics, _
		ByVal PageSettings As PageSettings, ByVal charFrom As Integer, _
		ByVal charTo As Integer, ByVal RenderText As Boolean) As Integer
	With PageSettings
		'   define character range   
		Dim cr As CharRange
		cr.cpMin = charFrom : cr.cpMax = charTo
		'   define margins
		Dim rc As Rect
		rc.top = HundredthInchToTwips(.Bounds.Top + .Margins.Top)
		rc.bottom = HundredthInchToTwips(.Bounds.Bottom - .Margins.Bottom)
		rc.left = HundredthInchToTwips(.Bounds.Left + .Margins.Left)
		rc.right = HundredthInchToTwips(.Bounds.Right - .Margins.Right)
		'   define page size
		Dim rcPage As Rect
		rcPage.top = HundredthInchToTwips(.Bounds.Top)
		rcPage.bottom = HundredthInchToTwips(.Bounds.Bottom)
		rcPage.left = HundredthInchToTwips(.Bounds.Left)
		rcPage.right = HundredthInchToTwips(.Bounds.Right)
		'   handle device context
		Dim hdc As IntPtr = Graphics.GetHdc
		'   handle full format info
		Dim fr As FormatRangeStructure
		fr.chrg = cr : fr.hdc = hdc
		fr.hdcTarget = hdc : fr.rc = rc
		fr.rcPage = rcPage
		'   prepare to render/measure page   
		Dim wParam As Int32
		If RenderText Then
			wParam = 1 'render text
		 Else
			wParam = 0 'measure only
		End If
		Dim lParam As IntPtr
		lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr))
		Marshal.StructureToPtr(fr, lParam, False)
		'   render/measure page and return first char of next page
		Dim res As Integer = _
			SendMessage(_RichTextBox.Handle, EM_FORMATRANGE, wParam, lParam)
		Marshal.FreeCoTaskMem(lParam)
		Graphics.ReleaseHdc(hdc)
		Return res
	End With
	End Function

	Private Function HundredthInchToTwips(ByVal n As Integer) As Int32
	'   convert units
	Return Convert.ToInt32((n * TWIPS_PER_INCH) \ 100)
	End Function

	Private Sub FormatRangeDone()
	'   flag printing done
	Dim lParam As New IntPtr(0)
	SendMessage(_RichTextBox.Handle, EM_FORMATRANGE, 0, lParam)
	End Sub

	Private Sub GetPageIndexes()
	'   determine indexes for page beginnings, based on printer info
	_PageCount = 0
	Dim FirstCharOnPage As Integer = 0
	Do
		'   store index for start of current page
		ReDim Preserve _PageIndexes(_PageCount)
		_PageIndexes(_PageCount) = FirstCharOnPage
		'   measure current page
		FirstCharOnPage = _
			FormatRange(_PrintDocument.PrinterSettings.CreateMeasurementGraphics, _
				_PrintDocument.DefaultPageSettings, _
				FirstCharOnPage, _TextLength, False)
		'   prepare for next page
		_PageCount += 1
	Loop While FirstCharOnPage < _TextLength
	End Sub

	Private Function GetPageNumber(ByVal Position As Integer) As Integer
	'   search for page containing a given caret Position
	Dim PageNumber As Integer = Array.BinarySearch(_PageIndexes, Position)
	If PageNumber < 0 Then
		PageNumber = (PageNumber Xor -1) 'caret is inside of page
	 Else
		PageNumber += 1                  'caret is at beginning of page
	End If
	Return PageNumber
	End Function

	Private Function PrintSetUp(ByVal RichTextBox As RichTextBox, _
		ByVal PrintDocument As PrintDocument, ByVal PageIndexesOnly As Boolean, _
	WhichPagesToPrint As WhichPages, PageList() As Integer) As Boolean
	'   prepare for print job
	_RichTextBox = RichTextBox : _TextLength = RichTextBox.TextLength
	_PrintDocument = PrintDocument : GetPageIndexes()
	_PagesToPrint = Nothing
	If PageIndexesOnly Then
		Return True 'leave with page indexes
	End If
	'   else prepare to preview/print
	With PrintDocument
		'   determine which pages to print/preview
		Dim PrintablePages As List(Of Integer) = New List(Of Integer)
		With .PrinterSettings
				Select Case WhichPagesToPrint
				Case WhichPages.PrintSpecifiedPages
					'   print specified pages (skip the rest)
					For page As Integer = 1 To _PageCount
						If Array.IndexOf(Of Integer)(PageList, page) > -1 _
								AndAlso Not PrintablePages.Contains(page) Then
							PrintablePages.Add(page)
						End If
					Next page
				Case WhichPages.SkipSpecifiedPages
					'   skip specified pages (print the rest)
					For page As Integer = 1 To _PageCount
						If Array.IndexOf(Of Integer)(PageList, page) = -1 _
								AndAlso Not PrintablePages.Contains(page) Then
							PrintablePages.Add(page)
						End If
					Next page
				Case Else
					'   else print pages in a range
					Dim _FromPage, _ToPage As Integer
					Select Case .PrintRange
						Case PrintRange.AllPages
							'   all pages
							_FromPage = 1 : _ToPage = _PageCount
						Case PrintRange.SomePages
							'   range of pages
							_FromPage = .FromPage : _ToPage = .ToPage
							If _FromPage > Math.Min(_PageCount, .MaximumPage) _
									OrElse _ToPage < Math.Max(1, .MinimumPage) Then
								Return False 'invalid range
							End If
							'   normalize range
							If _FromPage < Math.Max(1, .MinimumPage) Then
								_FromPage = Math.Max(1, .MinimumPage)
							End If
							If _ToPage > Math.Min(_PageCount, .MaximumPage) Then
								_ToPage = Math.Min(_PageCount, .MaximumPage)
							End If
						Case PrintRange.Selection
							'   pages of selected text
							_FromPage = GetPageNumber(_RichTextBox.SelectionStart)
							If _RichTextBox.SelectionLength = 0 Then
								Return False 'no selection
							 Else
								_ToPage = GetPageNumber(_RichTextBox.SelectionStart _
									+ _RichTextBox.SelectionLength - 1)
							End If
						Case Else
							'   page at caret position
							_FromPage = GetPageNumber(_RichTextBox.SelectionStart)
							_ToPage = _FromPage
					End Select
					'   check to see if we are to print every other page in range
					For page As Integer = _FromPage To _ToPage
						Select Case WhichPagesToPrint
							Case WhichPages.OddPagesInRange
								'   odd-numbered pages only
								If (page Mod 2) = 1 Then
									PrintablePages.Add(page)
								End If
							Case WhichPages.EvenPagesInRange
								'   even-numbered pages only
								If (page Mod 2) = 0 Then
									PrintablePages.Add(page)
								End If
							Case Else
								'   all pages in range
								PrintablePages.Add(page)
						End Select
					Next page
			End Select
		End With
		If _PageCount > 0 Then
			'   pages to print -- wire up events
			'      (RemoveHandler is used before AddHandler to guard against double-firing
			'         of events in the event this routine is called multiple times)
			RemoveHandler .BeginPrint, AddressOf BeginPrint 'remove any pre-existing handler
			AddHandler .BeginPrint, AddressOf BeginPrint    'add a new handler
			RemoveHandler .PrintPage, AddressOf PrintPage
			AddHandler .PrintPage, AddressOf PrintPage
			RemoveHandler .EndPrint, AddressOf EndPrint
			AddHandler .EndPrint, AddressOf EndPrint
			PrintablePages.Sort() 'shouldn't be necessary
			_PagesToPrint = PrintablePages.ToArray() : _PageCount = PrintablePages.Count
			Return True
		 Else
			_PagesToPrint = Nothing : Return False 'no pages to print
		End If
	End With
	End Function

	'         scrolling/text-width procedures

	Private Function GetFontAtCharacterPosition( _
		RichTextBox As RichTextBox, CharPos As Integer) As Font
	'   get font at specified character position
	'   NOTE: Selection isn't (temporarily) changed if it's already where it needs to be,
	'      lest recursion result if GetRightMostCharacterPosition is called within
	'      the control's SelectionChanged event and recursion results!
	With RichTextBox
		Dim CharFont As Font = Nothing
		If .SelectionStart = CharPos AndAlso .SelectionLength = 0 Then
			'   temporarily change selection
			Dim CurrentSS As Integer = .SelectionStart, _
				CurrentSL As Integer = .SelectionLength, _
				ScrollPosition As Point = .GetScrollPosition()
			.SetRedrawMode(False) : .Select(CharPos, 0)
			CharFont = .SelectionFont : .Select(CurrentSS, CurrentSL)
			.SetScrollPosition(ScrollPosition) : .SetRedrawMode(True)
		 Else
			CharFont = .SelectionFont
		End If
		Return CharFont
	End With
	End Function

	Private Function GetRightMostCharacterPosition(RichTextBox As RichTextBox) As Integer
	'   get width of widest line in text
	Dim RightMostPos As Integer = 0
	Dim LineLength, LineWidth, LineIndex, CharIndex, CharWidth As Integer
	Dim Line As String, Character As Char
	Dim CharFont As Font = Nothing
	With RichTextBox
		'   go through each line
		For LineNumber As Integer = 0 To .Lines.GetUpperBound(0)
			'   how long is this line?
			Line = .Lines(LineNumber) : LineLength = Line.Length
			If LineLength > 0 Then
				'   get rightmost character in line
				LineIndex = _
					.GetFirstCharIndexFromLine(LineNumber)
				CharIndex = LineIndex + LineLength - 1
				Character = Line.Chars(LineLength - 1)
				'   get width up to rightmost char
				LineWidth = .GetPositionFromCharIndex(CharIndex).X _
					- .GetPositionFromCharIndex(LineIndex).X
				'   get size of char
				CharFont = GetFontAtCharacterPosition(RichTextBox, CharIndex + 1)
				Using g As Graphics = .CreateGraphics()
					CharWidth = _
						CType(.CreateGraphics.MeasureString(Character, CharFont).Width _
							* RichTextBox.ZoomFactor, Integer)
				End Using
				LineWidth += CharWidth
				'   see if new line is longer than previous ones
				If LineWidth > RightMostPos Then
					RightMostPos = LineWidth
				End If
			End If
		Next LineNumber
	End With
	Return RightMostPos
	End Function

	'         image insertion procedures

	Private Function GetImagePrefix(RichTextBox As RichTextBox, _
		ByVal image As Image) As String
	'   get RTF prefix for image
	Dim sb As New StringBuilder()
	Using gr As Graphics = RichTextBox.CreateGraphics
		'   current width of the image in himetrics
		Dim picw As Integer = _
			CType(Math.Truncate((image.Width / gr.DpiX) * HMM_PER_INCH), Integer)
		sb.Append("\picw") : sb.Append(picw)
		'   current height of the image himetrics
		Dim pich As Integer = _
			CType(Math.Truncate((image.Height / gr.DpiY) * HMM_PER_INCH), Integer)
		sb.Append("\pich") : sb.Append(pich)
		'    target width of the image in twips
		Dim picwgoal As Integer = _
			CType(Math.Truncate((image.Width / gr.DpiX) * TWIPS_PER_INCH), Integer)
		sb.Append("\picwgoal") : sb.Append(picwgoal)
		'   target height of the image in twips
		Dim pichgoal As Integer = _
			CType(Math.Truncate((image.Height / gr.DpiY) * TWIPS_PER_INCH), Integer)
		sb.Append("\pichgoal") : sb.Append(pichgoal) : sb.Append(" ")
	End Using
	Return sb.ToString()
	End Function

	Private Function GetRtfImage(RichTextBox As RichTextBox, _
		ByVal image As Image) As String
	'   get RTF for image info
	Dim sb As StringBuilder = Nothing
	'   store enhanced metafile in memory stream
	Dim ms As MemoryStream = Nothing, mf As Metafile = Nothing
	Try
		sb = New StringBuilder() : ms = New MemoryStream()
		'   get graphics content from RichTextBox
		Using gr As Graphics = RichTextBox.CreateGraphics
			'   create enhanced metafile from the graphics context
			Dim hDC As IntPtr = gr.GetHdc()
			mf = New Metafile(ms, hDC) : gr.ReleaseHdc(hDC)
		End Using
		' Get a graphics context from the Enhanced Metafile
		Using gr As Graphics = Graphics.FromImage(mf)
			'   draw image onto metafile
			gr.DrawImage(image, New Rectangle(0, 0, image.Width, image.Height))
		End Using
		' Get the handle of the Enhanced Metafile
		Dim mfHandle As IntPtr = mf.GetHenhmetafile()
		Try
			'   get size of buffer for metafile info
			Dim _bufferSize As UInteger = _
				GdipEmfToWmfBits(mfHandle, 0, Nothing, MM_ANISOTROPIC, _
					EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault)
			Dim _buffer(CType(_bufferSize - 1, Integer)) As Byte
			'   copy metafile info into buffer and get actual size
			Dim _convertedSize As UInteger = _
				GdipEmfToWmfBits(mfHandle, _bufferSize, _buffer, MM_ANISOTROPIC, _
					EmfToWmfBitsFlags.EmfToWmfBitsFlagsDefault)
			'    copy buffer contents into string of hex values
			For index As Integer = 0 To _buffer.Length - 1
				sb.Append(String.Format("{0:X2}", _buffer(index)))
			Next index
			Return sb.ToString()
		  Finally
			'   avoid memory leak
			DeleteEnhMetaFile(mfHandle)
		End Try
	  Finally
		'   handle metafile and memory-stream disposal
		If mf IsNot Nothing Then
			mf.Dispose()
		End If
		If ms IsNot Nothing Then
			ms.Close() : ms.Dispose()
		End If
	End Try
	End Function




	'      PrintDocument event procedures

	Private Sub BeginPrint(ByVal sender As Object, _
		ByVal e As System.Drawing.Printing.PrintEventArgs)
	'   prepare to start printing   
	_CurrentPage = 1
	End Sub

	Private Sub PrintPage(ByVal sender As Object, _
		ByVal e As System.Drawing.Printing.PrintPageEventArgs)
	'   print current page
	Dim FirstCharOnNextPage As Integer = FormatRange(e.Graphics, e.PageSettings, _
		_PageIndexes(_PagesToPrint(_CurrentPage - 1) - 1), _TextLength, True)
	_CurrentPage += 1
	'   are we at the end?
	e.HasMorePages = _
		_CurrentPage <= _PageCount AndAlso FirstCharOnNextPage < _TextLength
	End Sub

	Private Sub EndPrint(ByVal sender As Object, _
		ByVal e As System.Drawing.Printing.PrintEventArgs)
	'   finish printing
	FormatRangeDone()
	End Sub



	'   public components (extension methods for WinForms RichTextBox)

	'      constant for selection margin

	''' <summary>
	''' Width of left-side "selection margin" for highlighting whole lines
	''' when a RichTextBox's ShowSelection property is True
	''' </summary>
	Public Const SelectionMargin As Single = 8F

	'      enums for GetScrollInfo call (used by GetScrollBarInfo)

	<Flags()> _
	Public Enum ScrollBarMask
		Range = 1 : PageSize = 2 : Position = 4 : TrackPosition = 16
		Everything = _
			Range Or PageSize Or Position Or TrackPosition
	End Enum

	<Flags> _
	Public Enum ScrollBarType
		Horizontal = 0 : Vertical = 1
		Control = 2 'ScrollInfo's nBar parameter when hwnd is a handle to a ScrollBar 
	End Enum

	'      structure for GetScrollInfo call (used by GetScrollBarInfo)

	<StructLayout(LayoutKind.Sequential)> _
	Public Structure ScrollInfo
		'   INPUT: What information to get
		Public SizeOfStructure As Integer 'infrastructure
		Public ScrollBarMask As Integer 'tells which fields below to retrieve information for
		'   OUTPUT: Scrollbar information
		Public MinimumScrollPosition As Integer
		Public MaximumScrollPosition As Integer
		Public PageSize As Integer
		Public Position As Integer
		Public TrackPosition As Integer
	End Structure

	'      enum for list types
	Public Enum RTBListStyle
		NoList = 0
		Bullets
		Numbers
		LowercaseLetters
		UppercaseLetters
		LowercaseRomanNumerals
		UppercaseRomanNumerals
		MaxListStyles
	End Enum

	'      enum for list-leader syntaxes
	Public Enum RTBNumberingSyntax
		RightParenthesisOnly = 0
		BothParentheses
		Period
		Plain
		MaxNumberingSyntaxes
	End Enum

	'      enum for alignment
	Public Enum RTBAlignment
		Left = HorizontalAlignment.Left
		Right = HorizontalAlignment.Right
		Center = HorizontalAlignment.Center
		Justify = 3 'full justification (requires rich edit control version 3.0 or higher)
		MaxAlignments
	End Enum

	'      enum for superscript/subscript
	Public Enum RTBScriptStyle
		Subscript = -1
		Normal = 0
		Superscript = 1
	End Enum

	'      enum for printout
	Public Enum WhichPages
		PrintSpecifiedPages
		SkipSpecifiedPages
		AllPagesInRange
		OddPagesInRange  'right-sided pages
		EvenPagesInRange 'left-sided pages
	End Enum



	'      printout methods

	''' <summary>
	''' Print RichTextBox contents or a range of pages thereof
	''' </summary>
	''' <param name="PrintDocument">Instance of PrintDocument</param>
	''' <param name="WhichPages">Whether to print all pages in range (default),
	''' odd only, or even only</param>
	''' <param name="PageList">Array of pages in rage that may be printed
	''' (defaults to all pages in range)</param>
	''' <remarks>NOTES:<br></br>
	''' 1. If both WhichPages AND PageList are included, then any page
	'''    in the range must both be in the list and be the right type (odd/even)<br></br>
	''' 2. If no pages qualify, then nothing is printed</remarks>
	<Extension()> _
	Public Sub Print(ByVal RichTextBox As RichTextBox, _
		ByVal PrintDocument As PrintDocument, _
		Optional ByVal WhichPages As WhichPages = WhichPages.AllPagesInRange, _
		Optional ByVal PageList() As Integer = Nothing)
	'   print document
	If PrintSetUp(RichTextBox, PrintDocument, False, WhichPages, PageList) Then
		PrintDocument.Print()
	End If
	End Sub

	''' <summary>
	''' Preview RichTextBox contents or a range of pages thereof to be printed
	''' </summary>
	''' <param name="PrintPreviewDialog">Instance of PrintPreviewDialog</param>
	''' <param name="WhichPages">Whether to print all pages in range (default),
	''' odd only, or even only</param>
	''' <param name="PageList">Array of pages in rage that may be printed
	''' (defaults to all pages in range)</param>
	''' <returns>Result of Print Preview dialog</returns>
	''' <remarks>NOTES:<br></br>
	''' 1. If both WhichPages AND PageList are included, then any page
	'''    in the range must both be in the list and be the right type (odd/even)<br></br>
	''' 2. If no pages qualify, then nothing is previewed,
	'''    and DialogResult.None is returned</remarks>
	<Extension()> _
	Public Function PrintPreview(ByVal RichTextBox As RichTextBox, _
		ByVal PrintPreviewDialog As PrintPreviewDialog, _
		Optional ByVal WhichPages As WhichPages = WhichPages.AllPagesInRange, _
		Optional ByVal PageList() As Integer = Nothing) As DialogResult
		'   preview document
	If PrintSetUp(RichTextBox, PrintPreviewDialog.Document, False, WhichPages, PageList) Then
		Return _
			PrintPreviewDialog.ShowDialog()
	 Else
		Return DialogResult.None
	End If
	End Function

	''' <summary>
	''' Get array of indexes for beginnings of pages
	''' </summary>
	''' <param name="PrintDocument">Instance of PrintDocument</param>
	''' <returns></returns>
	''' <remarks>Pages are measured according to PrintDocument.DefaultPageSettings;
	''' no print job is performed. There is always at least one index (array element)
	''' returned, and the first index is always 0, representing the beginning of all text.
	''' </remarks>
	<Extension()> _
	Public Function PageIndexes(RichTextBox As RichTextBox, _
		PrintDocument As PrintDocument) As Integer()
	'   acquire page indexes
	PrintSetUp(RichTextBox, PrintDocument, True, WhichPages.AllPagesInRange, Nothing)
	Return _PageIndexes
	End Function

	''' <summary>
	''' Set RightMargin property of RichTextBox to width of printer page
	''' (within horizontal margins)
	''' so that text wraps at the same position in the text box as on the printer
	''' </summary>
	''' <param name="PageSettings">Instance of PageSettings</param>
	''' <param name="MakeExact">True (default) to set printer-width back to
	''' calculated right-margin (to adjust for conversion errors and make sure
	''' that word-wrap line breaks are the same on screen and page), False
	''' to make no after-adjustments</param>
	<Extension()> _
	Public Sub SetRightMarginToPrinterWidth(RichTextBox As RichTextBox, _
		PageSettings As PageSettings, Optional MakeExact As Boolean = True)
	RightMarginFromPrinterWidth(RichTextBox, PageSettings)
	If MakeExact Then
		'   adjust for errors
		PrinterWidthFromRightMargin(RichTextBox, PageSettings)
	End If
	End Sub

	''' <summary>
	''' Set PageSettings right margin to RichTextBox's RightMargin value
	''' (or maximum line width if RightMargin is 0 or negative)
	''' so that text wraps at the same position on the printer as in the text box
	''' </summary>
	''' <param name="PageSettings">Instance of PageSettings</param>
	''' <param name="MakeExact">True (default) to set right-margin back to
	''' calculated printer-width (to adjust for conversion errors and make sure
	''' that word-wrap line breaks are the same on screen and page), False
	''' to make no after-adjustments</param>
	<Extension()> _
	Public Sub SetPrinterWidthToRightMargin(RichTextBox As RichTextBox, _
		PageSettings As PageSettings, Optional ByVal MakeExact As Boolean = True)
	PrinterWidthFromRightMargin(RichTextBox, PageSettings)
	If MakeExact Then
		'   adjust for errors
		RightMarginFromPrinterWidth(RichTextBox, PageSettings)
	End If
	End Sub

	'      scrolling/text-width methods

	''' <summary>
	''' Get scroll position of RichTextBox
	''' </summary>
	''' <returns>Point structure containing current horizontal (.x)
	''' and vertical (.y) scroll positions in pixels</returns>
	''' <remarks></remarks>
	<Extension()> _
	Public Function GetScrollPosition(RichTextBox As RichTextBox) As Point
	Dim RTBScrollPoint As Point = Nothing
	SendMessage(RichTextBox.Handle, EM_GETSCROLLPOS, 0, RTBScrollPoint)
	Return RTBScrollPoint
	End Function

	''' <summary>
	''' Set scroll position of RichTextBox
	''' </summary>
	''' <param name="RichTextBox"></param>
	''' <param name="RTBScrollPoint">Point structure containing new horizontal (.x)
	''' and vertical (.y) scroll positions in pixels</param>
	''' <remarks></remarks>
	<Extension()> _
	Public Sub SetScrollPosition(RichTextBox As RichTextBox, _
		ByVal RTBScrollPoint As Point)
	SendMessage(RichTextBox.Handle, EM_SETSCROLLPOS, 0, RTBScrollPoint)
	End Sub

	''' <summary>
	''' Get information about a RichTextBox scroll bar
	''' </summary>
	''' <param name="ScrollBarType">ScrollBarType value (.Horizontal or .Vertical)</param>
	''' <param name="ScrollBarMask">ScrollBarMask flags indicating what to get
	''' (range, page size, position, track position; defaults to everything)</param>
	''' <returns>ScrollInfo structure with requested info</returns>
	''' <remarks></remarks>
	<Extension()> _
	Public Function GetScrollBarInfo(RichTextBox As RichTextBox, _
		ScrollBarType As ScrollBarType, _
		Optional ScrollBarMask As ScrollBarMask = ScrollBarMask.Everything) As ScrollInfo
	Dim si As New ScrollInfo
	si.SizeOfStructure = Marshal.SizeOf(si) 'must always be set to the size of structure
	si.ScrollBarMask = ScrollBarMask 'tells the GetScrollInfo what to get
	GetScrollInfo(RichTextBox.Handle, ScrollBarType, si) 'horizontal or vertical?
	Return si
	End Function

	''' <summary>
	''' Get effective maximum text width of RichTextBox in pixels
	''' </summary>
	''' <returns>Maximum available physical width for any text.
	''' (-1 if we're in a recursive loop--see remarks)</returns>
	''' <remarks>This value is calculated as follows:<br></br>
	''' 1. If control's RightMargin propert is non-zero, then that us used<br></br>
	''' 2. Otherwise, if WordWrap is True, then the control's client-area width
	'''    minus any left-edge "selection" margin is used<br></br>
	''' 3. Otherwise, if horizontal scrollbars are enabled, then the "maximum horizontal
	'''    scroll position" plus the client width, or the width of the longest physical line,
	'''    whichever is longer, is used<br></br>
	''' 4. Otherwise, the width of the longest physical line is uesd</remarks>
	<Extension()> _
	Public Function GetMaximumWidth(RichTextBox As RichTextBox) As Integer
	With RichTextBox
		'   see if text width is fixed
		If .RightMargin > 0 Then
			'   yes, so return with fixed width
			Return .RightMargin
		End If
		'   else start with width of text box
		Dim SelectionOffset As Integer = .Padding.Left + 1, _
			MaxTextWidth As Integer = .ClientRectangle.Width - .Padding.Right - 1
		If .ShowSelectionMargin Then
			'   account for any selection margin
			SelectionOffset += CType(.ZoomFactor * SelectionMargin, Integer)
		End If
		MaxTextWidth -= SelectionOffset
		'   determine maximum linewidth
		If Not .WordWrap Then
			If (.ScrollBars And RichTextBoxScrollBars.Horizontal) _
						= RichTextBoxScrollBars.Horizontal Then
				'   determine rightmost character position from scrollbar info
				MaxTextWidth += _
					.GetScrollBarInfo(ScrollBarType.Horizontal).MaximumScrollPosition
			End If
			'   make sure value is at least as large as widest line
			MaxTextWidth = _
				Math.Max(MaxTextWidth, GetRightMostCharacterPosition(RichTextBox))
		End If
		'   return value
		Return MaxTextWidth
	End With
	End Function

	'      RTF methods

	''' <summary>
	''' Gets full length of selected text, including any hidden text
	''' </summary>
	''' <param name="RichTextBox"></param>
	''' <returns>Full length of selection</returns>
	''' <remarks>This is different from the SelectionLength property only when
	''' the host program is targeted for .NET 4.7 or later and hidden text is present
	''' in document</remarks>
	<Extension()> _
	Public Function FullSelectionLength(RichTextBox As RichTextBox) As Integer
	'   get start and end of selection
	Const Flags As BindingFlags = _
		BindingFlags.Instance Or BindingFlags.NonPublic

	Dim TypeRTB As Type = RichTextBox.GetType
	Dim fi As FieldInfo = TypeRTB.GetField("curSelStart", Flags)
	Dim nCurSelStart As Integer = CType(fi.GetValue(RichTextBox), Integer)
	fi = TypeRTB.GetField("curSelEnd", Flags)
	Dim nCurSelEnd As Integer = _
		Math.Min(CType(fi.GetValue(RichTextBox), Integer), RichTextBox.TextLength)
	If nCurSelStart = -1 AndAlso nCurSelEnd = -1 Then
		Return 0
	 Else
		Return _
			nCurSelEnd - nCurSelStart
	End If
	End Function

	''' <summary>
	''' Gets full plain text of rich-text box (or selection thereof),
	''' including any hidden text
	''' </summary>
	''' <param name="SelectionOnly">True to get only selected text,
	''' False (default) to get entire text</param>
	''' <returns>Full plain text, be it entire document or selection</returns>
	''' <remarks>This is different from the Text or SelectedText property only when
	''' the host program is targeted for .NET 4.7 or later and hidden text is present
	''' in document</remarks>
	<Extension()> _
	Public Function ExtendedText(RichTextBox As RichTextBox, _
		Optional SelectionOnly As Boolean = False) As String
	'   get plain text including any hidden text	
	Const Flags As BindingFlags = _
		BindingFlags.Instance Or BindingFlags.NonPublic

	Dim TypeRTB As Type = RichTextBox.GetType
	'   access private WindowText property
	Dim prop As PropertyInfo = TypeRTB.GetProperty("WindowText", Flags)
	Dim Text As String = _
		CType(prop.GetValue(RichTextBox, {}), String).Replace(ControlChars.Cr, "")
	If SelectionOnly Then
		'   get start and end of selection
		Dim fi As FieldInfo = TypeRTB.GetField("curSelStart", Flags)
		Dim nCurSelStart As Integer = CType(fi.GetValue(RichTextBox), Integer)
		fi = TypeRTB.GetField("curSelEnd", Flags)
		Dim nCurSelEnd As Integer = _
			Math.Min(CType(fi.GetValue(RichTextBox), Integer), Text.Length)
		'   extract substring
		If (nCurSelStart = -1 AndAlso nCurSelEnd = -1) _
				OrElse nCurSelStart = nCurSelEnd Then
			Text = "" 'no selection
		 Else
			Text = Text.Substring(nCurSelStart, nCurSelEnd - nCurSelStart)
		End If
	End If
	Return Text
	End Function

	'      Find methods

	''' <summary>
	''' extended version of Find method--is reliable even when hidden text is in document
	''' (overload 1 of 2--search for string)
	''' </summary>
	''' <param name="SearchText">String to search for</param>
	''' <param name="StartPos">Starting position (if EndPos is ommitted)
	''' or beginning of range (if EndPos is included) of search</param>
	''' <param name="EndPos">End of range of search (if included),
	''' may be -1 to indicate end of document</param>
	''' <param name="options">RichTextBoxFinds search options</param>
	''' <returns>Index of first matching string found, -1 if none found</returns>
	''' <remarks>This is functionally different from the regular Find method only
	''' when the host program is targeted for .NET 4.7 or later and hidden text
	''' is present in document</remarks>
	<Extension()> _
	Public Function ExtendedFind(RichTextBox As RichTextBox, _
		ByVal SearchText As String, _
		Optional ByVal StartPos As Integer = -1, _
		Optional ByVal EndPos As Integer = -2,
		Optional ByVal options As RichTextBoxFinds = RichTextBoxFinds.None) As Integer
	Dim position As Integer, Offset As Integer, _
		eText As String = RichTextBox.ExtendedText, _
		sc As StringComparison
	'   document too short or no text to search for?
	If String.IsNullOrEmpty(SearchText) _
			OrElse eText.Length < SearchText.Length Then
		Return -1
	End If
	If StartPos < 0 OrElse StartPos > eText.Length Then
		'   no start position--default to caret
		StartPos = RichTextBox.SelectionStart
	End If
	'   does case matter?
	If (options And RichTextBoxFinds.MatchCase) = 0 Then
		sc = StringComparison.OrdinalIgnoreCase
	 Else
		sc = StringComparison.Ordinal
	End If
	'   direction?
	If (options And RichTextBoxFinds.Reverse) = 0 Then
		'   going forward
		Offset = 1 : position = StartPos
		If EndPos <= -1 OrElse EndPos > eText.Length Then
			EndPos = eText.Length
		End If
	 Else
		'   going backward
		Offset = -1
		If EndPos <= -2 Then
			'   no end position--start at start-position parameter
			position = StartPos : StartPos = 0 : EndPos = eText.Length
		 Else
			'   start at end-position parameter
			If EndPos = -1 OrElse EndPos > eText.Length Then
				EndPos = eText.Length : position = eText.Length - 1
			 Else
				position = EndPos - 1
			End If
			If position < 0 Then
				Return -1
			End If
		End If
	End If
	'   do find
	Do
		'   search for text
		If Offset = 1 Then
			'   look forward
			position = eText.IndexOf(SearchText, position, sc)
			If position = -1 OrElse position > EndPos - SearchText.Length Then
				'   not found or gone past end
				Return -1
			End If
		 Else
			'   look backward
			position = eText.LastIndexOf(SearchText, position, sc)
			If position = -1 OrElse position < StartPos Then
				'   not found or gone past start
				Return -1
			End If
		End If
		'   check for whole word
		If (options And RichTextBoxFinds.WholeWord) = 0 Then
			'  doesn't have to be whole word
			Exit Do
		 Else
			'   is this a whole word?
			Dim EndOfText As Integer = position + SearchText.Length
			If (position > 0 _
						AndAlso Char.IsLetterOrDigit(eText.Chars(position - 1))) _
					OrElse (EndOfText < eText.Length _
						AndAlso Char.IsLetterOrDigit(eText.Chars(EndOfText))) Then
				'   in the middle of word, so look again
				position += Offset
			 Else
				'   whole word
				Exit Do
			End If
		End If
	Loop Until position = -1
	'   text found--highlight it?
	If position > -1 AndAlso (options And RichTextBoxFinds.NoHighlight) = 0 Then
		RichTextBox.Select(position, SearchText.Length)
	End If
	Return position
	End Function

	''' <summary>
	''' extended version of Find method--is reliable even when hidden text is in document
	''' (overload 2 of 2--search for any of a series of characters)
	''' </summary>
	''' <param name="SearchChars">Characters to search for</param>
	''' <param name="StartPos">Starting position (if EndPos is ommitted)
	''' or beginning of range (if EndPos is included) of search</param>
	''' <param name="EndPos">End of range of search (if included),
	''' may be -1 to indicate end of document</param>
	''' <param name="Reverse">True to search backwards, False (default)
	''' to search forwards</param>
	''' <returns>Index of first matching character found, -1 if none found</returns>
	''' <remarks>This is functionally different from the regular Find method only
	''' when the host program is targeted for .NET 4.7 or later and hidden text
	''' is present in document, or when one chooses to search BACKWARDS for
	''' characters in document</remarks>
	<Extension()> _
	Public Function ExtendedFind(RichTextBox As RichTextBox, _
		ByVal SearchChars() As Char, _
		Optional ByVal StartPos As Integer = -1, _
		Optional ByVal EndPos As Integer = -2,
		Optional ByVal Reverse As Boolean = False) As Integer
	Dim position As Integer, _
		eText As String = RichTextBox.ExtendedText
	'   document too short or no characters to look for?
	If eText.Length < 1 _
			OrElse SearchChars Is Nothing OrElse SearchChars.Length = 0 Then
		Return -1
	End If
	If StartPos < 0 OrElse StartPos > eText.Length Then
		'   no start position--default to caret
		StartPos = RichTextBox.SelectionStart
	End If
	'   direction?
	If Reverse Then
		'   going backward
		If EndPos <= -2 Then
			'   no end position--start at start-position parameter
			position = StartPos : StartPos = 0 : EndPos = eText.Length
		 Else
			'   start at end-position parameter
			If EndPos <= -1 OrElse EndPos > eText.Length Then
				EndPos = eText.Length : position = eText.Length - 1
			 Else
				position = EndPos - 1
			End If
			If position < 0 Then
				Return -1
			End If
		End If
		'   look
		position = eText.LastIndexOfAny(SearchChars, position)
		If position = -1 OrElse position < StartPos Then
			'   not found or gone past start
			Return -1
		End If
	 Else
		'   going forward
		position = StartPos
		If EndPos <= -1 OrElse EndPos > eText.Length Then
			EndPos = eText.Length
		End If
		'   look
		position = eText.IndexOfAny(SearchChars, position)
		If position = -1 OrElse position >= EndPos Then
			'   not found or gone past end
			Return -1
		End If
	End If
	'   text found
	Return position
	End Function

	'      fomratting methods

	''' <summary>
	''' Determine if ANY text in a rich-text box (or the selected region thereof)
	''' is protected
	''' </summary>
	''' <param name="SelectionOnly">True to check only selected text,
	''' False (default) to check entire text</param>
	''' <returns>True if any text in question is protected, False if not</returns>
	''' <remarks>This differs from the SelectionProtected property
	''' in the following ways:<br></br>
	''' 1. It can be used for either selected text or all text<br></br>
	''' 2. It is not necessary for all text in question to be protected--
	'''    only some of it--for this method to return True</remarks>
	<Extension()> _
	Public Function ContainsProtectedText(RichTextBox As RichTextBox, _
		Optional ByVal SelectionOnly As Boolean = False) As Boolean
	With RichTextBox
		If SelectionOnly Then
			'   is selection fully protected or with "\protect" tag?
			If .SelectionProtected _
					OrElse ProtectedTextChecker.IsMatch(.SelectedRtf) Then
				Return True 'yes
			End If
			'   else is hidden text here and platform >= .NET 4.7?
			Dim FullLength As Integer = .FullSelectionLength
			If FullLength = 0 OrElse .SelectionLength = FullLength Then
				Return False 'no
			End If
			'   else look for protected text character by chararcter
			Dim ScrollPosition As Point = .GetScrollPosition(), _
				Start As Integer = .SelectionStart, _
				IsProtected As Boolean = False
			.SetRedrawMode(False)
			For position As Integer = Start To Start + FullLength - 1
				.Select(position, 1)
				If .SelectionProtected Then
					'   protected character found
					IsProtected = True : Exit For
				End If
			Next position
			'   restore selection and report finding
			.Select(Start, FullLength)
			.SetScrollPosition(ScrollPosition) : .SetRedrawMode(True)
			Return IsProtected
		 Else
			'   parse whole text for "\protect" tag
			Return _
				ProtectedTextChecker.IsMatch(.Rtf)
		End If
	End With
	End Function

	''' <summary>
	''' Determine if ANY text in a rich-text box (or the selected region thereof)
	''' is hidden (marked as invisible)
	''' </summary>
	''' <param name="SelectionOnly">True to check only selected text,
	''' False (default) to check entire text</param>
	''' <returns>True if any text in question is hidden, False if not</returns>
	<Extension()> _
	Public Function ContainsHiddenText(RichTextBox As RichTextBox, _
		Optional ByVal SelectionOnly As Boolean = False) As Boolean
	With RichTextBox
		If SelectionOnly Then
			Return _
				HiddenTextChecker.IsMatch(.SelectedRtf) _
					OrElse .SelectionLength < .FullSelectionLength '(if >= .NET Framework 4.7)
		 Else
			Return _
				HiddenTextChecker.IsMatch(.Rtf) _
					OrElse .Text.Length < .ExtendedText.Length     '(if >= .NET Framework 4.7)
		End If
	End With
	End Function

	''' <summary>
	''' Mark selected text in RichTextBox as a list, using a given leader style, a given
	''' starting number, a given numbering syntax, and a given indentation (all optional)
	''' </summary>
	''' <param name="ListStyle">Style of paragraph leaders (no leader, bullets, Arabic
	''' numbers, lowercase letters, uppercase letters, lowercase Roman numerals, or uppercase
	''' Roman numerals; defaults to numbers)</param>
	''' <param name="StartingNumber">Number of item for BEGINNING of list with given leader
	''' style (0 - 65535 [&amp;HFFFF]; defaults to 1)</param>
	''' <param name="NumberingSyntax">Syntax for paragraph leader (surround in parentheses
	''' [i.e., "(1)"], follow with right parenthesis ["1)"], follow with period ["1."], or
	''' give plain ["1"]; defaults to period)</param>
	''' <param name="Indentation">Indentation in pixels of left edge of paragraph proper from
	''' left edge of paragraph leader; defaults to minimum allowed value</param>
	<Extension()> _
	Public Sub SetListStyle(RichTextBox As RichTextBox, _
		Optional ByVal ListStyle As RTBListStyle = RTBListStyle.Numbers, _
		Optional ByVal StartingNumber As Integer = 1, _
		Optional ByVal NumberingSyntax As RTBNumberingSyntax = RTBNumberingSyntax.Period, _
		Optional ByVal Indentation As Integer = 0)
	With RichTextBox
		Dim pf As PARAFORMAT2 = New PARAFORMAT2() : pf.dwMask = PFM_ALLNUMBERING
		'   set numbering format
		pf.wNumbering = _
			CType((ListStyle And USHORT_MASK) Mod RTBListStyle.MaxListStyles, UShort)
		'   set starting #
		pf.wNumberingStart = CType(StartingNumber And USHORT_MASK, UShort)
		'   set numbering syntax
		pf.wNumberingStyle = _
			CType(((NumberingSyntax And USHORT_MASK) _
					Mod RTBNumberingSyntax.MaxNumberingSyntaxes) _
				* STYLE_FACTOR, UShort)
		'   set indentation for paragraphs (convert pixels to twips)
		Indentation = CType(Indentation * TWIPS_PER_INCH / .CreateGraphics.DpiX, Integer)
		pf.wNumberingTab = CType(Indentation And USHORT_MASK, UShort)
		'   set new numbering options
		Dim rv As IntPtr = SendMessage(.Handle, EM_SETPARAFORMAT, 0, pf)
	End With
	End Sub

	''' <summary>
	''' Get listing attributes of selected text; returns leader style--and can
	''' optionally return (as variables passed by reference) starting number, numbering
	''' syntax, and/or indentation)--for paragraph at caret or first selected paragraph
	''' </summary>
	''' <param name="StartingNumber">Item number for BEGINNING OF LIST using leader-style at
	''' caret or beginning of selection</param>
	''' <param name="NumberingSyntax">Snytax of paragraph leaders (surrounded by parentheses
	''' [i.e., "(1)"], followed by right parenthesis ["1)"], followed by period ["1."], or
	''' given plain ["1"])</param>
	''' <param name="Indentation">Indentation in pixels of left edge of paragraph proper from
	''' left edge of paragraph leader (0 means minimum allowed value is being used)</param>
	''' <returns>Style of paragraph leader (no leader, bullets, Arabic numbers, lowercase
	''' letters, uppercase letters, lowercase Roman numerals, uppercase Roman numerals)</returns>
	<Extension()> _
	Public Function GetListStyle(RichTextBox As RichTextBox, _
		Optional ByRef StartingNumber As Integer = 1, _
		Optional ByRef NumberingSyntax As Integer = RTBNumberingSyntax.Period, _
		Optional ByRef Indentation As Integer = 0) As RTBListStyle
	With RichTextBox
		'   get current numbering options
		Dim pf As PARAFORMAT2 = New PARAFORMAT2() : pf.dwMask = PFM_ALLNUMBERING
		Dim rv As IntPtr = SendMessage(.Handle, EM_GETPARAFORMAT, 0, pf)
		'   get starting # and numbering syntax
		StartingNumber = pf.wNumberingStart : NumberingSyntax = pf.wNumberingStyle \ STYLE_FACTOR
		'   get indentation (convert twips to pixels)
		Indentation = CType(pf.wNumberingTab * .CreateGraphics.DpiX / TWIPS_PER_INCH, Integer)
		'   return numbering format
		Return CType(pf.wNumbering, RTBListStyle)
	End With
	End Function

	''' <summary>
	''' Sets horizontal alignment of selected text; differs from setting SelectionAlignment
	''' property in that full justification (rich edit version 3.0 or higher only) is an option
	''' </summary>
	''' <param name="Alignment">Horizontal alignment (left, right, center, or full-justify)</param>
	<Extension()> _
	Public Sub SetAlignment(RichTextBox As RichTextBox, _
		ByVal Alignment As RTBAlignment)
	With RichTextBox
		Dim pf As PARAFORMAT2 = New PARAFORMAT2() : pf.dwMask = PFM_ALIGNMENT
		'   set alignment
		pf.wAlignment = _
			CType((Alignment And USHORT_MASK) Mod RTBAlignment.MaxAlignments, UShort) + 1US
		Dim ri As Integer = _
			SendMessage(.Handle, EM_SETYPOGRAPHYOPTIONS, TO_ADVANCEDTYPOGRAPHY, TO_ADVANCEDTYPOGRAPHY)
		Dim rp As IntPtr = SendMessage(.Handle, EM_SETPARAFORMAT, 0, pf)
	End With
	End Sub

	''' <summary>
	''' Gets horizontal alignment of selected text; differs from reading SelectionAlignment property
	''' in that full justification (rich edit version 3.0 or higher only) is a possible value and
	''' that, if multiple paragraphs are selected with differing alignments, then the value for the
	''' first paragraph in the selection, rather than HorizontalAlignment.Left, is returned
	''' </summary>
	''' <returns>Horizontal alignment (left, right, center, or fully-justified) at carat (if no selection)
	''' or of first selected paragraph (if selection)</returns>
	<Extension()> _
	Public Function GetAlignment(RichTextBox As RichTextBox) As RTBAlignment
	With RichTextBox
		'   get current alignment
		Dim pf As PARAFORMAT2 = New PARAFORMAT2() : pf.dwMask = PFM_ALIGNMENT
		Dim rv As IntPtr = SendMessage(.Handle, EM_GETPARAFORMAT, 0, pf)
		Return CType(pf.wAlignment - 1US, RTBAlignment)
	End With
	End Function

	''' <summary>
	''' Set text in rich-text box to use advanced typography (rich edit version 3.0 or higher only)
	''' </summary>
	''' <remarks>This is recommend for usage after loading or setting RTF text that contains
	''' advanced formatting, such as full-justification</remarks>
	<Extension()> _
	Public Sub UseAdvancedTypographyOnText(RichTextBox As RichTextBox)
	With RichTextBox
		Dim ri As Integer = _
			SendMessage(.Handle, EM_SETYPOGRAPHYOPTIONS, _
				TO_ADVANCEDTYPOGRAPHY, TO_ADVANCEDTYPOGRAPHY)
	End With
	End Sub

	''' <summary>
	''' Set selected text to superscript
	''' </summary>
	''' <param name="ScriptStyle">Superscript (positive), subscript (negative), or normal (0)</param>
	<Extension()> _
	Public Sub SetScriptStyle(RichTextBox As RichTextBox, _
		ByVal ScriptStyle As RTBScriptStyle)
	Dim cf As CHARFORMAT2 = New CHARFORMAT2() : cf.dwMask = CFM_SUBORSUPERSCRIPT
	ScriptStyle = CType(Math.Sign(ScriptStyle), RTBScriptStyle)
	With RichTextBox
		Select Case ScriptStyle
		   Case RTBScriptStyle.Subscript
				cf.dwEffects = CFE_SUBSCRIPT
			Case RTBScriptStyle.Superscript
				cf.dwEffects = CFE_SUPERSCRIPT
			Case Else 'normal
				cf.dwEffects = 0
		End Select
		'   set style
		Dim rv As IntPtr = SendMessage(.Handle, EM_SETCHARFORMAT, SCF_SELECTION, cf)
	End With
	End Sub

	''' <summary>
	''' Determine whether selected text is superscript, subscript, or normal
	''' </summary>
	''' <returns>Superscript (+1), subscript (-1), or nrmal (0)</returns>
	''' <remarks>If script-formatting in selection is mixed, then the value at the caret
	''' (begining or end of selection, depending on the direction of user selection) is returned</remarks>
	<Extension()> _   
	Public Function GetScriptStyle(RichTextBox As RichTextBox) As RTBScriptStyle
	With RichTextBox
		Dim cf As CHARFORMAT2 = New CHARFORMAT2() : cf.dwMask = CFM_SUBORSUPERSCRIPT
		'   get style
		Dim rv As IntPtr = SendMessage(.Handle, EM_GETCHARFORMAT, SCF_SELECTION, cf)
		If (cf.dwEffects And CFE_SUBSCRIPT) = CFE_SUBSCRIPT Then
			Return RTBScriptStyle.Subscript
		 ElseIf (cf.dwEffects And CFE_SUPERSCRIPT) = CFE_SUPERSCRIPT Then
			Return RTBScriptStyle.Superscript
		 Else
			Return RTBScriptStyle.Normal
		End If
	End With
	End Function

	'      RTF conversion and insertion methods

	''' <summary>
	''' Convert plain-text string into RTF string
	''' </summary>
	''' <param name="PlainText">Plain-text string</param>
	''' <returns>String with special characters (i.e., "{", "\", "}", non-ASCII) escaped
	''' </returns>
	<Extension()> _
	Public Function EscapedRTFText(RichTextBox As RichTextBox, _
		ByVal PlainText As String) As String
	'   escape any special RTF characters in string
	Dim sb As StringBuilder = New StringBuilder()
	For CharIndex As Integer = 0 To PlainText.Length - 1
		'   parse plain-text string
		Dim Character As Char = PlainText(CharIndex)
		Select Case Character
			Case " "c, "{"c, "\"c, "}"c, "0"c To "9"c
				'   escape character
				sb.Append("\" & Character)
			Case Is < " "c, Is > "~"c
				'   non-ASCII character
				Dim CharacterCode As Integer = AscW(Character)
				If CharacterCode > &H7FFF Then
					CharacterCode -= &H10000
				End If
				sb.Append("\u" & CharacterCode.ToString & "?")
			Case Else
				'   normal character
				sb.Append(Character)
		End Select
	Next CharIndex
	'   return escaped result
	Return sb.ToString()
	End Function

	''' <summary>
	''' Insert RTF text into rich-text box at a given position
	''' </summary>
	''' <param name="RtfTextToInsert">RTF-format text to insert</param>
	''' <param name="position">position in rich-text box to make insertion
	''' (defaults to current selection position, overwriting selection, if omitted)</param>
	''' <remarks>This is the "safe" way to insert, as it accounts for "template RTF"
	''' that's expected in any inserted RTF text</remarks>
	<Extension()> _
	Public Sub InsertRtf(RichTextBox As RichTextBox, _
		ByVal RtfTextToInsert As String, _
		Optional ByVal position As Integer = -1)
	With RichTextBox
		'   position defaults to current selection point
		Dim Length As Integer = .SelectionLength
		If position < 0 Or position > .TextLength Then
			position = .SelectionStart
		 Else
			Length = 0 'arbitrary insertion
		End If
		'   get RTF template from an empty selection and insert new RTF text
		.Select(position, 0)
		Dim RtfText As String = RichTextBox.SelectedRtf
		If String.IsNullOrEmpty(RtfText) Then
			RtfText = "{\rtf1}"
		End If
		If Length > 0 Then
			.Select(position, Length) 'restore selection if overwriting
		End If
		Dim EndBrace As Integer = _
			RtfText.LastIndexOf("}"c) 'new text comes before RTF closing brace
		RtfText = RtfText.Insert(EndBrace, RtfTextToInsert)
		RichTextBox.SelectedRtf = RtfText
	End With
	End Sub

	'      picture insertion methods

	''' <summary>
	''' Insert image (bitmap or metafile) into the RichTextBox at a given position
	''' </summary>
	''' <param name="image">Image to insert</param>
	''' <param name="position">position in rich-text box to make insertion
	''' (defaults to current selection position, overwriting selection, if omitted)</param>
	''' <remarks>
	''' The image is first wrapped in a Windows Format Metafile, then written in RTF
	''' format to the control using hex strings. The WMF is required because, although
	''' RTF 1.6 says one can insert numerous image formats directly into an RTF document,
	''' this control ignores it without the wrapper.
	''' </remarks>
	<Extension()> _
	Public Sub InsertImage(RichTextBox As RichTextBox, _
		ByVal image As Image, Optional ByVal position As Integer = -1)
		Dim sb As New StringBuilder()
		sb.Append("{\pict\wmetafile8")                'header
		sb.Append(GetImagePrefix(RichTextBox, image)) 'image prefix
		sb.Append(GetRtfImage(RichTextBox, image))    'image info
		sb.Append("}")                                'trailer
		RichTextBox.InsertRtf(sb.ToString, position)
	End Sub

	''' <summary>
	''' Insert icon into the RichTextBox at a given position
	''' </summary>
	''' <param name="icon">Icon to insert</param>
	''' <param name="position">position in rich-text box to make insertion
	''' (defaults to current selection position, overwriting selection, if omitted)</param>
	''' <remarks>The icon is converted into a bitmap, using RichTextBox.SelectionBackColor
	''' as the transparent color, unless that value is Color.Empty (multiple background colors
	''' in selection), in which case RichTextBox.BackColor is used. The resulting bitmap is
	''' then wrapped in a Windows Format Metafile, then written in RTF format to the control
	''' using hex strings. The WMF is required because, although RTF 1.6 says one can insert
	''' numerous image formats directly into an RTF document, this control ignores it without
	''' the wrapper.</remarks>
	<Extension()> _
	Public Sub InsertIcon(RichTextBox As RichTextBox, _
		ByVal icon As Icon, Optional ByVal position As Integer = -1)
	With RichTextBox
		Dim bitmap As Bitmap = New Bitmap(icon.Width, icon.Height)
		Using gr As Graphics = Graphics.FromImage(bitmap)
			'   determine transparent color and draw icon on bitmap
			If .SelectionBackColor = Color.Empty Then
				gr.Clear(.BackColor)
			 Else
				gr.Clear(.SelectionBackColor)
			End If
			gr.DrawIcon(icon, icon.Width, icon.Height)
			'   insert bitmap into control
			.InsertImage(bitmap, position)
		End Using
	End With
	End Sub

	'      drawing methods
	
	''' <summary>
	''' Turns on or off redrawing of rich-text box
	''' This can be used to make multiple changes to the text box while preventing
	''' the intermediate rendering that would cause flicker
	''' </summary>
	''' <param name="OnOrOff">True to activate auto-redraw, False to deactivate it</param>
	''' <remarks>Specifying True forces the cumulative results
	''' of previous operations to be rendered</remarks>
	<Extension()> _
	Public Sub SetRedrawMode(Control As Windows.Forms.Control, _
		ByVal OnOrOff As Boolean)
		With Control
			'   set redraw mode
			SendMessage(.Handle, WM_SETREDRAW, CType(OnOrOff, Integer) And 1, 1)
			If OnOrOff Then
				.Refresh() 'force redraw
			End If
		End With
	End Sub
End Module
