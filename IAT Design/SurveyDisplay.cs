using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    partial class SurveyDisplay : UserControl
    {
        private bool IsDisposed { get; set; } = false;
        private bool HasCaption;

        private int ScrollBarWidth;
        bool AdjustingForScrollbar;
        private int DocHeight;

        private static readonly Brush Rosebud = new SolidBrush(Color.FromArgb(182, 95, 154));
        private static readonly Brush Rosebloom = new SolidBrush(Color.FromArgb(226, 144, 178));
        private static readonly Brush BlueHorizon = new SolidBrush(Color.FromArgb(78, 100, 130));

        /// <summary>
        /// contains survey items that have been cut or copied from the survey
        /// </summary>
        private List<CSurveyItem> Clipboard = new List<CSurveyItem>();

        /// <summary>
        /// the margin around each survey question
        /// </summary>
        static public Padding QuestionDisplayMargin = new Padding(10, 10, 10, 10);


        /// <summary>
        /// the interior padding of the survey display control
        /// </summary>
        static public Padding InteriorPadding = new Padding(5, 5, 30, 5);

        /// <summary>
        /// the width of the line that pads the left and right of currently selected questions
        /// </summary>
        private const int SelectionLineWidth = 6;

        /// <summary>
        /// a list of the question displays in the survey display
        /// </summary>
        private List<ISurveyItemDisplay> QuestionEdits;

        /// <summary>
        /// the number of currently selected questions
        /// </summary>
        private int NumSelectedQuestions;

        /// <summary>
        /// the index of the most recently selected question
        /// </summary>
        public int LastSelectedQuestionNdx { get; private set; }

        /// <summary>
        /// indicates whether the contents of the clipboard have been pasted
        /// a value of false indicates that the clipboard contents have not been pasted
        /// a value of true indicates that the clipboard contents have beenn pasted
        /// </summary>
        private bool ClipboardFresh;

        /// <summary>
        /// enables or disables the question insertion feature as oppose to appending questions
        /// </summary>
        /// <param name="bEnable">true to enable, false to disable</param>
        /// <param name="SelectInsert">true to select insert when the function is called</param>
        public delegate void InsertEnabler(bool bEnable, bool SelectInsert);

        /// <summary>
        /// sets the "survey has caption check"
        /// </summary>
        /// <param name="bChecked">true to set the check, false to clear it</param>
        public delegate void CaptionCheckSetter(bool bChecked);

        /// <summary>
        /// a delegate that enables or disables the insert feature in the appropriate control(s)
        /// </summary>
        public InsertEnabler EnableInsert;
        private bool GroupRecalc { get; set; } = false;

        /// <summary>
        /// a delegate that enables or disables the "has caption" check in the appropriate forms
        /// </summary>
        public CaptionCheckSetter SetCaptionCheck;

        private CSurvey _Survey = null;
        public CSurvey Survey
        {
            get
            {
                if (_Survey == null)
                    return null;
                return _Survey;
            }
            set
            {
                Action<CSurvey> action = new Action<CSurvey>((value) =>
                {
                    for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
                        Controls.Remove(QuestionEdits[ctr] as Control);
                    QuestionEdits.Clear();
                    if (value == null)
                        return;
                    _Survey = value;
                    QuestionDisplay qd;
                    SuspendLayout();
                    SetCaptionCheck?.Invoke(false);
                    for (int ctr = 0; ctr < _Survey.Items.Count; ctr++)
                    {
                        ISurveyItemDisplay iSID = null;
                        if (_Survey.Items[ctr].IsCaption)
                        {
                            CaptionDisplay cd = new CaptionDisplay();
                            QuestionEdits.Add(cd);
                            QuestionEdits[ctr].SurveyItem = _Survey.Items[ctr].Clone() as CSurveyItem;
                            HasCaption = true;
                            SetCaptionCheck?.Invoke(true);
                            Controls.Add(cd);
                            iSID = cd;
                        }
                        else if (_Survey.Items[ctr].ItemType == SurveyItemType.SurveyImage)
                        {
                            SurveyImageDisplay sid = new SurveyImageDisplay();
                            Controls.Add(sid);
                            QuestionEdits.Add(sid);
                            QuestionEdits[ctr].SurveyItem = _Survey.Items[ctr].Clone() as CSurveyItem;
                            iSID = sid;
                        }
                        else if (_Survey.Items[ctr].Response.ResponseType == CResponse.EResponseType.Instruction)
                        {
                            InstructionDisplay instruct = new InstructionDisplay()
                            {
                                SurveyItem = _Survey.Items[ctr].Clone() as CSurveyItem,
                                BackColor = this.BackColor
                            };
                            iSID = instruct;
                            QuestionEdits.Add(instruct);
                            Controls.Add(instruct);
                        }
                        else
                        {
                            qd = new QuestionDisplay();
                            qd.AutoScaleMode = AutoScaleMode.Dpi;
                            qd.AutoScaleDimensions = new SizeF(72F, 72F);
                            Controls.Add(qd);
                            qd.BackColor = this.BackColor;
                            qd.SurveyItem = _Survey.Items[ctr].Clone() as CSurveyItem;
                            QuestionEdits.Add(qd);
                            iSID = qd;
                        }/*
                    if (_Survey.Items[ctr].ItemType != SurveyItemType.Caption)
                    {
                        iSID.Location = new Point(QuestionDisplayMargin.Left, DocHeight + QuestionDisplayMargin.Top);
                        iSID.Size = new Size(ClientSize.Width - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, this.Height);
                    } else
                    {
                        iSID.Location = new Point(0, 0);
                        iSID.Size = new Size(ClientSize.Width, 1);
                    }*/
                    }
                    _QuestionDisplayWidth = ClientSize.Width;
                    if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri != null)
                        if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri.Equals(_Survey.URI))
                            QuestionEdits.Where((qe) => ((qe.SurveyItem.Response.ResponseType != CResponse.EResponseType.Instruction) && (qe.SurveyItem.ItemType != SurveyItemType.SurveyImage))).ToList()[CIAT.SaveFile.IAT.UniqueResponse.ItemNum - 1].IsUnique = true;
                    ResumeLayout(true);
                    RecalcSize();
                });
                if (!IsHandleCreated)
                    HandleCreated += (sender, obj) => Invoke(action, value);
                else
                    action.Invoke(value);
            }
        }

        public int UniqueResponseItemNum
        {
            get
            {
                return CIAT.SaveFile.IAT.UniqueResponse.ItemNum;
            }
        }

        public void RefreshSurveyItems()
        {
            foreach (CSurveyItem si in _Survey.Items)
                si.Dispose();
            foreach (var qd in QuestionEdits)
            {
                if (qd.SurveyItem.Text == String.Empty)
                {
                    if (qd.SurveyItem.Response.ResponseType == CResponse.EResponseType.Instruction)
                        qd.SurveyItem.Text = Properties.Resources.sDefaultInstructionText;
                    else
                        qd.SurveyItem.Text = Properties.Resources.sDefaultQuestionText;
                }
                _Survey.Items.Add(qd.SurveyItem);
            }
        }


        /// <summary>
        /// the no-arg constructor
        /// </summary>
        public SurveyDisplay()
        {
            InitializeComponent();
            Clipboard = new List<CSurveyItem>();
            QuestionEdits = new List<ISurveyItemDisplay>();
            this.MouseEnter += new EventHandler(SurveyDisplay_MouseEnter);
            this.Load += (sender, arg) =>
            {
                GroupRecalc = true;
                foreach (ISurveyItemDisplay sid in QuestionEdits)
                    sid.RecalcSize(true);
                GroupRecalc = false;
                RecalcSize();
            };
            NumSelectedQuestions = 0;
            LastSelectedQuestionNdx = -1;
            EnableInsert = null;
            SetCaptionCheck = null;
            HasCaption = false;
            DocHeight = 0;
            ClipboardFresh = false;
            this.AutoScroll = false;
            this.HorizontalScroll.Enabled = false;
            this.HorizontalScroll.Visible = false;
        }

        void SurveyDisplay_MouseEnter(object sender, EventArgs e)
        {
            ClearActiveQuestion();
        }

        /// <summary>
        /// clears the active question selection
        /// </summary>
        public void ClearActiveQuestion()
        {
            for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
                QuestionEdits[ctr].Active = false;
        }

        /// <summary>
        /// clears the question selection
        /// </summary>
        protected void ClearSelectedQuestions()
        {
            for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
                QuestionEdits[ctr].Selected = false;
            NumSelectedQuestions = 0;
            if (EnableInsert != null)
                EnableInsert(false, false);
        }

        /// <summary>
        /// paints the survey display
        /// </summary>
        /// <param name="sender">object that sent the paint call</param>
        /// <param name="e">the paint event args</param>
        private void SurveyDisplay_Paint(object sender, PaintEventArgs e)
        {
            for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
            {
                // paint the selection marks around the selected questions
                if (QuestionEdits[ctr].IsUnique && QuestionEdits[ctr].Selected)
                {
                    e.Graphics.FillRectangle(BlueHorizon, new Rectangle(0, QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, SelectionLineWidth / 2,
                        QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));

                    //           e.Graphics.FillRectangle(BlueHorizon, new Rectangle(ClientSize.Width - SelectionLineWidth / 2, 
                    //         QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, SelectionLineWidth / 2, QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));

                    e.Graphics.FillRectangle(Rosebloom, new Rectangle(SelectionLineWidth / 2, QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, SelectionLineWidth / 2,
                        QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));

                    //         e.Graphics.FillRectangle(Rosebloom, new Rectangle(ClientSize.Width - SelectionLineWidth, 
                    //           QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, SelectionLineWidth / 2, QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));

                }
                else if (QuestionEdits[ctr].Selected || QuestionEdits[ctr].IsUnique)
                {
                    Brush br = (QuestionEdits[ctr].Selected) ? Rosebloom : BlueHorizon;
                    e.Graphics.FillRectangle(br, new Rectangle(0, QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, SelectionLineWidth,
                        QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));

                    //   e.Graphics.FillRectangle(br, new Rectangle(ClientSize.Width - SelectionLineWidth, QuestionEdits[ctr].Location.Y - QuestionDisplayMargin.Top, 
                    //     InteriorPadding.Right, QuestionEdits[ctr].Size.Height + QuestionDisplayMargin.Vertical));
                }
            }
        }


        private readonly object recalcLock1 = new object();
        public void RecalcSize(bool recalcChildren = true)
        {
            SuspendLayout();
            DocHeight = 0;
            if (recalcChildren)
            {
                lock (recalcLock1)
                {
                    Func<Task<int>> f = async () =>
                        {
                            int height = 0;
                            foreach (var qEdit in QuestionEdits.Select((edit, ndx) => new { edit = edit, ndx = ndx }))
                            {
                                var h = await qEdit.edit.RecalcSize(true);
                                height += h + QuestionDisplayMargin.Vertical;
                                foreach (var qe in QuestionEdits.Where((qe, ndx) => ndx > qEdit.ndx))
                                    qe.Location = new Point(qe.Location.X, qe.Location.Y + h + QuestionDisplayMargin.Vertical);
                            }
                            ResumeLayout();
                            this.Height = height;
                            return height;
                        };

                    var asyncResult = this.BeginInvoke(f);
                }
            }

            else
            {
                this.BeginInvoke(new Action(() =>
                {
                    lock (recalcLock1)
                    {
                        int h = 0;
                        foreach (var qEdit in QuestionEdits)
                        {
                            qEdit.Location = new Point(QuestionDisplayMargin.Left + InteriorPadding.Left, h + QuestionDisplayMargin.Top);
                            h += qEdit.Height + QuestionDisplayMargin.Vertical;
                        }
                        this.Height = h;
                        ResumeLayout();
                    }
                }));
            }
            Invalidate();
        }

        private int _QuestionDisplayWidth;
        public int QuestionDisplayWidth
        {
            get
            {
                //        return _QuestionDisplayWidth;
                return ClientSize.Width;
            }
            set
            {
                /*
                if (_QuestionDisplayWidth == value)
                    return;
                _QuestionDisplayWidth = value;
                for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
                {
                    if (HasCaption && (ctr == 0))
                    {
                        if (QuestionEdits[0].Width != value)
                            QuestionEdits[0].Width = value;
                        QuestionEdits[0].Width = ClientSize.Width;

                    }
                    else if (QuestionEdits[ctr].Width != value - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal)
                        QuestionEdits[ctr].Width = value - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal;
                }*/
                //            this.Width = value;
            }
        }

        public void UpdateUniqueResponse()
        {
            foreach (QuestionDisplay qe in QuestionEdits.Where(edit => edit is QuestionDisplay))
                qe.IsUnique = false;
            if ((CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri != null) && Survey.URI.Equals(CIAT.SaveFile.IAT.UniqueResponse.SurveyUri))
                QuestionEdits[CIAT.SaveFile.GetSurveyItem(CIAT.SaveFile.IAT.UniqueResponse.SurveyItemUri).GetItemIndex()].IsUnique = true;
            Invalidate();
        }

        public CSurveyItem GetUniqueResponseItem()
        {
            foreach (SurveyItemDisplay qEdit in QuestionEdits.Where(qe => qe is SurveyItemDisplay))
                if (qEdit.IsUnique)
                    return qEdit.SurveyItem;
            return null;
        }

        public void ClearUniqueResponseItem()
        {
            foreach (SurveyItemDisplay qEdit in QuestionEdits.Where(qe => qe is SurveyItemDisplay))
                qEdit.IsUnique = false;
            CIAT.SaveFile.IAT.UniqueResponse.Clear();
            Invalidate();
        }


        public void SelectionChanged(ISurveyItemDisplay sender, Keys ModifierKeys)
        {
            if (((ModifierKeys & Keys.Control) == Keys.Control) && ((ModifierKeys & Keys.Shift) != Keys.Shift))
            {
                if (sender.Selected)
                {
                    sender.Selected = false;
                    NumSelectedQuestions--;
                    LastSelectedQuestionNdx = -1;
                }
                else
                {
                    sender.Selected = true;
                    NumSelectedQuestions++;
                    LastSelectedQuestionNdx = QuestionEdits.IndexOf(sender);
                }
            }
            else if ((ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (NumSelectedQuestions == 0)
                {
                    sender.Selected = true;
                    NumSelectedQuestions++;
                    LastSelectedQuestionNdx = QuestionEdits.IndexOf(sender);
                }
                else
                {
                    if ((ModifierKeys & Keys.Control) != Keys.Control)
                        ClearSelectedQuestions();
                    if (LastSelectedQuestionNdx != -1)
                    {
                        int ndx = LastSelectedQuestionNdx;
                        int senderNdx = QuestionEdits.IndexOf(sender);
                        while (ndx != senderNdx)
                        {
                            if (QuestionEdits[ndx].Selected == false)
                            {
                                QuestionEdits[ndx].Selected = true;
                                NumSelectedQuestions++;
                            }
                            ndx += (ndx > senderNdx) ? -1 : 1;
                        }
                        LastSelectedQuestionNdx = senderNdx;
                    }
                    if (!sender.Selected)
                    {
                        sender.Selected = true;
                        NumSelectedQuestions++;
                    }
                }
            }
            else
            {
                ClearSelectedQuestions();
                if (!sender.Selected)
                {
                    sender.Selected = true;
                    NumSelectedQuestions++;
                    LastSelectedQuestionNdx = QuestionEdits.IndexOf(sender); ;
                }
            }
            if (EnableInsert != null)
                EnableInsert(NumSelectedQuestions == 1, ClipboardFresh);
            Invalidate();
        }

        public void AddItem(CResponse.EResponseType Type, bool bInsert)
        {
            SurveyItemDisplay qEdit;
            CSurveyItem si = new CSurveyItem()
            {
                Response = CResponse.Create(Type)
            };
            if (Type == CResponse.EResponseType.Instruction)
            {
                qEdit = new InstructionDisplay()
                {
                    Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 1),
                    BackColor = this.BackColor,
                    SurveyItem = si
                };
                Controls.Add(qEdit);
            }
            else
            {
                qEdit = new QuestionDisplay();
                qEdit.AutoScaleMode = AutoScaleMode.Dpi;
                qEdit.AutoScaleDimensions = new SizeF(72F, 72F);
                Controls.Add(qEdit);
                qEdit.Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 1);
                qEdit.BackColor = this.BackColor;
                qEdit.SurveyItem = si;
            }
            if (!bInsert)
                QuestionEdits.Add(qEdit);
            else if ((HasCaption) && (LastSelectedQuestionNdx == 0))
                QuestionEdits.Insert(1, qEdit);
            else
                QuestionEdits.Insert(LastSelectedQuestionNdx, qEdit);
            if (bInsert)
                LastSelectedQuestionNdx++;
            RecalcSize(true);
        }

        public void AddImage(bool bInsert, String imgFilename)
        {
            SurveyImageDisplay sid = new SurveyImageDisplay();
            sid.OnHeightChanged = new Action(() => RecalcSize(true));
            if (bInsert)
            {
                if ((HasCaption) && (LastSelectedQuestionNdx == 0))
                    QuestionEdits.Insert(1, sid);
                else
                    QuestionEdits.Insert(LastSelectedQuestionNdx, sid);
            }
            else
                QuestionEdits.Add(sid);
            sid.Size = new Size(QuestionDisplayWidth - (QuestionDisplayMargin.Horizontal << 1) - InteriorPadding.Horizontal, 2000);
            sid.Location = new Point(InteriorPadding.Left + QuestionDisplayMargin.Left, DocHeight + QuestionDisplayMargin.Top);
            sid.BackColor = this.BackColor;
            CSurveyItemImage sii = new CSurveyItemImage(new DISurveyImage(imgFilename));
            Controls.Add(sid);
            sid.SurveyItem = sii;
        }

        public bool SelectionContainsUniqueResponse()
        {
            int ctr = 0;
            while (ctr < QuestionEdits.Count)
            {
                if (QuestionEdits[ctr].Selected && QuestionEdits[ctr].IsUnique)
                    return true;
                ctr++;
            }
            return false;
        }

        public bool SelectionContainsUniqueResponseCandidate()
        {
            foreach (var qe in QuestionEdits.Where((qe) => qe.Selected))
                if (CUniqueResponse.UniqueResponseTypes.Contains(qe.SurveyItem.Response.ResponseType))
                    return true;
            return false;
        }

        public void DeleteSelected()
        {
            int ctr = 0;
            SuspendLayout();
            while (ctr < QuestionEdits.Count)
            {
                if (QuestionEdits[ctr].Selected)
                {
                    Controls.Remove(QuestionEdits[ctr] as Control);
                    Survey.Items.Remove(QuestionEdits[ctr].SurveyItem);
                    QuestionEdits.RemoveAt(ctr);
                }
                else
                    ctr++;
            }
            ResumeLayout(true);
            RecalcSize();
        }

        public void CutSelected()
        {
            Clipboard.Clear();
            for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
            {
                if (QuestionEdits[ctr].Selected)
                    Clipboard.Add(QuestionEdits[ctr].SurveyItem.Clone() as CSurveyItem);
            }
            ClipboardFresh = true;
            DeleteSelected();
        }

        public void CopySelected()
        {
            Clipboard.Clear();
            for (int ctr = 0; ctr < QuestionEdits.Count; ctr++)
            {
                if (QuestionEdits[ctr].Selected)
                    Clipboard.Add(QuestionEdits[ctr].SurveyItem.Clone() as CSurveyItem);
            }
            ClipboardFresh = true;
        }

        public void PasteInsert()
        {
            SuspendLayout();
            for (int ctr = 0; ctr < Clipboard.Count; ctr++)
            {
                if (Clipboard[ctr].Response.ResponseType == CResponse.EResponseType.Instruction)
                {
                    if (Clipboard[ctr].ItemType == SurveyItemType.SurveyImage)
                    {
                        SurveyImageDisplay sid = new SurveyImageDisplay();
                        sid.Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 2000);
                        sid.BackColor = this.BackColor;
                        sid.SurveyItem = Clipboard[ctr];
                        Controls.Add(sid);
                        if ((LastSelectedQuestionNdx == 0) && HasCaption)
                        {
                            QuestionEdits.Insert(1, sid);
                            Survey.Items.Insert(1, sid.SurveyItem);
                        }
                        else
                        {
                            QuestionEdits.Insert(LastSelectedQuestionNdx, sid);
                            Survey.Items.Insert(LastSelectedQuestionNdx, sid.SurveyItem);
                        }
                    }
                    else
                    {
                        InstructionDisplay instruct = new InstructionDisplay();
                        instruct.SurveyItem = Clipboard[ctr];
                        instruct.Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 1);
                        instruct.BackColor = this.BackColor;
                        Controls.Add(instruct);
                        if ((LastSelectedQuestionNdx == 0) && HasCaption)
                        {
                            QuestionEdits.Insert(1, instruct);
                            Survey.Items.Insert(1, instruct.SurveyItem);
                        }
                        else
                        {
                            QuestionEdits.Insert(LastSelectedQuestionNdx, instruct);
                            Survey.Items.Insert(LastSelectedQuestionNdx, instruct.SurveyItem);
                        }
                    }
                }
                else
                {
                    QuestionDisplay qEdit = new QuestionDisplay();
                    qEdit.AutoScaleMode = AutoScaleMode.Dpi;
                    qEdit.AutoScaleDimensions = new SizeF(72F, 72F);
                    qEdit.SurveyItem = Clipboard[ctr];
                    Controls.Add(qEdit);
                    qEdit.Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 1);
                    qEdit.BackColor = this.BackColor;
                    if (HasCaption)
                    {
                        QuestionEdits.Insert(LastSelectedQuestionNdx + 1, qEdit);
                        Survey.Items.Insert(LastSelectedQuestionNdx + 1, qEdit.SurveyItem);
                    }
                    else
                    {
                        QuestionEdits.Insert(LastSelectedQuestionNdx, qEdit);
                        Survey.Items.Insert(LastSelectedQuestionNdx, qEdit.SurveyItem);
                    }
                }
            }
            ClearSelectedQuestions();
            if ((DisplayRectangle.Height > this.Height) && (VerticalScroll.Maximum > ClientSize.Height))
            {
                VerticalScroll.Value = VerticalScroll.Maximum - ClientSize.Height;
                AdjustFormScrollbars(true);
            }
            ResumeLayout(true);
            RecalcSize(true);
            ClipboardFresh = false;
        }

        public void PasteAppend()
        {
            SuspendLayout();
            for (int ctr = 0; ctr < Clipboard.Count; ctr++)
            {
                if (Clipboard[ctr].Response.ResponseType == CResponse.EResponseType.Instruction)
                {
                    if (Clipboard[ctr].ItemType == SurveyItemType.SurveyImage)
                    {
                        SurveyImageDisplay sid = new SurveyImageDisplay();
                        sid.Size = new Size(QuestionDisplayWidth - (QuestionDisplayMargin.Horizontal << 1) - InteriorPadding.Horizontal, 2000);
                        sid.BackColor = this.BackColor;
                        sid.SurveyItem = Clipboard[ctr];
                        Controls.Add(sid);
                        QuestionEdits.Add(sid);
                    }
                    else
                    {
                        InstructionDisplay instruct = new InstructionDisplay();
                        instruct.SurveyItem = Clipboard[ctr];
                        Controls.Add(instruct);
                        QuestionEdits.Add(instruct);
                        instruct.Size = new Size(QuestionDisplayWidth - QuestionDisplayMargin.Horizontal - InteriorPadding.Horizontal, 1);
                        instruct.BackColor = this.BackColor;
                    }
                }
                else
                {
                    QuestionDisplay qEdit = new QuestionDisplay();
                    qEdit.AutoScaleMode = AutoScaleMode.Dpi;
                    qEdit.AutoScaleDimensions = new SizeF(72F, 72F);
                    qEdit.SurveyItem = Clipboard[ctr];
                    Controls.Add(qEdit);
                    QuestionEdits.Add(qEdit);
                    qEdit.Size = new Size(QuestionDisplayWidth - InteriorPadding.Horizontal - QuestionDisplayMargin.Horizontal, 1);
                    qEdit.BackColor = this.BackColor;
                }
            }
            ClearSelectedQuestions();
            if ((DisplayRectangle.Height > this.Height) && (VerticalScroll.Maximum > ClientSize.Height))
            {
                VerticalScroll.Value = VerticalScroll.Maximum - ClientSize.Height;
                AdjustFormScrollbars(true);
            }
            ResumeLayout(true);
            RecalcSize(true);
            ClipboardFresh = false;
        }

        public void AddCaption()
        {
            HasCaption = true;
            CaptionDisplay caption = new CaptionDisplay();
            caption.Size = new Size(QuestionDisplayWidth, 1);
            QuestionEdits.Insert(0, caption);
            Controls.Add(caption);
            caption.SurveyItem = new CSurveyCaption();
            caption.RefreshCaptionPreview();
        }
        public void RemoveCaption()
        {
            HasCaption = false;
            Controls.Remove(QuestionEdits[0] as Control);
            QuestionEdits.RemoveAt(0);
            RecalcSize(true);
        }

        public new void Dispose()
        {
            if (IsDisposed == true)
                return;
            IsDisposed = true;
            base.Dispose();
        }
    }
}
