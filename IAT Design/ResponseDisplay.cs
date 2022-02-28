using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    abstract partial class ResponseDisplay : UserControl
    {
        private bool _IsCollapsed;
        protected static Padding InteriorPadding = new Padding(3, 3, 3, 3);
        protected SurveyItemFormat _Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
        private Font _ControlFont = null, _DisplayFont = null;

        protected readonly object lockObj = new object();
        protected readonly ManualResetEvent LayoutEvent = new ManualResetEvent(true);



        protected virtual void ChangeResponseFont()
        {
            LayoutControl();
        }


        protected Font DisplayFont
        {
            get
            {
                if (_DisplayFont == null)
                    _DisplayFont = new Font(Format.FontFamily, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, Format.FontStyle, GraphicsUnit.Pixel);
                return _DisplayFont;
            }
            set
            {
                if (_DisplayFont == value)
                    return;
                lock (fontLockObj)
                {
                    var f = _DisplayFont;
                    _DisplayFont = value;
                    ChangeResponseFont();
                    if (!_DisplayFont.Equals(f))
                        f?.Dispose();
                    Invalidate();
                }
            }
        }

        private readonly object fontLockObj = new object();

        /*
         protected virtual Font ControlFont
                {
                    get
                    {
                        if (_ControlFont == null)
                            _ControlFont = new Font(Format.FontFamily, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, FontStyle.Regular, GraphicsUnit.Pixel);
                        return _ControlFont;
                    }
                    set
                    {
                        lock (lockObj)
                        {


                            if (_ControlFont == value)
                                return;
                            var f = _ControlFont;
                            _ControlFont = value;
                            if (IsHandleCreated)
                                        ChangeResponseFont();
                            if (!ControlFont.Equals(f))
                                f?.Dispose();
                        }
                    }
                }
        */
        public SurveyItemFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {
                _Format = value;
                DisplayFont = new Font(value.FontFamily, value.FontSizeAsPixels * CIATLayout.yDpi / 96F, value.FontStyle, GraphicsUnit.Pixel);
            }
        }
        public bool IsCollapsed
        {
            get
            {
                return _IsCollapsed;
            }
            set
            {
                _IsCollapsed = value;
            }
        }


        protected CSurveyItem SurveyItem
        {
            get
            {
                if (Parent == null)
                    return null;
                return (Parent as QuestionDisplay).SurveyItem;
            }
        }

        public ResponseDisplay()
        {
            InitializeComponent();
            _IsCollapsed = false;
            Paint += new PaintEventHandler(ResponseDisplay_Paint);
            Load += (sender, args) =>
            {
                ChangeResponseFont();
            };
            this.MouseEnter += new EventHandler(ResponseDisplay_MouseEnter);
            Padding = InteriorPadding;
            this.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
        }

        protected void ResponseDisplay_MouseClick(object sender, MouseEventArgs e)
        {
            OnSelected(ModifierKeys);
        }

        protected void OnSelected(Keys ModifierKeys)
        {
            if (Parent != null)
                ((SurveyDisplay)Parent.Parent).SelectionChanged(Parent as QuestionDisplay, ModifierKeys);
        }

        void ResponseDisplay_MouseEnter(object sender, EventArgs e)
        {
            if (Parent != null)
                ((QuestionDisplay)Parent).Active = true;
        }

        virtual public void OnActivate(bool BecomingActive)
        {
        }

        virtual protected void ResponseDisplay_Load(object sender, EventArgs e)
        {
            for (int ctr = 0; ctr < Controls.Count; ctr++)
                Controls[ctr].MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
        }

        abstract protected void ResponseDisplay_Paint(object sender, PaintEventArgs e);

        abstract protected CResponse GetResponse();

        abstract public void SetResponse(CResponse response);

        abstract public void LayoutControl();
    }
}
