using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    abstract partial class ResponseDisplay : UserControl
    {
        private bool _IsCollapsed;
        protected static Padding InteriorPadding = new Padding(3, 3, 3, 3);
        protected SurveyItemFormat _Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
        private Font _ControlFont = null, _DisplayFont;
        protected bool UpdatingFromCode { get; set; } = false;


        protected virtual void ChangeResponseFont()
        {
            LayoutControl();
            Invalidate();
        }


        protected Font DisplayFont
        {
            get
            {
                lock (lockObj)
                {
                    if (_DisplayFont == null)
                        _DisplayFont = new Font(Format.Font.Family, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, Format.FontStyle, GraphicsUnit.Pixel);
                    return _DisplayFont;
                }
            }
            set
            {
                lock (lockObj)
                {
                    this.Invoke(new Action(() => SuspendLayout()));
                    _DisplayFont?.Dispose();
                    _DisplayFont = value;
                    this.Invoke(new Action(() => ResumeLayout(false)));
                }
            }

        }

        private object lockObj = new object();

        protected virtual Font ControlFont
        {
            get
            {
                lock (lockObj)
                {
                    if (_ControlFont == null)
                        _ControlFont = new Font(Format.Font.Family, Format.FontSizeAsPixels * CIATLayout.yDpi / 96F, FontStyle.Regular, GraphicsUnit.Pixel);
                    return _ControlFont;
                }
            }
            set
            {
                lock (lockObj)
                {
                    this.Invoke(new Action(() => SuspendLayout()));
                    _ControlFont?.Dispose();
                    _ControlFont = value;
                    this.Invoke(new Action(() => ResumeLayout(false)));
                }
            }
        }

        public SurveyItemFormat Format
        {
            get
            {
                return _Format;
            }
            set
            {
                if (!IsHandleCreated)
                    HandleCreated += (sender, args) =>
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            _Format = value;
                            DisplayFont = new Font(value.Font.Family, value.FontSizeAsPixels * CIATLayout.yDpi / 96F, value.FontStyle, GraphicsUnit.Pixel);
                            ControlFont = new Font(value.Font.Family, value.FontSizeAsPixels * CIATLayout.yDpi / 96F, FontStyle.Regular, GraphicsUnit.Pixel);
                            ChangeResponseFont();
                        }));
                    };
                else
                    this.BeginInvoke(new Action(() =>
                    {
                        _Format = value;
                        DisplayFont = new Font(value.Font.Family, value.FontSizeAsPixels * CIATLayout.yDpi / 96F, value.FontStyle, GraphicsUnit.Pixel);
                        ControlFont = new Font(value.Font.Family, value.FontSizeAsPixels * CIATLayout.yDpi / 96F, FontStyle.Regular, GraphicsUnit.Pixel);
                        ChangeResponseFont();
                    }));
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
            Load += (sender, args) => LayoutControl();
            this.MouseEnter += new EventHandler(ResponseDisplay_MouseEnter);
            Padding = InteriorPadding;
            this.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            this.HandleCreated += (sender, args) => { LayoutControl(); };
            this.Resize += (sender, args) => LayoutControl();
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

        abstract protected void LayoutControl();
    }
}
