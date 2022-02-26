using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.IO.Packaging;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Security.Policy;

namespace IATClient
{
    public class LayoutItem : Enumeration
    {
        public static readonly LayoutItem None = new LayoutItem(0, "None", new Func<Rectangle>(() => Rectangle.Empty));
        public static readonly LayoutItem Lambda = new LayoutItem(1, "Lambda", new Func<Rectangle>(() => new Rectangle(0, 0, CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height)));
        public static readonly LayoutItem Stimulus = new LayoutItem(2, "Stimulus", new Func<Rectangle>(() => CIAT.SaveFile.Layout.StimulusRectangle));
        public static readonly LayoutItem LeftResponseKey = new LayoutItem(3, "LeftResponseKey", new Func<Rectangle>(() => CIAT.SaveFile.Layout.LeftKeyValueRectangle));
        public static readonly LayoutItem RightResponseKey = new LayoutItem(4, "RightResponseKey", new Func<Rectangle>(() => CIAT.SaveFile.Layout.RightKeyValueRectangle));
        public static readonly LayoutItem BlockInstructions = new LayoutItem(5, "BlockInstructions", new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionsRectangle));
        public static readonly LayoutItem ErrorMark = new LayoutItem(6, "ErrorMark", new Func<Rectangle>(() => CIAT.SaveFile.Layout.ErrorRectangle));
        public static readonly LayoutItem ContinueInstructions = new LayoutItem(7, "ContinueInstructions", new Func<Rectangle>(() => CIAT.SaveFile.Layout.ContinueInstructionsRectangle));
        public static readonly LayoutItem TextInstructionScreen = new LayoutItem(8, "TextInstructionScreen", new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle));
        public static readonly LayoutItem KeyedInstructionScreen = new LayoutItem(9, "KeyedInstructionScreen", new Func<Rectangle>(() => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle));
        public static readonly LayoutItem MockItemInstructions = new LayoutItem(10, "MockItemInstructions", new Func<Rectangle>(() => CIAT.SaveFile.Layout.MockItemInstructionsRectangle));
        public static readonly LayoutItem LeftResponseKeyOutline = new LayoutItem(11, "LeftResponseKeyOutline", new Func<Rectangle>(() => CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle));
        public static readonly LayoutItem RightResponseKeyOutline = new LayoutItem(11, "RightResponseKeyOutline", new Func<Rectangle>(() => CIAT.SaveFile.Layout.RightKeyValueOutlineRectangle));
        public static readonly LayoutItem FullWindow = new LayoutItem(12, "FullWindow", new Func<Rectangle>(() => new Rectangle(new Point(0, 0), CIAT.SaveFile.Layout.InteriorSize)));


        private Func<Rectangle> GetBoundingRectangle { get; set; }
        public Rectangle BoundingRectangle
        {
            get
            {
                return GetBoundingRectangle();
            }
        }

        private LayoutItem(int id, String name, Func<Rectangle> bRect)
            : base(id, name)
        {
            GetBoundingRectangle = bRect;
        }

        private readonly static LayoutItem[] All = new LayoutItem[] { Lambda, Stimulus, LeftResponseKey, RightResponseKey, BlockInstructions, ErrorMark, ContinueInstructions, 
            TextInstructionScreen, KeyedInstructionScreen, MockItemInstructions, LeftResponseKeyOutline, RightResponseKeyOutline };
        public static LayoutItem FromString(String str)
        {
            return All.Where((lc) => str == lc.Name).First();
        }
    }


    public class CIATLayout : IStoredInXml, IPackagePart, IDisposable
    {
        /// <summary>
        /// A structure of constant values that defines bitmasks for rectangle overlap
        /// </summary>
        public struct Overlap
        {
            public const uint StimulusRectangle = 0x1;
            public const uint KeyValueRectangles = 0x2;
            public const uint ErrorRectangle = 0x4;
            public const uint InstructionRectangle = 0x8;
        };

        public static float xDpi { get; private set; }
        public static float yDpi { get; private set; } 

        static CIATLayout()
        {
            if (Application.OpenForms[Properties.Resources.sMainFormName].IsHandleCreated)
            {
                using (Graphics g = Graphics.FromHwnd(Application.OpenForms[Properties.Resources.sMainFormName].Handle))
                {
                    xDpi = g.DpiX;
                    yDpi = g.DpiY;
                }
            } else
            {
                Application.OpenForms[Properties.Resources.sMainFormName].HandleCreated += (sender, args) =>
                {
                    using (Graphics g = Graphics.FromHwnd(Application.OpenForms[Properties.Resources.sMainFormName].Handle))
                    {
                        xDpi = g.DpiX;
                        yDpi = g.DpiY;
                    }
                };
            }
        }

        // default values for rectangle sizes
        private static Size DefaultInteriorSize = new Size(600, 600);
        private static Size DefaultStimulusSize = new Size(540, 300);
        private static Size DefaultKeyValueSize = new Size(200, 120);
        private static Size DefaultInstructionsSize = new Size(570, 80);
        private static Size DefaultErrorSize = new Size(50, 50);
        public static readonly FontFamily DefaultErrorMarkFontFamily = FontFamily.GenericSansSerif;
        private static Size DefaultContinueInstructionsSize = new Size(570, 30);
        private static ObservableUri _ErrorMarkObservableUri = null;
        public static ObservableUri ErrorMarkObservableUri
        {
            get
            {
                if (_ErrorMarkObservableUri == null)
                    _ErrorMarkObservableUri = new ObservableUri();
                return _ErrorMarkObservableUri;
            } 
            private set
            {
                if (_ErrorMarkObservableUri != null)
                    _ErrorMarkObservableUri.Dispose();
                _ErrorMarkObservableUri = value;
            }
        }
        private static ObservableUri _LeftKeyValueOutlineObservableUri = null;
        public static ObservableUri LeftKeyValueOutlineObservableUri
        {
            get
            {
                if (_LeftKeyValueOutlineObservableUri == null)
                    _LeftKeyValueOutlineObservableUri = new ObservableUri();
                return _LeftKeyValueOutlineObservableUri;
            } 
            private set
            {
                if (_LeftKeyValueOutlineObservableUri != null)
                    _LeftKeyValueOutlineObservableUri.Dispose();
                _LeftKeyValueOutlineObservableUri = value;
            }
        }
        private static ObservableUri _RightKeyValueOutlineObservableUri = null;
        public static ObservableUri RightKeyValueOutlineObservableUri
        {
            get
            {
                if (_RightKeyValueOutlineObservableUri == null)
                    _RightKeyValueOutlineObservableUri = new ObservableUri();
                return _RightKeyValueOutlineObservableUri;
            }
            private set
            {
                if (_RightKeyValueOutlineObservableUri != null)
                    _RightKeyValueOutlineObservableUri.Dispose();
                _RightKeyValueOutlineObservableUri = value;
            }
        }
        public static IUri IErrorMarkUri { get { return new UriObserver(ErrorMarkObservableUri); } }
        public static IUri ILeftKeyValueOutlineUri { get { return new UriObserver(LeftKeyValueOutlineObservableUri); } }
        public static IUri IRightKeyValueOutlineUri { get { return new UriObserver(RightKeyValueOutlineObservableUri); } }
        public Type BaseType { get { return typeof(CIATLayout); } }
        public String MimeType { get { return "text/xml+" + typeof(CIATLayout).ToString(); } }
        public Uri URI { get; set; }

        // miscellaneous default values
        private static System.Drawing.Color DefaultBackColor = System.Drawing.Color.Black;
        private static System.Drawing.Color DefaultBorderColor = System.Drawing.Color.LightYellow;
        private static System.Drawing.Color DefaultOutlineColor = System.Drawing.Color.LimeGreen;
        private static System.Drawing.Color DefaultWebpageBackColor = System.Drawing.Color.Black;
        private static int DefaultBorderWidth = 10;
        private static int DefaultTextStimulusPaddingTop = 50;

        public Size TotalSize
        {
            get
            {
                return new Size(_InteriorSize.Width, _InteriorSize.Height) + new Size(_BorderWidth * 2, _BorderWidth * 2);
            }
        }

        // the interior size of the IAT box, not including the border
        private Size _InteriorSize;

        /// <summary>
        /// gets or sets the interior size of the IAT box
        /// </summary>
        public Size InteriorSize
        {
            get
            {
                return new Size(_InteriorSize.Width, _InteriorSize.Height);
            }
            set
            {
                _InteriorSize = value;
                CalcAllRectangles();
            }
        }

        // the size of the key value box
        private Size _KeyValueSize;

        /// <summary>
        /// gets or sets the size of the key value rectangles
        /// </summary>
        public Size KeyValueSize
        {
            get
            {
                return new Size(_KeyValueSize.Width, _KeyValueSize.Height);
            }
            set
            {
                _KeyValueSize = value;
                CalcAllRectangles();
            }
        }

        // the key value rectangles
        private Rectangle _LeftKeyValueRectangle, _RightKeyValueRectangle;

        /// <summary>
        /// gets the left key value bounding rectangle
        /// </summary>
        public Rectangle LeftKeyValueRectangle
        {
            get
            {
                return new Rectangle(_LeftKeyValueRectangle.X, _LeftKeyValueRectangle.Y, _LeftKeyValueRectangle.Width, _LeftKeyValueRectangle.Height);
            }
        }

        public Rectangle LeftKeyValueOutlineRectangle
        {
            get
            {
                Rectangle r = LeftKeyValueRectangle;
                r.Inflate(3, 3);
                return r;
            }
        }

        /// <summary>
        /// gets the right key value bounding rectangle
        /// </summary>
        public Rectangle RightKeyValueRectangle
        {
            get
            {
                return new Rectangle(_RightKeyValueRectangle.X, _RightKeyValueRectangle.Y, _RightKeyValueRectangle.Width, _RightKeyValueRectangle.Height);
            }
        }

        public Rectangle RightKeyValueOutlineRectangle
        {
            get
            {
                Rectangle r = RightKeyValueRectangle;
                r.Inflate(3, 3);
                return r;
            }
        }



        // the size of the stimulus box
        private Size _StimulusSize;

        /// <summary>
        /// gets or sets the maximum size of a stimulus
        /// </summary>
        public Size StimulusSize
        {
            get
            {
                return new Size(_StimulusSize.Width, _StimulusSize.Height);
            }
            set
            {
                _StimulusSize = value;
                CalcAllRectangles();
            }
        }

        // the stimulus rectangle
        private Rectangle _StimulusRectangle;

        /// <summary>
        /// gets the stimulus bounding rectangle
        /// </summary>
        public Rectangle StimulusRectangle
        {
            get
            {
                return new Rectangle(_StimulusRectangle.X, _StimulusRectangle.Y, _StimulusRectangle.Width, _StimulusRectangle.Height);
            }
        }

        // the size of the instruction box
        private Size _InstructionsSize;

        /// <summary>
        /// gets of sets the size of the instructions in the IAT window
        /// </summary>
        public Size InstructionsSize
        {
            get
            {
                return new Size(_InstructionsSize.Width, _InstructionsSize.Height);
            }
            set
            {
                _InstructionsSize = value;
                CalcAllRectangles();
            }
        }

        // the instruction rectangle
        private Rectangle _InstructionsRectangle;

        /// <summary>
        /// gets the Instructions bounding rectangle
        /// </summary>
        public Rectangle InstructionsRectangle
        {
            get
            {
                return new Rectangle(_InstructionsRectangle.X, _InstructionsRectangle.Y, _InstructionsRectangle.Width, _InstructionsRectangle.Height);

            }
        }


        // the size of the error rectangle
        private Size _ErrorSize;

        /// <summary>
        /// gets or sets the size of the error rectangle
        /// </summary>
        public Size ErrorSize
        {
            get
            {
                return new Size(_ErrorSize.Width, _ErrorSize.Height);
            }
            set
            {
                _ErrorSize = value;
                CalcAllRectangles();
            }
        }

        // the error rectangle
        private Rectangle _ErrorRectangle;

        public Rectangle ErrorRectangle
        {
            get
            {
                return new Rectangle(_ErrorRectangle.X, _ErrorRectangle.Y, _ErrorRectangle.Width, _ErrorRectangle.Height);
            }
        }

        private Uri _ErrorMarkUri;
        private Uri ErrorMarkUri
        {
            get
            {
                if (_ErrorMarkUri == null)
                    _ErrorMarkUri = new DIErrorMark().URI;
                return _ErrorMarkUri;
            }
            set
            {
                if (value == null)
                {
                    if (_ErrorMarkUri == null)
                        return;
                    ErrorMarkObservableUri.Value = null;
                    ErrorMark.Dispose();
                    _ErrorMarkUri = null;
                }
                else if (value.Equals(_ErrorMarkUri))
                    return;
                if (_ErrorMarkUri != null)
                    CIAT.SaveFile.GetDI(_ErrorMarkUri).Dispose();
                
                if ((value != null) && (ErrorMarkObservableUri.Value.Equals(_ErrorMarkUri)))
                    ErrorMarkObservableUri.Value = value;
                _ErrorMarkUri = value;
            }
        }
        public DIErrorMark ErrorMark
        {
            get
            {
                return CIAT.SaveFile.GetDI(ErrorMarkUri) as DIErrorMark;
            } 
        }

        private Uri _LeftKeyValueOutlineUri = null;
        private Uri LeftKeyValueOutlineUri
        {
            get
            {
                if (_LeftKeyValueOutlineUri == null)
                    _LeftKeyValueOutlineUri = new DIKeyValueOutline(KeyedDirection.Left).URI;
                return _LeftKeyValueOutlineUri;
            } set
            {
                if ((value == null) && (_LeftKeyValueOutlineUri != null))
                {
                    _LeftKeyValueOutlineObservableUri.Value = null;
                    LeftKeyValueOutline.Dispose();
                    _LeftKeyValueOutlineUri = null;
                    return;
                }
                    LeftKeyValueOutlineObservableUri.Value  = value; 
                _LeftKeyValueOutlineUri = value;
            }
        }
        public DIKeyValueOutline LeftKeyValueOutline
        {
            get
            {
                return CIAT.SaveFile.GetDI(LeftKeyValueOutlineUri) as DIKeyValueOutline;
            }
        }

        private Uri _RightKeyValueOutlineUri = null;
        private Uri RightKeyValueOutlineUri
        {
            get
            {
                if (_RightKeyValueOutlineUri == null)
                    _RightKeyValueOutlineUri = new DIKeyValueOutline(KeyedDirection.Right).URI;
                return _RightKeyValueOutlineUri;
            }
            set
            {
                if ((value == null) && (RightKeyValueOutlineUri != null))
                {
                    RightKeyValueOutlineObservableUri.Value = null;
                    RightKeyValueOutline.Dispose();
                    _RightKeyValueOutlineUri = null;
                    return;
                }
                if (RightKeyValueOutlineObservableUri.Value.Equals(_RightKeyValueOutlineUri))
                    RightKeyValueOutlineObservableUri.Value = value;
                _RightKeyValueOutlineUri = value;
            }
        }
        public DIKeyValueOutline RightKeyValueOutline
        {
            get
            {
                return CIAT.SaveFile.GetDI(RightKeyValueOutlineUri) as DIKeyValueOutline;
            }
        }

        // the width of the IAT box border
        private int _BorderWidth;

        /// <summary>
        /// gets or sets the width of the IAT box border
        /// </summary>
        public int BorderWidth
        {
            get
            {
                return _BorderWidth;
            }
            set
            {
                _BorderWidth = value;
            }
        }

        // the background color of the IAT box 
        private System.Drawing.Color _BackColor;

        /// <summary>
        /// gets or sets the background color of the IAT box
        /// </summary>
        public System.Drawing.Color BackColor
        {
            get
            {
                return _BackColor;
            }
            set
            {
                _BackColor = value;

            }
        }

        // the border color of the IAT box
        private System.Drawing.Color _BorderColor;

        /// <summary>
        /// gets or sets the border color of the IAT box
        /// </summary>
        public System.Drawing.Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
            }
        }

        // the color of outlining
        private System.Drawing.Color _OutlineColor;

        /// <summary>
        /// gets or sets the outline color
        /// </summary>
        public System.Drawing.Color OutlineColor
        {
            get
            {
                return _OutlineColor;
            }
            set
            {
                _OutlineColor = value;
            }
        }

        private Color _WebpageBackColor;

        public Color WebpageBackColor
        {
            get
            {
                return _WebpageBackColor;
            }
            set
            {
                _WebpageBackColor = value;
            }
        }

        private Image _WebpageBackgroundImage = null;
        private String _WebpageBackgroundImageFilename = String.Empty;
        private System.Drawing.Imaging.ImageFormat WebpageImageFormat;

        public String WebpageBackgroundImageFilename
        {
            get
            {
                return _WebpageBackgroundImageFilename;
            }
            set
            {
                _WebpageBackgroundImageFilename = value;
                _WebpageBackgroundImage = Image.FromFile(value);
                String extension = value.Substring(value.LastIndexOf(".") + 1);
                switch (extension.ToLower())
                {
                    case "bmp": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                    case "emf": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Emf; break;
                    case "exif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Exif; break;
                    case "gif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Gif; break;
                    case "ico": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Icon; break;
                    case "jpeg": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                    case "jpg": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                    case "png": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Png; break;
                    case "tif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                    case "tiff": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                    case "wmf": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Wmf; break;
                    default:
                        WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Png; break;
                }
            }
        }

        public Image WebpageBackgroundImage
        {
            get
            {
                return _WebpageBackgroundImage;
            }
        }

        public bool WebpageImage
        {
            get
            {
                return (_WebpageBackgroundImage != null);
            }
        }

        public class EWebpageBackgroundImageOrientation
        {
            public enum _EWebpageBackgroundImageOrientation
            {
                none = 1,
                Tiled = 2,
                Centered = 3,
                Left = 3,
                Top = 4,
                Right = 5,
                Bottom = 6,
                UpperLeft = 7,
                UpperRight = 8,
                LowerLeft = 9,
                LowerRight = 10
            };
            private _EWebpageBackgroundImageOrientation EnumValue;
            private String EnumValueName = String.Empty;

            public static readonly EWebpageBackgroundImageOrientation none = new EWebpageBackgroundImageOrientation("None", _EWebpageBackgroundImageOrientation.none);
            public static readonly EWebpageBackgroundImageOrientation Tiled = new EWebpageBackgroundImageOrientation("Tiled", _EWebpageBackgroundImageOrientation.Tiled);
            public static readonly EWebpageBackgroundImageOrientation Centered = new EWebpageBackgroundImageOrientation("Centered", _EWebpageBackgroundImageOrientation.Centered);
            public static readonly EWebpageBackgroundImageOrientation Left = new EWebpageBackgroundImageOrientation("Left", _EWebpageBackgroundImageOrientation.Left);
            public static readonly EWebpageBackgroundImageOrientation Top = new EWebpageBackgroundImageOrientation("Top", _EWebpageBackgroundImageOrientation.Top);
            public static readonly EWebpageBackgroundImageOrientation Right = new EWebpageBackgroundImageOrientation("Right", _EWebpageBackgroundImageOrientation.Right);
            public static readonly EWebpageBackgroundImageOrientation Bottom = new EWebpageBackgroundImageOrientation("Bottom", _EWebpageBackgroundImageOrientation.Bottom);
            public static readonly EWebpageBackgroundImageOrientation UpperLeft = new EWebpageBackgroundImageOrientation("Upper Left", _EWebpageBackgroundImageOrientation.UpperLeft);
            public static readonly EWebpageBackgroundImageOrientation UpperRight = new EWebpageBackgroundImageOrientation("Upper Right", _EWebpageBackgroundImageOrientation.UpperRight);
            public static readonly EWebpageBackgroundImageOrientation LowerLeft = new EWebpageBackgroundImageOrientation("Lower Left", _EWebpageBackgroundImageOrientation.LowerLeft);
            public static readonly EWebpageBackgroundImageOrientation LowerRight = new EWebpageBackgroundImageOrientation("Lower Right", _EWebpageBackgroundImageOrientation.LowerRight);

            protected EWebpageBackgroundImageOrientation(String enumValueName, _EWebpageBackgroundImageOrientation enumValue)
            {
                EnumValue = enumValue;
                EnumValueName = enumValueName;
            }

            public override String ToString()
            {
                return EnumValueName;
            }
        }

        private EWebpageBackgroundImageOrientation _WebpageImageLayout;

        public EWebpageBackgroundImageOrientation WebpageImageLayout
        {
            get
            {
                return _WebpageImageLayout;
            }
            set
            {
                _WebpageImageLayout = value;
            }
        }

        // the top padding for a text stimulus
        private int _TextStimulusPaddingTop;

        /// <summary>
        /// gets or sets the top padding for a text stimulus
        /// </summary>
        public int TextStimulusPaddingTop
        {
            get
            {
                return _TextStimulusPaddingTop;
            }
            set
            {
                _TextStimulusPaddingTop = value;
            }
        }

        private int _ResponseValueRectMargin
        {
            get
            {
                return ((_KeyValueSize.Width / 25 + _KeyValueSize.Height / 25) >> 1);
            }
        }

        /// <summary>
        /// gets or sets the margin around the response value rectangle
        /// </summary>
        public int ResponseValueRectMargin
        {
            get
            {
                return _ResponseValueRectMargin;
            }
        }

        // the size of the continue instruction rectangle at the bottom of an instruction screen
        private Size _ContinueInstructionsSize;

        /// <summary>
        /// gets or sets the size of the continue instrucction rectangle at the bottom of an instruction screen
        /// </summary>
        public Size ContinueInstructionsSize
        {
            get
            {
                return new Size(_ContinueInstructionsSize.Width, _ContinueInstructionsSize.Height);
            }
            set
            {
                _ContinueInstructionsSize = value;
            }
        }



        /// <summary>
        /// gets the instruction text area rectangle for an instruction screen in an IAT
        /// </summary>
        public Rectangle InstructionScreenTextAreaRectangle
        {
            get
            {
                Rectangle InstructionsRect = new Rectangle(new Point(0, 0), _InteriorSize);
                InstructionsRect.Inflate(-25, -25);
                InstructionsRect.Height -= _ContinueInstructionsSize.Height;
                return new Rectangle(InstructionsRect.X, InstructionsRect.Y, InstructionsRect.Width, InstructionsRect.Height);
            }
        }

        /// <summary>
        /// gets the instruction text area rectangle for an instruction screen that includes a response key
        /// </summary>
        public Rectangle KeyInstructionScreenTextAreaRectangle
        {
            get
            {
                Rectangle InstructionsRect = new Rectangle(new Point(0, 0), _InteriorSize);
                InstructionsRect.Inflate(-25, -25);
                InstructionsRect.Height -= _ContinueInstructionsSize.Height;
                InstructionsRect.Y += _KeyValueSize.Height;
                InstructionsRect.Height -= _KeyValueSize.Height;
                return new Rectangle(InstructionsRect.X, InstructionsRect.Y, InstructionsRect.Width, InstructionsRect.Height);
            }
        }

        /// <summary>
        /// gets the bounding rectangle for instructions to accompany a mock item in an instruction screen
        /// </summary>
        public Rectangle MockItemInstructionsRectangle
        {
            get
            {
                Rectangle InstructionsRect = _InstructionsRectangle;
                InstructionsRect.Height -= _ContinueInstructionsSize.Height;
                return new Rectangle(InstructionsRect.X, InstructionsRect.Y, InstructionsRect.Width, InstructionsRect.Height);
            }
        }

        /// <summary>
        /// gets the rectangle for the "continue instructions" for an instruction screen in an IAT
        /// </summary>
        public Rectangle ContinueInstructionsRectangle
        {
            get
            {
                return new Rectangle(0, InteriorSize.Height - ContinueInstructionsSize.Height, InteriorSize.Width, ContinueInstructionsSize.Height);
            }
        }

        /// <summary>
        /// Calculates the Instructions bounding rectangle
        /// </summary>
        private void CalcInstructionsRectangle()
        {
            _InstructionsRectangle.X = (_InteriorSize.Width - _InstructionsSize.Width) >> 1;
            _InstructionsRectangle.Y = _InteriorSize.Height - _InstructionsSize.Height;
            _InstructionsRectangle.Width = _InstructionsSize.Width;
            _InstructionsRectangle.Height = _InstructionsSize.Height;
        }

        /// <summary>
        /// Calculates the Key Value bounding rectangles
        /// </summary>
        private void CalcKeyValueRectangles()
        {
            _LeftKeyValueRectangle.X = _ResponseValueRectMargin;
            _LeftKeyValueRectangle.Y = _ResponseValueRectMargin;
            _LeftKeyValueRectangle.Size = _KeyValueSize;

            _RightKeyValueRectangle.X = _InteriorSize.Width - _KeyValueSize.Width - ResponseValueRectMargin - 1;
            _RightKeyValueRectangle.Y = _ResponseValueRectMargin;
            _RightKeyValueRectangle.Size = _KeyValueSize;
        }

        /// <summary>
        /// Calculate the best position for the stimulus rectangle
        /// </summary>
        private void CalcStimulusRectangle()
        {
            // check for a stimulus rectangle that can fit between the key value rectangles
            if (_StimulusSize.Width <= _InteriorSize.Width - (_KeyValueSize.Width << 1) - (ResponseValueRectMargin << 2))
            {
                int nFreeVerticalSpace = _InteriorSize.Height - _StimulusSize.Height - _InstructionsSize.Height - _ErrorSize.Height;
                // if there is no free vertical space, assign 0 as the top of the rectangle
                if (nFreeVerticalSpace < 0)
                    _StimulusRectangle.Y = 0;
                // else put half the free vertical space above the top of the stimulus rectangle
                else
                    _StimulusRectangle.Y = nFreeVerticalSpace >> 1;
                _StimulusRectangle.X = (_InteriorSize.Width - _StimulusSize.Width) >> 1;
                _StimulusRectangle.Size = _StimulusSize;

            }
            // else put the stimulus rectangle just below key value rectangles, if it doesn't overflow the bottom of the
            // interior rectangle
            else if (_StimulusSize.Height + _KeyValueSize.Height + (ResponseValueRectMargin << 1) < _InteriorSize.Height)
            {
                // check for free space, again putting half of it above the stimulus rectangle
                int nFreeVerticalSpace = _InteriorSize.Height - _StimulusSize.Height - _InstructionsSize.Height - _ErrorSize.Height - _KeyValueSize.Height
                    - (ResponseValueRectMargin << 1);
                if (nFreeVerticalSpace > 0)
                    _StimulusRectangle.Y = _KeyValueSize.Height + (ResponseValueRectMargin << 1) + (nFreeVerticalSpace >> 1);
                // else put the stimulus rectangle at the bottom of the key value rectangles
                else
                    _StimulusRectangle.Y = _KeyValueSize.Height + (ResponseValueRectMargin << 1);

                // assign remaining values, centering stimulus rectangle horizontally
                _StimulusRectangle.X = (_InteriorSize.Width - _StimulusSize.Width) >> 1;
                _StimulusRectangle.Size = _StimulusSize;
            }
            // else center the stimulus rectangle horizontally and vertically
            else
            {
                _StimulusRectangle.Y = (_InteriorSize.Height - _StimulusSize.Height) >> 1;
                _StimulusRectangle.X = (_InteriorSize.Width - _StimulusSize.Width) >> 1;
                _StimulusRectangle.Size = _StimulusSize;
            }
        }

        private void CalcErrorRectangle()
        {
            // calculate a midpoint for the error rectangle and place it, hoping for the best
            Point midPoint = new Point(_InteriorSize.Width >> 1, _StimulusRectangle.Bottom + (Math.Abs(_InstructionsRectangle.Top - _StimulusRectangle.Bottom) >> 1));
            _ErrorRectangle.X = midPoint.X - (_ErrorSize.Width >> 1);
            _ErrorRectangle.Y = midPoint.Y - (_ErrorSize.Height >> 1);
            _ErrorRectangle.Size = _ErrorSize;
        }

        private bool LayoutSuspended { get; set; } = false;
        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public void ResumeLayout()
        {
            LayoutSuspended = false;
            CalcAllRectangles();
        }

        /// <summary>
        /// Calculates all the bounding rectangles
        /// </summary>
        public void CalcAllRectangles()
        {
            if (!LayoutSuspended)
            {
                // WARNING -- do not change order of function calls
                CalcInstructionsRectangle();
                CalcKeyValueRectangles();
                CalcStimulusRectangle();
                CalcErrorRectangle();
            }
        }

        public void InvalidateMarks()
        {
            ErrorMark?.ScheduleInvalidation();
            LeftKeyValueOutline?.ScheduleInvalidation();
            RightKeyValueOutline?.ScheduleInvalidation();
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CIATLayout()
        {
            this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            this._BackColor = DefaultBackColor;
            this._BorderColor = DefaultBorderColor;
            this._WebpageBackColor = DefaultWebpageBackColor;
            this._BorderWidth = DefaultBorderWidth;
            this._InteriorSize = DefaultInteriorSize;
            this._StimulusSize = DefaultStimulusSize;
            this._InstructionsSize = DefaultInstructionsSize;
            this._KeyValueSize = DefaultKeyValueSize;
            this._ErrorSize = DefaultErrorSize;
            this._TextStimulusPaddingTop = DefaultTextStimulusPaddingTop;
            this._ContinueInstructionsSize = DefaultContinueInstructionsSize;
            this._OutlineColor = DefaultOutlineColor;
            CalcAllRectangles();
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="layout">The CIATLayout object to create a copy of</param>
        public CIATLayout(CIATLayout layout)
        {
            this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            this._ErrorSize = layout._ErrorSize;
            this._BackColor = layout._BackColor;
            this._BorderColor = layout._BorderColor;
            this._BorderWidth = layout._BorderWidth;
            this._InteriorSize = layout._InteriorSize;
            this._StimulusSize = layout._StimulusSize;
            this._InstructionsSize = layout._InstructionsSize;
            this._KeyValueSize = layout._KeyValueSize;
            this._TextStimulusPaddingTop = layout._TextStimulusPaddingTop;
            this._ContinueInstructionsSize = layout._ContinueInstructionsSize;
            this._OutlineColor = layout._OutlineColor;
            this._WebpageImageLayout = layout._WebpageImageLayout;
            this._WebpageBackgroundImage = layout._WebpageBackgroundImage;
            this._WebpageBackColor = layout._WebpageBackColor;
            CalcAllRectangles();
        }

        public CIATLayout(Uri uri)
        {
            this.URI = uri;
            _ErrorMarkObservableUri = null;
            _LeftKeyValueOutlineObservableUri = null;
            _RightKeyValueOutlineObservableUri = null;
            Load(uri);
            CalcAllRectangles();
        }

        public void Activate()
        {
            ErrorMarkObservableUri.Value = ErrorMarkUri;
            LeftKeyValueOutlineObservableUri.Value = LeftKeyValueOutlineUri;
            RightKeyValueOutlineObservableUri.Value = RightKeyValueOutlineUri;
        }

        public void Dispose()
        {
            ErrorMarkUri = null;
            LeftKeyValueOutlineUri = null;
            RightKeyValueOutlineUri = null;
            CIAT.SaveFile.DeletePackageLevelRelationship(BaseType);
            CIAT.SaveFile.DeletePart(this.URI);
        }

        /// <summary>
        /// Finds overlap between the bounding rectangles
        /// </summary>
        /// <returns>zero on no overlap, else a bitmask of CIATLayout.Overlap values</returns>
        public uint FindOverlap()
        {
            uint returnValue = 0;

            // check for overlap between key value rectangles with each other
            if (Rectangle.Intersect(LeftKeyValueRectangle, RightKeyValueRectangle) != Rectangle.Empty)
                returnValue |= Overlap.KeyValueRectangles;

            // check for overlap between key value rectangles and stimulus rectangle
            if (Rectangle.Intersect(LeftKeyValueRectangle, StimulusRectangle) != Rectangle.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.StimulusRectangle;

            // check for overlap between key value rectangles and error rectangle
            if (Rectangle.Intersect(LeftKeyValueRectangle, ErrorRectangle) != Rectangle.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.ErrorRectangle;

            // check for overlap between key value rectangles and instruction rectangle
            if (Rectangle.Intersect(LeftKeyValueRectangle, InstructionsRectangle) != Rectangle.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.InstructionRectangle;

            // check for overlap between stimulus rectangle and error rectangle
            if (Rectangle.Intersect(StimulusRectangle, ErrorRectangle) != Rectangle.Empty)
                returnValue |= Overlap.StimulusRectangle | Overlap.ErrorRectangle;

            // check for overlap between stimulus rectangle and instruction rectangle
            if (Rectangle.Intersect(StimulusRectangle, InstructionsRectangle) != Rectangle.Empty)
                returnValue |= Overlap.StimulusRectangle | Overlap.InstructionRectangle;

            // check for overlap between error rectangle and instruction rectangle
            if (Rectangle.Intersect(InstructionsRectangle, ErrorRectangle) != Rectangle.Empty)
                returnValue |= Overlap.InstructionRectangle | Overlap.ErrorRectangle;

            return returnValue;
        }

        /// <summary>
        /// Calculates the maximum height of the stimulus rectangle that will not cause the stimulus rectangle to overlap with any
        /// other layout element
        /// </summary>
        /// <returns>that maximum height</returns>
        public int GetMaxStimulusHeight()
        {
            Size stimSize = _StimulusSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.StimulusRectangle) != 0)
            {
                do
                {
                    _StimulusSize.Height--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.StimulusRectangle) != 0) && (_StimulusSize.Height > 0));
            }
            else
            {
                while (((overlap & Overlap.StimulusRectangle) == 0) && (_StimulusSize.Height < InteriorSize.Height))
                {
                    _StimulusSize.Height++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _StimulusSize.Height--;
            }
            int maxHeight = _StimulusSize.Height;
            StimulusSize = stimSize;
            return maxHeight;
        }

        /// <summary>
        /// Calculates the maximum width of the stimulus rectangle that will not cause the stimulus rectangle to overlap with any
        /// other layout elements
        /// </summary>
        /// <returns>that maximum width</returns>
        public int GetMaxStimulusWidth()
        {
            Size stimSize = _StimulusSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.StimulusRectangle) != 0)
            {
                do
                {
                    _StimulusSize.Width--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.StimulusRectangle) != 0) && (_StimulusSize.Width > 0));
            }
            else
            {
                while (((overlap & Overlap.StimulusRectangle) == 0) && (_StimulusSize.Width < InteriorSize.Width))
                {
                    _StimulusSize.Width++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _StimulusSize.Width--;
            }
            int maxWidth = _StimulusSize.Width;
            StimulusSize = stimSize;
            return maxWidth;
        }

        /// <summary>
        /// Calculates the maximum height of the instructions rectangle that will not cause the instructions rectangle to overlap with any
        /// other layout element
        /// </summary>
        /// <returns>that maximum height</returns>
        public int GetMaxInstructionsHeight()
        {
            Size stimSize = _InstructionsSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.InstructionRectangle) != 0)
            {
                do
                {
                    _InstructionsSize.Height--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.InstructionRectangle) != 0) && (_InstructionsSize.Height > 0));
            }
            else
            {
                while (((overlap & Overlap.InstructionRectangle) == 0) && (_InstructionsSize.Height < _InteriorSize.Height))
                {
                    _InstructionsSize.Height++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _InstructionsSize.Height--;
            }
            int maxHeight = _InstructionsSize.Height;
            InstructionsSize = stimSize;
            return maxHeight;
        }

        /// <summary>
        /// Calculates the maximum width of the Instructions rectangle that will not cause the Instructions rectangle to overlap with any
        /// other layout elements
        /// </summary>
        /// <returns>that maximum width</returns>
        public int GetMaxInstructionsWidth()
        {
            Size stimSize = _InstructionsSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.InstructionRectangle) != 0)
            {
                do
                {
                    _InstructionsSize.Width--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.InstructionRectangle) != 0) && (_InstructionsSize.Width > 0));
            }
            else
            {
                while (((overlap & Overlap.InstructionRectangle) == 0) && (_InstructionsSize.Width < _InteriorSize.Width))
                {
                    _InstructionsSize.Width++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _InstructionsSize.Width--;
            }
            int maxWidth = _InstructionsSize.Width;
            InstructionsSize = stimSize;
            return maxWidth;
        }

        /// <summary>
        /// Calculates the maximum height of the error mark rectangle that will not cause the error mark rectangle to overlap with any
        /// other layout element
        /// </summary>
        /// <returns>that maximum height</returns>
        public int GetMaxErrorMarkHeight()
        {
            Size stimSize = _ErrorSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.ErrorRectangle) != 0)
            {
                do
                {
                    _ErrorSize.Height--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.ErrorRectangle) != 0) && (_ErrorSize.Height > 0));
            }
            else
            {
                while (((overlap & Overlap.ErrorRectangle) == 0) && (_ErrorSize.Height < _InteriorSize.Height))
                {
                    _ErrorSize.Height++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _ErrorSize.Height--;
            }
            int maxHeight = _ErrorSize.Height;
            ErrorSize = stimSize;
            return maxHeight;
        }

        /// <summary>
        /// Calculates the maximum width of the error mark rectangle that will not cause the error mark rectangle to overlap with any
        /// other layout elements
        /// </summary>
        /// <returns>that maximum width</returns>
        public int GetMaxErrorMarkWidth()
        {
            Size stimSize = _ErrorSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.ErrorRectangle) != 0)
            {
                do
                {
                    _ErrorSize.Width--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.ErrorRectangle) != 0) && (_ErrorSize.Width > 0));
            }
            else
            {
                while (((overlap & Overlap.ErrorRectangle) == 0) && (_ErrorSize.Width < _InteriorSize.Width))
                {
                    _ErrorSize.Width++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _ErrorSize.Width--;
            }
            int maxWidth = _ErrorSize.Width;
            ErrorSize = stimSize;
            return maxWidth;
        }

        /// <summary>
        /// Calculates the maximum height of the response key value rectangles that will not cause the response key value rectangles to overlap with any
        /// other layout element
        /// </summary>
        /// <returns>that maximum height</returns>
        public int GetMaxKeyValueHeight()
        {
            Size stimSize = _KeyValueSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if (((overlap & Overlap.KeyValueRectangles) & (Overlap.StimulusRectangle | Overlap.ErrorRectangle | Overlap.InstructionRectangle)) != 0)
            {
                do
                {
                    _KeyValueSize.Height--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while ((((overlap & Overlap.KeyValueRectangles) & (Overlap.StimulusRectangle | Overlap.ErrorRectangle | Overlap.InstructionRectangle)) != 0)
                    && (_KeyValueSize.Height > 0));
            }
            else
            {
                while ((((overlap & Overlap.KeyValueRectangles) & (Overlap.StimulusRectangle | Overlap.ErrorRectangle | Overlap.InstructionRectangle)) == 0)
                    && (_KeyValueSize.Height < _InteriorSize.Height))
                {
                    _KeyValueSize.Height++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _KeyValueSize.Height--;
            }
            int maxHeight = _KeyValueSize.Height;
            KeyValueSize = stimSize;
            return maxHeight;
        }

        /// <summary>
        /// Calculates the maximum width of the response key value rectangles that will not cause the response key value rectangles to overlap with any
        /// other layout elements
        /// </summary>
        /// <returns>that maximum width</returns>
        public int GetMaxKeyValueWidth()
        {
            Size stimSize = _KeyValueSize;
            CalcAllRectangles();
            uint overlap = FindOverlap();
            if ((overlap & Overlap.KeyValueRectangles) != 0)
            {
                do
                {
                    _KeyValueSize.Width--;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                } while (((overlap & Overlap.KeyValueRectangles) != 0) && (_KeyValueSize.Width > 0));
            }
            else
            {
                while (((overlap & Overlap.KeyValueRectangles) == 0) && (_KeyValueSize.Width < _InteriorSize.Width))
                {
                    _KeyValueSize.Width++;
                    CalcAllRectangles();
                    overlap = FindOverlap();
                }
                _KeyValueSize.Width--;
            }
            int maxWidth = _KeyValueSize.Width;
            KeyValueSize = stimSize;
            return maxWidth;
        }


        /// <summary>
        /// Tests to see if the layout data members are valid
        /// </summary>
        /// <returns>"true" if none of the layout elements overlap, otherwise "false"</returns>
        public bool IsValid()
        {
            if (FindOverlap() == 0)
                return true;
            return false;
        }

        public bool Equals(CIATLayout l)
        {
            if (!_ErrorSize.Equals(l._ErrorSize))
                return false;
            if (!_BackColor.Equals(l._BackColor))
                return false;
            if (!_BorderColor.Equals(l._BorderColor))
                return false;
            if (_BorderWidth != l._BorderWidth)
                return false;
            if (!_InteriorSize.Equals(l._InteriorSize))
                return false;
            if (!_StimulusSize.Equals(l._StimulusSize))
                return false;
            if (!_InstructionsSize.Equals(l._InstructionsSize))
                return false;
            if (!_KeyValueSize.Equals(l._KeyValueSize))
                return false;
            if (_TextStimulusPaddingTop != l._TextStimulusPaddingTop)
                return false;
            if (!_ContinueInstructionsSize.Equals(l._ContinueInstructionsSize))
                return false;
            if (!_OutlineColor.Equals(l._OutlineColor))
                return false;
            if (_WebpageImageLayout != l._WebpageImageLayout)
                return false;
            if (!_WebpageBackColor.Equals(l._WebpageBackColor))
                return false;
            return true;
        }

        public void Load(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            if (xDoc.Root.Attribute("ErrorMarkObservable") == null)
            {
                ErrorMarkUri = new DIErrorMark().URI;
                ErrorMarkObservableUri = new ObservableUri();
                ErrorMarkObservableUri.Value = ErrorMarkUri;
            } else
            {
                ErrorMarkObservableUri = CIAT.SaveFile.GetObservableUri(new Uri(xDoc.Root.Attribute("ErrorMarkObservable").Value, UriKind.Relative));
                ErrorMarkUri = ErrorMarkObservableUri.Value;
            }
            if (xDoc.Root.Attribute("LeftKeyValueOutlineObservable") == null)
            {
                LeftKeyValueOutlineUri = new DIKeyValueOutline(KeyedDirection.Left).URI;
                LeftKeyValueOutlineObservableUri = new ObservableUri();
                LeftKeyValueOutlineObservableUri.Value = LeftKeyValueOutlineUri;
            } else
            {
                LeftKeyValueOutlineObservableUri = CIAT.SaveFile.GetObservableUri(new Uri(xDoc.Root.Attribute("LeftKeyValueOutlineObservable").Value, UriKind.Relative));
                LeftKeyValueOutlineUri = LeftKeyValueOutlineObservableUri.Value;
            }
            if (xDoc.Root.Attribute("RightKeyValueOutlineObservable") == null)
            {
                RightKeyValueOutlineUri = new DIKeyValueOutline(KeyedDirection.Left).URI;
                RightKeyValueOutlineObservableUri = new ObservableUri();
                RightKeyValueOutlineObservableUri.Value = RightKeyValueOutlineUri;
            }
            else
            {
                RightKeyValueOutlineObservableUri = CIAT.SaveFile.GetObservableUri(new Uri(xDoc.Root.Attribute("LeftKeyValueOutlineObservable").Value, UriKind.Relative));
                RightKeyValueOutlineUri = RightKeyValueOutlineObservableUri.Value;
            }
            if (CVersion.Compare(CIAT.SaveFile.Version, new CVersion("1.1.0.20")) >= 0)
            {
                XElement sizeElem = xDoc.Root.Element("Interior");
                _InteriorSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("Instructions");
                _InstructionsSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("ResponseKey");
                _KeyValueSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("ErrorMark");
                _ErrorSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("Stimulus");
                _StimulusSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("ContinueInstructions");
                _ContinueInstructionsSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
            } else
            {
                XElement sizeElem = xDoc.Root.Element("Interior");
                _InteriorSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                sizeElem = xDoc.Root.Element("Instructions");
                _InstructionsSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                _InstructionsRectangle = new Rectangle(new Point(Convert.ToInt32(sizeElem.Element("X").Value), Convert.ToInt32(sizeElem.Element("Y").Value)), _InstructionsSize);
                sizeElem = xDoc.Root.Element("LeftKeyValueRectangle");
                _KeyValueSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                _LeftKeyValueRectangle = new Rectangle(new Point(Convert.ToInt32(sizeElem.Element("X").Value), Convert.ToInt32(sizeElem.Element("Y").Value)), _KeyValueSize);
                sizeElem = xDoc.Root.Element("RightKeyValueRectangle");
                _KeyValueSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                _RightKeyValueRectangle = new Rectangle(new Point(Convert.ToInt32(sizeElem.Element("X").Value), Convert.ToInt32(sizeElem.Element("Y").Value)), _KeyValueSize);
                sizeElem = xDoc.Root.Element("ErrorMark");
                _ErrorSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                _ErrorRectangle = new Rectangle(new Point(Convert.ToInt32(sizeElem.Element("X").Value), Convert.ToInt32(sizeElem.Element("Y").Value)), _KeyValueSize);
                sizeElem = xDoc.Root.Element("Stimulus");
                _StimulusSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
                _StimulusRectangle = new Rectangle(new Point(Convert.ToInt32(sizeElem.Element("X").Value), Convert.ToInt32(sizeElem.Element("Y").Value)), _KeyValueSize);
                sizeElem = xDoc.Root.Element("ContinueInstructions");
                _ContinueInstructionsSize = new Size(Convert.ToInt32(sizeElem.Element("Width").Value), Convert.ToInt32(sizeElem.Element("Height").Value));
            }
            _BackColor = Color.FromName(xDoc.Root.Element("BackColor").Value);
            _BorderColor = Color.FromName(xDoc.Root.Element("BorderColor").Value);
            _BorderWidth = Convert.ToInt32(xDoc.Root.Element("BorderWidth").Value);
            _WebpageBackColor = Color.FromName(xDoc.Root.Element("WebpageBackground").Element("WebpageBackgroundColor").Value);
        }

        public void Save()
        {
            CIAT.SaveFile.DeleteRelationshipsByType(URI, BaseType, typeof(DIBase));
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement("Layout", new XElement("Interior", new XElement("Width", _InteriorSize.Width.ToString()), new XElement("Height", _InteriorSize.Height.ToString())),
                new XElement("Instructions", new XElement("X", InstructionsRectangle.Location.X.ToString()), new XElement("Y", InstructionsRectangle.Location.Y.ToString()),
                    new XElement("Width", _InstructionsSize.Width.ToString()), new XElement("Height", _InstructionsSize.Height.ToString())),
                new XElement("LeftKeyValueRectangle", new XElement("X", LeftKeyValueRectangle.Location.X.ToString()), new XElement("Y", LeftKeyValueRectangle.Location.Y.ToString()), 
                    new XElement("Width", _KeyValueSize.Width.ToString()), new XElement("Height", _KeyValueSize.Height.ToString())),
                new XElement("RightKeyValueRectangle", new XElement("X", RightKeyValueRectangle.Location.X.ToString()), new XElement("Y", RightKeyValueRectangle.Location.Y.ToString()),
                    new XElement("Width", _KeyValueSize.Width.ToString()), new XElement("Height", _KeyValueSize.Height.ToString())),
                new XElement("ErrorMark", new XElement("X", ErrorRectangle.Location.X.ToString()), new XElement("Y", ErrorRectangle.Location.Y.ToString()), 
                    new XElement("Width", _ErrorSize.Width.ToString()), new XElement("Height", _ErrorSize.Height.ToString())),
                new XElement("Stimulus", new XElement("X", StimulusRectangle.Location.X.ToString()), new XElement("Y", StimulusRectangle.Location.Y.ToString()), 
                    new XElement("Width", _StimulusSize.Width.ToString()), new XElement("Height", _StimulusSize.Height.ToString())),
                new XElement("ContinueInstructions", new XElement("Width", _ContinueInstructionsSize.Width.ToString()), new XElement("Height", _ContinueInstructionsSize.Height.ToString())),
                new XElement("BackColor", _BackColor.Name), new XElement("BorderColor", _BorderColor.Name), new XElement("BorderWidth", _BorderWidth.ToString()),
                new XElement("WebpageBackground", new XElement("WebpageBackgroundColor", _WebpageBackColor.Name), new XElement("WebpageBackgroundImage"))));
            xDoc.Document.Root.Add(new XAttribute("ErrorMarkObservable", CIATLayout.ErrorMarkObservableUri.URI.ToString()));
            xDoc.Document.Root.Add(new XAttribute("LeftKeyValueOutlineObservable", CIATLayout.LeftKeyValueOutlineObservableUri.URI.ToString()));
            xDoc.Document.Root.Add(new XAttribute("RightKeyValueOutlineObservable", CIATLayout.RightKeyValueOutlineObservableUri.URI.ToString()));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void WriteToXml(XmlTextWriter writer)
        {
            if (!IsValid())
                throw new Exception();
            CalcAllRectangles();
            writer.WriteStartElement("CIATLayout");
            writer.WriteElementString("InteriorWidth", InteriorSize.Width.ToString());
            writer.WriteElementString("InteriorHeight", InteriorSize.Height.ToString());
            writer.WriteElementString("InstructionsWidth", InstructionsSize.Width.ToString());
            writer.WriteElementString("InstructionsHeight", InstructionsSize.Height.ToString());
            writer.WriteElementString("KeyValueWidth", KeyValueSize.Width.ToString());
            writer.WriteElementString("KeyValueHeight", KeyValueSize.Height.ToString());
            writer.WriteElementString("ErrorWidth", ErrorSize.Width.ToString());
            writer.WriteElementString("ErrorHeight", ErrorSize.Height.ToString());
            writer.WriteElementString("StimulusWidth", StimulusSize.Width.ToString());
            writer.WriteElementString("StimulusHeight", StimulusSize.Height.ToString());
            writer.WriteElementString("BackColor", BackColor.Name);
            writer.WriteElementString("BorderColor", BorderColor.Name);
            writer.WriteElementString("BorderWidth", BorderWidth.ToString());
            writer.WriteStartElement("WebpageBackgroud");
            writer.WriteAttributeString("Type", (WebpageBackgroundImage == null) ? "Image" : "Color");
            writer.WriteElementString("WebpageBackgroundColor", WebpageBackColor.Name);
            writer.WriteStartElement("WebpageImage");
            if (WebpageBackgroundImage != null)
            {
                writer.WriteElementString("WebpageImageLayout", WebpageImageLayout.ToString());
                MemoryStream memStream = new MemoryStream();
                WebpageBackgroundImage.Save(memStream, WebpageImageFormat);
                ICryptoTransform toB64Trans = new ToBase64Transform();
                memStream.Seek(0, SeekOrigin.Begin);
                CryptoStream cStream = new CryptoStream(memStream, toB64Trans, CryptoStreamMode.Read);
                byte[] buff = new byte[toB64Trans.OutputBlockSize * 65536];
                int bytesRead;
                writer.WriteStartElement("WebpageImageData");
                writer.WriteAttributeString("ImageFormat", WebpageImageFormat.ToString());
                while ((bytesRead = cStream.Read(buff, 0, toB64Trans.OutputBlockSize)) != 0)
                    writer.WriteString(System.Text.Encoding.Default.GetString(buff, 0, bytesRead));
                writer.WriteEndElement();
                memStream.Dispose();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public bool LoadFromXml(XmlNode node)
        {
            // check the node name
            if (node.Name != "CIATLayout")
                throw new Exception();

            int nodeCtr = 0;
            // load the data
            _InteriorSize.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _InteriorSize.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _InstructionsSize.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _InstructionsSize.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _KeyValueSize.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _KeyValueSize.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _ErrorSize.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _ErrorSize.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _StimulusSize.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _StimulusSize.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _BackColor = System.Drawing.Color.FromName(node.ChildNodes[nodeCtr++].InnerText);
            _BorderColor = System.Drawing.Color.FromName(node.ChildNodes[nodeCtr++].InnerText);
            _BorderWidth = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            if (nodeCtr == node.ChildNodes.Count)
            {
                _WebpageBackColor = System.Drawing.Color.Black;
                _WebpageBackgroundImage = null;
                _WebpageImageLayout = EWebpageBackgroundImageOrientation.none;
                _WebpageBackgroundImageFilename = String.Empty;
                WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
            }
            else if (nodeCtr == node.ChildNodes.Count - 1)
            {
                _WebpageBackColor = System.Drawing.Color.FromName(node.ChildNodes[nodeCtr++].InnerText);
                _WebpageBackgroundImage = null;
                _WebpageImageLayout = EWebpageBackgroundImageOrientation.none;
                _WebpageBackgroundImageFilename = String.Empty;
                WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Bmp;
            }
            else
            {
                String webpageBackType = node.ChildNodes[nodeCtr++].InnerText;
                _WebpageBackColor = Color.FromName(node.ChildNodes[nodeCtr++].InnerText);
                if (webpageBackType == "Image")
                {
                    _WebpageImageLayout = (EWebpageBackgroundImageOrientation)Enum.Parse(typeof(EWebpageBackgroundImageOrientation), node.ChildNodes[nodeCtr].ChildNodes[0].InnerText);
                    String imageFormatStr = node.ChildNodes[nodeCtr].ChildNodes[1].Attributes["ImageFormat"].Value;
                    switch (imageFormatStr)
                    {
                        case "bmp": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                        case "emf": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Emf; break;
                        case "exif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Exif; break;
                        case "gif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Gif; break;
                        case "ico": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Icon; break;
                        case "jpeg": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                        case "jpg": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                        case "png": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Png; break;
                        case "tif": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                        case "tiff": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                        case "wmf": WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Wmf; break;
                        default:
                            WebpageImageFormat = System.Drawing.Imaging.ImageFormat.Png; break;
                    }
                    ICryptoTransform fromB64Trans = new FromBase64Transform();
                    String imageDataStr = node.ChildNodes[nodeCtr].ChildNodes[1].InnerText;
                    MemoryStream memStream = new MemoryStream();
                    CryptoStream cStream = new CryptoStream(memStream, fromB64Trans, CryptoStreamMode.Write);
                    cStream.Write(System.Text.Encoding.Default.GetBytes(imageDataStr), 0, System.Text.Encoding.Default.GetByteCount(imageDataStr));
                    cStream.FlushFinalBlock();
                    memStream.Seek(0, SeekOrigin.Begin);
                    _WebpageBackgroundImage = Image.FromStream(memStream);
                    memStream.Dispose();
                }
            }
            CalcAllRectangles();

            // success
            return true;
        }
    }
}
