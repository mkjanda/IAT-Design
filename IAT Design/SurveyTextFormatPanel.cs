using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class SurveyTextFormatPanel : UserControl
    {
        public event EventHandler OnDone = null;

        private SurveyTextFormatControl ItemPanel, ResponsePanel;
        private CResponse.EResponseType _ItemType;
        private Button DoneButton = new Button();
        private Panel PickerPanel;
        private GroupBox ItemGroup = null, ResponseGroup = null;
        private ColorPicker Picker;
        private SurveyTextFormatControl ActiveColorSelect = null;

        public CResponse.EResponseType ItemType
        {
            get
            {
                return _ItemType;
            }
            set
            {
                _ItemType = value;
                if ((Controls.Contains(ResponseGroup)) && (value == CResponse.EResponseType.Instruction))
                {
                    SuspendLayout();
                    ItemGroup.Text = "Question Format";
                    Controls.Remove(ResponseGroup);
                    PickerPanel.Location = new Point(5, ItemGroup.Bottom + 10);
                    PickerPanel.Size = new Size(PickerPanel.Width - 5, DoneButton.Top - 10 - PickerPanel.Top);
                    ResumeLayout(false);
                }
                else if (!Controls.Contains(ResponseGroup) && (value != CResponse.EResponseType.Instruction))
                {
                    SuspendLayout();
                    ItemGroup.Text = "Text Format";
                    Controls.Add(ResponseGroup);
                    PickerPanel.Location = new Point(5, ResponseGroup.Bottom + 10);
                    PickerPanel.Size = new Size(PickerPanel.Width - 5, DoneButton.Top - 10 - PickerPanel.Top);
                    ResumeLayout(false);
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
                ResponsePanel.ItemFormat = value;
            }
        }

        public SurveyTextFormatPanel(CResponse.EResponseType respType)
        {
            ItemGroup = new GroupBox();
            _ItemType = respType;
            if (ItemType == CResponse.EResponseType.Instruction)
                ItemGroup.Text = "Text format";
            else
                ItemGroup.Text = "Question format";
            ItemPanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Item);
            ItemPanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            ItemPanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ItemPanel.OnColorSelectionStart += (sender, args) => { if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null)) ResponsePanel.HaltColorSelection(); ActiveColorSelect = ItemPanel; };
            ItemPanel.Location = new Point(3, 18);
            ItemPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            Controls.Add(ItemPanel);

            ResponsePanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Response);
            ResponsePanel.Location = new Point(0, ItemPanel.Bottom);
            ResponsePanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
            ResponsePanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ResponsePanel.OnColorSelectionStart += (sender, args) => { if (ActiveColorSelect == ItemPanel) ItemPanel.HaltColorSelection(); ActiveColorSelect = ResponsePanel; };
            ResponsePanel.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            if (respType != CResponse.EResponseType.Instruction)
                Controls.Add(ResponsePanel);
            DoneButton.Text = "Done";
            DoneButton.Location = new Point(((this.Width - DoneButton.Width) >> 1), this.Height - (DoneButton.Height + 5));
            DoneButton.Click += (s, args) => { if (OnDone != null) OnDone(this, args); };
            DoneButton.Anchor = AnchorStyles.Bottom;
            this.Controls.Add(DoneButton);
            this.HandleCreated += (sender, args) =>
            {
                this.PickerPanel = new Panel();
                this.PickerPanel.AutoScroll = true;
                this.PickerPanel.Location = new Point(5, (respType == CResponse.EResponseType.Instruction) ? ItemPanel.Bottom + 10 : ResponsePanel.Bottom + 10);
                this.PickerPanel.Size = new Size(this.Width - 10, (DoneButton.Top - 5) - this.PickerPanel.Top);
                this.PickerPanel.Anchor = AnchorStyles.Bottom;
                Picker = new ColorPicker(Color.Black);
                Picker.SelectionMade += (s, args) =>
                {
                    if (ActiveColorSelect == ItemPanel)
                        ItemPanel.TextColor = args.Color;
                    else if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null))
                        ResponsePanel.TextColor = args.Color;
                };
                if (ItemType != CResponse.EResponseType.Instruction)
                {
                    this.PickerPanel.Location = new Point(this.Picker.Left, ResponsePanel.Bottom + 5);
                    this.PickerPanel.Size = new Size(this.PickerPanel.Width, DoneButton.Top - 10 - this.PickerPanel.Top);
                }
                else
                {
                    this.PickerPanel.Location = new Point(this.Picker.Left, ItemPanel.Bottom + 5);
                    this.PickerPanel.Size = new Size(this.PickerPanel.Width, DoneButton.Top - 10 - this.PickerPanel.Top);

                }
                PickerPanel.Controls.Add(Picker);
                this.Controls.Add(PickerPanel);
            };

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
            PerformLayout();
        }
    }
}
