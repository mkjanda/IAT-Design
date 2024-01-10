using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public partial class CombinedResponseKeyPanel : UserControl, IDisposable
    {
        private TextEditControl ConjunctionEdit;
        private Uri LeftKeyValueUri { get; set; } = null;
        private Uri RightKeyValueUri { get; set; } = null;
        private CDualKeyLayout KeyLayout;
        private const int ConjunctionEditWidth = 375;
        private static Point ConjunctionEditLocation = new Point(3, 18);

        private ResponseKeyDialog MainForm
        {
            get
            {
                return (ResponseKeyDialog)ParentControl.Parent;
            }
        }

        public ResponseKeyPanel ParentControl
        {
            get
            {
                return Parent as ResponseKeyPanel;
            }
        }

        Uri _ConjunctionUri = null;
        public Uri ConjunctionUri
        {
            get
            {
                return _ConjunctionUri;
            }
            set
            {
                if (value == ConjunctionUri)
                    return;
                if (value == null)
                    return;
                if (_ConjunctionUri != null)
                {
                    CIAT.SaveFile.GetDI(_ConjunctionUri).ReleaseOwner(LeftValue.URI);
                    CIAT.SaveFile.GetDI(_ConjunctionUri).ReleaseOwner(RightValue.URI);
                }
                _ConjunctionUri = value;
                CIAT.SaveFile.GetDI(value).AddOwner(LeftValue.URI);
                CIAT.SaveFile.GetDI(value).AddOwner(RightValue.URI);
                ConjunctionEdit.TextDisplayItemUri = value;
                new CDualKeyLayout(LeftValue, RightValue)
                {
                    Key1Uri = BaseKey1Uri,
                    Key2Uri = BaseKey2Uri,
                    ConjunctionUri = value
                }.PerformLayout();
            }
        }

        private Uri _Key1Uri = null;
        public Uri BaseKey1Uri
        {
            get
            {
                return _Key1Uri;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException();
                if (value.Equals(BaseKey1Uri))
                    return;
                if (_Key1Uri != null)
                {
                    CIAT.SaveFile.GetIATKey(_Key1Uri).LeftValue.ReleaseOwner(LeftValue.URI);
                    CIAT.SaveFile.GetIATKey(_Key1Uri).RightValue.ReleaseOwner(RightValue.URI);
                }
                _Key1Uri = value;
                CIAT.SaveFile.GetIATKey(value).LeftValue.AddOwner(LeftValue.URI);
                CIAT.SaveFile.GetIATKey(value).RightValue.AddOwner(RightValue.URI);
                new CDualKeyLayout(LeftValue, RightValue)
                {
                    Key1Uri = BaseKey1Uri,
                    Key2Uri = BaseKey2Uri,
                    ConjunctionUri = ConjunctionUri
                }.PerformLayout();
                FirstCombinedKey.SelectedItem = CIAT.SaveFile.GetIATKey(value);
                ParentControl?.ValidateInput();
            }
        }

        private Uri _Key2Uri = null;
        public Uri BaseKey2Uri
        {
            get
            {
                return _Key2Uri;
            }
            set
            {
                if (value == null)
                    throw new ArgumentException();
                if (value.Equals(BaseKey2Uri))
                    return;
                _Key2Uri = value;
                new CDualKeyLayout(LeftValue, RightValue)
                {
                    Key1Uri = BaseKey1Uri,
                    Key2Uri = BaseKey2Uri,
                    ConjunctionUri = ConjunctionUri
                }.PerformLayout();
                SecondCombinedKey.SelectedItem = CIAT.SaveFile.GetIATKey(value);
                ParentControl?.ValidateInput();
            }
        }

        public DIDualKey LeftValue
        {
            get
            {
                if (LeftKeyValueUri == null)
                    return null;
                return CIAT.SaveFile.GetDI(LeftKeyValueUri) as DIDualKey;
            }
        }

        public DIDualKey RightValue
        {
            get
            {
                if (RightKeyValueUri == null)
                    return null;
                return CIAT.SaveFile.GetDI(RightKeyValueUri) as DIDualKey;
            }
        }

        public CombinedResponseKeyPanel(IImageDisplay leftPreview, IImageDisplay rightPreview)
        {
            InitializeComponent();
            ConjunctionEdit = new TextEditControl(ConjunctionEditWidth, DIText.UsedAs.Conjunction, true)
            {
                Location = ConjunctionEditLocation
            };
            DIConjunction c = CIAT.SaveFile.GetDI(ConjunctionEdit.TextDisplayItemUri) as DIConjunction;
            c.Phrase = "or";
            c.PhraseFontFamily = CIAT.SaveFile.FontPreferences[DIText.UsedAs.Conjunction].FontFamily;
            c.PhraseFontColor = CIAT.SaveFile.FontPreferences[DIText.UsedAs.Conjunction].FontColor;
            c.PhraseFontSize = CIAT.SaveFile.FontPreferences[DIText.UsedAs.Conjunction].FontSize;
            this.ParentChanged += (sender, args) =>
            {
                if (Parent != null)
                    c.ValidateData = ParentControl.ValidateInput;
            };
            ConjunctionEdit.Size = ConjunctionEdit.CalculatedSize;
            ConjunctionEdit.TextDisplayItemUri = c.URI;
            ConjunctionGroup.Controls.Add(ConjunctionEdit);
            LeftKeyValueUri = new DIDualKey().URI;
            RightKeyValueUri = new DIDualKey().URI;
            LeftValue.PreviewPanel = leftPreview;
            RightValue.PreviewPanel = rightPreview;
            this.FirstCombinedKey.DropDown += (sender, args) => PopulateKeyDrop1();
            SecondCombinedKey.DropDown += (sender, args) => PopulateKeyDrop2();
            //            if (CIAT.SaveFile.IAT.Is7Block)
            ///          {
            //           FirstCombinedKey.Enabled = false;
            //         SecondCombinedKey.Enabled = false;
            //   }
            ConjunctionGroup.Resize += (sender, args) =>
            {
                ConjunctionEdit.Width = (int)(ConjunctionEdit.Width * 1.4);
            };
            PopulateKeyDrop1();
            PopulateKeyDrop2();
        }

        /// <summary>
        /// validates the form's data, setting error messages in the main form as appropriate
        /// </summary>
        /// <returns>"true" if the form's data contains no errors, otherwise "false"</returns>
        public bool ValidateInput()
        {
            if (Parent != null)
            {
                // test to make sure two keys have been selected to form the combined key
                if ((FirstCombinedKey.SelectedIndex == -1) || (SecondCombinedKey.SelectedIndex == -1))
                {
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        MainForm.ErrorMsg = Properties.Resources.sCombinedResponseIncompleteException;
                    }));
                    return false;
                }

                // test to make sure the same key isn't selected twice
                if (FirstCombinedKey.SelectedIndex == SecondCombinedKey.SelectedIndex)
                {
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        MainForm.ErrorMsg = Properties.Resources.sCombinedResponseDuplicateKeysException;
                    }));
                    return false;
                }

                // test to make sure a conjunction has been entered
                if (ConjunctionEdit.TextValue == String.Empty)
                {
                    MainForm.BeginInvoke(new Action(() =>
                    {
                        MainForm.ErrorMsg = Properties.Resources.sCombinedResponseNoConjunctionException;
                    }));
                    return false;
                }
                return true;
            }
            this.ParentChanged += (sender, args) =>
            {
                ValidateInput();
            };
            return true;
        }

        /// <summary>
        /// populates the drop-down lists of keys that can be combined into a conjoined response key
        /// </summary>
        private void PopulateKeyDrop1()
        {
            FirstCombinedKey.Items.Clear();
            IEnumerable<Uri> keyUris = CIAT.SaveFile.GetAllIATKeyUris();
            foreach (Uri u in keyUris)
            {
                CIATKey key = CIAT.SaveFile.GetIATKey(u);
                if ((key.KeyType == IATKeyType.SimpleKey) || (key.KeyType == IATKeyType.ReversedKey))
                {
                    FirstCombinedKey.Items.Add(key);
                }
            }
        }

        public void Clear()
        {
            FirstCombinedKey.SelectedItem = null;
            SecondCombinedKey.SelectedItem = null;
            ConjunctionEdit.TextDisplayItemUri = null;
        }

        private void PopulateKeyDrop2()
        {
            SecondCombinedKey.Items.Clear();
            IEnumerable<Uri> keyUris = CIAT.SaveFile.GetAllIATKeyUris();
            foreach (Uri u in keyUris)
            {
                CIATKey key = CIAT.SaveFile.GetIATKey(u);
                if ((key.KeyType == IATKeyType.SimpleKey) || (key.KeyType == IATKeyType.ReversedKey))
                {
                    SecondCombinedKey.Items.Add(key);
                }
            }

        }


        private void FirstCombinedKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            BaseKey1Uri = (FirstCombinedKey.SelectedItem as CIATKey)?.URI;
            ValidateInput();
        }

        private void SecondCombinedKey_SelectedIndexChanged(object sender, EventArgs e)
        {
            BaseKey2Uri = (SecondCombinedKey.SelectedItem as CIATKey)?.URI;
            ValidateInput();
        }

        public new void Dispose()
        {
            LeftValue.Dispose();
            RightValue.Dispose();
        }
    }
}
