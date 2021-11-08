using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class StimulusPanel : Panel
    {
        private List<StimulusEdit> _StimulusEdits;
        private Font _DataFont, _LabelFont, _InstructionsFont;
        private System.Drawing.Color _ChildBackColor, _DataFontColor, _LabelFontColor, _SubControlForeColor, _InstructionsFontColor, _StimulusNameColor;
        private StimulusEdit _ActiveStimulusEdit;
        private CColorComboItem[] ColorRectComboItems;
        private CFontComboItem[] FontFaceComboItems;
        private Size _ColorRectSize;
        private int _MaxFontFaceWidth, _MaxFontSizeWidth, _MaxColorWidth, _MaxFontFaceImageWidth;
        private static float[] _FontSizes = { 12, 14, 16, 18, 22, 26, 32, 38, 48, 58 };
//        private IATItemPreviewPanel _PreviewPanel;
        static private Padding _StimulusEditPadding = new Padding(0, 5, 0, 5);
        private bool bUpdatingFromCode;
        public delegate void StimulusChangedHandler(CDisplayItem stimulus, int ndx);
        public delegate void KeyedDirectionChangedHandler(KeyedDirection keyedDir, int ndx);
        public StimulusChangedHandler OnStimulusChanged;
        public KeyedDirectionChangedHandler OnKeyedDirectionChanged;

        public delegate void StimulusDeletedHandler(int ndx);
        public StimulusDeletedHandler OnStimulusDeleted;

        public Padding StimulusEditPadding
        {
            get
            {
                return _StimulusEditPadding;
            }
        }

        public StimulusEdit ActiveStimulusEdit
        {
            get
            {
                return _ActiveStimulusEdit;
            }
            set
            {
                _ActiveStimulusEdit = value;
            }
        }

        public CDisplayItem ActiveStimulus
        {
            get
            {
                if (ActiveStimulusEdit == null)
                    return null;
                return ActiveStimulusEdit.Stimulus;
            }
            set
            {
                if (ActiveStimulusEdit == null)
                    return;
                ActiveStimulusEdit.Stimulus = value;
            }
        }

        public KeyedDirection ActiveStimulusKeyedDir
        {
            get
            {
                if (ActiveStimulusEdit == null)
                    return CIATItem.EKeyedDir.None;
                return ActiveStimulusEdit.KeyedDir;
            }
            set
            {
                if (ActiveStimulusEdit == null)
                    return;
                switch (value)
                {
                    case CIATItem.EKeyedDir.None:
                        ActiveStimulusEdit.IsLeftKeyed = false;
                        ActiveStimulusEdit.IsRightKeyed = false;
                        break;

                    case CIATItem.EKeyedDir.Left:
                        ActiveStimulusEdit.IsLeftKeyed = true;
                        break;

                    case CIATItem.EKeyedDir.Right:
                        ActiveStimulusEdit.IsRightKeyed = true;
                        break;
                }
            }
        }

/*
        public IATItemPreviewPanel PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                _PreviewPanel = value;
            }
        }
*/
        new private IATBlockPanel Parent
        {
            get
            {
                if (base.Parent == null)
                    return null;
                return (IATBlockPanel)base.Parent;
            }
        }

        public static float[] FontSizes
        {
            get
            {
                return _FontSizes;
            }
        }

        public Size ColorRectSize
        {
            get
            {
                return _ColorRectSize;
            }
        }

        public int MaxFontFaceWidth
        {
            get
            {
                return _MaxFontFaceWidth;
            }
        }

        public int MaxFontFaceImageWidth
        {
            get
            {
                return _MaxFontFaceImageWidth;
            }
        }

        public int MaxFontSizeWidth
        {
            get
            {
                return _MaxFontSizeWidth;
            }
        }

        public int MaxColorWidth
        {
            get
            {
                return _MaxColorWidth;
            }
        }

        public List<StimulusEdit> StimulusEdits
        {
            get 
            {
                return _StimulusEdits;
            }
        }

        public System.Drawing.Color ChildBackColor
        {
            get
            {
                return _ChildBackColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _ChildBackColor = value;
                else
                    throw new Exception("The ChildBackColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        public System.Drawing.Color DataFontColor
        {
            get
            {
                return _DataFontColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _DataFontColor = value;
                else
                    throw new Exception("The DataFontColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        public System.Drawing.Color LabelFontColor
        {
            get
            {
                return _LabelFontColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _LabelFontColor = value;
                else
                    throw new Exception("The LabelFontColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        public System.Drawing.Color StimulusNameColor
        {
            get
            {
                return _StimulusNameColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _StimulusNameColor = value;
                else
                    throw new Exception("The StimulusNameColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        
        public System.Drawing.Color InstructionsFontColor
        {
            get
            {
                return _InstructionsFontColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _InstructionsFontColor = value;
                else
                    throw new Exception("The InstructionsFontColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        public System.Drawing.Color SubControlForeColor
        {
            get
            {
                return _SubControlForeColor;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                    _SubControlForeColor = value;
                else
                    throw new Exception("The SubControlForeColor property of StimulusPanel should be set before any child controls are created.");
            }
        }

        public Font DataFont
        {
            get
            {
                return _DataFont;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                {
                    if (_DataFont != System.Drawing.SystemFonts.DefaultFont)
                        _DataFont.Dispose();
                    _DataFont = value;
                }
                else
                    throw new Exception("The DataFont property of StimulusPanel should be set before creating any child controls.");
            }
        }

        public Font LabelFont
        {
            get
            {
                return _LabelFont;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                {
                    if (_LabelFont != System.Drawing.SystemFonts.DefaultFont)
                        _LabelFont.Dispose();
                    _LabelFont = value;
                }
                else
                    throw new Exception("The LabelFont property of StiulusPanel should be set before creating any child controls.");
            }
        }

        public Font InstructionsFont
        {
            get
            {
                return _InstructionsFont;
            }
            set
            {
                if (StimulusEdits.Count == 0)
                {
                    if (_InstructionsFont != System.Drawing.SystemFonts.DefaultFont)
                        _InstructionsFont.Dispose();
                    _InstructionsFont = value;
                }
                else
                    throw new Exception("The InstructionsFont property of StiulusPanel should be set before creating any child controls.");
            }
        }


        public StimulusPanel()
        {
            _LabelFont = System.Drawing.SystemFonts.DefaultFont;
            _DataFont = System.Drawing.SystemFonts.DefaultFont;
            _InstructionsFont = System.Drawing.SystemFonts.DefaultFont;
            _ChildBackColor = System.Drawing.SystemColors.Control;
            _DataFontColor = System.Drawing.SystemColors.ControlText;
            _LabelFontColor = System.Drawing.SystemColors.ControlText;
            _InstructionsFontColor = System.Drawing.SystemColors.ControlText;
            _SubControlForeColor = System.Drawing.SystemColors.ControlDark;
            _StimulusEdits = new List<StimulusEdit>();
            ColorRectComboItems = null;
            _ActiveStimulusEdit = null;
            _ColorRectSize = Size.Empty;
            this.ParentChanged += new EventHandler(StimulusPanel_ParentChanged);
            this.SizeChanged += new EventHandler(StimulusPanel_SizeChanged);
            BorderStyle = BorderStyle.Fixed3D;
            bUpdatingFromCode = false;
            OnStimulusChanged = null;
            OnKeyedDirectionChanged = null;
            HorizontalScroll.Enabled = false;
            OnStimulusDeleted = null;
        }

        void StimulusPanel_SizeChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            bUpdatingFromCode = true;
            for (int ctr = 0; ctr < StimulusEdits.Count; ctr++)
                StimulusEdits[ctr].RecalcLayout();
            bUpdatingFromCode = false;
            ResumeLayout();
        }

        private void StimulusPanel_ParentChanged(object sender, EventArgs e)
        {
            /*
            if (base.Parent != null)
                if (PreviewPanel == null)
                    throw new Exception("A StimulusPanel object must have it's Preview Panel property set before it is added to a control collection.");
             */
        }

        private void GenerateColorRectComboItems()
        {
            _ColorRectSize = new Size((3 * DataFont.Height) >> 1, LabelFont.Height);
            ColorRectComboItems = CColorComboItem.GenerateColorComboItems(LabelFont, ColorRectSize);
            for (int ctr = 0; ctr < ColorRectComboItems.Length; ctr++)
                ColorRectComboItems[ctr].NameColor = DataFontColor;
        }

        private void GenerateFontFamilyComboItems()
        {
            FontFaceComboItems = new CFontComboItem[IATConfigMainForm.AvailableFonts.Length];
            for (int ctr = 0; ctr < IATConfigMainForm.AvailableFonts.Length; ctr++)
            {
                FontFaceComboItems[ctr] = new CFontComboItem();
                FontFaceComboItems[ctr].FontFaceName = IATConfigMainForm.AvailableFonts[ctr].FamilyName;
                FontFaceComboItems[ctr].FontFaceImage = IATConfigMainForm.AvailableFonts[ctr].FontImage;
            }
        }

        private void CalcMaxComboSizes()
        {
            Size sz;
            _MaxFontFaceWidth = -1;
            _MaxFontFaceImageWidth = -1;
            for (int ctr = 0; ctr < IATConfigMainForm.AvailableFonts.Length; ctr++)
            {
                sz = TextRenderer.MeasureText(IATConfigMainForm.AvailableFonts[ctr].FamilyName, DataFont);
                if (sz.Width > _MaxFontFaceWidth)
                    _MaxFontFaceWidth = sz.Width;
                if (IATConfigMainForm.AvailableFonts[ctr].FontImage.Width > _MaxFontFaceImageWidth)
                    _MaxFontFaceImageWidth = IATConfigMainForm.AvailableFonts[ctr].FontImage.Width;
            }
            if (_MaxFontFaceWidth > 125)
                _MaxFontFaceWidth = 125;

            _MaxFontSizeWidth = -1;
            for (int ctr = 0; ctr < FontSizes.Length; ctr++)
            {
                sz = TextRenderer.MeasureText(String.Format("{0:F00}pt", FontSizes[ctr]), DataFont);
                if (sz.Width > _MaxFontSizeWidth)
                    _MaxFontSizeWidth = sz.Width;
            }

            _MaxColorWidth = -1;
            for (int ctr = 0; ctr < ColorRectComboItems.Length; ctr++)
            {
                sz = ColorRectComboItems[ctr].Measure();
                if (sz.Width > _MaxColorWidth)
                    _MaxColorWidth = sz.Width;
            }
        }

        public void AddStimulusEdit()
        {
            if (ColorRectComboItems == null)
            {
                GenerateColorRectComboItems();
                GenerateFontFamilyComboItems();
                CalcMaxComboSizes();
            }
            StimulusEdit sEdit = new StimulusEdit();
            StimulusEdits.Add(sEdit);
            sEdit.Size = new Size(ClientRectangle.Width, 1);
            sEdit.OnKeyedDirectionChanged = new StimulusEdit.KeyedDirectionChangedEventHandler(StimulusEdit_KeyedDirChanged);
            sEdit.OnStimulusChanged = new StimulusEdit.StimulusChangedEventHandler(StimulusEdit_OnStimulusChanged);
            sEdit.InitFontFamilyCombo(FontFaceComboItems);
            sEdit.InitFontColorCombo(ColorRectComboItems);
            sEdit.StimulusFontFace = CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontFamily;
            sEdit.StimulusColor = CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontColor;
            sEdit.StimulusFontSize = CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontSize;
            Controls.Add(sEdit);
            Invalidate();
        }

        public void AddStimulusEdit(CDisplayItem stimulus, KeyedDirection keyedDir)
        {
            if (ColorRectComboItems == null)
            {
                GenerateColorRectComboItems();
                GenerateFontFamilyComboItems();
                CalcMaxComboSizes();
            }
            StimulusEdit sEdit = new StimulusEdit();
            sEdit.OnKeyedDirectionChanged = new StimulusEdit.KeyedDirectionChangedEventHandler(StimulusEdit_KeyedDirChanged);
            sEdit.OnStimulusChanged = new StimulusEdit.StimulusChangedEventHandler(StimulusEdit_OnStimulusChanged);
            sEdit.InitFontFamilyCombo(FontFaceComboItems);
            sEdit.InitFontColorCombo(ColorRectComboItems);
            StimulusEdits.Add(sEdit);
            Controls.Add(sEdit);
            sEdit.SuspendLayout();
            sEdit.Stimulus = stimulus;
            if (keyedDir == CIATItem.EKeyedDir.Left)
                sEdit.IsLeftKeyed = true;
            else if (keyedDir == CIATItem.EKeyedDir.Right)
                sEdit.IsRightKeyed = true;
            sEdit.ResumeLayout();
            Invalidate();
        }

        public void DeleteActiveStimulus()
        {
            if (ActiveStimulusEdit != null)
            {
                int ndx = StimulusEdits.IndexOf(ActiveStimulusEdit);
                StimulusEdits.RemoveAt(ndx);
                Controls.Remove(ActiveStimulusEdit);
                ActiveStimulusEdit.Dispose();
                _ActiveStimulusEdit = null;
                if (OnStimulusDeleted == null)
                    throw new Exception("Attempt made to delete stimulus in a StimulusPanel that has not been assigned a StimulusDeletedHandler");
                OnStimulusDeleted(ndx);
            }
        }

        private void StimulusEdit_KeyedDirChanged(StimulusEdit sender, KeyedDirection keyedDir)
        {
            if (OnKeyedDirectionChanged == null)
                throw new Exception("A stimulus panel has been made use of without assigning a value to the \"keyed direction changed\" event handler");
            OnKeyedDirectionChanged(keyedDir, StimulusEdits.IndexOf(sender));
        }

        private void StimulusEdit_OnStimulusChanged(StimulusEdit sender, CDisplayItem stimulus)
        {
            if (OnStimulusChanged == null)
                throw new Exception("A stimulus panel has been made use of without assigning a value to the \"stimulus changed changed\" event handler");
            OnStimulusChanged(stimulus, StimulusEdits.IndexOf(sender));
        }

        public void SetActiveEdit(StimulusEdit sEdit)
        {
            if (ActiveStimulusEdit != null)
                if (ActiveStimulusEdit != sEdit)
                    ActiveStimulusEdit.Collapse();
            _ActiveStimulusEdit = sEdit;
            RecalcLayout();
        }

        public void RecalcLayout()
        {
            if (bUpdatingFromCode)
                return;
            int DocHeight = 0;
            SuspendLayout();
            int ScrollPos = 0;
            if (VerticalScroll.Enabled)
                ScrollPos = VerticalScroll.Value;
            int ctrStart = 0;

            for (int ctr = ctrStart; ctr < StimulusEdits.Count; ctr++)
            {
                Point ptStimEdit = StimulusEdits[ctr].Location;
                ptStimEdit.Y = DocHeight - ScrollPos;
                StimulusEdits[ctr].Location = ptStimEdit;
                DocHeight += StimulusEdits[ctr].Height;
            }
            this.Height = DocHeight;
            if (DocHeight > ClientSize.Height)
            {
                VerticalScroll.Enabled = true;
                VerticalScroll.Minimum = 0;
                VerticalScroll.Maximum = DocHeight;
                VerticalScroll.Value = ScrollPos;
                AdjustFormScrollbars(true);
            }
            else
            {
                VerticalScroll.Enabled = false;
                VerticalScroll.Minimum = 0;
                VerticalScroll.Maximum = DocHeight;
                VerticalScroll.Value = 0;
            }
            ResumeLayout();
            Invalidate();
        }

        public void RandomizeOrder()
        {
            List<StimulusEdit> tempList = new List<StimulusEdit>();
            Random rand = new Random();
            while (StimulusEdits.Count > 0)
            {
                int ndx = rand.Next(StimulusEdits.Count);
                tempList.Add(StimulusEdits[ndx]);
                StimulusEdits.RemoveAt(ndx);
            }
            _StimulusEdits = tempList;
            RecalcLayout();
        }

        public void Clear()
        {
            SuspendLayout();
            StimulusEdits.Clear();
            Controls.Clear();
            _ActiveStimulusEdit = null;
            ResumeLayout(false);
            RecalcLayout();
        }

    }
}
