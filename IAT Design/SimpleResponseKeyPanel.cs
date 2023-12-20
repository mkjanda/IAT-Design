using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    public class ResponseKeySide : Enumeration
    {
        public static readonly ResponseKeySide None = new ResponseKeySide(0, "None");
        public static readonly ResponseKeySide Left = new ResponseKeySide(1, "Left");
        public static readonly ResponseKeySide Right = new ResponseKeySide(2, "Right");
        private ResponseKeySide(int value, String name) : base(value, name)
        {
        }

        private static IEnumerable<ResponseKeySide> All = new ResponseKeySide[] { None, Left, Right };
        public static ResponseKeySide FromString(String str)
        {
            return All.Where((val) => val.Name == str).FirstOrDefault();
        }
    }

    public partial class SimpleResponseKeyPanel : UserControl
    {

        private void InitializeComponent()
        {
            // 
            // SimpleResponseKeyPanel
            // 
            this.Name = "SimpleResponseKeyPanel";
            this.ParentChanged += new System.EventHandler(this.SimpleResponseKeyPanel_ParentChanged);
            this.ResumeLayout(false);

        }


        // the text and image response key panels
        private TextEditControl TextPanel;
        private ImageKeyPanel ImagePanel;
        private static Point TextPanelLocation = new Point(5, 50);
        private static Point ImagePanelLocation = new Point(5, 40);
        private static Point TextRadioLocation = new Point(100, 10);
        private static Point ImageRadioLocation = new Point(155, 10);
        private Label StimulusTypeLabel = new Label();
        private RadioButton TextRadio = new RadioButton(), ImageRadio = new RadioButton();
        private static int TextPanelWidth = 375;
        private static Size ImagePanelSize = new Size(375, 77);
        private GroupBox SimpleResponseKeyGroup = new GroupBox();
        private IImageDisplay KeyPreview;
        private ResponseKeySide Side;
        private Uri _DisplayItemUri = null;


        public Uri DisplayItemUri
        {
            get
            {
                if (TextRadio.Checked)
                    _DisplayItemUri = TextPanel.TextDisplayItemUri;
                else if (ImageRadio.Checked)
                    _DisplayItemUri = ImagePanel.DisplayItemUri;
                return _DisplayItemUri;
            }
            set
            {
                if (value.Equals(_DisplayItemUri))
                    return;
                if (value == DIBase.DINull.URI)
                {
                    TextRadio.Checked = false;
                    ImageRadio.Checked = false;
                    _DisplayItemUri = value;
                    return;
                }
                SuspendLayout();
                DIBase di = CIAT.SaveFile.GetDI(value);
                if (di.Type == DIType.ResponseKeyText)
                {
                    _DisplayItemUri = value;
                    TextRadio.Checked = true;
                    ImageRadio.Checked = false;
                    TextPanel.TextDisplayItemUri = di.URI;
                    di.PreviewPanel = KeyPreview;
                }
                else if (di.Type == DIType.ResponseKeyImage)
                {
                    _DisplayItemUri = value;
                    ImageRadio.Checked = true;
                    TextRadio.Checked = false;
                    di.PreviewPanel = KeyPreview;
                }
                else
                {
                    _DisplayItemUri = value;
                    ImageRadio.Checked = false;
                    TextRadio.Checked = false;
                    di.PreviewPanel = KeyPreview;
                }
                ResumeLayout(false);
            }
        }

        public ResponseKeyDialog MainForm
        {
            get
            {
                return ParentControl.Parent as ResponseKeyDialog;
            }
        }

        public ResponseKeyPanel ParentControl
        {
            get
            {
                return (ResponseKeyPanel)Parent;
            }
        }

        public String Title
        {
            set
            {
                SimpleResponseKeyGroup.Text = value;
            }
        }

        public bool ValidateInput()
        {
            if ((!TextRadio.Checked) && (!ImageRadio.Checked))
            {
                if (Side == ResponseKeySide.Left)
                {
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        MainForm.ErrorMsg = Properties.Resources.sUntypedLeftResponseValueException;
                    }));
                    return false;
                }
                else if (Side == ResponseKeySide.Right)
                {
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        MainForm.ErrorMsg = Properties.Resources.sUntypedRightResponseValueException;

                    }));
                    return false;
                }
            }
            DIBase DisplayItem = CIAT.SaveFile.GetDI(DisplayItemUri);
            if (DisplayItem == null)
                return true;
            if (DisplayItem.Type == DIType.ResponseKeyImage)
            {
                if (ImagePanel.ImageFileName == String.Empty)
                {
                    if (Side == ResponseKeySide.Left)
                    {
                        MainForm.BeginInvoke(new Action(() =>
                        {
                            MainForm.ErrorMsg = Properties.Resources.sUndefinedLeftImageResponseException;
                        }));
                        return false;
                    }
                    else if (Side == ResponseKeySide.Right)
                    {
                        MainForm.BeginInvoke(new Action(() =>
                        {
                            MainForm.ErrorMsg = Properties.Resources.sUndefinedRightImageResponseException;
                        }));
                        return false;
                    }
                }
            }
            else if (DisplayItem.Type == DIType.ResponseKeyText)
            {
                if ((CIAT.SaveFile.GetDI(TextPanel.TextDisplayItemUri) as DIResponseKeyText).Phrase == String.Empty)
                {
                    if (Side == ResponseKeySide.Left)
                    {
                        MainForm.BeginInvoke(new Action(() =>
                        {
                            MainForm.ErrorMsg = Properties.Resources.sUndefinedLeftTextResponseException;
                        }));
                        return false;
                    }
                    if (Side == ResponseKeySide.Right)
                    {
                        MainForm.BeginInvoke(new Action(() =>
                        {
                            MainForm.ErrorMsg = Properties.Resources.sUndefinedRightTextResponseException;
                        }));
                        return false;
                    }
                }
            }
            return true;
        }

        public SimpleResponseKeyPanel(ResponseKeySide side, IImageDisplay previewPanel)
        {
            TextPanel = null;
            ImagePanel = null;
            Side = side;
            KeyPreview = previewPanel;
            AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            if (side == ResponseKeySide.Left)
                SimpleResponseKeyGroup.Text = "Left Stimulus";
            else if (side == ResponseKeySide.Right)
                SimpleResponseKeyGroup.Text = "Right Stimulus";
            SimpleResponseKeyGroup.Dock = DockStyle.Fill;
            Controls.Add(SimpleResponseKeyGroup);
            SimpleResponseKeyGroup.Dock = DockStyle.Fill;
            StimulusTypeLabel.Text = "Stimulus Type: ";
            StimulusTypeLabel.AutoSize = true;
            StimulusTypeLabel.Location = new Point(20, 25);
            TextRadio.Location = new Point(StimulusTypeLabel.Right + 20, 25);
            TextRadio.Text = "Text";
            TextRadio.AutoSize = true;
            ImageRadio.Location = new Point(TextRadio.Right + 20, 25);
            ImageRadio.Text = "Image";
            ImageRadio.AutoSize = true;
            SimpleResponseKeyGroup.Controls.Add(StimulusTypeLabel);
            SimpleResponseKeyGroup.Controls.Add(TextRadio);
            SimpleResponseKeyGroup.Controls.Add(ImageRadio);
            TextRadio.CheckedChanged += new EventHandler(TextRadio_CheckedChanged);
            ImageRadio.CheckedChanged += new EventHandler(ImageRadio_CheckedChanged);
            this.ParentChanged += (sender, args) =>
            {
                if (Parent != null)
                {
                    if (ImagePanel != null)
                        ImagePanel.ValidateData = ParentControl.ValidateInput;
                    if (TextPanel != null)
                        (CIAT.SaveFile.GetDI(TextPanel.TextDisplayItemUri) as DIResponseKeyText).ValidateData = ParentControl.ValidateInput;
                }
            };
            TextPanel = new TextEditControl(this.Width - (TextPanelLocation.X << 1), DIText.UsedAs.ResponseKey, false)
            {
                Visible = false,
                Location = TextPanelLocation,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            this.Resize += (sender, args) =>
            {
                TextPanel.Width = this.Width;
            };
            ImagePanel = new ImageKeyPanel()
            {
                Location = TextPanelLocation,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Visible = false,
                Size = new Size(SimpleResponseKeyGroup.Width, SimpleResponseKeyGroup.Height - 18),
                Preview = KeyPreview
            };
            SimpleResponseKeyGroup.Controls.Add(ImagePanel);
            SimpleResponseKeyGroup.Controls.Add(TextPanel);
        }
        private void TextRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (TextRadio.Checked)
            {
                TextPanel.Visible = true;
                ImagePanel.Visible = false;
                DIBase di = CIAT.SaveFile.GetDI(DisplayItemUri);
                if (di.Type == DIType.Null)
                    KeyPreview.SetImage(di.IImage);
                else
                    di.PreviewPanel = KeyPreview;
            }
        }

        private void ImageRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ImageRadio.Checked)
            {
                TextPanel.Visible = false;
                ImagePanel.Visible = true;
                DIBase di = CIAT.SaveFile.GetDI(DisplayItemUri);
                if (di.Type == DIType.Null)
                    KeyPreview.SetImage(di.IImage);
                else
                    di.PreviewPanel = KeyPreview;
            }
        }

        public new void Dispose()
        {
            if (DisplayItemUri != null)
                CIAT.SaveFile.GetDI(DisplayItemUri).Dispose();
            base.Dispose();
        }

        private void SimpleResponseKeyPanel_ParentChanged(object sender, EventArgs e)
        {
        }
    }
}
