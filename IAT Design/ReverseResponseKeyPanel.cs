using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public class ReverseResponseKeyPanel : UserControl
    {

        private void InitializeComponent()
        {
            this.ReverseResponseGroup = new System.Windows.Forms.GroupBox();
            this.ReversibleKeyDropList = new System.Windows.Forms.ComboBox();
            this.ReverseResponseKeyLabel = new System.Windows.Forms.Label();
            this.ReverseResponseGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReverseResponseGroup
            // 
            this.ReverseResponseGroup.Controls.Add(this.ReversibleKeyDropList);
            this.ReverseResponseGroup.Controls.Add(this.ReverseResponseKeyLabel);
            this.ReverseResponseGroup.Location = new System.Drawing.Point(3, 3);
            this.ReverseResponseGroup.Name = "ReverseResponseGroup";
            this.ReverseResponseGroup.Size = new System.Drawing.Size(346, 51);
            this.ReverseResponseGroup.TabIndex = 0;
            this.ReverseResponseGroup.TabStop = false;
            this.ReverseResponseGroup.Text = "Reversed Response Key";
            // 
            // ReversibleKeyDropList
            // 
            this.ReversibleKeyDropList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ReversibleKeyDropList.FormattingEnabled = true;
            this.ReversibleKeyDropList.Location = new System.Drawing.Point(188, 19);
            this.ReversibleKeyDropList.Name = "ReversibleKeyDropList";
            this.ReversibleKeyDropList.Size = new System.Drawing.Size(151, 21);
            this.ReversibleKeyDropList.TabIndex = 1;
            // 
            // ReverseResponseKeyLabel
            // 
            this.ReverseResponseKeyLabel.AutoSize = true;
            this.ReverseResponseKeyLabel.Location = new System.Drawing.Point(6, 22);
            this.ReverseResponseKeyLabel.Name = "ReverseResponseKeyLabel";
            this.ReverseResponseKeyLabel.Size = new System.Drawing.Size(176, 13);
            this.ReverseResponseKeyLabel.TabIndex = 0;
            this.ReverseResponseKeyLabel.Text = "Select a Response Key to Reverse:";
            // 
            // ReverseResponseKeyPanel
            // 
            this.Controls.Add(this.ReverseResponseGroup);
            this.Name = "ReverseResponseKeyPanel";
            this.Size = new System.Drawing.Size(353, 61);
            this.ParentChanged += new System.EventHandler(this.ReverseResponseKeyPanel_ParentChanged);
            this.ReverseResponseGroup.ResumeLayout(false);
            this.ReverseResponseGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.GroupBox ReverseResponseGroup;
        private System.Windows.Forms.Label ReverseResponseKeyLabel;
        private System.Windows.Forms.ComboBox ReversibleKeyDropList;

        private IImageDisplay LeftKeyPreview, RightKeyPreview;
        private Uri _BaseKeyUri = null;
        public Uri BaseKeyUri
        {
            get
            {
                return _BaseKeyUri;
            }
            set
            {
                _BaseKeyUri = value;
                if (value != null)
                {
                    if (ReversibleKeyDropList.Enabled)
                        ReversibleKeyDropList.SelectedItem = CIAT.SaveFile.GetIATKey(value);
                    else
                    {
                        CIATKey baseKey = CIAT.SaveFile.GetIATKey(value);
                        LeftKeyPreview.SetImage(baseKey.RightValue.IImage);
                        RightKeyPreview.SetImage(baseKey.LeftValue.IImage);
                    }
                }
            }
        }
        public ResponseKeyDialog MainForm
        {
            get
            {
                return ParentControl.Parent as ResponseKeyDialog;
            }
        }

        /// <summary>
        /// gets the parent control
        /// </summary>
        public ResponseKeyPanel ParentControl
        {
            get
            {
                return (ResponseKeyPanel)Parent;
            }
        }

        /// <summary>
        /// gets the name of the key to be reversed
        /// </summary>
        public String ReversedKeyName
        {
            get
            {
                if (ReversibleKeyDropList.SelectedItem == null)
                    return String.Empty;
                return ReversibleKeyDropList.SelectedItem.ToString();
            }
        }


        public bool ValidateInput()
        {
            if (ReversibleKeyDropList.SelectedItem == null)
            {
                MainForm.BeginInvoke(new Action(() =>
                {
                    MainForm.ErrorMsg = Properties.Resources.sUndefinedReversedKeyException;
                }));
                return false;
            }
            MainForm.ErrorMsg = String.Empty;
            return true;
        }

        public void Clear()
        {
            ReversibleKeyDropList.SelectedIndex = -1;
        }


        protected void PopulateReversibleKeyDropList()
        {
            ReversibleKeyDropList.Items.Clear();
            foreach (Uri u in CIAT.SaveFile.GetAllIATKeyUris())
            {
                CIATKey key = CIAT.SaveFile.GetIATKey(u);
                if ((key.KeyType == IATKeyType.SimpleKey) || (key.KeyType == IATKeyType.DualKey))
                    ReversibleKeyDropList.Items.Add(key);
            }
            if (CIAT.SaveFile.IAT.Is7Block)
                ReversibleKeyDropList.Enabled = false;
        }

        /// <summary>
        /// the default constructor
        /// </summary>
        public ReverseResponseKeyPanel(IImageDisplay leftKeyPreview, IImageDisplay rightKeyPreview)
        {
            InitializeComponent();
            this.AutoScaleDimensions = new System.Drawing.SizeF(72F, 72F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            LeftKeyPreview = leftKeyPreview;
            RightKeyPreview = rightKeyPreview;
            PopulateReversibleKeyDropList();
            this.Resize += (sender, args) =>
            {
                this.ReverseResponseGroup.Width = this.Width - 12;
                ReversibleKeyDropList.Location = new Point(this.ReverseResponseKeyLabel.Right + 3, ReversibleKeyDropList.Top);
                ReversibleKeyDropList.Width = this.ReverseResponseGroup.Width - this.ReversibleKeyDropList.Width - 24;
            };
            ReversibleKeyDropList.SelectedValueChanged += new EventHandler(ReversibleKeyDropList_SelectedItemChanged);
            ReversibleKeyDropList.DropDown += (sender, args) => PopulateReversibleKeyDropList();
            ReversibleKeyDropList.SelectedIndexChanged += new EventHandler(ReversibleKeyDropList_SelectedItemChanged);
        }

        private void ReversibleKeyDropList_SelectedItemChanged(object sender, EventArgs e)
        {
            CIATKey baseKey = ReversibleKeyDropList.SelectedItem as CIATKey;
            _BaseKeyUri = baseKey.URI;
            LeftKeyPreview.SetImage(baseKey.RightValue.IImage);
            RightKeyPreview.SetImage(baseKey.LeftValue.IImage);
            if (Parent != null)
                ParentControl.ValidateInput();
        }

        private void ReverseResponseKeyPanel_ParentChanged(object sender, EventArgs e)
        {
        }

        public new void Dispose()
        {
            if (IsDisposed)
                return;
            LeftKeyPreview.ClearImage();
            RightKeyPreview.ClearImage();
            base.Dispose();
        }
    }
}
