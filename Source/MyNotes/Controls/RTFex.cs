using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MyNotes
{
	public class RichTextBoxEx : RichTextBox
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct STRUCT_RECT
        {
            public Int32 left;
            public Int32 top;
            public Int32 right;
            public Int32 bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STRUCT_CHARRANGE
        {
            public Int32 cpMin;
            public Int32 cpMax;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STRUCT_FORMATRANGE
        {
            public IntPtr hdc;
            public IntPtr hdcTarget;
            public STRUCT_RECT rc;
            public STRUCT_RECT rcPage;
            public STRUCT_CHARRANGE chrg;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STRUCT_CHARFORMAT
        {
            public int cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct CHARFORMAT2W
        {
            public static uint SIZE = (uint)Marshal.SizeOf(typeof(CHARFORMAT2W));

            public uint cbSize;
            public CHARFORMAT_MASKS dwMask;
            public CHARFORMAT_EFFECTS dwEffects;
            public int yHeight;
            public int yOffset;
            public int crTextColor; // 0x00bbggrr
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szFaceName;
            public ushort wWeight;
            public short sSpacing;
            public int crBackColor; // 0x00bbggrr
            public uint lcid;
            public uint dwReserved;
            public short sStyle;
            public ushort wKerning;
            public CHARFORMAT_UNDERLINE bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bUnderlineColor; // bReserved1

            public string FaceName
            {
                get { return System.Text.Encoding.Unicode.GetString(szFaceName); }
                set { szFaceName = System.Text.Encoding.Unicode.GetBytes(value); }
            }

            public int yHeight_Points
            {
                get { return yHeight / 20; }
                set { yHeight = value * 20; }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CHARFORMAT2_STRUCT
        {
            public UInt32 cbSize;
            public UInt32 dwMask;
            public UInt32 dwEffects;
            public Int32 yHeight;
            public Int32 yOffset;
            public Int32 crTextColor;
            public byte bCharSet;
            public byte bPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public char[] szFaceName;
            public UInt16 wWeight;
            public UInt16 sSpacing;
            public int crBackColor; // Color.ToArgb() -> int
            public int lcid;
            public int dwReserved;
            public Int16 sStyle;
            public Int16 wKerning;
            public byte bUnderlineType;
            public byte bAnimation;
            public byte bRevAuthor;
            public byte bReserved1;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary(string dllName);

        /* DIFFERENT VERSIONS OF THESendMessage Win32 API  */

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, Int32 msg, Int32 wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
        private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, [In][Out] ref CHARFORMAT2W lParam);

        [DllImport("user32", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT lParam);

      


        private const Int32 WM_SETREDRAW = 0xB;
        private const int FALSE = 0;
        private const int TRUE = 1;

        private const int SCF_SELECTION = 0x0001;
        private const int SCF_WORD = 0x0002;
        private const int SCF_ALL = 0x0004;

        private const int TO_ADVANCEDTYPOGRAPHY = 0x1;

        private const Int32 WM_USER = 0x400;
        private const Int32 EM_FORMATRANGE = WM_USER + 57;
        private const Int32 EM_GETCHARFORMAT = WM_USER + 58;
        private const Int32 EM_SETCHARFORMAT = WM_USER + 68;

        private const int EM_SETTYPOGRAPHYOPTIONS = 1226;
        private const int EM_GETPARAFORMAT = WM_USER + 61;
        private const int EM_SETPARAFORMAT = WM_USER + 71; //= 1095;
        private const int EM_SETUNDOLIMIT = WM_USER + 82;

        //Flags:
        private const UInt32 CFE_LINK = 0x0020;
        private const UInt32 CFM_LINK = 0x00000020;

        [StructLayout(LayoutKind.Sequential)]
        public struct PARAFORMAT
        {
            /* https://learn.microsoft.com/en-us/windows/win32/api/richedit/ns-richedit-paraformat  */

            /// <summary>Structure size, in bytes. The member must be filled before passing to the rich edit control.</summary>
            public int cbSize;
            /// <summary>Members containing valid information or attributes to set. see PARAFORMAT_MASKS</summary>
            public uint dwMask;
            /// <summary>Value specifying numbering options. This member can be zero or PFN_BULLET.</summary>
            public short wNumbering;

            /// <summary>Rich Edit 1.0:: This member is named wReserved. Reserved; the value must be zero.</summary>
           // public short wReserved;

            /// <summary>Rich Edit 2.0: This member is named wEffects. A bit flag that specifies a paragraph effect.</summary>
            public short wEffects;

            /// <summary>Indentation of the first line in the paragraph, in twips. If the paragraph formatting is being set and PFM_OFFSETINDENT is specified, this member is treated as a relative value that is added to the starting indentation of each affected paragraph.</summary>
            public int dxStartIndent;
            /// <summary>Size, of the right indentation relative to the right margin, in twips.</summary>
            public int dxRightIndent;
            /// <summary>Indentation of the second and subsequent lines of a paragraph relative to the starting indentation, in twips. The first line is indented if this member is negative or outdented if this member is positive.</summary>
            public int dxOffset;

            /// <summary>Value specifying the paragraph alignment.</summary>
            public short wAlignment;
            /// <summary>Number of tab stops.</summary>
            public short cTabCount;

            /// <summary>Array of absolute tab stop positions. Each element in the array specifies information about a tab stop. The 24 low-order bits specify the absolute offset, in twips. To use this member, set the PFM_TABSTOPS flag in the dwMask member.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;


            //**************************** PARAFORMAT2 from here onwards **********************************************
            //   https://learn.microsoft.com/en-us/windows/win32/api/richedit/ns-richedit-paraformat2

            /// <summary>Size of the spacing above the paragraph, in twips. To use this member, set the PFM_SPACEBEFORE flag in the dwMask member. The value must be greater than or equal to zero.</summary>
            public int dySpaceBefore;

            /// <summary>Specifies the size of the spacing below the paragraph, in twips. To use this member, set the PFM_SPACEAFTER flag in the dwMask </summary>
            public int dySpaceAfter;
            /// <summary>Spacing between lines. To use this member, set the PFM_LINESPACING flag in the dwMask</summary>
            public int dyLineSpacing;
            /// <summary>Text style. To use this member, set the PFM_STYLE flag in the dwMask member.</summary>
            public short sStyle;
            /// <summary>Type of line spacing. To use this member, set the PFM_LINESPACING flag in the dwMask member.</summary>
            public byte bLineSpacingRule;

            /// <summary>Reserved; must be zero.</summary>
            public byte bOutlineLevel;
            /// <summary>Percentage foreground color used in shading. The wShadingStyle member specifies the foreground and background shading colors. A value of 5 indicates a shading color consisting of 5 percent foreground color and 95 percent background color. To use these members, set the PFM_SHADING flag in the dwMask</summary>
            public short wShadingWeight;
            /// <summary>Style and colors used for background shading. Bits 0 to 3 contain the shading style, bits 4 to 7 contain the foreground color index, and bits 8 to 11 contain the background color index. To use this member, set the PFM_SHADING flag in the dwMask</summary>
            public short wShadingStyle;
            /// <summary>Starting number or Unicode value used for numbered paragraphs. Use this member in conjunction with the wNumbering member</summary>
            public short wNumberingStart;
            /// <summary>Numbering style used with numbered paragraphs. Use this member in conjunction with the wNumbering member.</summary>
            public short wNumberingStyle;
            /// <summary>Minimum space between a paragraph number and the paragraph text, in twips. Use this member in conjunction with the wNumbering member.</summary>
            public short wNumberingTab;
            /// <summary>The space between the border and the paragraph text, in twips.</summary>
            public short wBorderSpace;
            /// <summary>Border width, in twips. To use this member, set the PFM_BORDER flag in the dwMask member.</summary>
            public short wBorderWidth;
            /// <summary>Border location, style, and color. Bits 0 to 7 specify the border locations, bits 8 to 11 specify the border style, and bits 12 to 15 specify the border color index. To use this member, set the PFM_BORDER flag in the dwMask </summary>
            public short wBorders;
        }


        public enum PARAFORMAT_LINESPACINGRULE : byte
        {
            Single = 0,
            OneAndAHalf = 1,
            Double = 2,

            Use_min_single_dyLineSpacing = 3,
            Use_exact_dyLineSpacing = 4,
            Use_dyLineSpacing_div_20 = 5 // aka use points (or, dyLineSpacing is in points, not twips)
        }

        [Flags]
        public enum PARAFORMAT_MASKS : uint
        {
            PFM_STARTINDENT = 0x00000001,
            PFM_RIGHTINDENT = 0x00000002,
            PFM_OFFSET = 0x00000004,
            PFM_ALIGNMENT = 0x00000008,
            PFM_TABSTOPS = 0x00000010,
            PFM_NUMBERING = 0x00000020,
            PFM_OFFSETINDENT = 0x80000000,

            PFM_SPACEBEFORE = 0x00000040,
            PFM_SPACEAFTER = 0x00000080,
            PFM_LINESPACING = 0x00000100,
            PFM_STYLE = 0x00000400,
            PFM_BORDER = 0x00000800,
            PFM_SHADING = 0x00001000,
            PFM_NUMBERINGSTYLE = 0x00002000,
            PFM_NUMBERINGTAB = 0x00004000,
            PFM_NUMBERINGSTART = 0x00008000,

            PFM_RTLPARA = 0x00010000,
            PFM_KEEP = 0x00020000,
            PFM_KEEPNEXT = 0x00040000,
            PFM_PAGEBREAKBEFORE = 0x00080000,
            PFM_NOLINENUMBER = 0x00100000,
            PFM_NOWIDOWCONTROL = 0x00200000,
            PFM_DONOTHYPHEN = 0x00400000,
            PFM_SIDEBYSIDE = 0x00800000,
            PFM_TABLE = 0x40000000,
            PFM_TEXTWRAPPINGBREAK = 0x20000000,
            PFM_TABLEROWDELIMITER = 0x10000000,

            PFM_COLLAPSED = 0x01000000,
            PFM_OUTLINELEVEL = 0x02000000,
            PFM_BOX = 0x04000000,
            PFM_RESERVED2 = 0x08000000,

            PFM_ALL = (PFM_STARTINDENT | PFM_RIGHTINDENT | PFM_OFFSET | PFM_ALIGNMENT | PFM_TABSTOPS | PFM_NUMBERING | PFM_OFFSETINDENT | PFM_RTLPARA),
            PFM_EFFECTS = (PFM_RTLPARA | PFM_KEEP | PFM_KEEPNEXT | PFM_TABLE | PFM_PAGEBREAKBEFORE | PFM_NOLINENUMBER | PFM_NOWIDOWCONTROL | PFM_DONOTHYPHEN | PFM_SIDEBYSIDE | PFM_TABLE | PFM_TABLEROWDELIMITER),
            PFM_ALL2 = (PFM_ALL | PFM_EFFECTS | PFM_SPACEBEFORE | PFM_SPACEAFTER | PFM_LINESPACING | PFM_STYLE | PFM_SHADING | PFM_BORDER | PFM_NUMBERINGTAB | PFM_NUMBERINGSTART | PFM_NUMBERINGSTYLE),
        }

        [Flags]
        public enum PARAFORMAT_EFFECTS : ushort
        {
            PFE_RTLPARA = (ushort)(PARAFORMAT_MASKS.PFM_RTLPARA >> 16),
            PFE_KEEP = (ushort)(PARAFORMAT_MASKS.PFM_KEEP >> 16),
            PFE_KEEPNEXT = (ushort)(PARAFORMAT_MASKS.PFM_KEEPNEXT >> 16),
            PFE_PAGEBREAKBEFORE = (ushort)(PARAFORMAT_MASKS.PFM_PAGEBREAKBEFORE >> 16),
            PFE_NOLINENUMBER = (ushort)(PARAFORMAT_MASKS.PFM_NOLINENUMBER >> 16),
            PFE_NOWIDOWCONTROL = (ushort)(PARAFORMAT_MASKS.PFM_NOWIDOWCONTROL >> 16),
            PFE_DONOTHYPHEN = (ushort)(PARAFORMAT_MASKS.PFM_DONOTHYPHEN >> 16),
            PFE_SIDEBYSIDE = (ushort)(PARAFORMAT_MASKS.PFM_SIDEBYSIDE >> 16),
            PFE_TEXTWRAPPINGBREAK = (ushort)(PARAFORMAT_MASKS.PFM_TEXTWRAPPINGBREAK >> 16),

            // The following four effects are read only
            PFE_COLLAPSED = (ushort)(PARAFORMAT_MASKS.PFM_COLLAPSED >> 16),
            PFE_BOX = (ushort)(PARAFORMAT_MASKS.PFM_BOX >> 16),
            PFE_TABLE = (ushort)(PARAFORMAT_MASKS.PFM_TABLE >> 16),
            PFE_TABLEROWDELIMITER = (ushort)(PARAFORMAT_MASKS.PFM_TABLEROWDELIMITER >> 16),
        }

        public enum PARAFORMAT_NUMBERING : ushort
        {
            // PARAFORMAT numbering options 
            PFN_BULLET = 1, // tomListBullet

            // PARAFORMAT2 wNumbering options 
            PFN_ARABIC = 2, // tomListNumberAsArabic: 0, 1, 2, ...
            PFN_LCLETTER = 3, // tomListNumberAsLCLetter: a, b, c, ...
            PFN_UCLETTER = 4, // tomListNumberAsUCLetter: A, B, C, ...
            PFN_LCROMAN = 5, // tomListNumberAsLCRoman: i, ii, iii, ...
            PFN_UCROMAN = 6, // tomListNumberAsUCRoman: I, II, III, ...
        }

        [Flags]
        public enum PARAFORMAT_NUMBERINGSTYLE : ushort
        {
            // PARAFORMAT2 wNumberingStyle options 
            PFNS_PAREN = 0x000, // default, e.g., 1) 
            PFNS_PARENS = 0x100, // tomListParentheses/256, e.g., (1) 
            PFNS_PERIOD = 0x200, // tomListPeriod/256, e.g., 1. 
            PFNS_PLAIN = 0x300, // tomListPlain/256, e.g., 1 
            PFNS_NONUMBER = 0x400, // Used for continuation w/o number

            PFNS_NEWNUMBER = 0x8000, // Start new number with wNumberingStart (can be combined with other PFNS_xxx)
        }

        public enum PARAFORMAT_ALIGNMENT : ushort
        {
            // PARAFORMAT alignment options 
            PFA_LEFT = 1,
            PFA_RIGHT = 2,
            PFA_CENTER = 3,

            // PARAFORMAT2 alignment options 
            PFA_JUSTIFY = 4,
            PFA_FULL_INTERWORD = 4,
            PFA_FULL_INTERLETTER = 5,
            PFA_FULL_SCALED = 6,
            PFA_FULL_GLYPHS = 7,
            PFA_SNAP_GRID = 8,
        }

       

        [Flags]
        public enum CHARFORMAT_MASKS : uint
        {
            CFM_BOLD = 0x00000001,
            CFM_ITALIC = 0x00000002,
            CFM_UNDERLINE = 0x00000004,
            CFM_STRIKEOUT = 0x00000008,
            CFM_PROTECTED = 0x00000010,
            CFM_LINK = 0x00000020, // Exchange hyperlink extension 
            CFM_SIZE = 0x80000000,
            CFM_COLOR = 0x40000000,
            CFM_FACE = 0x20000000,
            CFM_OFFSET = 0x10000000,
            CFM_CHARSET = 0x08000000,

            CFM_SMALLCAPS = 0x0040,
            CFM_ALLCAPS = 0x0080,
            CFM_HIDDEN = 0x0100,
            CFM_OUTLINE = 0x0200,
            CFM_SHADOW = 0x0400,
            CFM_EMBOSS = 0x0800,
            CFM_IMPRINT = 0x1000,
            CFM_DISABLED = 0x2000,
            CFM_REVISED = 0x4000,

            CFM_BACKCOLOR = 0x04000000,
            CFM_LCID = 0x02000000,
            CFM_UNDERLINETYPE = 0x00800000,
            CFM_WEIGHT = 0x00400000,
            CFM_SPACING = 0x00200000,
            CFM_KERNING = 0x00100000,
            CFM_STYLE = 0x00080000,
            CFM_ANIMATION = 0x00040000,
            CFM_REVAUTHOR = 0x00008000,

            CFM_SUBSCRIPT = (CHARFORMAT_EFFECTS.CFE_SUBSCRIPT | CHARFORMAT_EFFECTS.CFE_SUPERSCRIPT),
            CFM_SUPERSCRIPT = CFM_SUBSCRIPT,

            //CFM_EFFECTS = (CFM_BOLD | CFM_ITALIC | CFM_UNDERLINE | CFM_COLOR | CFM_STRIKEOUT | CHARFORMAT_EFFECTS.CFE_PROTECTED | CFM_LINK),
            //CFM_ALL = (CFM_EFFECTS | CFM_SIZE | CFM_FACE | CFM_OFFSET | CFM_CHARSET),
            //CFM_EFFECTS2 = (CFM_EFFECTS | CFM_DISABLED | CFM_SMALLCAPS | CFM_ALLCAPS | CFM_HIDDEN | CFM_OUTLINE | CFM_SHADOW | CFM_EMBOSS | CFM_IMPRINT | CFM_DISABLED | CFM_REVISED | CFM_SUBSCRIPT | CFM_SUPERSCRIPT | CFM_BACKCOLOR),
            //CFM_ALL2 = (CFM_ALL | CFM_EFFECTS2 | CFM_BACKCOLOR | CFM_LCID | CFM_UNDERLINETYPE | CFM_WEIGHT | CFM_REVAUTHOR | CFM_SPACING | CFM_KERNING | CFM_STYLE | CFM_ANIMATION),
        }

        [Flags]
        public enum CHARFORMAT_EFFECTS : uint
        {
            CFE_BOLD = 0x0001,
            CFE_ITALIC = 0x0002,
            CFE_UNDERLINE = 0x0004,
            CFE_STRIKEOUT = 0x0008,
            CFE_PROTECTED = 0x0010,
            CFE_LINK = 0x0020,
            CFE_AUTOCOLOR = 0x40000000, // NOTE: this corresponds to CFM_COLOR, which controls it

            CFE_SUBSCRIPT = 0x00010000, // Superscript and subscript are mutually exclusive
            CFE_SUPERSCRIPT = 0x00020000,

            CFE_SMALLCAPS = CHARFORMAT_MASKS.CFM_SMALLCAPS,
            CFE_ALLCAPS = CHARFORMAT_MASKS.CFM_ALLCAPS,
            CFE_HIDDEN = CHARFORMAT_MASKS.CFM_HIDDEN,
            CFE_OUTLINE = CHARFORMAT_MASKS.CFM_OUTLINE,
            CFE_SHADOW = CHARFORMAT_MASKS.CFM_SHADOW,
            CFE_EMBOSS = CHARFORMAT_MASKS.CFM_EMBOSS,
            CFE_IMPRINT = CHARFORMAT_MASKS.CFM_IMPRINT,
            CFE_DISABLED = CHARFORMAT_MASKS.CFM_DISABLED,
            CFE_REVISED = CHARFORMAT_MASKS.CFM_REVISED,

            CFE_AUTOBACKCOLOR = CHARFORMAT_MASKS.CFM_BACKCOLOR, // CFE_AUTOCOLOR and CFE_AUTOBACKCOLOR correspond to CFM_COLOR and // CFM_BACKCOLOR, respectively, which control them
        }

        public enum CHARFORMAT_UNDERLINE : byte
        {
            CFU_CF1UNDERLINE = 0xFF, // Map charformat's bit underline to CF2
            CFU_INVERT = 0xFE, // For IME composition fake a selection

            CFU_UNDERLINETHICKLONGDASH = 18,
            CFU_UNDERLINETHICKDOTTED = 17,
            CFU_UNDERLINETHICKDASHDOTDOT = 16,
            CFU_UNDERLINETHICKDASHDOT = 15,
            CFU_UNDERLINETHICKDASH = 14,
            CFU_UNDERLINELONGDASH = 13,
            CFU_UNDERLINEHEAVYWAVE = 12,
            CFU_UNDERLINEDOUBLEWAVE = 11,
            CFU_UNDERLINEHAIRLINE = 10,
            CFU_UNDERLINETHICK = 9,
            CFU_UNDERLINEWAVE = 8,
            CFU_UNDERLINEDASHDOTDOT = 7,
            CFU_UNDERLINEDASHDOT = 6,
            CFU_UNDERLINEDASH = 5,
            CFU_UNDERLINEDOTTED = 4,
            CFU_UNDERLINEDOUBLE = 3,
            CFU_UNDERLINEWORD = 2,
            CFU_UNDERLINE = 1,
            CFU_UNDERLINENONE = 0,
        }

        public enum TextAlignment
        {
            Left = 1,
            Right,
            Center,
            Justify
        }

        public CHARFORMAT2W SelectionCharFormat
        {
            get { return GetCharFormat(true); }
            set { SetCharFormat(true, value); }
        }

        [DefaultValue(false)]
        public new bool DetectUrls
        {
            get { return base.DetectUrls; }
            set { base.DetectUrls = value; }
        }

		public RichTextBoxEx()
		{
            this.DetectUrls = false;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;
                if (LoadLibrary("msftedit.dll") != IntPtr.Zero)
                {
                    baseParams.ClassName = "RichEdit50W"; // "RICHEDIT50W";
                }
                return baseParams;
            }
        }

        //Overrides OnHandleCreated to enable RTB advances options
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // EM_SETTYPOGRAPHYOPTIONS allows to enable RTB (RichEdit) Advanced Typography
            SendMessage(this.Handle, EM_SETTYPOGRAPHYOPTIONS, TO_ADVANCEDTYPOGRAPHY, (IntPtr)TO_ADVANCEDTYPOGRAPHY);
        }

        public new TextAlignment SelectionAlignment
        {
            // SelectionAlignment is not overridable
            get
            {
                PARAFORMAT pf = new PARAFORMAT();
                pf.cbSize = Marshal.SizeOf(pf);

                SendMessage(new HandleRef(this, Handle),
                         EM_GETPARAFORMAT,
                         SCF_SELECTION,
                         ref pf
                       );

				if ((pf.dwMask & (uint)PARAFORMAT_MASKS.PFM_ALIGNMENT) == 0) return TextAlignment.Left;
				return (TextAlignment)pf.wAlignment;
			}
            set
            {
                PARAFORMAT fmt = new PARAFORMAT();
                fmt.cbSize = Marshal.SizeOf(fmt);
                fmt.dwMask = (uint)PARAFORMAT_MASKS.PFM_ALIGNMENT;
                fmt.wAlignment = (short)value;

                SendMessage(new HandleRef(this, Handle),
                             EM_SETPARAFORMAT,
                             SCF_SELECTION,
                             ref fmt
                           );
            }
        }

        /// <summary>Set the spacing from one line to the next in the selected Parragraph.</summary>
        /// <param name="bLineSpacingRule">Type of line spacing:
        /// 0=Single spacing.The dyLineSpacing member is ignored.
        /// 1=One-and-a-half spacing. The dyLineSpacing member is ignored.
        /// 2=Double spacing. The dyLineSpacing member is ignored.
        /// 3=The dyLineSpacing member specifies the spacingfrom one line to the next, in twips.However, if dyLineSpacing specifies a value that is less than single spacing, the control displays single-spaced text.
        /// 4=The dyLineSpacing member specifies the spacing from one line to the next, in twips.The control uses the exact spacing specified, even if dyLineSpacing specifies a value that is less than single spacing.
        /// 5=The value of dyLineSpacing / 20 is the spacing, in lines, from one line to the next. Thus, setting dyLineSpacing to 20 produces single-spaced text, 40 is double spaced, 60 is triple spaced, and so on.</param>
        /// <param name="dyLineSpacing">specifies the spacing from one line to the next, in twips, only when 'bLineSpacingRule' is > 2</param>
        public void SetLineFormat(byte bLineSpacingRule = 0, int dyLineSpacing = 100)
        {
            PARAFORMAT fmt = new PARAFORMAT();
            fmt.cbSize = Marshal.SizeOf(fmt);
            fmt.dwMask = (uint)PARAFORMAT_MASKS.PFM_LINESPACING;
            fmt.dyLineSpacing = dyLineSpacing;
            fmt.bLineSpacingRule = bLineSpacingRule;
          
            SendMessage(new HandleRef(this, Handle),
                         EM_SETPARAFORMAT,
                         SCF_SELECTION,
                         ref fmt
                       );
        }
       
        public PARAFORMAT GetSelectionFormat()
        {
            PARAFORMAT fmt = new PARAFORMAT();
            fmt.cbSize = Marshal.SizeOf(fmt);

            SendMessage(new HandleRef(this, Handle),
                         EM_GETPARAFORMAT,
                         SCF_SELECTION,
                         ref fmt
                       );
            return fmt;
        }
        public CHARFORMAT2W GetCharFormat(bool fSelection)
        {
            var format = new CHARFORMAT2W();
            format.cbSize = CHARFORMAT2W.SIZE;
            SendMessage(new HandleRef(this, Handle), EM_GETCHARFORMAT, (fSelection ? 1 : 0), ref format);
            return format;
        }

        public void SetCharFormat(bool fSelection, CHARFORMAT2W format)
        {
            format.cbSize = CHARFORMAT2W.SIZE;
            SendMessage(new HandleRef(this, Handle), EM_SETCHARFORMAT, (fSelection ? 1 : 0), ref format);
        }

        public int SetUndoLimit(int limit)
        {
            return (int)SendMessage(new HandleRef(this, Handle), EM_SETUNDOLIMIT, limit, 0);
        }

        public int GetCharacterIndexOfSelection()
        {
            var wordWrappedIndex = this.SelectionStart;

            RichTextBox scratch = new RichTextBox();
            scratch.Lines = this.Lines;
            scratch.SelectionStart = wordWrappedIndex;
            scratch.SelectionLength = 1;
            scratch.WordWrap = false;
            return scratch.SelectionStart;
        }
        public int GetLineNumberOfSelection()
        {
            var selectionStartIndex = GetCharacterIndexOfSelection();

            RichTextBox scratch = new RichTextBox();
            scratch.Lines = this.Lines;
            scratch.SelectionStart = selectionStartIndex;
            scratch.SelectionLength = 1;
            scratch.WordWrap = false;
            return scratch.GetLineFromCharIndex(selectionStartIndex);
        }

        /// <summary>
		/// Insert a given text as a link into the RichTextBox at the current insert position.
		/// </summary>
		/// <param name="text">Text to be inserted</param>
		public void InsertLink(string text)
        {
            InsertLink(text, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. 
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="position">Insert position</param>
        public void InsertLink(string text, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException("position");

            this.SelectionStart = position;
            this.SelectedText = text;
            this.Select(position, text.Length);
            this.SetSelectionLink(true);
            this.Select(position + text.Length, 0);
        }

        /// <summary>
        /// Insert a given text at at the current input position as a link.
        /// The link text is followed by a hash (#) and the given hyperlink text, both of
        /// them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        public void InsertLink(string text, string hyperlink)
        {
            InsertLink(text, hyperlink, this.SelectionStart);
        }

        /// <summary>
        /// Insert a given text at a given position as a link. The link text is followed by
        /// a hash (#) and the given hyperlink text, both of them invisible.
        /// When clicked on, the whole link text and hyperlink string are given in the
        /// LinkClickedEventArgs.
        /// </summary>
        /// <param name="text">Text to be inserted</param>
        /// <param name="hyperlink">Invisible hyperlink string to be inserted</param>
        /// <param name="position">Insert position</param>
        //public void InsertLink(string text, string hyperlink, int position)
        //{
        //    //if (position < 0 || position > this.Text.Length)
        //    //    throw new ArgumentOutOfRangeException("position");

        //    this.SelectionStart = position;
        //    this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
        //    this.Select(position, text.Length + hyperlink.Length + 1);
        //    this.SetSelectionLink(true);
        //    this.Select(position + text.Length + hyperlink.Length + 1, 0);
        //}
        public void InsertLink(string text, string hyperlink, int position)
        {
            if (position < 0 || position > this.Text.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            this.SelectionStart = position;
            //this.SelectedRtf = @"{\rtf1\ansi " + text + @"\v #" + hyperlink + @"\v0}";
            this.SelectedRtf =
                @"{\rtf1\ansi\ansicpg1252\deff0\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;}}" +
                @"{\colortbl ;\red0\green0\blue255;}{\*\generator Msftedit 5.41.21.2509;}" +
                @"\viewkind4\uc1\pard\sa200\sl276\slmult1\lang9\f0\fs22{\field{\*\fldinst{HYPERLINK """ + hyperlink + @"""}}" +
                @"{\fldrslt{\ul\cf1 " + text + @"}}}\f0\fs22}";
            this.Select(position, text.Length + hyperlink.Length + 1);
            this.SetSelectionLink(true);
            this.Select(position + text.Length + hyperlink.Length + 1, 0);
        }

        /// <summary>
        /// Set the current selection's link style
        /// </summary>
        /// <param name="link">true: set link style, false: clear link style</param>
        public void SetSelectionLink(bool link)
        {
            SetSelectionStyle(CFM_LINK, link ? CFE_LINK : 0);
        }
        /// <summary>
        /// Get the link style for the current selection
        /// </summary>
        /// <returns>0: link style not set, 1: link style set, -1: mixed</returns>
        public int GetSelectionLink()
        {
            return GetSelectionStyle(CFM_LINK, CFE_LINK);
        }


        private void SetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.dwMask = mask;
            cf.dwEffects = effect;

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_SETCHARFORMAT, wpar, lpar);

            Marshal.FreeCoTaskMem(lpar);
        }

        private int GetSelectionStyle(UInt32 mask, UInt32 effect)
        {
            CHARFORMAT2_STRUCT cf = new CHARFORMAT2_STRUCT();
            cf.cbSize = (UInt32)Marshal.SizeOf(cf);
            cf.szFaceName = new char[32];

            IntPtr wpar = new IntPtr(SCF_SELECTION);
            IntPtr lpar = Marshal.AllocCoTaskMem(Marshal.SizeOf(cf));
            Marshal.StructureToPtr(cf, lpar, false);

            IntPtr res = SendMessage(Handle, EM_GETCHARFORMAT, wpar, lpar);

            cf = (CHARFORMAT2_STRUCT)Marshal.PtrToStructure(lpar, typeof(CHARFORMAT2_STRUCT));

            int state;
            // dwMask holds the information which properties are consistent throughout the selection:
            if ((cf.dwMask & mask) == mask)
            {
                if ((cf.dwEffects & effect) == effect)
                    state = 1;
                else
                    state = 0;
            }
            else
            {
                state = -1;
            }

            Marshal.FreeCoTaskMem(lpar);
            return state;
        }

        /// <summary>
        /// Calculate or render the contents of our RichTextBox for printing
        /// </summary>
        /// <param name="measureOnly">If true, only the calculation is performed, otherwise the text is rendered as well</param>
        /// <param name="e">The PrintPageEventArgs object from the PrintPage event</param>
        /// <param name="charFrom">Index of first character to be printed</param>
        /// <param name="charTo">Index of last character to be printed</param>
        /// <returns> (Index of last character that fitted on the page) + 1</returns>
        public int FormatRange(bool measureOnly, PrintPageEventArgs e, int charFrom, int charTo)
        {
            // Specify which characters to print
            STRUCT_CHARRANGE cr = default(STRUCT_CHARRANGE);
            cr.cpMin = charFrom;
            cr.cpMax = charTo;

            // Specify the area inside page margins
            STRUCT_RECT rc = default(STRUCT_RECT);
            rc.top = HundredthInchToTwips(e.MarginBounds.Top);
            rc.bottom = HundredthInchToTwips(e.MarginBounds.Bottom);
            rc.left = HundredthInchToTwips(e.MarginBounds.Left);
            rc.right = HundredthInchToTwips(e.MarginBounds.Right);

            // Specify the page area
            STRUCT_RECT rcPage = default(STRUCT_RECT);
            rcPage.top = HundredthInchToTwips(e.PageBounds.Top);
            rcPage.bottom = HundredthInchToTwips(e.PageBounds.Bottom);
            rcPage.left = HundredthInchToTwips(e.PageBounds.Left);
            rcPage.right = HundredthInchToTwips(e.PageBounds.Right);

            // Get device context of output device
            IntPtr hdc = default(IntPtr);
            hdc = e.Graphics.GetHdc();

            // Fill in the FORMATRANGE structure
            STRUCT_FORMATRANGE fr = default(STRUCT_FORMATRANGE);
            fr.chrg = cr;
            fr.hdc = hdc;
            fr.hdcTarget = hdc;
            fr.rc = rc;
            fr.rcPage = rcPage;

            // Non-Zero wParam means render, Zero means measure
            Int32 wParam = default(Int32);
            if (measureOnly)
            {
                wParam = 0;
            }
            else
            {
                wParam = 1;
            }

            // Allocate memory for the FORMATRANGE struct and
            // copy the contents of our struct to this memory
            IntPtr lParam = default(IntPtr);
            lParam = Marshal.AllocCoTaskMem(Marshal.SizeOf(fr));
            Marshal.StructureToPtr(fr, lParam, false);

            // Send the actual Win32 message
            int res = 0;
            res = SendMessage(Handle, EM_FORMATRANGE, wParam, lParam);

            // Free allocated memory
            Marshal.FreeCoTaskMem(lParam);

            // and release the device context
            e.Graphics.ReleaseHdc(hdc);

            return res;
        }

        /// <summary>
        /// Convert between 1/100 inch (unit used by the .NET framework)
        /// and twips (1/1440 inch, used by Win32 API calls)
        /// </summary>
        /// <param name="n">Value in 1/100 inch</param>
        /// <returns>Value in twips</returns>
        private Int32 HundredthInchToTwips(int n)
        {
            return Convert.ToInt32(n * 14.4);
        }

        /// <summary>
        /// Free cached data from rich edit control after printing
        /// </summary>
        public void FormatRangeDone()
        {
            IntPtr lParam = new IntPtr(0);
            SendMessage(Handle, EM_FORMATRANGE, 0, lParam);
        }
    }



    /// <summary>Un Cuadro de Texto que puede cambiar de tamaño en tiempo de ejecucion.</summary>
    public class ResizeableTextBox : TextBox
    {
        private const long WS_BORDER =	0x00800000L;
        private const long WS_THICKFRAME = 0x00040000L;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.Style |= 0x840000;  // Turn on WS_BORDER + WS_THICKFRAME
                return cp;
            }
        }
    }
}
