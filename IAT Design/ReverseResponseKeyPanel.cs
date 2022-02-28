using System;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ReverseResponseKeyPanel : UserControl
    {
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
            LeftKeyPreview = leftKeyPreview;
            RightKeyPreview = rightKeyPreview;
            PopulateReversibleKeyDropList();
            ReversibleKeyDropList.SelectedValueChanged += new EventHandler(ReversibleKeyDropList_SelectedItemChanged);
            PopulateReversibleKeyDropList();
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
