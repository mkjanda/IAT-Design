using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public class ResponseKeyPanel : UserControl
    {

        private void InitializeComponent()
        {
            this.AutoScaleDimensions = new SizeF(72F, 72F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.CreateResponseKeyButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ModifyResponseKeyCombo = new System.Windows.Forms.ComboBox();
            this.SimpleRadio = new System.Windows.Forms.RadioButton();
            this.ReversedRadio = new System.Windows.Forms.RadioButton();
            this.CombinedRadio = new System.Windows.Forms.RadioButton();
            this.ResponseKeyNameLabel = new System.Windows.Forms.Label();
            this.ResponseKeyName = new System.Windows.Forms.TextBox();
            this.CreateButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.ResponseKeyGroup = new System.Windows.Forms.GroupBox();
            this.Instructions = new System.Windows.Forms.TextBox();
            this.InstructionsGroup = new System.Windows.Forms.GroupBox();
            this.LeftKeyGroup = new System.Windows.Forms.GroupBox();
            this.RightKeyBox = new System.Windows.Forms.GroupBox();
            this.ResponseKeyGroup.SuspendLayout();
            this.InstructionsGroup.SuspendLayout();
            this.LeftKeyGroup.SuspendLayout();
            this.RightKeyBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CreateResponseKeyButton
            // 
            this.CreateResponseKeyButton.Location = new System.Drawing.Point(31, 103);
            this.CreateResponseKeyButton.Name = "CreateResponseKeyButton";
            this.CreateResponseKeyButton.Size = new System.Drawing.Size(147, 23);
            this.CreateResponseKeyButton.TabIndex = 1;
            this.CreateResponseKeyButton.Text = "Create New Resonse Key";
            this.CreateResponseKeyButton.UseVisualStyleBackColor = true;
            this.CreateResponseKeyButton.Click += new System.EventHandler(this.CreateResponseKeyButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Or Select a Response Key to Modify:";
            // 
            // ModifyResponseKeyCombo
            // 
            this.ModifyResponseKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModifyResponseKeyCombo.FormattingEnabled = true;
            this.ModifyResponseKeyCombo.Location = new System.Drawing.Point(371, 105);
            this.ModifyResponseKeyCombo.Name = "ModifyResponseKeyCombo";
            this.ModifyResponseKeyCombo.Size = new System.Drawing.Size(144, 21);
            this.ModifyResponseKeyCombo.TabIndex = 3;
            this.ModifyResponseKeyCombo.SelectedIndexChanged += new System.EventHandler(this.ModifyResponseKeyCombo_SelectedIndexChanged);
            // 
            // SimpleRadio
            // 
            this.SimpleRadio.AutoSize = true;
            this.SimpleRadio.Location = new System.Drawing.Point(10, 47);
            this.SimpleRadio.Name = "SimpleRadio";
            this.SimpleRadio.Size = new System.Drawing.Size(128, 17);
            this.SimpleRadio.TabIndex = 4;
            this.SimpleRadio.TabStop = true;
            this.SimpleRadio.Text = "Simple Response Key";
            this.SimpleRadio.UseVisualStyleBackColor = true;
            this.SimpleRadio.CheckedChanged += new System.EventHandler(this.SimpleRadio_CheckedChanged);
            // 
            // ReversedRadio
            // 
            this.ReversedRadio.AutoSize = true;
            this.ReversedRadio.Location = new System.Drawing.Point(144, 47);
            this.ReversedRadio.Name = "ReversedRadio";
            this.ReversedRadio.Size = new System.Drawing.Size(143, 17);
            this.ReversedRadio.TabIndex = 5;
            this.ReversedRadio.TabStop = true;
            this.ReversedRadio.Text = "Reversed Response Key";
            this.ReversedRadio.UseVisualStyleBackColor = true;
            this.ReversedRadio.CheckedChanged += new System.EventHandler(this.ReversedRadio_CheckedChanged);
            this.ReversedRadio.Enabled = true;
            // 
            // CombinedRadio
            // 
            this.CombinedRadio.AutoSize = true;
            this.CombinedRadio.Location = new System.Drawing.Point(293, 47);
            this.CombinedRadio.Name = "CombinedRadio";
            this.CombinedRadio.Size = new System.Drawing.Size(144, 17);
            this.CombinedRadio.TabIndex = 6;
            this.CombinedRadio.TabStop = true;
            this.CombinedRadio.Text = "Combined Response Key";
            this.CombinedRadio.UseVisualStyleBackColor = true;
            this.CombinedRadio.CheckedChanged += new System.EventHandler(this.CombinedRadio_CheckedChanged);
            // 
            // ResponseKeyNameLabel
            // 
            this.ResponseKeyNameLabel.AutoSize = true;
            this.ResponseKeyNameLabel.Location = new System.Drawing.Point(54, 22);
            this.ResponseKeyNameLabel.Name = "ResponseKeyNameLabel";
            this.ResponseKeyNameLabel.Size = new System.Drawing.Size(110, 13);
            this.ResponseKeyNameLabel.TabIndex = 8;
            this.ResponseKeyNameLabel.Text = "Response Key Name:";
            // 
            // ResponseKeyName
            // 
            this.ResponseKeyName.Location = new System.Drawing.Point(168, 19);
            this.ResponseKeyName.Name = "ResponseKeyName";
            this.ResponseKeyName.Size = new System.Drawing.Size(124, 20);
            this.ResponseKeyName.TabIndex = 9;
            this.ResponseKeyName.TextChanged += new System.EventHandler(this.ResponseKeyName_TextChanged);
            // 
            // CreateButton
            // 
            this.CreateButton.Location = new System.Drawing.Point(541, 461);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(111, 35);
            this.CreateButton.TabIndex = 12;
            this.CreateButton.Text = "Create Response";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(667, 461);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(111, 35);
            this.BackButton.TabIndex = 13;
            this.BackButton.Text = "Previous Window";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // ResponseKeyGroup
            // 
            this.ResponseKeyGroup.Controls.Add(this.ResponseKeyName);
            this.ResponseKeyGroup.Controls.Add(this.ResponseKeyNameLabel);
            this.ResponseKeyGroup.Controls.Add(this.CombinedRadio);
            this.ResponseKeyGroup.Controls.Add(this.ReversedRadio);
            this.ResponseKeyGroup.Controls.Add(this.SimpleRadio);
            this.ResponseKeyGroup.Location = new System.Drawing.Point(50, 132);
            this.ResponseKeyGroup.Name = "ResponseKeyGroup";
            this.ResponseKeyGroup.Size = new System.Drawing.Size(444, 82);
            this.ResponseKeyGroup.TabIndex = 15;
            this.ResponseKeyGroup.TabStop = false;
            this.ResponseKeyGroup.Text = "Response Key";
            // 
            // Instructions
            // 
            this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Instructions.Location = new System.Drawing.Point(6, 19);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new System.Drawing.Size(769, 66);
            this.Instructions.TabIndex = 16;
            // 
            // InstructionsGroup
            // 
            this.InstructionsGroup.Controls.Add(this.Instructions);
            this.InstructionsGroup.Location = new System.Drawing.Point(3, 3);
            this.InstructionsGroup.Name = "InstructionsGroup";
            this.InstructionsGroup.Size = new System.Drawing.Size(781, 96);
            this.InstructionsGroup.TabIndex = 17;
            this.InstructionsGroup.TabStop = false;
            this.InstructionsGroup.Text = "Instructions";
            // 
            // LeftKeyGroup
            // 
            this.LeftKeyGroup.Controls.Add(this.LeftPreviewPane);
            this.LeftKeyGroup.Location = new System.Drawing.Point(520, 105);
            this.LeftKeyGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LeftKeyGroup.Name = "LeftKeyGroup";
            this.LeftKeyGroup.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LeftKeyGroup.Size = new System.Drawing.Size(258, 173);
            this.LeftKeyGroup.TabIndex = 18;
            this.LeftKeyGroup.TabStop = false;
            this.LeftKeyGroup.Text = "Left Key Preview";
            // 
            // RightKeyBox
            // 
            this.RightKeyBox.Controls.Add(this.RightPreviewPane);
            this.RightKeyBox.Location = new System.Drawing.Point(520, 282);
            this.RightKeyBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RightKeyBox.Name = "RightKeyBox";
            this.RightKeyBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RightKeyBox.Size = new System.Drawing.Size(258, 173);
            this.RightKeyBox.TabIndex = 19;
            this.RightKeyBox.TabStop = false;
            this.RightKeyBox.Text = "Right Key Preview";
            // 
            // RightPreviewPane
            // 
            // ResponseKeyPanel
            // 
            this.Controls.Add(this.RightKeyBox);
            this.Controls.Add(this.LeftKeyGroup);
            this.Controls.Add(this.InstructionsGroup);
            this.Controls.Add(this.ResponseKeyGroup);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.CreateButton);
            this.Controls.Add(this.ModifyResponseKeyCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CreateResponseKeyButton);
            this.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10F);
            this.Dock = DockStyle.Fill;
            this.Name = "ResponseKeyPanel";
            this.Load += new System.EventHandler(this.ResponseKeyPanel_Load);
            this.ParentChanged += new System.EventHandler(this.ResponseKeyPanel_ParentChanged);
            this.ResponseKeyGroup.ResumeLayout(false);
            this.ResponseKeyGroup.PerformLayout();
            this.InstructionsGroup.ResumeLayout(false);
            this.InstructionsGroup.PerformLayout();
            this.LeftKeyGroup.ResumeLayout(false);
            this.RightKeyBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button CreateResponseKeyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ModifyResponseKeyCombo;
        private System.Windows.Forms.RadioButton SimpleRadio;
        private System.Windows.Forms.RadioButton ReversedRadio;
        private System.Windows.Forms.RadioButton CombinedRadio;
        private System.Windows.Forms.Label ResponseKeyNameLabel;
        private System.Windows.Forms.TextBox ResponseKeyName;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.GroupBox ResponseKeyGroup;
        private System.Windows.Forms.TextBox Instructions;
        private System.Windows.Forms.GroupBox InstructionsGroup;
        private System.Windows.Forms.GroupBox LeftKeyGroup;
        private System.Windows.Forms.GroupBox RightKeyBox;

        // size and location values for the control and child controls
        static public Size ResponseKeyPanelSize = new Size(794, 574);
        static public Size SimpleResponseKeyPanelSize = new Size(415, 135);
        static public Point LeftSimpleResponseKeyPanelLocation = new Point(75, 240);
        static public Point RightSimpleResponseKeyPanelLocation = new Point(75, 395);
        static public Size ReverseResponseKeyPanelSize = new Size(615, 81);
        static public Point ReverseResponseKeyPanelLocation = new Point(85, 260);
        static public Size CombinedResponseKeyPanelSize = new Size(615, 260);
        static public Point CombinedResponseKeyPanelLocation = new Point(95, 260);

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
            if (LeftSimplePanel.Visible)
                if (!LeftSimplePanel.ValidateInput())
                    return;
            if (RightSimplePanel.Visible)
                if (!RightSimplePanel.ValidateInput())
                    return;
            if (RightSimplePanel.Visible && LeftSimplePanel.Visible)
            {
                MainForm.ErrorMsg = String.Empty;
                HasErrors = false;
                return;
            }

            // test a reversed key
            if (ReversePanel.Visible)
            {
                if (!ReversePanel.ValidateInput())
                    return;
                else
                {
                    MainForm.ErrorMsg = String.Empty;
                    HasErrors = false;
                }
            }

            if (CombinedPanel.Visible)
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
                //                Size = Images.ImageMediaType.ResponseKeyPreview.ImageSize,
                Dock = DockStyle.Fill,
                TabIndex = 0,
                TabStop = false,
                BackColor = CIAT.SaveFile.Layout.BackColor,
                AutoScaleDimensions = new SizeF(72F, 72F),
                AutoScaleMode = AutoScaleMode.Dpi
            };
            RightPreviewPane = new ImageDisplay()
            {
                Location = new Point(2, 15),
                Margin = new Padding(2, 2, 2, 2),
                //                Size = Images.ImageMediaType.ResponseKeyPreview.ImageSize,
                Dock = DockStyle.Fill,
                TabIndex = 0,
                TabStop = false,
                BackColor = CIAT.SaveFile.Layout.BackColor,
                AutoScaleDimensions = new SizeF(72F, 72F),
                AutoScaleMode = AutoScaleMode.Dpi
            };
            LeftKeyGroup.Controls.Add(LeftPreviewPane);
            RightKeyBox.Controls.Add(RightPreviewPane);
            LeftSimplePanel = new SimpleResponseKeyPanel(ResponseKeySide.Left, LeftPreviewPane)
            {
                Location = LeftSimpleResponseKeyPanelLocation,
                Size = SimpleResponseKeyPanelSize,
                DisplayItemUri = DIBase.DINull.URI,
                Visible = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            Controls.Add(LeftSimplePanel);
            RightSimplePanel = new SimpleResponseKeyPanel(ResponseKeySide.Right, RightPreviewPane)
            {
                Location = RightSimpleResponseKeyPanelLocation,
                Size = SimpleResponseKeyPanelSize,
                DisplayItemUri = DIBase.DINull.URI,
                Visible = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            Controls.Add(RightSimplePanel);
            ReversePanel = new ReverseResponseKeyPanel(LeftPreviewPane, RightPreviewPane)
            {
                Size = ReverseResponseKeyPanelSize,
                Location = ReverseResponseKeyPanelLocation,
                Visible = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };
            Controls.Add(ReversePanel);
            CombinedPanel = new CombinedResponseKeyPanel(LeftPreviewPane, RightPreviewPane)
            {
                Size = CombinedResponseKeyPanelSize,
                Location = CombinedResponseKeyPanelLocation,
                ConjunctionUri = (ModifyResponseKeyCombo.SelectedItem == null) ? new DIConjunction().URI : (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).ConjunctionUri,
                Visible = false,
                Anchor = AnchorStyles.Left
            };
            Controls.Add(CombinedPanel);
            PerformLayout();
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
                ResponseKeyName.Enabled = true;
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
                LeftSimplePanel.DisplayItemUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? DIBase.DINull.URI :
                    ((ModifyResponseKeyCombo.SelectedItem as CIATKey).LeftValue as DIBase).URI;
                RightSimplePanel.DisplayItemUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? DIBase.DINull.URI :
                    ((ModifyResponseKeyCombo.SelectedItem as CIATKey).RightValue as DIBase).URI;
                if (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).Type == DIType.ResponseKeyText)
                    (CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri) as DIResponseKeyText).ResumeLayout(true);
                if (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).Type == DIType.ResponseKeyText)
                    (CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri) as DIResponseKeyText).ResumeLayout(true);
                LeftSimplePanel.Visible = true;
                RightSimplePanel.Visible = true;
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                    CIAT.SaveFile.GetDI(LeftSimplePanel.DisplayItemUri).ScheduleInvalidation();
                    CIAT.SaveFile.GetDI(RightSimplePanel.DisplayItemUri).ScheduleInvalidation();
                }
            }
            else
            {
                LeftSimplePanel.Visible = false;
                RightSimplePanel.Visible = false;
                LeftSimplePanel.Clear();
                RightSimplePanel.Clear();
                ResponseKeyName.Text = String.Empty;
            }
        }

        private void ReversedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ReversedRadio.Checked)
            {
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                ReversePanel.BaseKeyUri = (ModifyResponseKeyCombo.SelectedIndex == -1) ? null : (ModifyResponseKeyCombo.SelectedItem as CIATKey).URI;
                ReversePanel.Visible = true;
            }
            else
            {
                ReversePanel.Visible = false;
                ReversePanel.Clear();
            }
        }

        private void CombinedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (CombinedRadio.Checked)
            {
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                {
                    CombinedPanel.BaseKey1Uri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).BaseKey1Uri;
                    CombinedPanel.BaseKey2Uri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).BaseKey2Uri;
                    CombinedPanel.ConjunctionUri = (ModifyResponseKeyCombo.SelectedItem as CIATDualKey).ConjunctionUri;
                };
                if (ModifyResponseKeyCombo.SelectedIndex != -1)
                    ResponseKeyName.Text = (ModifyResponseKeyCombo.SelectedItem as CIATKey).Name;
                CombinedPanel.Visible = true;
            }
            else
            {
                CombinedPanel.Visible = false;
                CombinedPanel.Clear();
            }
        }

        private void CreateResponseKeyButton_Click(object sender, EventArgs e)
        {
            ResponseKeyGroup.Enabled = true;
            ResponseKeyName.Text = String.Empty;
            ModifyResponseKeyCombo.SelectedIndex = -1;
            SimpleRadio.Checked = true;
            CombinedRadio.Checked = false;
            CombinedRadio.Enabled = false;
            ReversedRadio.Checked = false;
            ReversedRadio.Enabled = false;
            ResponseKeyGroup.Enabled = true;
            LeftPreviewPane.SetImage(DINull.DINull.IImage);
            RightPreviewPane.SetImage(DINull.DINull.IImage);
            ValidateInput();
        }

        private void ModifyResponseKeyCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ModifyResponseKeyCombo.SelectedIndex == -1)
                return;
            SuspendLayout();
            ResponseKeyGroup.Enabled = false;
            SimpleRadio.Checked = false;
            ReversedRadio.Checked = false;
            CombinedRadio.Checked = false;
            CIATKey key = (ModifyResponseKeyCombo.SelectedItem) as CIATKey;
            ResponseKeyName.Text = key.Name;
            if (key.KeyType == IATKeyType.SimpleKey)
            {
                SimpleRadio.Checked = true;
            }
            if (key.KeyType == IATKeyType.ReversedKey)
            {
                ReversedRadio.Checked = true;
                ReversePanel.BaseKeyUri = key.URI;
            }
            if (key.KeyType == IATKeyType.DualKey)
            {
                CombinedRadio.Checked = true;
                CombinedPanel.BaseKey1Uri = (key as CIATDualKey).BaseKey1Uri;
                CombinedPanel.BaseKey2Uri = (key as CIATDualKey).BaseKey2Uri;
                CombinedPanel.ConjunctionUri = (key as CIATDualKey).ConjunctionUri;
            }
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
                    simpleKey.InvalidateBlockPreviews();
                    simpleKey.ValueChanged();
                }
                simpleKey.Save();
                simpleKey.LeftValue.PreviewPanel = null;
                simpleKey.RightValue.PreviewPanel = null;
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
            LeftPreviewPane.SetImage(DINull.DINull.IImage);
            RightPreviewPane.SetImage(DINull.DINull.IImage);
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
