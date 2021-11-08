using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class SurveyTextFormatPanel : Panel
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
            ItemGroup.Location = new Point(0, 0);
            ItemPanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Item);
            ItemPanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            ItemPanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ItemPanel.OnColorSelectionStart += (sender, args) => { if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null)) ResponsePanel.HaltColorSelection(); ActiveColorSelect = ItemPanel; };
            ItemGroup.Controls.Add(ItemPanel);
            ItemGroup.Height = ItemPanel.Height + 20;
            this.Controls.Add(ItemGroup);


            ResponseGroup = new GroupBox();
            ResponseGroup.Text = "Response format";
            ResponseGroup.Location = new Point(0, ItemPanel.Bottom);
            ResponsePanel = new SurveyTextFormatControl(SurveyItemFormat.EFor.Response);
            ResponsePanel.ItemFormat = new SurveyItemFormat(SurveyItemFormat.EFor.Response);
            ResponsePanel.OnColorSelectionEnd += (sender, args) => { ActiveColorSelect = null; };
            ResponsePanel.OnColorSelectionStart += (sender, args) => { if (ActiveColorSelect == ItemPanel) ItemPanel.HaltColorSelection(); ActiveColorSelect = ResponsePanel; };
            ResponseGroup.Controls.Add(ResponsePanel);
            ResponseGroup.Height = ResponsePanel.Height + 20;
            if (respType != CResponse.EResponseType.Instruction)
                this.Controls.Add(ResponseGroup);

            DoneButton.Text = "Done";
            DoneButton.Location = new Point(((this.Width - DoneButton.Width) >> 1), this.Height - (DoneButton.Height + 5));
            DoneButton.Click += (s, args) => { if (OnDone != null) OnDone(this, args); };
            this.Controls.Add(DoneButton);

            this.PickerPanel = new Panel();
            this.PickerPanel.AutoScroll = true;
            this.PickerPanel.Location = new Point(5, (respType == CResponse.EResponseType.Instruction) ? ItemGroup.Bottom + 10 : ResponseGroup.Bottom + 10);
            this.PickerPanel.Size = new Size(this.Width - 10, (DoneButton.Top - 5) - this.PickerPanel.Top);
            Picker = new ColorPicker(Color.Black);
            Picker.SelectionMade += (s, args) =>
            {
                if (ActiveColorSelect == ItemPanel)
                    ItemPanel.TextColor = args.Color;
                else if ((ActiveColorSelect == ResponsePanel) && (ResponsePanel != null))
                    ResponsePanel.TextColor = args.Color;
            };
            PickerPanel.Controls.Add(Picker);
            this.Controls.Add(PickerPanel);

            this.SizeChanged += (sender, args) =>
            {
                SuspendLayout();
                this.DoneButton.Location = new Point(((this.Width - DoneButton.Width) >> 1), this.Height - (DoneButton.Height + 5));
                PickerPanel.Size = new Size(this.Width - 10, (DoneButton.Top - 10) - this.PickerPanel.Top);
                ResponseGroup.Size = new Size(this.Width - 10, ResponsePanel.Height + 40);
                ResponseGroup.Location = new Point(ResponseGroup.Left, ItemGroup.Bottom + 5);
                ItemGroup.Size = new Size(this.Width - 10, ItemPanel.Height + 40);
                if (ItemType != CResponse.EResponseType.Instruction)
                {
                    this.PickerPanel.Location = new Point(this.Picker.Left, ResponseGroup.Bottom + 5);
                    this.PickerPanel.Size = new Size(this.PickerPanel.Width, DoneButton.Top - 10 - this.PickerPanel.Top);
                }

                ResumeLayout(false);
            };
        }
    }
}
