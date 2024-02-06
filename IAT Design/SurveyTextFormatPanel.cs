using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class SurveyTextFormatPanel : UserControl
    {
        public event EventHandler OnDone = null;

        private SurveyTextFormatControl ItemPanel, ResponsePanel;
        //        private CResponse.EResponseType _ItemType;
        private Button DoneButton = new Button();
        private Panel PickerPanel;
        private GroupBox ItemGroup = null, ResponseGroup = null;
        private ColorPicker Picker;
        private SurveyTextFormatControl ActiveColorSelect = null;

        public CResponse.EResponseType ItemType
        {
            get
            {
                return CResponse.EResponseType.Instruction;
            }
            set
            {
                if (value == CResponse.EResponseType.Instruction)
                {
                    ItemPanel.Visible = true;
                }
                else if (value != CResponse.EResponseType.Instruction)
                {
                    ResponsePanel.Visible = false;
                }
            }
        }

        public SurveyItemFormat ItemFormat
        {
            get
            {
                return ItemPanel.ItemFormat;
            }
            set
            {
                if (!Controls.Contains(ItemGroup))
                    Controls.Add(ItemGroup);
                ItemPanel.ItemFormat = value;
            }
        }

        public SurveyItemFormat ResponseFormat
        {
            get
            {
                if (ResponsePanel == null)
                    return null;
                return ResponsePanel.ItemFormat;
            }
            set
            {
                if (ResponsePanel == null)
                    return;
                if (!Controls.Contains(ResponseGroup))
                    Controls.Add(ResponseGroup);
                ResponsePanel.ItemFormat = value;
            }
        }

        public SurveyTextFormatPanel()
        {
            ItemGroup = new GroupBox();
            ItemGroup.Text = "Question format";
            ItemPanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Item);
            ItemPanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            ItemPanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ItemPanel.OnColorSelectionStart += (sender, args) => { if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null)) ResponsePanel.HaltColorSelection(); ActiveColorSelect = ItemPanel; };
            ItemPanel.Location = new Point(3, 18);
            ItemPanel.AutoScaleDimensions = new SizeF(72F, 72F);
            ItemPanel.AutoScaleMode = AutoScaleMode.Dpi;
            //ItemPanel.Dock = DockStyle.Fill;
            ItemGroup.Location = new Point(0, 0);
            ItemGroup.Size = ItemPanel.Size + new Size(6, 24);
            ItemGroup.Controls.Add(ItemPanel);
            Controls.Add(ItemGroup);

            ResponsePanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Response);
            ResponsePanel.Location = new Point(3, 18);
            ResponsePanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
            ResponsePanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ResponsePanel.OnColorSelectionStart += (sender, args) => { if (ActiveColorSelect == ItemPanel) ItemPanel.HaltColorSelection(); ActiveColorSelect = ResponsePanel; };
            ResponsePanel.AutoScaleDimensions = new SizeF(72F, 72F);
            ResponsePanel.AutoScaleMode = AutoScaleMode.Dpi;
            ResponseGroup = new GroupBox();
            ResponseGroup.Controls.Add(ResponsePanel);
            ResponseGroup.Text = "Response format";
            ResponseGroup.Location = new Point(0, ItemGroup.Bottom + DoneButton.Height);
            ResponseGroup.Size = ResponsePanel.Size + new Size(6, 24);
            Controls.Add(ResponseGroup);
            Picker = new ColorPicker(Color.Black);
            Picker.AutoScaleDimensions = new SizeF(72F, 72F);
            Picker.AutoScaleMode = AutoScaleMode.Dpi;
            Picker.Location = new Point(0, ResponseGroup.Bottom + 5);
            Picker.SelectionMade += (s, args) =>
            {
                if (ActiveColorSelect == ItemPanel)
                    ItemPanel.TextColor = args.Color;
                else if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null))
                    ResponsePanel.TextColor = args.Color;
            };
            Controls.Add(Picker);
            this.Height = ResponsePanel.Bottom + Picker.Height + (DoneButton.Height << 1) >> 1;
            DoneButton.Text = "Done";
            DoneButton.Location = new Point(((this.Width - DoneButton.Width) >> 1), this.Height - (DoneButton.Height << 1));
            DoneButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            DoneButton.Click += (s, args) => { if (OnDone != null) OnDone(this, args); };
            DoneButton.Anchor = AnchorStyles.Bottom;
            this.Controls.Add(DoneButton);

            /*
                        this.SizeChanged += (sender, args) =>
                        {
                            SuspendLayout();
                            this.DoneButton.Location = new Point(((this.Width - DoneButton.Width) >> 1), this.Height - (DoneButton.Height + 5));
                            PickerPanel.Size = new Size(this.Width - 10, (DoneButton.Top - 10) - this.PickerPanel.Top);
                            ResponseGroup.Size = new Size(this.Width - 10, ResponsePanel.Height + 40);
                            ResponseGroup.Location = new Point(ResponseGroup.Left, ItemGroup.Bottom + 5);
                            ItemGroup.Size = new Size(this.Width - 10, ItemPanel.Height + 40);

                            ResumeLayout(false);
                        };*/
            this.AutoScroll = true;
            PerformLayout();
        }
    }
}
