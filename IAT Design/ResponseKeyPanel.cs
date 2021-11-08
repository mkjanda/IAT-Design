using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ResponseKeyPanel : UserControl
    {
        // size and location values for the control and child controls
        static public Size ResponseKeyPanelSize = new Size(794, 574);
        static public Size SimpleResponseKeyPanelSize = new Size(415, 135);
        static public Point LeftSimpleResponseKeyPanelLocation = new Point(75, 220);
        static public Point RightSimpleResponseKeyPanelLocation = new Point(75, 365);
        static public Size ReverseResponseKeyPanelSize = new Size(415, 61);
        static public Point ReverseResponseKeyPanelLocation = new Point(85, 220);
        static public Size CombinedResponseKeyPanelSize = new Size(420, 158);
        static public Point CombinedResponseKeyPanelLocation = new Point(60, 220);

        // child panels
        protected SimpleResponseKeyPanel LeftSimplePanel, RightSimplePanel;
        protected ReverseResponseKeyPanel ReversePanel;
        protected CombinedResponseKeyPanel CombinedPanel;
        protected ImageDisplay LeftPreviewPane, RightPreviewPane;
        private IATKeyType _KeyType = IATKeyType.None;
        public bool HasErrors { get; private set; } = false;
        private object lockObject = new object();


        /*
                /// <summary>
                /// gets or sets the view state
                /// </summary>
                protected IATKeyType KeyType
                {
                    get
                    {
                        return _KeyType;
                    }
                    set
                    {
                        SuspendLayout();
                        if (_KeyType != value)
                        {
                            if (_KeyType == IATKeyType.DualKey)
                            {
                                CombinedRadio.Checked = false;
                            }
                            _KeyType = value;
                            if (value == IATKeyType.SimpleKey)
                            {
                                SimpleRadio.Checked = true;
                                if (ModifyResponseKeyCombo.SelectedIndex == -1)
                                {
                                    LeftSimplePanel.DisplayItemUri = new DINull().URI;
                                    RightSimplePanel.DisplayItemUri = new DINull().URI;
                                }
                            }
                            else if (value == IATKeyType.ReversedKey)
                            {
                            }
                            else if (value == IATKeyType.DualKey)
                            {
                            }
                        }
                        if (value == IATKeyType.SimpleKey) { 
                        }
                        if (KeyType == IATKeyType.ReversedKey) {
                            ReversedRadio.Checked = true;
                        }
                        if (KeyType == IATKeyType.DualKey) {
                        ResumeLayout(false);
                    }
                }

                public void SetKey(CIATKey key)
                {
                    KeyType = key.KeyType;
            {
                CIATKey k = ModifyResponseKeyCombo.SelectedItem as CIATKey;
                LeftSimplePanel.DisplayItemUri = (k.LeftValue.Clone() as DIBase).URI;
                RightSimplePanel.DisplayItemUri = (k.RightValue.Clone() as DIBase).URI;
            }
        }
        */

        public ResponseKeyDialog MainForm
        {
            get
            {
                return (ResponseKeyDialog)Parent;
            }
        }

        public String KeyName
        {
            get
            {
                if (ModifyResponseKeyCombo.SelectedText != String.Empty)
                    return ModifyResponseKeyCombo.SelectedText;
                return ResponseKeyName.Text;
            }
        }

        private void DoValidateInput()
        {
            HasErrors = true;
            if (ResponseKeyGroup.Enabled == false)
            {
                MainForm.ErrorMsg = Properties.Resources.sSelectCreateOrModifyResponseKeyMessage;
                return;
            }

            // ensure that a name has been entered for the response key
            if ((ModifyResponseKeyCombo.SelectedItem == null) && (ResponseKeyName.Text == String.Empty))
            {
                MainForm.ErrorMsg = Properties.Resources.sUnnamedResponseKeyException;
                return;
            }

            // ensure that a response key type has been selected
            if ((!SimpleRadio.Checked) && (!ReversedRadio.Checked) && (!CombinedRadio.Checked))
            {
                MainForm.ErrorMsg = Properties.Resources.sUntypedResponseKeyException;
                return;
            }

            // test a simple key
            if (Controls.Contains(LeftSimplePanel))
                if (!LeftSimplePanel.ValidateInput())
                    return;
            if (Controls.Contains(RightSimplePanel))
                if (!RightSimplePanel.ValidateInput())
                    return;

            // test a reversed key
            if (Controls.Contains(ReversePanel))
                if (!ReversePanel.ValidateInput())
                    return;

            // test a combined key
            if (Controls.Contains(CombinedPanel))
                if (!CombinedPanel.ValidateInput())
                    return;

            MainForm.ErrorMsg = String.Empty;
            HasErrors = false;
        }

        public void ValidateInput()
        {
            if (this.IsHandleCreated)
                this.BeginInvoke(new Action(DoValidateInput));
        }

        public ResponseKeyPanel()
        {
            InitializeComponent();
            LeftPreviewPane = new ImageDisplay()
            {
                Location = new Point(2, 15),
                Margin = new Padding(2, 2, 2, 2),
                Size = Images.ImageMediaType.ResponseKeyPreview.ImageSize,
                TabIndex = 0,
                TabStop = false,
                BackColor = CIAT.SaveFile.Layout.BackColor
            };
            RightPreviewPane = new ImageDisplay()
            {
                Location = new Point(2, 15),
                Margin = new Padding(2, 2, 2, 2),
                Size = Images.ImageMediaType.ResponseKeyPreview.ImageSize,
                TabIndex = 0,
                TabStop = false,
                BackColor = CIAT.SaveFile.Layout.BackColor,
            };
            LeftKeyGroup.Controls.Add(LeftPreviewPane);
            RightKeyBox.Controls.Add(RightPreviewPane);
            AutoScaleMode = AutoScaleMode.Font;
        }

        protected void HideKeyPanels()
        {
            if (Controls.Contains(LeftSimplePanel))
                Controls.Remove(LeftSimplePanel);
            if (Controls.Contains(RightSimplePanel))
                Controls.Remove(RightSimplePanel);
            if (Controls.Contains(ReversePanel))
                Controls.Remove(ReversePanel);
            if (Controls.Contains(CombinedPanel))
                Controls.Remove(CombinedPanel);
        }

        /// <summary>
        /// populates the list of response keys that can be modified
        /// </summary>
        private void PopulateResponseKeyList()
        {
            ModifyResponseKeyCombo.Items.Clear();
            ModifyResponseKeyCombo.Sorted = true;
            foreach (Uri u in CIAT.SaveFile.GetAllIATKeyUris())
                ModifyResponseKeyCombo.Items.Add(CIAT.SaveFile.GetIATKey(u));
        }


        private void ResponseKeyPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                // perform initialization prior to displaying the panel
                ResponseKeyName.Enabled = false;
                PopulateResponseKeyList();
                ResponseKeyGroup.Enabled = false;
            }
            else
            {
                LeftSimplePanel.Dispose();
                RightSimplePanel.Dispose();
                ReversePanel.Dispose();
                CombinedPanel.Dispose();
            }
        }

        private void SimpleRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (SimpleRadio.Checked)
            {
                LeftSimplePanel = new SimpleResponseKeyPanel(ResponseKeySide.Left, LeftPreviewPane)
                {
                    Location = LeftSimpleResponseKeyPanelLocation,
                    Size = SimpleResponseKeyPanelSize,
                    DisplayItemUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? DIBase.DINull.URI : ((ModifyResponseKeyCombo.SelectedItem as CIATKey).LeftValue.Clone() as DIBase).URI,
                    AutoScaleMode = AutoScaleMode.Font
                };
                RightSimplePanel = new SimpleResponseKeyPanel(ResponseKeySide.Right, RightPreviewPane)
                {
                    Location = RightSimpleResponseKeyPanelLocation,
                    Size = SimpleResponseKeyPanelSize,
                    DisplayItemUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? DIBase.DINull.URI : ((ModifyResponseKeyCombo.SelectedItem as CIATKey).RightValue.Clone() as DIBase).URI,
                    AutoScaleMode = AutoScaleMode.Font
                };
                if (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).Type == DIType.ResponseKeyText)
                    (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri) as DIResponseKeyText).ResumeLayout(true);
                if (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).Type == DIType.ResponseKeyText)
                    (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri) as DIResponseKeyText).ResumeLayout(true);
                Controls.Add(LeftSimplePanel);
                Controls.Add(RightSimplePanel);
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                    CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).ScheduleInvalidation();
                    CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).ScheduleInvalidation();
                }
            }
            else
            {
                if (LeftSimplePanel != null)
                {
                    if (Controls.Contains(LeftSimplePanel))
                        Controls.Remove(LeftSimplePanel);
                    LeftSimplePanel.Dispose();
                    LeftSimplePanel = null;
                }
                if (RightSimplePanel != null)
                {
                    if (Controls.Contains(RightSimplePanel))
                        Controls.Remove(RightSimplePanel);
                    RightSimplePanel.Dispose();
                    RightSimplePanel = null;
                }
                ResponseKeyName.Text = String.Empty;
            }
        }

        private void ReversedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ReversedRadio.Checked)
            {
                ReversePanel = new ReverseResponseKeyPanel(LeftPreviewPane, RightPreviewPane)
                {
                    Size = ReverseResponseKeyPanelSize,
                    Location = ReverseResponseKeyPanelLocation,
                    AutoScaleMode = AutoScaleMode.Font
                };
                Controls.Add(ReversePanel);
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                ReversePanel.BaseKeyUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? null : (ModifyResponseKeyCombo.SelectedItem as CIATKey).URI;
            }
            else
            {
                if (ReversePanel != null)
                {
                    if (Controls.Contains(ReversePanel))
                        Controls.Remove(ReversePanel);
                    ReversedRadio.Checked = false;
                    ReversePanel.Dispose();
                    ReversePanel = null;
                }
            }
        }

        private void CombinedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (CombinedRadio.Checked)
            {
                CombinedPanel = new CombinedResponseKeyPanel(LeftPreviewPane, RightPreviewPane)
                {
                    Size = CombinedResponseKeyPanelSize,
                    Location = CombinedResponseKeyPanelLocation,
                    ConjunctionUri = (ModifyResponseKeyCombo.SelectedItem == null) ? new DIConjunction().URI : (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).ConjunctionUri,
                    AutoScaleMode = AutoScaleMode.Font
                };
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    CombinedPanel.BaseKey1Uri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).BaseKey1Uri;
                    CombinedPanel.BaseKey2Uri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).BaseKey2Uri;
                    CombinedPanel.ConjunctionUri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).ConjunctionUri;
                };
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                CombinedRadio.Checked = true;
                Controls.Add(CombinedPanel);
            }
            else
            {
                if (CombinedPanel != null)
                {
                    if (Controls.Contains(CombinedPanel))
                        Controls.Remove(CombinedPanel);
                    CombinedPanel.Dispose();
                    CombinedPanel = null;
                }
            }
        }

        private void CreateResponseKeyButton_Click(object sender, EventArgs e)
        {
            ResponseKeyName.Enabled = true;
            ResponseKeyName.Text = String.Empty;
            ModifyResponseKeyCombo.SelectedIndex = -1;
            SimpleRadio.Enabled = true;
            CombinedRadio.Enabled = true;
            ReversedRadio.Enabled = true;
            SimpleRadio.Checked = false;
            CombinedRadio.Checked = false;
            ReversedRadio.Checked = false;
            ResponseKeyGroup.Enabled = true;
            ValidateInput();
        }

        private void ModifyResponseKeyCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModifyResponseKeyCombo.SelectedIndex == -1)
                return;
            SuspendLayout();
            ResponseKeyName.Enabled = false;
            ResponseKeyGroup.Enabled = true;
            SimpleRadio.Checked = false;
            ReversedRadio.Checked = false;
            CombinedRadio.Checked = false;
            CIATKey key = (ModifyResponseKeyCombo.SelectedItem) as CIATKey;
            ResponseKeyName.Text = key.Name;
            if (key.KeyType == IATKeyType.SimpleKey)
                SimpleRadio.Checked = true;
            if (key.KeyType == IATKeyType.ReversedKey)
                ReversedRadio.Checked = true;
            if (key.KeyType == IATKeyType.DualKey)
                CombinedRadio.Checked = true;
            SimpleRadio.Enabled = false;
            ReversedRadio.Enabled = false;
            CombinedRadio.Enabled = false;
            ResumeLayout(false);
            ValidateInput();
        }

        private void ResponseKeyName_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void ResponseKeyPanel_Load(object sender, EventArgs e)
        {
            Instructions.Text = Properties.Resources.sResponseKeyPanelInstructions;
        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            DoValidateInput();
            if (HasErrors)
                return;
            MainForm.ErrorMsg = String.Empty;
            if (SimpleRadio.Checked)
            {
                CIATKey simpleKey = (ModifyResponseKeyCombo.SelectedIndex == -1) ? new CIATKey()
                {
                    LeftValueUri = (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).Clone() as DIBase).URI,
                    RightValueUri = (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).Clone() as DIBase).URI,
                    Name = KeyName
                } : ModifyResponseKeyCombo.SelectedItem as CIATKey;
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    simpleKey.LeftValueUri = (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).Clone() as DIBase).URI;
                    simpleKey.RightValueUri = (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).Clone() as DIBase).URI;
                    simpleKey.LeftValue.PreviewPanel = null;
                    simpleKey.RightValue.PreviewPanel = null;
                    simpleKey.InvalidateBlockPreviews();
                    simpleKey.ValueChanged();
                }
                simpleKey.Save();
                if (simpleKey.LeftValue.Type == DIType.ResponseKeyText)
                    (simpleKey.LeftValue as DIResponseKeyText).SuspendLayout();
                if (simpleKey.RightValue.Type == DIType.ResponseKeyText)
                    (simpleKey.RightValue as DIResponseKeyText).SuspendLayout();
            }
            else if (ReversedRadio.Checked)
            {
                CIATReversedKey rKey = (ModifyResponseKeyCombo.SelectedIndex == -1) ? new CIATReversedKey()
                {
                    BaseKey = CIAT.SaveFile.GetIATKey(ReversePanel.BaseKeyUri),
                    Name = KeyName
                } : ModifyResponseKeyCombo.SelectedItem as CIATReversedKey;
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    rKey.BaseKey = CIAT.SaveFile.GetIATKey(ReversePanel.BaseKeyUri);
                    rKey.Name = KeyName;
                    rKey.InvalidateBlockPreviews();
                    rKey.ValueChanged();
                }
                rKey.LeftValue.PreviewPanel = null;
                rKey.RightValue.PreviewPanel = null;
                rKey.Save();
            }
            else if (CombinedRadio.Checked)
            {
                CIATDualKey dk = (ModifyResponseKeyCombo.SelectedIndex == -1) ? new CIATDualKey()
                {
                    BaseKey1Uri = CombinedPanel.BaseKey1Uri,
                    BaseKey2Uri = CombinedPanel.BaseKey2Uri,
                    ConjunctionUri = CombinedPanel.ConjunctionUri,
                    Name = KeyName
                } : ModifyResponseKeyCombo.SelectedItem as CIATDualKey;
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    dk.BaseKey1Uri = CombinedPanel.BaseKey1Uri;
                    dk.BaseKey2Uri = CombinedPanel.BaseKey2Uri;
                    dk.Name = KeyName;
                    dk.ConjunctionUri = CombinedPanel.ConjunctionUri;
                    dk.GenerateKeyValues();
                    dk.InvalidateBlockPreviews();
                    dk.ValueChanged();
                }
                (CIAT.SaveFile.GetDI(dk.ConjunctionUri) as DIConjunction).SuspendLayout();
                dk.LeftValue.PreviewPanel = null;
                dk.RightValue.PreviewPanel = null;
            }
            ResponseKeyName.Enabled = true;
            ResponseKeyName.Text = String.Empty;
            ModifyResponseKeyCombo.SelectedIndex = -1;
            SimpleRadio.Checked = false;
            ReversedRadio.Checked = false;
            CombinedRadio.Checked = false;
            ResponseKeyGroup.Enabled = false;
            PopulateResponseKeyList();
            MainForm.Modified = true;
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            LeftSimplePanel?.Dispose();
            RightSimplePanel?.Dispose();
            ReversePanel?.Dispose();
            CombinedPanel?.Dispose();
            MainForm.Close();
        }

        public new void Dispose()
        {
            LeftSimplePanel.Dispose();
            RightSimplePanel.Dispose();
            ReversePanel.Dispose();
            CombinedPanel.Dispose();
            base.Dispose();
        }
    }
}
