using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Text.RegularExpressions;
using IATClient.ResultData;

namespace IATClient
{
    class CBoundedLengthResponseObject : CResponseObject
    {
        private Panel PreviewPanel = null;
        private String _Answer = String.Empty;
        private CSearch _Root = null;
        private CSearchContainerCollection _SearchTree = new CSearchContainerCollection();
        private TextBox AnswerBox = null;
        private List<Panel> BeginsWithCritPanels = new List<Panel>(), ContainsCritPanels = new List<Panel>(), NotContainsCritPanels = new List<Panel>();
        private List<Panel> EqualsCritPanels = new List<Panel>(), NotEqualsCritPanels = new List<Panel>(), RegExCritPanels = new List<Panel>();
        private List<Panel> CompoundSearchPanels = new List<Panel>();
        private Panel BeginsWithCritParentPanel = null, ContainsCritParentPanel = null, NotContainsCritParentPanel = null;
        private Panel EqualsCritParentPanel = null, NotEqualsCritParentPanel = null, RegExCritParentPanel = null, BlankCritParentPanel = null;
        private Panel SimpleCriteriaPanel = null, CompoundSearchParentPanel = null;
        private TextBox CompoundSearchNameBox = null, CompoundSearchDefinitionBox = null;
        private RadioButton BeginsWithRadio = null, ContainsRadio = null, NotContainsRadio = null, EqualsRadio = null, NotEqualsRadio = null, RegExRadio = null;
        private Panel CompoundCriteriaPanel = null;
        private RadioButton AllRadio = null, AnyRadio = null, NotAllRadio = null, NoneRadio = null;
        private List<Panel> CompoundCriteriaPanels = new List<Panel>();
        private Padding CriteriaPadding = new Padding(4);
        private System.Drawing.Color ControlBackColor, ControlForeColor, ControlHighlightColor = System.Drawing.Color.Blue;
        private List<CSearch> CriteriaSelection = new List<CSearch>();
        private GroupBox SimpleGroup = null;
        private Font PreviewFont = null;
        private TextBox SimpleSearchBox = null;
        private Button SimpleSearchAddButton = null, SimpleSearchDeleteButton = null, SimpleSearchRenameButton = null;
        private Button CreateCompoundSearchButton = null, ModifyCompoundSearchButton = null, DeleteCompoundSearchButton = null;
        private RadioButton AllConcatRadio = null, AnyConcatRadio = null, NoneConcatRadio = null;
        private CSearchFunctions.EFunction ActiveSimpleSearchFunction = CBoundedLengthResponseObject.CSearchFunctions.EFunction.None;
        private bool IsSimpleSearchRenaming = false;
        private GroupBox CompoundGroup = null;
        private const float SimpleSearchGroupRatio = (float)(4.0 / 10.0);
        private const float CompoundSearchGroupRatio = (float)(5.0 / 10.0);
        private Label SearchNameLabel = null;
        private TextBox SearchNameBox = null, SearchDefinitionBox = null;
        private Panel FinalSearchBox = null;
        private float FinalSearchFontSize = float.NaN;
        private Panel ActiveCompoundCriteriaPanel = null;
        private bool FinalSearchBoxInverted = false;
        private int CritPanelWidth = -1;
        private delegate void ColorPanelHandler(Panel p);
        private int TotalWidth = -1;
        private Size SimplePanelSize = Size.Empty, CompoundPanelSize = Size.Empty;
        private List<CSearch> BeginsWithTests = new List<CSearch>(), ContainsTests = new List<CSearch>(), NotContainsTests = new List<CSearch>(), EqualsTests = new List<CSearch>(), 
            NotEqualsTests = new List<CSearch>(), RegExTests = new List<CSearch>();
        private Dictionary<CSearch, Panel> SearchPanelDictionary = new Dictionary<CSearch, Panel>();
        private Dictionary<CSearch, TextBox> SearchTextBoxDictionary = new Dictionary<CSearch, TextBox>();
        private Panel ActiveSearchPanel;
        private delegate int GetBoundHandler();
        private Func<CResponseObject.CResponseSpecifier> GetBounds = null;
        private new bool bIsNew = true;

        public CBoundedLengthResponseObject(EType type, Response response)
            : base(type, response)
        {
            BoundedLength resp = (BoundedLength)response;
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetBounds);
        }

        public CBoundedLengthResponseObject(EType type, CSurveyItem sci) 
            : base(type, sci)
        {
            CBoundedLengthResponse resp = (CBoundedLengthResponse)sci.Response;
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetBounds);
        }

        public CBoundedLengthResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CBoundedLengthResponseObject(CBoundedLengthResponseObject obj, BoundedLength resp) : base(obj.Type, resp)
        {
            GetBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetBounds);
            _SearchTree = obj.SearchTree.DeepCopy();
            _Root = obj._Root;
        }

        public int MinChars
        {
            get
            {
                CResponseObject.CRange spec = (CResponseObject.CRange)GetBounds();
                return Convert.ToInt32(spec.Specifier.Substring(0, spec.Specifier.IndexOf('-')));
            }
        }

        public int MaxChars
        {
            get
            {
                CResponseObject.CRange spec = (CResponseObject.CRange)GetBounds();
                return Convert.ToInt32(spec.Specifier.Substring(spec.Specifier.IndexOf('-') + 1));
            }
        }

        public override ISurveyItemResponse Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
                if (value.IsAnswered)
                {
                    _Answer = value.Value;
                    AnswerBox.Text = value.Value;
                }
                else
                {
                    _Answer = String.Empty;
                    AnswerBox.Text = String.Empty;
                }
            }
        }


        private void DisposeOfSimpleCritPanels()
        {
            BeginsWithCritParentPanel.Controls.Clear();
            BeginsWithCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in BeginsWithCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            BeginsWithCritPanels.Clear();


            ContainsCritParentPanel.Controls.Clear();
            ContainsCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in ContainsCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            ContainsCritPanels.Clear();

            NotContainsCritParentPanel.Controls.Clear();
            NotContainsCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in NotContainsCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            NotContainsCritPanels.Clear();


            EqualsCritParentPanel.Controls.Clear();
            EqualsCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in EqualsCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            EqualsCritPanels.Clear();

            NotEqualsCritParentPanel.Controls.Clear();
            NotEqualsCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in NotEqualsCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            NotEqualsCritPanels.Clear();

            RegExCritParentPanel.Controls.Clear();
            RegExCritParentPanel.Height = SimplePanelSize.Height;
            foreach (Panel p in RegExCritPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            RegExCritPanels.Clear();
        }

        private void DisposeOfCompoundCritPanels()
        {
            CompoundCriteriaPanel.Controls.Clear();
            CompoundCriteriaPanel.Height = CompoundPanelSize.Height;
            foreach (Panel p in CompoundCriteriaPanels)
            {
                foreach (Control c in p.Controls)
                    c.Dispose();
                p.Dispose();
            }
            CompoundCriteriaPanels.Clear();
        }

        private void ResetSearches(List<CSearch> searches)
        {
            Panel p;
            int beginsWithOffset = 0, containsOffset = 0, notContainsOffset = 0, equalsOffset = 0, notEqualsOffset = 0, regexOffset = 0, compoundOffset = 0;
            CompoundGroup.ResumeLayout();
            SimpleGroup.SuspendLayout();
            DisposeOfSimpleCritPanels();
            DisposeOfCompoundCritPanels();
            foreach (CSearch search in searches)
            {
                if (search.IsTest)
                {
                    switch (search.Function)
                    {
                        case CSearchFunctions.EFunction.BeginsWith:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, beginsWithOffset);
                            BeginsWithCritPanels.Add(p);
                            BeginsWithCritParentPanel.Controls.Add(p);
                            beginsWithOffset += p.Height;
                            break;

                        case CSearchFunctions.EFunction.Contains:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, containsOffset);
                            ContainsCritPanels.Add(p);
                            ContainsCritParentPanel.Controls.Add(p);
                            containsOffset += p.Height;
                            break;

                        case CSearchFunctions.EFunction.DoesNotContain:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, notContainsOffset);
                            NotContainsCritPanels.Add(p);
                            NotContainsCritParentPanel.Controls.Add(p);
                            notContainsOffset += p.Height;
                            break;

                        case CSearchFunctions.EFunction.Equals:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, equalsOffset);
                            EqualsCritPanels.Add(p);
                            EqualsCritParentPanel.Controls.Add(p);
                            equalsOffset += p.Height;
                            break;

                        case CSearchFunctions.EFunction.NotEquals:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, notEqualsOffset);
                            NotEqualsCritPanels.Add(p);
                            NotEqualsCritParentPanel.Controls.Add(p);
                            notEqualsOffset += p.Height;
                            break;

                        case CSearchFunctions.EFunction.MatchesRegex:
                            p = InitSimplePanel(search);
                            p.Location = new Point(0, regexOffset);
                            RegExCritPanels.Add(p);
                            RegExCritParentPanel.Controls.Add(p);
                            regexOffset += p.Height;
                            break;
                    }
                }
                else
                {
                    p = InitCompoundPanel(search, CompoundCriteriaPanel.ClientRectangle.Width);
                    p.Location = new Point(0, compoundOffset);
                    CompoundCriteriaPanels.Add(p);
                    CompoundCriteriaPanels.Add(p);
                    compoundOffset += p.Height;
                }
            }
            if (BeginsWithCritParentPanel.Height < beginsWithOffset)
                BeginsWithCritParentPanel.Height = beginsWithOffset;
            if (EqualsCritParentPanel.Height < equalsOffset)
                EqualsCritParentPanel.Height = equalsOffset;
            if (NotEqualsCritParentPanel.Height < notEqualsOffset)
                NotEqualsCritParentPanel.Height = notEqualsOffset;
            if (ContainsCritParentPanel.Height < containsOffset)
                ContainsCritParentPanel.Height = containsOffset;
            if (NotContainsCritParentPanel.Height < notContainsOffset)
                NotContainsCritParentPanel.Height = notContainsOffset;
            if (RegExCritParentPanel.Height < regexOffset)
                RegExCritParentPanel.Height = regexOffset;
            if (CompoundCriteriaPanel.Height < compoundOffset)
                CompoundCriteriaPanel.Height = compoundOffset;
            SimpleGroup.ResumeLayout(false);
            CompoundGroup.ResumeLayout(false);
        }

        private int NumSimpleCriteriaSelected(CSearchFunctions.EFunction type)
        {
            int nSelected = 0;
            switch (type)
            {
                case CSearchFunctions.EFunction.BeginsWith:
                    foreach (CSearch s in BeginsWithTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;

                case CSearchFunctions.EFunction.Contains:
                    foreach (CSearch s in ContainsTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;

                case CSearchFunctions.EFunction.DoesNotContain:
                    foreach (CSearch s in NotContainsTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;

                case CSearchFunctions.EFunction.Equals:
                    foreach (CSearch s in EqualsTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;

                case CSearchFunctions.EFunction.NotEquals:
                    foreach (CSearch s in NotEqualsTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;

                case CSearchFunctions.EFunction.MatchesRegex:
                    foreach (CSearch s in RegExTests)
                        if (CriteriaSelection.Contains(s))
                            nSelected++;
                    break;
            }
            return nSelected;
        }

        private void SimpleCriterion_Click(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Panel p = (Panel)tb.Parent;
            CSearch search = (CSearch)tb.Tag;

            if (IsSimpleSearchRenaming)
                CancelRename();

            p.SuspendLayout();
            if (CriteriaSelection.Contains(search))
            {
                tb.BackColor = ControlBackColor;
                p.BackColor = ControlBackColor;
                CriteriaSelection.Remove(search);
            }
            else
            {
                tb.BackColor = ControlHighlightColor;
                p.BackColor = ControlHighlightColor;
                CriteriaSelection.Add(search);
            }
            if (NumSimpleCriteriaSelected(ActiveSimpleSearchFunction) == 1)
                SimpleSearchRenameButton.Enabled = true;
            else
                SimpleSearchRenameButton.Enabled = false;
            p.ResumeLayout(false);
        }

        private List<CSearch> GetSelectedSimpleCriteria(CSearchFunctions.EFunction type)
        {
            List<CSearch> result = new List<CSearch>();
            switch (type)
            {
                case CSearchFunctions.EFunction.BeginsWith:
                    foreach (CSearch s in BeginsWithTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;

                case CSearchFunctions.EFunction.Contains:
                    foreach (CSearch s in ContainsTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;

                case CSearchFunctions.EFunction.DoesNotContain:
                    foreach (CSearch s in NotContainsTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;

                case CSearchFunctions.EFunction.Equals:
                    foreach (CSearch s in EqualsTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;

                case CSearchFunctions.EFunction.NotEquals:
                    foreach (CSearch s in NotEqualsTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;

                case CSearchFunctions.EFunction.MatchesRegex:
                    foreach (CSearch s in RegExTests)
                        if (CriteriaSelection.Contains(s))
                            result.Add(s);
                    break;
            }
            return result;
        }

        private void CancelRename()
        {
            IsSimpleSearchRenaming = false;
            SimpleSearchBox.Text = String.Empty;
            SimpleSearchRenameButton.Text = "Rename";
            SimpleSearchAddButton.Text = "Add";
        }

        private void SimpleSearchRename_Click(object sender, EventArgs e)
        {
            if (!IsSimpleSearchRenaming)
            {
                IsSimpleSearchRenaming = true;
                SimpleSearchBox.Text = GetSelectedSimpleCriteria(ActiveSimpleSearchFunction)[0].Description;
                SimpleSearchRenameButton.Text = "Canel";
                SimpleSearchAddButton.Text = "Set";
            }
            else
                CancelRename();
        }

        private void DeleteSimpleSearch(CSearch crit)
        {
            List<CSearch> anscestry = SearchTree.GetAncestry(crit);
            SearchTree.DeleteSearch(crit);
            Panel containerPanel = null;
            List<Panel> containerList = null;
            switch (crit.Function)
            {
                case CSearchFunctions.EFunction.BeginsWith:
                    containerPanel = BeginsWithCritParentPanel;
                    containerList = BeginsWithCritPanels;
                    break;

                case CSearchFunctions.EFunction.Equals:
                    containerPanel = EqualsCritParentPanel;
                    containerList = EqualsCritPanels;
                    break;

                case CSearchFunctions.EFunction.NotEquals:
                    containerPanel = NotEqualsCritParentPanel;
                    containerList = NotEqualsCritPanels;
                    break;

                case CSearchFunctions.EFunction.Contains:
                    containerPanel = ContainsCritParentPanel;
                    containerList = ContainsCritPanels;
                    break;

                case CSearchFunctions.EFunction.DoesNotContain:
                    containerPanel = NotContainsCritParentPanel;
                    containerList = NotContainsCritPanels;
                    break;

                case CSearchFunctions.EFunction.MatchesRegex:
                    containerPanel = RegExCritParentPanel;
                    containerList = RegExCritPanels;
                    break;
            }
            Panel critPanel = null;
            foreach (Panel p in containerList)
                if ((CSearch)p.Tag == crit)
                {
                    critPanel = p;
                    break;
                }
            containerPanel.SuspendLayout();
            containerPanel.Controls.Remove(critPanel);
            for (int ctr = containerList.IndexOf(critPanel) + 1; ctr < containerList.Count; ctr++)
                containerList[ctr].Location = containerList[ctr].Location - new Size(0, critPanel.Height);
            if (containerPanel.Height - critPanel.Height < SimpleGroup.Height - 18)
                containerPanel.Size = new Size(containerPanel.Width, SimpleGroup.Height - 18);
            else
                containerPanel.Size = new Size(containerPanel.Width, containerPanel.Height - critPanel.Height);
            containerList.Remove(critPanel);
            containerPanel.ResumeLayout(false);
        }

        private void SimpleSearchDelete_Click(object sender, EventArgs e)
        {
            List<CSearch> delCriteria = GetSelectedSimpleCriteria(ActiveSimpleSearchFunction);
            bool bParentExists = false;
            foreach (CSearch crit in delCriteria)
                if (SearchTree.HasParent(crit))
                    bParentExists = true;
            if (bParentExists)
                if (MessageBox.Show("One or more of the search tests you have flagged for deletion are incorporated into compound tests.  If you delete them, these compound tests will " +
                    "be deleted as well.  If you wish to preserve your compoud tests, click the Canel button then select each compound test that incorporates one of the tests marked for deletion. " +
                    "For each of these compound tests, click the search tests you wish to delete to remove them from the compound test.  Make sure the item is not highlighted.  It is the highlighted " +
                    "items that comprise the compound test.  Click OK to proceed with the deletion of these compound tests or click cancel to halt it.", "Tests Incorporated Into Other Tests",
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return;
            Panel containerPanel = null;
            List<CSearch> containerList = null;
            switch (ActiveSimpleSearchFunction)
            {
                case CSearchFunctions.EFunction.BeginsWith:
                    containerPanel = BeginsWithCritParentPanel;
                    containerList = BeginsWithTests;
                    break;

                case CSearchFunctions.EFunction.Equals:
                    containerPanel = EqualsCritParentPanel;
                    containerList = EqualsTests;
                    break;

                case CSearchFunctions.EFunction.NotEquals:
                    containerPanel = NotEqualsCritParentPanel;
                    containerList = NotEqualsTests;
                    break;

                case CSearchFunctions.EFunction.Contains:
                    containerPanel = ContainsCritParentPanel;
                    containerList = ContainsTests;
                    break;

                case CSearchFunctions.EFunction.DoesNotContain:
                    containerPanel = NotContainsCritParentPanel;
                    containerList = NotContainsTests;
                    break;

                case CSearchFunctions.EFunction.MatchesRegex:
                    containerPanel = RegExCritParentPanel;
                    containerList = RegExTests;
                    break;
            }
            containerPanel.SuspendLayout();
            while (delCriteria.Count > 0)
            {
                CSearch crit = delCriteria[0];
                Panel p = SearchPanelDictionary[crit];
                SearchTree.DeleteSearch(crit);
                containerPanel.Controls.Remove(p);
                if (containerPanel.Height - p.Height < SimpleGroup.Height - 18)
                    containerPanel.Height = SimpleGroup.Height - 18;
                else
                    containerPanel.Height = containerPanel.Height - p.Height;
                containerList.Remove(crit);
                delCriteria.Remove(crit);
            }
            containerPanel.ResumeLayout(false);
        }

        private void AddSimpleSearch(CSearch search)
        {
            Panel containerPanel = null;
            List<Panel> containerList = null;
            switch (search.Function)
            {
                case CSearchFunctions.EFunction.BeginsWith:
                    containerPanel = BeginsWithCritParentPanel;
                    containerList = BeginsWithCritPanels;
                    break;

                case CSearchFunctions.EFunction.Equals:
                    containerPanel = EqualsCritParentPanel;
                    containerList = EqualsCritPanels;
                    break;

                case CSearchFunctions.EFunction.NotEquals:
                    containerPanel = NotEqualsCritParentPanel;
                    containerList = NotEqualsCritPanels;
                    break;

                case CSearchFunctions.EFunction.Contains:
                    containerPanel = ContainsCritParentPanel;
                    containerList = ContainsCritPanels;
                    break;

                case CSearchFunctions.EFunction.DoesNotContain:
                    containerPanel = NotContainsCritParentPanel;
                    containerList = NotContainsCritPanels;
                    break;

                case CSearchFunctions.EFunction.MatchesRegex:
                    containerPanel = RegExCritParentPanel;
                    containerList = RegExCritPanels;
                    break;
            }
            Panel p = InitSimplePanel(search);
            p.Location = new Point(0, containerList[containerList.Count - 1].Bottom);
            containerList.Add(p);
            containerPanel.SuspendLayout();
            containerPanel.Size = new Size(containerPanel.Width, containerPanel.Height + p.Height);
            containerPanel.Controls.Add(p);
            containerPanel.ResumeLayout(false);
        }

        private void SimpleSearchAdd_Click(object sender, EventArgs e)
        {
            if (SimpleSearchBox.Text.Trim() == String.Empty)
                return;
            if (IsSimpleSearchRenaming)
            {
                GetSelectedSimpleCriteria(ActiveSimpleSearchFunction)[0].Description = SimpleSearchBox.Text;
                SearchTextBoxDictionary[GetSelectedSimpleCriteria(ActiveSimpleSearchFunction)[0]].Text = SimpleSearchBox.Text;
                CancelRename();
                return;
            }
            else
            {
                String str = SimpleSearchBox.Text;
                switch (ActiveSimpleSearchFunction)
                {
                    case CSearchFunctions.EFunction.BeginsWith:
                        if (str.Length > MaxChars)
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedLengthBeginsWithError, MaxChars), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            SimpleSearchBox.Text = String.Empty;
                            return;
                        }
                        break;

                    case CSearchFunctions.EFunction.Contains:
                        if (str.Length > MaxChars)
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedLengthContainsError, MaxChars), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            SimpleSearchBox.Text = String.Empty;
                            return;
                        }
                        break;

                    case CSearchFunctions.EFunction.DoesNotContain:
                        if (str.Length > MaxChars)
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedLengthDoesNotContainError, MaxChars), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            SimpleSearchBox.Text = String.Empty;
                            return;
                        }
                        break;

                    case CSearchFunctions.EFunction.Equals:
                        if ((str.Length < MinChars) || (str.Length > MaxChars))
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedLengthEqualsError, MinChars, MaxChars), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            SimpleSearchBox.Text = String.Empty;
                            return;
                        }
                        break;

                    case CSearchFunctions.EFunction.NotEquals:
                        if ((str.Length < MinChars) || (str.Length > MaxChars))
                        {
                            MessageBox.Show(MainForm, String.Format(Properties.Resources.sBoundedLengthNotEqualsError, MinChars, MaxChars), Properties.Resources.sInvalidInput, MessageBoxButtons.OK);
                            SimpleSearchBox.Text = String.Empty;
                            return;
                        }
                        break;
                }
                CSearch search = new CSearch(SearchTree, ActiveSimpleSearchFunction, SimpleSearchBox.Text);
                AddSimpleSearch(search);
            }
        }

        private void SimpleSearchRadio_CheckedChanged(object sender, EventArgs e)
        {
            SimpleGroup.SuspendLayout();
            RadioButton rb = (RadioButton)sender;
            Panel p = (Panel)rb.Tag;
            if (rb.Checked)
            {
                if (ActiveSimpleSearchFunction == CSearchFunctions.EFunction.None)
                    SimpleGroup.Controls.Remove(BlankCritParentPanel);
                SimpleGroup.Controls.Add(p);
                ActiveSimpleSearchFunction = (CSearchFunctions.EFunction)Enum.Parse(typeof(CSearchFunctions.EFunction), p.Name);
            }
            else
            {
                SimpleGroup.Controls.Remove(p);
            }
            SimpleGroup.ResumeLayout(false);
        }

        private Panel InitSimplePanel(CSearch search)
        {
            Panel p = new Panel();
            p.Size = new Size(CritPanelWidth, TextRenderer.MeasureText(search.SearchValue, System.Drawing.SystemFonts.DialogFont,
                new Size(CritPanelWidth, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left).Height) + new Size(CriteriaPadding.Horizontal, CriteriaPadding.Vertical);
            TextBox tb = new TextBox();
            tb.Size = new Size(p.Width - CriteriaPadding.Horizontal, p.Height - CriteriaPadding.Vertical);
            tb.Location = new Point(CriteriaPadding.Left, CriteriaPadding.Top);
            tb.BorderStyle = BorderStyle.None;
            tb.Multiline = true;
            tb.ReadOnly = true;
            tb.BackColor = ControlBackColor;
            tb.ForeColor = ControlForeColor;
            tb.Text = search.SearchValue;
            tb.Tag = search;
            tb.Click += new EventHandler(SimpleCriterion_Click);
            p.Controls.Add(tb);
            p.BorderStyle = BorderStyle.FixedSingle;
            p.BackColor = ControlBackColor;
            p.ForeColor = ControlForeColor;
            p.Tag = search;
            SearchTextBoxDictionary[search] = tb;
            SearchPanelDictionary[search] = p;

            return p;
        }


        private void CreateSimplePanels(int totalWidth)
        {
            String[] RadioLabels = { "Begins With", "Contains", "Does Not Contain", "Equals", "Does Not Equal", "Matches RegExp" };
            int totalSize = 0;
            for (int ctr = 0; ctr < RadioLabels.Length; ctr++)
            {
                int nWidth = TextRenderer.MeasureText(RadioLabels[ctr], PreviewFont).Width + 10;
                if (nWidth > totalSize)
                    totalSize = nWidth;
            }

            SimpleGroup = new GroupBox();
            SimpleGroup.Size = new Size((int)(totalSize * SimpleSearchGroupRatio), 380);
            SimpleGroup.BackColor = ControlBackColor;
            SimpleGroup.ForeColor = ControlForeColor;
            SimpleGroup.Font = PreviewFont;
            SimpleGroup.Text = "Simple Search Criteria";

            int InteriorGroupWidth = (int)(totalSize * SimpleSearchGroupRatio) - 6;
            int xPos = 3 + (InteriorGroupWidth / 2) - (3 * totalSize / 2);
            BeginsWithRadio = new RadioButton();
            BeginsWithRadio.Appearance = Appearance.Button;
            BeginsWithRadio.Font = PreviewFont;
            BeginsWithRadio.Width = totalSize;
            BeginsWithRadio.Location = new Point(xPos, SimpleGroup.Height - 3 - (2 * BeginsWithRadio.Height));
            BeginsWithRadio.Text = RadioLabels[0];
            BeginsWithRadio.BackColor = ControlBackColor;
            BeginsWithRadio.ForeColor = ControlForeColor;
            BeginsWithRadio.Tag = BeginsWithCritParentPanel;
            BeginsWithRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);

            RegExRadio = new RadioButton();
            RegExRadio.Location = new Point(xPos, BeginsWithRadio.Bottom);
            RegExRadio.Font = PreviewFont;
            RegExRadio.Text = RadioLabels[5];
            RegExRadio.Width = totalSize;
            RegExRadio.BackColor = ControlBackColor;
            RegExRadio.ForeColor = ControlForeColor;
            RegExRadio.Appearance = Appearance.Button;
            RegExRadio.Tag = RegExCritParentPanel;
            RegExRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);

            xPos += totalWidth;
            EqualsRadio = new RadioButton();
            EqualsRadio.Location = new Point(xPos, BeginsWithRadio.Top);
            EqualsRadio.Font = PreviewFont;
            EqualsRadio.Text = RadioLabels[1];
            EqualsRadio.Width = totalSize;
            EqualsRadio.BackColor = ControlBackColor;
            EqualsRadio.ForeColor = ControlForeColor;
            EqualsRadio.Appearance = Appearance.Button;
            EqualsRadio.Tag = EqualsCritParentPanel;
            EqualsRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);

            NotEqualsRadio = new RadioButton();
            NotEqualsRadio.Location = new Point(xPos, RegExRadio.Top);
            NotEqualsRadio.Font = PreviewFont;
            NotEqualsRadio.Text = RadioLabels[2];
            NotEqualsRadio.Width = totalSize;
            NotEqualsRadio.BackColor = ControlBackColor;
            NotEqualsRadio.ForeColor = ControlForeColor;
            NotEqualsRadio.Appearance = Appearance.Button;
            NotEqualsRadio.Tag = NotEqualsCritParentPanel;
            NotEqualsRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);

            xPos += totalSize;
            ContainsRadio = new RadioButton();
            ContainsRadio.Location = new Point(xPos, BeginsWithRadio.Top);
            ContainsRadio.Font = PreviewFont;
            ContainsRadio.Text = RadioLabels[3];
            ContainsRadio.Width = totalSize;
            ContainsRadio.BackColor = ControlBackColor;
            ContainsRadio.ForeColor = ControlForeColor;
            ContainsRadio.Appearance = Appearance.Button;
            ContainsRadio.Tag = ContainsCritParentPanel;
            ContainsRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);

            NotContainsRadio = new RadioButton();
            NotContainsRadio.Location = new Point(xPos, RegExRadio.Top);
            NotContainsRadio.Font = PreviewFont;
            NotContainsRadio.Text = RadioLabels[4];
            NotContainsRadio.Width = totalSize;
            NotContainsRadio.BackColor = ControlBackColor;
            NotContainsRadio.ForeColor = ControlForeColor;
            NotContainsRadio.Appearance = Appearance.Button;
            NotContainsRadio.Tag = NotContainsCritParentPanel;
            NotContainsRadio.CheckedChanged += new EventHandler(SimpleSearchRadio_CheckedChanged);


            int nMaxButtonWidth = 0;
            String[] ButtonLabels = { "Rename", "Cancel", "Delete", "Add", "Set" };
            for (int ctr = 0; ctr < ButtonLabels.Length; ctr++)
            {
                int ButWidth = TextRenderer.MeasureText(ButtonLabels[ctr], PreviewFont).Width + 10;
                if (ButWidth > nMaxButtonWidth)
                    nMaxButtonWidth = ButWidth;
            }

            SimpleSearchRenameButton = new Button();
            SimpleSearchRenameButton.BackColor = ControlBackColor;
            SimpleSearchRenameButton.ForeColor = ControlForeColor;
            SimpleSearchRenameButton.Font = PreviewFont;
            SimpleSearchRenameButton.Text = "Rename";
            SimpleSearchRenameButton.Width = nMaxButtonWidth;
            SimpleSearchRenameButton.Click += new EventHandler(SimpleSearchRename_Click);

            SimpleSearchDeleteButton = new Button();
            SimpleSearchDeleteButton.BackColor = ControlBackColor;
            SimpleSearchDeleteButton.ForeColor = ControlForeColor;
            SimpleSearchDeleteButton.Font = PreviewFont;
            SimpleSearchDeleteButton.Text = "Delete";
            SimpleSearchDeleteButton.Width = nMaxButtonWidth;
            SimpleSearchDeleteButton.Click += new EventHandler(SimpleSearchDelete_Click);

            SimpleSearchAddButton = new Button();
            SimpleSearchAddButton.BackColor = ControlBackColor;
            SimpleSearchAddButton.ForeColor = ControlForeColor;
            SimpleSearchAddButton.Text = "Add";
            SimpleSearchAddButton.Font = PreviewFont;
            SimpleSearchAddButton.Width = nMaxButtonWidth;
            SimpleSearchAddButton.Click += new EventHandler(SimpleSearchAdd_Click);

            SimpleSearchBox = new TextBox();
            SimpleSearchBox.BackColor = ControlBackColor;
            SimpleSearchBox.ForeColor = ControlForeColor;
            SimpleSearchBox.Font = PreviewFont;
            SimpleSearchBox.Size = new Size(InteriorGroupWidth - 3 * nMaxButtonWidth, SimpleSearchRenameButton.Height);
            SimpleSearchBox.Location = new Point(3, 15);

            SimpleSearchAddButton.Location = new Point(SimpleSearchBox.Right, 15);
            SimpleSearchRenameButton.Location = new Point(SimpleSearchAddButton.Right, 15);
            SimpleSearchDeleteButton.Location = new Point(SimpleSearchRenameButton.Right, 15);

            SimpleGroup.Controls.Add(BeginsWithRadio);
            SimpleGroup.Controls.Add(NotEqualsRadio);
            SimpleGroup.Controls.Add(ContainsRadio);
            SimpleGroup.Controls.Add(NotContainsRadio);
            SimpleGroup.Controls.Add(RegExRadio);
            SimpleGroup.Controls.Add(EqualsRadio);
            SimpleGroup.Controls.Add(SimpleSearchAddButton);
            SimpleGroup.Controls.Add(SimpleSearchDeleteButton);
            SimpleGroup.Controls.Add(SimpleSearchRenameButton);
            SimpleGroup.Controls.Add(SimpleSearchBox);

            SimplePanelSize = new Size(InteriorGroupWidth, BeginsWithRadio.Top - SimpleSearchBox.Bottom);
            Point PanelLoc = new Point(3, SimpleSearchBox.Bottom);

            BlankCritParentPanel = new Panel();
            BlankCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            BlankCritParentPanel.Size = SimplePanelSize;
            BlankCritParentPanel.Location = PanelLoc;
            BlankCritParentPanel.BackColor = ControlBackColor;
            BlankCritParentPanel.ForeColor = ControlForeColor;
            BlankCritParentPanel.Name = CSearchFunctions.EFunction.None.ToString();
            SimpleGroup.Controls.Add(BlankCritParentPanel);

            BeginsWithCritParentPanel = new Panel();
            BeginsWithCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            BeginsWithCritParentPanel.Size = SimplePanelSize;
            BeginsWithCritParentPanel.Location = PanelLoc;
            BeginsWithCritParentPanel.BackColor = ControlBackColor;
            BeginsWithCritParentPanel.ForeColor = ControlForeColor;
            BeginsWithCritParentPanel.AutoScroll = true;
            BeginsWithCritParentPanel.VerticalScroll.Visible = true;
            BeginsWithCritParentPanel.Name = CSearchFunctions.EFunction.BeginsWith.ToString();
            CritPanelWidth = BeginsWithCritParentPanel.ClientSize.Width;
            int yOffset = 0;
            foreach (CSearch s in BeginsWithTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                BeginsWithCritPanels.Add(p);
                BeginsWithCritParentPanel.Controls.Add(p);
                if (p.Bottom > BeginsWithCritParentPanel.Height)
                    BeginsWithCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }

            EqualsCritParentPanel = new Panel();
            EqualsCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            EqualsCritParentPanel.Size = SimplePanelSize;
            EqualsCritParentPanel.Location = PanelLoc;
            EqualsCritParentPanel.BackColor = ControlBackColor;
            EqualsCritParentPanel.ForeColor = ControlForeColor;
            EqualsCritParentPanel.AutoScroll = true;
            EqualsCritParentPanel.VerticalScroll.Visible = true;
            EqualsCritParentPanel.Name = CSearchFunctions.EFunction.Equals.ToString();
            yOffset = 0;
            foreach (CSearch s in EqualsTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                EqualsCritPanels.Add(p);
                EqualsCritParentPanel.Controls.Add(p);
                if (p.Bottom > EqualsCritParentPanel.Height)
                    EqualsCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }

            ContainsCritParentPanel = new Panel();
            ContainsCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            ContainsCritParentPanel.Size = SimplePanelSize;
            ContainsCritParentPanel.Location = PanelLoc;
            ContainsCritParentPanel.BackColor = ControlBackColor;
            ContainsCritParentPanel.ForeColor = ControlForeColor;
            ContainsCritParentPanel.AutoScroll = true;
            ContainsCritParentPanel.VerticalScroll.Visible = true;
            ContainsCritParentPanel.Name = CSearchFunctions.EFunction.Contains.ToString();
            yOffset = 0;
            foreach (CSearch s in ContainsTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                ContainsCritPanels.Add(p);
                ContainsCritParentPanel.Controls.Add(p);
                if (p.Bottom > ContainsCritParentPanel.Height)
                    ContainsCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }

            NotContainsCritParentPanel = new Panel();
            NotContainsCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            NotContainsCritParentPanel.Size = SimplePanelSize;
            NotContainsCritParentPanel.Location = PanelLoc;
            NotContainsCritParentPanel.BackColor = ControlBackColor;
            NotContainsCritParentPanel.ForeColor = ControlForeColor;
            NotContainsCritParentPanel.AutoScroll = true;
            NotContainsCritParentPanel.VerticalScroll.Visible = true;
            NotContainsCritParentPanel.Name = CSearchFunctions.EFunction.DoesNotContain.ToString();
            yOffset = 0;
            foreach (CSearch s in NotContainsTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                NotContainsCritPanels.Add(p);
                NotContainsCritParentPanel.Controls.Add(p);
                if (p.Bottom > NotContainsCritParentPanel.Height)
                    NotContainsCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }

            NotEqualsCritParentPanel = new Panel();
            NotEqualsCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            NotEqualsCritParentPanel.Size = SimplePanelSize;
            NotEqualsCritParentPanel.Location = PanelLoc;
            NotEqualsCritParentPanel.BackColor = ControlBackColor;
            NotEqualsCritParentPanel.ForeColor = ControlForeColor;
            NotEqualsCritParentPanel.AutoScroll = true;
            NotEqualsCritParentPanel.VerticalScroll.Visible = true;
            NotEqualsCritParentPanel.Name = CSearchFunctions.EFunction.NotEquals.ToString();
            yOffset = 0;
            foreach (CSearch s in NotEqualsTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                NotEqualsCritPanels.Add(p);
                NotEqualsCritParentPanel.Controls.Add(p);
                if (p.Bottom > NotContainsCritParentPanel.Height)
                    NotEqualsCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }

            RegExCritParentPanel = new Panel();
            RegExCritParentPanel.BorderStyle = BorderStyle.Fixed3D;
            RegExCritParentPanel.Size = SimplePanelSize;
            RegExCritParentPanel.Location = PanelLoc;
            RegExCritParentPanel.BackColor = ControlBackColor;
            RegExCritParentPanel.ForeColor = ControlForeColor;
            RegExCritParentPanel.AutoScroll = true;
            RegExCritParentPanel.VerticalScroll.Visible = true;
            RegExCritParentPanel.Name = CSearchFunctions.EFunction.MatchesRegex.ToString();
            foreach (CSearch s in RegExTests)
            {
                Panel p = InitSimplePanel(s);
                p.Location = new Point(0, yOffset);
                yOffset += p.Size.Height;
                RegExCritPanels.Add(p);
                RegExCritParentPanel.Controls.Add(p);
                if (p.Bottom > RegExCritParentPanel.Height)
                    RegExCritParentPanel.Height = p.Bottom;
                p.Tag = s;
            }
        }


        private void FinalSearchBox_Paint(object sender, PaintEventArgs e)
        {
            Brush foreBrush = null;
            Brush backBrush = null;
            if (FinalSearchBoxInverted)
            {
                foreBrush = new SolidBrush(ControlBackColor);
                backBrush = new SolidBrush(System.Drawing.Color.Black);
            }
            else
            {
                foreBrush = new SolidBrush(System.Drawing.Color.Black);
                backBrush = new SolidBrush(ControlBackColor);
            }
            e.Graphics.FillRectangle(backBrush, FinalSearchBox.ClientRectangle);
            Font finalSearchFont = null;
            if (FinalSearchFontSize == float.NaN)
            {
                SizeF szText = new SizeF(0, 0);
                FinalSearchFontSize = PreviewFont.SizeInPoints - .5F;
                while ((szText.Width < FinalSearchBox.Width - 12) && (szText.Height < FinalSearchBox.Height - 12))
                {
                    if (finalSearchFont != null)
                        finalSearchFont.Dispose();
                    FinalSearchFontSize += .5F;
                    finalSearchFont = new Font(PreviewFont.FontFamily, FinalSearchFontSize, FontStyle.Bold);
                    szText = e.Graphics.MeasureString("Final Search", finalSearchFont);
                }
                finalSearchFont.Dispose();
                FinalSearchFontSize -= .5F;
            }
            finalSearchFont = new Font(PreviewFont.FontFamily, FinalSearchFontSize, FontStyle.Bold);
            SizeF szFinalText = e.Graphics.MeasureString("Final Search", finalSearchFont);
            PointF ptF = new PointF((int)(FinalSearchBox.Width - szFinalText.Width) >> 1, ((int)(FinalSearchBox.Height - szFinalText.Width) >> 1));
            e.Graphics.DrawString("Final Search", finalSearchFont, foreBrush, ptF);
            finalSearchFont.Dispose();
            backBrush.Dispose();
            foreBrush.Dispose();
        }

        private void FinalSearchBox_Click(object sender, EventArgs e)
        {
            CSearch definedSearch = (CSearch)SearchDefinitionBox.Tag;
            CSearch finalSearch = (CSearch)FinalSearchBox.Tag;
            if ((definedSearch != null) && (definedSearch != finalSearch))
                return;
            if (definedSearch == finalSearch)
            {
                FinalSearchBoxInverted = false;
                SearchNameBox.Text = String.Empty;
                SearchNameBox.Enabled = false;
                SearchNameBox.ReadOnly = true;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Text = String.Empty;
                SearchDefinitionBox.Tag = null;
            }
            else if (Root == null)
            {
                FinalSearchBoxInverted = true;
                SearchNameBox.Enabled = true;
                SearchNameBox.ReadOnly = true;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchNameBox.Text = "Final Search";
                SearchDefinitionBox.Text = Root.Description;
                SearchDefinitionBox.Tag = Root;
            }
            FinalSearchBox.Invalidate();
        }


        private void HilightPanel(Panel p)
        {
            p.BackColor = System.Drawing.Color.Black;
            p.ForeColor = ControlBackColor;
            foreach (Control c in p.Controls)
            {
                c.BackColor = System.Drawing.Color.Black;
                c.ForeColor = ControlBackColor;
            }
        }

        private void InvertPanel(Panel p)
        {
            p.BackColor = ControlForeColor;
            p.ForeColor = ControlBackColor;
            foreach (Control c in p.Controls)
            {
                c.BackColor = ControlForeColor;
                c.ForeColor = ControlBackColor;
            }
        }

        private void DisinvertPanel(Panel p)
        {
            p.BackColor = ControlBackColor;
            p.ForeColor = ControlForeColor;
            foreach (Control c in p.Controls)
            {
                c.BackColor = ControlBackColor;
                c.ForeColor = ControlForeColor;
            }
        }

        private void ColorPanels(CSearch search)
        {
            CompoundSearchParentPanel.SuspendLayout();
            BeginsWithCritParentPanel.SuspendLayout();
            EqualsCritParentPanel.SuspendLayout();
            NotEqualsCritParentPanel.SuspendLayout();
            RegExCritParentPanel.SuspendLayout();
            ContainsCritParentPanel.SuspendLayout();
            NotContainsCritParentPanel.SuspendLayout();

            ColorPanelHandler childrenFun = null;
            CSearch definedSearch = (CSearch)SearchDefinitionBox.Tag;

            if (definedSearch == null)
            {
                childrenFun += new ColorPanelHandler(InvertPanel);
                Panel p = SearchPanelDictionary[search];
                HilightPanel(p);
            }
            else if (definedSearch == search)
            {
                childrenFun += new ColorPanelHandler(DisinvertPanel);
                Panel p = SearchPanelDictionary[search];
                DisinvertPanel(p);
            }
            else
                throw new InvalidOperationException();


            List<CSearchContainer> childContainers = SearchTree.SearchDefinitionMap[search];
            foreach (CSearchContainer sc in childContainers)
            {
                CSearch child = sc.Search;
                Panel p = SearchPanelDictionary[child];
                childrenFun(p);
            }
            
            CompoundSearchParentPanel.ResumeLayout(false);
            BeginsWithCritParentPanel.ResumeLayout(false);
            EqualsCritParentPanel.ResumeLayout(false);
            NotEqualsCritParentPanel.ResumeLayout(false);
            RegExCritParentPanel.ResumeLayout(false);
            ContainsCritParentPanel.ResumeLayout(false);
            NotContainsCritParentPanel.ResumeLayout(false);
        }


        private void Compound_Click(object sender, EventArgs e)
        {
            Panel senderPanel = (Panel)sender;
            CSearch senderSearch = (CSearch)senderPanel.Tag;
            SuspendLayout();
            if (ActiveSearchPanel == null)
            {
                ActiveSearchPanel = senderPanel;
                if (senderSearch.Description == String.Empty)
                    SearchNameBox.Text = "[Search]";
                else
                    SearchNameBox.Text = senderSearch.Description;
                AllConcatRadio.Checked = false;
                AnyConcatRadio.Checked = false;
                NoneConcatRadio.Checked = false;
                switch (senderSearch.ConcatMode)
                {
                    case ETestConcatination.all:
                        AllConcatRadio.Checked = true;
                        break;

                    case ETestConcatination.any:
                        AnyConcatRadio.Checked = true;
                        break;

                    case ETestConcatination.none:
                        NoneConcatRadio.Checked = true;
                        break;
                }
                SearchNameBox.Enabled = true;
                SearchNameBox.ReadOnly = false;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Text = senderSearch.Description;
                SearchDefinitionBox.Enabled = true;
                SearchDefinitionBox.BackColor = ControlBackColor;
                SearchDefinitionBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Tag = senderSearch;
                ColorPanels(senderSearch);
            }
            else if (ActiveSearchPanel == senderPanel)
            {
                if ((SearchNameBox.Text[0] != '[') && (SearchNameBox.Text[SearchNameBox.Text.Length] != ']'))
                    senderSearch.Description = SearchNameBox.Text;
                SearchNameBox.Text = String.Empty;
                SearchNameBox.Enabled = false;
                SearchNameBox.ReadOnly = true;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Text = String.Empty;
                SearchDefinitionBox.Enabled = true;
                SearchDefinitionBox.BackColor = ControlBackColor;
                SearchDefinitionBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Tag = null;
                ColorPanels(senderSearch);
            }
            else
            {
                CSearch parentSearch = (CSearch)SearchDefinitionBox.Tag;
                List<CSearch> ancestry = SearchTree.GetAncestry(parentSearch);
                if (SearchTree.Contains(senderSearch))
                {
                    MessageBox.Show("The item you are attempting to add to your search contains the search you are defining as one of its child searches.");
                    return;
                }
                if (SearchTree.IsChildOf(parentSearch, senderSearch))
                {
                    SearchTree.RemoveChildFromSearch(parentSearch, senderSearch);
                    DisinvertPanel((Panel)sender);
                }
                else
                {
                    SearchTree.AddChildToSearch(parentSearch, senderSearch);
                    InvertPanel((Panel)sender);
                }
                SearchDefinitionBox.Text = parentSearch.Description;
            }
            ResumeLayout(false);
        }

        private void SuspendLayout()
        {
            SimpleGroup.SuspendLayout();
            CompoundGroup.SuspendLayout();
            SearchDefinitionBox.SuspendLayout();
            SearchNameBox.SuspendLayout();
        }

        private void ResumeLayout(bool b)
        {
            SimpleGroup.ResumeLayout(b);
            CompoundGroup.ResumeLayout(b);
            SearchDefinitionBox.ResumeLayout(b);
            SearchNameBox.ResumeLayout(b);
        }

        private void ConcatModeButton_Click(object sender, EventArgs e)
        {
            RadioButton senderButton = (RadioButton)sender;
            if (SearchDefinitionBox.Tag == null)
                return;
            CSearch definedSearch = (CSearch)SearchDefinitionBox.Tag;
            if (senderButton == AllConcatRadio)
                definedSearch.ConcatMode = ETestConcatination.all;
            else if (senderButton == AnyConcatRadio)
                definedSearch.ConcatMode = ETestConcatination.any;
            else if (senderButton == NoneConcatRadio)
                definedSearch.ConcatMode = ETestConcatination.none;
            else
                throw new InvalidOperationException();
        }

        private void CreateCompoundSearch_Click(object sender, EventArgs e)
        {
            ETestConcatination concatMode = ETestConcatination.unset;
            if (AllConcatRadio.Checked)
                concatMode = ETestConcatination.all;
            else if (AnyConcatRadio.Checked)
                concatMode = ETestConcatination.any;
            else if (NoneConcatRadio.Checked)
                concatMode = ETestConcatination.none;

            CSearch s = SearchTree.CreateCompoundSearch(concatMode);
            Panel p = InitCompoundPanel(s, CompoundSearchParentPanel.ClientRectangle.Width);
            p.Location = new Point(0, CompoundSearchPanels[CompoundSearchPanels.Count - 1].Bottom);
            CompoundSearchParentPanel.SuspendLayout();
            CompoundSearchParentPanel.Controls.Add(p);
            if (CompoundSearchParentPanel.Height < CompoundGroup.Height - 18)
                CompoundSearchParentPanel.Height = CompoundGroup.Height - 18;
            else
                CompoundSearchParentPanel.Height = p.Bottom;
            CompoundSearchParentPanel.ResumeLayout(false);
        }

        private void DeleteCompoundSearch_Click(object sender, EventArgs e)
        {
            CSearch s = (CSearch)SearchDefinitionBox.Tag;
            List<CSearch> ancestry = SearchTree.GetAncestry(s);
            BoundedLengthDeleteConfirmation dlg = null;
            if (ancestry.Count > 0)
            {
                dlg = new BoundedLengthDeleteConfirmation(ancestry);
                if (dlg.ShowDialog() != DialogResult.OK)
                    return;
            }
            SearchTree.DeleteSearch(s);
        }


        private void CreateCompoundPanels(int totalSize)
        {

            CompoundGroup = new GroupBox();
            CompoundGroup.Size = new Size((int)(totalSize * CompoundSearchGroupRatio), 350);
            CompoundGroup.Font = PreviewFont;
            CompoundGroup.BackColor = ControlBackColor;
            CompoundGroup.ForeColor = ControlForeColor;
            CompoundGroup.Text = "Compound Searches";

            int interiorGroupWidth = CompoundGroup.Width - 6, interiorGroupHeight = CompoundGroup.Height - 18;

            CreateCompoundSearchButton = new Button();
            CreateCompoundSearchButton.Font = PreviewFont;
            CreateCompoundSearchButton.ForeColor = ControlForeColor;
            CreateCompoundSearchButton.BackColor = ControlBackColor;
            CreateCompoundSearchButton.Width = interiorGroupWidth / 3;
            CreateCompoundSearchButton.Text = "Create New Search";
            CreateCompoundSearchButton.Click += new EventHandler(CreateCompoundSearch_Click);
            CompoundGroup.Controls.Add(CreateCompoundSearchButton);

            DeleteCompoundSearchButton = new Button();
            DeleteCompoundSearchButton.Font = PreviewFont;
            DeleteCompoundSearchButton.ForeColor = ControlForeColor;
            DeleteCompoundSearchButton.BackColor = ControlBackColor;
            DeleteCompoundSearchButton.Width = interiorGroupWidth / 3;
            DeleteCompoundSearchButton.Text = "Delete Search";
            DeleteCompoundSearchButton.Click += new EventHandler(DeleteCompoundSearch_Click);
            CompoundGroup.Controls.Add(DeleteCompoundSearchButton);

            Size radioSize = CreateCompoundSearchButton.Size;
            int xOffset = CreateCompoundSearchButton.Left;
            int yOffset = CreateCompoundSearchButton.Bottom;

            AllConcatRadio = new RadioButton();
            AllConcatRadio.Appearance = Appearance.Button;
            AllConcatRadio.BackColor = ControlBackColor;
            AllConcatRadio.ForeColor = ControlForeColor;
            AllConcatRadio.Font = PreviewFont;
            AllConcatRadio.Text = "All Are True";
            AllConcatRadio.Checked = true;
            AllConcatRadio.CheckedChanged += new EventHandler(ConcatModeButton_Click);
            AllConcatRadio.Size = radioSize;
            AllConcatRadio.Location = new Point(xOffset, yOffset);
            CompoundGroup.Controls.Add(AllConcatRadio);

            xOffset += AllConcatRadio.Width;
            AnyConcatRadio = new RadioButton();
            AnyConcatRadio.Appearance = Appearance.Button;
            AnyConcatRadio.BackColor = ControlBackColor;
            AnyConcatRadio.ForeColor = ControlForeColor;
            AnyConcatRadio.Font = PreviewFont;
            AnyConcatRadio.Text = "Any Is True";
            AnyConcatRadio.CheckedChanged += new EventHandler(ConcatModeButton_Click);
            AnyConcatRadio.Size = radioSize;
            AnyConcatRadio.Location = new Point(xOffset, yOffset);
            CompoundGroup.Controls.Add(AnyConcatRadio);

            xOffset += AllConcatRadio.Width;
            NoneConcatRadio = new RadioButton();
            NoneConcatRadio.Appearance = Appearance.Button;
            NoneConcatRadio.BackColor = ControlBackColor;
            NoneConcatRadio.ForeColor = ControlForeColor;
            NoneConcatRadio.Font = PreviewFont;
            NoneConcatRadio.Text = "None Are True";
            NoneConcatRadio.CheckedChanged += new EventHandler(ConcatModeButton_Click);
            NoneConcatRadio.Size = radioSize;
            NoneConcatRadio.Location = new Point(xOffset, yOffset);
            CompoundGroup.Controls.Add(NoneConcatRadio);


            CompoundSearchParentPanel = new Panel();
            CompoundSearchParentPanel.BorderStyle = BorderStyle.Fixed3D;
            CompoundPanelSize = new Size(interiorGroupWidth, ModifyCompoundSearchButton.Top - (FinalSearchBox.Bottom + 5));
            CompoundSearchParentPanel.Size = CompoundPanelSize;
            CompoundSearchParentPanel.Location = new Point(3, 15);
            CompoundSearchParentPanel.BackColor = ControlBackColor;
            CompoundSearchParentPanel.ForeColor = ControlForeColor;
            CompoundSearchParentPanel.AutoScroll = true;
            CompoundSearchParentPanel.VerticalScroll.Visible = true;

            IEnumerable<CSearch> CompoundSearches = SearchTree.GetCompoundSearches();
            yOffset = 0;
            foreach (CSearch s in CompoundSearches)
            {
                Panel p = InitCompoundPanel(s, CompoundSearchParentPanel.ClientRectangle.Width);
                p.Location = new Point(0, yOffset);
                yOffset += p.Height;
                if (p.Bottom > CompoundSearchParentPanel.Height)
                    CompoundSearchParentPanel.Height = p.Bottom;
                CompoundSearchPanels.Add(p);
                CompoundSearchParentPanel.Controls.Add(p);
            }
            CompoundGroup.Controls.Add(CompoundSearchParentPanel);
        }



        private Panel InitCompoundPanel(CSearch search, int width)
        {
            Panel p = new Panel();
            p.Size = new Size(width, TextRenderer.MeasureText(search.Description, System.Drawing.SystemFonts.DialogFont,
                new Size(width, 0), TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left).Height) + new Size(CriteriaPadding.Horizontal, CriteriaPadding.Vertical);
            TextBox tb = new TextBox();
            tb.Size = new Size(p.Width - CriteriaPadding.Horizontal, p.Height - CriteriaPadding.Vertical);
            tb.Location = new Point(CriteriaPadding.Left, CriteriaPadding.Top);
            tb.BorderStyle = BorderStyle.None;
            tb.Multiline = true;
            tb.ReadOnly = true;
            tb.BackColor = ControlBackColor;
            tb.ForeColor = ControlForeColor;
            tb.Text = search.Description;
            tb.Tag = search;
            tb.Click += new EventHandler(Compound_Click);
            SearchTextBoxDictionary[search] = tb;
            p.Controls.Add(tb);
            p.BorderStyle = BorderStyle.FixedSingle;
            p.BackColor = ControlBackColor;
            p.ForeColor = ControlForeColor;
            p.Tag = search;

            return p;
        }


        public void DisposeOfControls(bool fullDispose)
        {
            DisposeOfSimpleCritPanels();
            DisposeOfCompoundCritPanels();
            CompoundCriteriaPanels.Clear();
            if (!fullDispose)
            {
                SearchNameBox.Text = String.Empty;
                SearchNameBox.Enabled = false;
                SearchNameBox.ReadOnly = true;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.Text = String.Empty;
                SearchDefinitionBox.Tag = null;
                return;
            }
            foreach (Control c in SimpleGroup.Controls)
                c.Dispose();
            if (SimpleGroup != null)
            {
                SimpleGroup.Dispose();
                SimpleGroup = null;
            }
            foreach (Control c in CompoundGroup.Controls)
                c.Dispose();
            if (CompoundGroup != null)
            {
                CompoundGroup.Dispose();
                CompoundGroup = null;
            }
            if (SearchNameBox != null)
            {
                SearchNameBox.Dispose();
                SearchNameBox = null;
            }
            if (SearchDefinitionBox != null)
            {
                SearchDefinitionBox.Dispose();
                SearchDefinitionBox = null;
            }
            if (FinalSearchBox != null)
            {
                FinalSearchBox.Dispose();
                FinalSearchBox = null;
            }
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            UpdateResponseObject();
            PreviewPanel = new Panel();
            PreviewPanel.ForeColor = foreColor;
            PreviewPanel.BackColor = backColor;
            PreviewPanel.BorderStyle = BorderStyle.None;
            PreviewFont = new Font(fontFamily, fontSize);
            ControlForeColor = foreColor;
            ControlBackColor = backColor;
            TotalWidth = clientWidth;
            Size szText;
            int nRows;
            if ((Type == EType.actual) || (Type == EType.dummy))
            {
                AnswerBox = new TextBox();
                AnswerBox.BackColor = backColor;
                AnswerBox.ForeColor = foreColor;
                AnswerBox.Font = PreviewFont;
                AnswerBox.Multiline = true;
                AnswerBox.Location = new Point(clientWidth / 5, 0);
                if (Type == EType.actual)
                {
                    AnswerBox.ReadOnly = true;
                    AnswerBox.BackColor = ControlBackColor;
                    AnswerBox.Text = Answer;
                    szText = TextRenderer.MeasureText(AnswerBox.Text, AnswerBox.Font, new Size(3 * clientWidth / 5, 0));
                    nRows = 8;
                    if (szText.Height / AnswerBox.Font.Height > nRows)
                    {
                        szText.Height = AnswerBox.Font.Height * nRows;
                        AnswerBox.ScrollBars = ScrollBars.Vertical;
                    }
                    szText += new Size(10, 10);
                    AnswerBox.Size = new Size(3 * clientWidth / 5, szText.Height);
                }
                else
                    AnswerBox.Size = new Size(3 * clientWidth / 5, 5 * AnswerBox.Font.Height + 6);
                PreviewPanel.Size = new Size(clientWidth, AnswerBox.Bottom);
                return PreviewPanel;
            }
            else if ((Type == EType.search) || (Type == EType.correct))
            {
                int totalWidth = clientWidth;
                CreateSimplePanels(totalWidth);
                CreateCompoundPanels(totalWidth);
                SearchNameLabel = new Label();
                SearchNameLabel.Font = PreviewFont;
                SearchNameLabel.ForeColor = ControlForeColor;
                SearchNameLabel.BackColor = ControlBackColor;
                SearchNameLabel.Text = "Searh Name:";
                SearchNameLabel.Size = TextRenderer.MeasureText(SearchNameLabel.Text, PreviewFont);
                SearchNameBox = new TextBox();
                SearchNameBox.Font = PreviewFont;
                SearchNameBox.Enabled = false;
                SearchNameBox.ForeColor = ControlForeColor;
                SearchNameBox.BackColor = ControlBackColor;
                SearchNameBox.Text = String.Empty;
                SearchNameBox.Width = (int)(.15 * totalWidth);
                SearchNameBox.Location = new Point((int)(.15 * totalWidth), 0);
                SearchNameLabel.Location = new Point((int)(.15 * totalWidth) - SearchNameLabel.Width - 5, (SearchNameBox.Height - SearchNameLabel.Height) / 2);
                SearchDefinitionBox = new TextBox();
                SearchDefinitionBox.Multiline = true;
                SearchDefinitionBox.WordWrap = true;
                SearchDefinitionBox.Size = new Size((int)(.5 * totalWidth), (int)(PreviewFont.Height / .975) * 3);
                SearchDefinitionBox.ReadOnly = true;
                SearchDefinitionBox.Font = PreviewFont;
                SearchDefinitionBox.ForeColor = ControlForeColor;
                SearchDefinitionBox.BackColor = ControlBackColor;
                SearchDefinitionBox.Location = new Point(SearchNameBox.Right + 15, 0);
                SearchDefinitionBox.Text = String.Empty;


                Font f = new Font(PreviewFont.FontFamily, 24.5F, FontStyle.Bold);
                CSearch finalSearch = SearchTree.CreateCompoundSearch(ETestConcatination.unset);
                FinalSearchBox = new Panel();
                FinalSearchBox.BackColor = ControlBackColor;
                FinalSearchBox.ForeColor = ControlForeColor;
                FinalSearchBox.Location = new Point(3, 15);
                FinalSearchBox.Size = new Size(CompoundGroup.Size.Width - 6, f.Height);
                FinalSearchBox.Paint += new PaintEventHandler(FinalSearchBox_Paint);
                FinalSearchBox.Click += new EventHandler(FinalSearchBox_Click);
                FinalSearchBox.Tag = finalSearch;
                PreviewPanel.Controls.Add(FinalSearchBox);
                _Root = finalSearch;
                SearchTree.MakeRoot(finalSearch);
                
                int horizGroupPadding = (int)(((SimpleSearchGroupRatio + CompoundSearchGroupRatio) * totalWidth) / 4);
                SimpleGroup.Location = new Point(horizGroupPadding, SearchDefinitionBox.Bottom + horizGroupPadding * 2);
                CompoundGroup.Location = new Point(TotalWidth - horizGroupPadding - CompoundGroup.Width, FinalSearchBox.Bottom + 8);
                PreviewPanel.Height = CompoundGroup.Bottom;
                PreviewPanel.SuspendLayout();
                PreviewPanel.Controls.Add(SearchNameLabel);
                PreviewPanel.Controls.Add(SearchNameBox);
                PreviewPanel.Controls.Add(SearchDefinitionBox);
                PreviewPanel.Controls.Add(SimpleGroup);
                PreviewPanel.Controls.Add(CompoundGroup);
                PreviewPanel.ResumeLayout(false);
                return PreviewPanel;
            }
            AnswerBox.Text = String.Empty;
            AnswerBox.Location = new Point(clientWidth - ElementPadding.Horizontal, 0);
            AnswerBox.Multiline = true;
            nRows = (int)Math.Ceiling((float)MaxChars / 48F);
            if (nRows > 8)
                nRows = 8;
            AnswerBox.Size = new Size(clientWidth, (int)((AnswerBox.Font.GetHeight() + 2)) * nRows) +
                new Size(AnswerBox.Margin.Horizontal, AnswerBox.Margin.Vertical);
            PreviewPanel.Controls.Add(AnswerBox);

            return PreviewPanel;
        }

        public override void DisposeOfControls()
        {
            if ((Type == CResponseObject.EType.actual) || (Type == EType.dummy))
            {
                if (AnswerBox != null)
                {
                    AnswerBox.Dispose();
                    AnswerBox = null;
                }
            }
            else
                DisposeOfControls(true);
        }


        public CSearch Root
        {
            get
            {
                return _Root;
            }
        }

        public CSearchContainerCollection SearchTree
        {
            get
            {
                return _SearchTree;
            }
        }

        public String Answer
        {
            get
            {
                return _Answer;
            }
        }

        public void SetRoot(CSearch root)
        {
            _Root = root;
        }

        public override bool IsSearchMatch(String val)
        {
            if ((Type != EType.search) || (SearchTree == null))
                throw new InvalidOperationException();
            return SearchTree.Test(val, Answer);
        }

        public class CSearchContainerCollection
        {
            private CSearch ParentSearch = null;
            private int _ID;
            private CSearchContainer ParentContainer = null;
            private int[] ChildIDs;
            public Dictionary<CSearchContainer, List<CSearchContainer>> ContainerMap = new Dictionary<CSearchContainer, List<CSearchContainer>>();
            public Dictionary<CSearch, List<CSearchContainer>> SearchDefinitionMap = new Dictionary<CSearch, List<CSearchContainer>>();
            private Dictionary<CSearch, List<CSearchContainer>> SearchToContainerMap = new Dictionary<CSearch, List<CSearchContainer>>();
            private CSearch _Root = null;
            public List<int> SearchIDList = new List<int>();
            public Dictionary<int, CSearch> SearchDictionary = new Dictionary<int, CSearch>();
            public List<int> ContainerIDList = new List<int>();

            public CSearch LookupSearch(int n)
            {
                return SearchDictionary[n];
            }

            public CSearch Root
            {
                get
                {
                    return _Root;
                }
                set
                {
                    _Root = value;
                }
            }

            public int Depth(CSearchContainer container)
            {
                CSearchContainer scParent = null;
                int depth = 0;
                bool bParentFound = false;
                while (bParentFound)
                    foreach (CSearchContainer sc1 in ContainerMap.Keys)
                    {
                        foreach (CSearchContainer sc2 in ContainerMap[sc1])
                            if (container == sc2)
                            {
                                scParent = sc1;
                                depth++;
                                bParentFound = true;
                                break;
                            }
                        if (bParentFound)
                            break;
                    }

                return depth;
            }


            public CSearchContainerCollection DeepCopy()
            {
                CSearchContainerCollection ssc = new CSearchContainerCollection();
                CopyNode(SearchToContainerMap[Root][0], ssc);
                ssc._Root = Root;
                return ssc;
            }

            protected CSearchContainer CopyNode(CSearchContainer sc, CSearchContainerCollection ssc)
            {
                if (ContainerMap.ContainsKey(sc))
                {
                    if (ContainerMap[sc].Count > 0)
                    {
                        foreach (CSearchContainer s in ContainerMap[sc])
                        {
                            if (s.Search.IsTest)
                                return new CSearchContainer(s, ssc);
                            else
                            {
                                CSearchContainer parent = new CSearchContainer(s, ssc);
                                foreach (CSearchContainer child in ContainerMap[s])
                                    AddChild(parent, CopyNode(child, ssc));
                                return parent;
                            }
                        }
                    }
                    return null;
                }
                return null;
            }

            public bool Contains(CSearch search)
            {
                foreach (CSearch s in SearchDefinitionMap.Keys)
                    if (s == search)
                        return true;
                return false;
            }

            public void MakeRoot(CSearch r)
            {
                Root = r;
            }

            public CSearch GetRoot()
            {
                return Root;
            }

            public List<CSearch> GetAllSearches()
            {
                List<CSearch> result = new List<CSearch>();
                foreach (CSearch s in SearchDefinitionMap.Keys)
                    result.Add(s);
                return result;
            }

            public void CreateNode(CSearch parent, List<CSearch> children)
            {
                SearchToContainerMap[parent] = new List<CSearchContainer>();
                CSearchContainer parentContainer = null;
                foreach (CSearchContainer container in ContainerMap.Keys)
                    if (container.Search == parent)
                    {
                        parentContainer = container;
                        break;
                    }

                foreach (CSearch s in children)
                {
                    if (s.IsTest)
                    {
                        CSearchContainer childContainer = new CSearchContainer(s, this);
                        SearchDefinitionMap[parent].Add(childContainer);
                    }
                    else
                    {
                        CSearchContainer childContainer = new CSearchContainer(s, this);
                        if (s.IsTest)
                        {
                            SearchDefinitionMap[parent].Add(childContainer);
                            ContainerMap[parentContainer].Add(childContainer);
                        }
                    }
                }
            }
            /*
            private void CreateChildNode(CSearch parent, CSearchContainer childContainer, List<CSearch> grandChildren)
            {
                childContainer.Parent = parent;
                ContainerMap[childContainer] = new List<CSearchContainer>();
                foreach (CSearch s in grandChildren)
                {
                    if (s.IsTest)
                    {
                        grandChildrenContainer = new CSearchContainer(s, this);
                        grandChildrenContainer.ParentContainer = childContainer;
                        ContainerMap[childContainer].Add(grandChildrenContainer);
                    }
                    else
                    {
                        grandChildrenContainer = new CSearchContainer(childContainer.Search, this);
                        CreateChildNode(childContainer, grandChildContainer, grandChildSearches);
                        ContainerMap[childContainer].Add(grandChildrenContainer);
                        grandChildrenContainer.ParentContainer = childContainer;
                    }
                }
            }
            */

            public void AddChild(CSearchContainer parent, CSearchContainer child)
            {
                ContainerMap[parent].Add(child);
                if (ContainerMap.Keys.Contains(child))
                    foreach (CSearchContainer grandChild in ContainerMap[child])
                        AddChild(child, grandChild);
            }



            /*
            private void AddChild(CSearch search)
            {
                CSearchContainer child = new CSearchContainer(search);
                if (!SearchToContainerMap.ContainsKey(search))
                    SearchToContainerMap[search] = new List<CSearchContainer>();
                SearchToContainerMap[search].Add(child);
                child.ParentContainer = this;
                child.ParentSearch = null;
                foreach (CSearchContainer sc in ContainerMap[sc])
                    child.AddChild(sc);
            }
            */
            public int AddChildToSearch(CSearch parent, CSearch child)
            {
                CSearchContainer cChild = new CSearchContainer(child, this);
                if (!SearchToContainerMap.ContainsKey(child))
                    SearchToContainerMap[child] = new List<CSearchContainer>();
                SearchToContainerMap[parent].Add(cChild);
                if (!SearchDefinitionMap.ContainsKey(parent))
                    SearchDefinitionMap[parent] = new List<CSearchContainer>();
                SearchDefinitionMap[parent].Add(cChild);
                if (!child.IsTest)
                {
                    if (!SearchDefinitionMap.ContainsKey(child))
                        SearchDefinitionMap[child] = new List<CSearchContainer>();
                    else if (SearchDefinitionMap[child].Count > 0)
                        foreach (CSearchContainer sc in SearchDefinitionMap[child])
                            AddChild(cChild, sc);
                }
                return cChild.ID;
            }

            private void RemoveChild(CSearchContainer parent, CSearchContainer child)
            {
                ContainerMap[parent].Remove(child);
                foreach (CSearchContainer sc in ContainerMap[child])
                    RemoveChild(child, sc);
            }

            public void RemoveChildFromSearch(CSearch parent, CSearch child)
            {
                foreach (CSearchContainer sc in SearchToContainerMap[parent])
                    foreach (CSearchContainer childSC in ContainerMap[sc])
                        if (childSC.Search == child)
                            RemoveChild(sc, childSC);
            }

            public bool IsChildOf(CSearch parent, CSearch child)
            {
                foreach (CSearchContainer sc in SearchToContainerMap[child])
                    if (SearchDefinitionMap[parent].Contains(sc))
                        return true;
                return false;
            }

            public List<CSearch> GetSimpleSearches()
            {
                List<CSearch> result = new List<CSearch>();
                foreach (CSearch s in SearchDefinitionMap.Keys)
                    if (s.IsTest)
                        result.Add(s);
                return result;
            }

            public List<CSearch> GetCompoundSearches()
            {
                List<CSearch> result = new List<CSearch>();
                foreach (CSearch s in SearchDefinitionMap.Keys)
                    if (!s.IsTest)
                        result.Add(s);
                return result;
            }

            public List<CSearch> GetParentSearches(CSearch search)
            {
                List<CSearch> result = new List<CSearch>();
                foreach (CSearchContainer sc in SearchToContainerMap[search])
                    foreach (CSearch s in SearchDefinitionMap.Keys)
                        if (SearchDefinitionMap[s].Contains(sc))
                            result.Add(s);
                return result;
            }

            private int MaxDepth(List<CSearch> searches)
            {
                int MaxDepth = 0;
                foreach (CSearch s in searches)
                    foreach (CSearchContainer sc in SearchToContainerMap[s])
                    {
                        int scDepth = Depth(sc);
                        if (scDepth > MaxDepth)
                            MaxDepth = scDepth;
                    }
                return MaxDepth;
            }

            public List<CSearch> GetAncestry(CSearch search)
            {
                List<CSearch> result = new List<CSearch>();
                List<CSearch> parentList = GetParentSearches(search);
                List<CSearch> tempList = null;
                result.AddRange(parentList);
                while (MaxDepth(parentList) > 0)
                {
                    foreach (CSearch s in parentList)
                        tempList.AddRange(GetParentSearches(s));
                    result.AddRange(tempList);
                    parentList.Clear();
                    parentList.AddRange(tempList);
                    tempList.Clear();
                }
                return result;
            }

            public CSearchContainer CreateTestNode(CSearch search)
            {
                CSearchContainer sc = new CSearchContainer(search, this);
                SearchToContainerMap[search] = new List<CSearchContainer>();
                return sc;
            }

            public CSearchContainer CreateCompoundNode(ETestConcatination concatMode)
            {
                CSearch search = new CSearch(this, concatMode);
                CSearchContainer sc = new CSearchContainer(search, this);
                SearchToContainerMap[search] = new List<CSearchContainer>();
                SearchDefinitionMap[search] = new List<CSearchContainer>();
                return sc;
            }

            public CSearch CreateCompoundSearch(ETestConcatination concatMode) 
            {
                CSearch s =  new CSearch(this, concatMode);
                SearchToContainerMap[s] = new List<CSearchContainer>();
                SearchDefinitionMap[s] = new List<CSearchContainer>();
                return s;
            }

            public void AddSearch(CSearch parent, List<CSearch> childNodes)
            {
                if (SearchDefinitionMap.ContainsKey(parent))
                    return;
                SearchDefinitionMap[parent] = new List<CSearchContainer>();
                for (int ctr = 0; ctr < parent.NumChildren; ctr++)
                    AddChildToSearch(parent, childNodes[ctr]);
            }

            public void AddChildrenToSearch(CSearch parent, List<CSearch> childNodes)
            {
                if (!SearchDefinitionMap.ContainsKey(parent))
                    return;
                for (int ctr = 0; ctr < childNodes.Count; ctr++)
                    AddChildToSearch(parent, childNodes[ctr]);
            }

            public void DeleteSearch(CSearch search)
            {
                foreach (CSearchContainer sc in SearchToContainerMap[search])
                    DeleteUp(sc);
                SearchToContainerMap.Remove(search);
                SearchDefinitionMap.Remove(search);
                search.Dispose();
            }


            private void DeleteUp(CSearchContainer container)
            {
                container.Dispose();
                List<CSearch> parents = new List<CSearch>();
                foreach (CSearchContainer con in ContainerMap.Keys)
                    if (ContainerMap[con].Contains(container))
                        if (!parents.Contains(con.Search))
                            parents.Add(con.Search);
                foreach (CSearch sch in SearchDefinitionMap.Keys)
                    if (SearchDefinitionMap[sch].Contains(container))
                        if (!parents.Contains(sch))
                            parents.Add(sch);
                foreach (CSearch sch in parents)
                    DeleteSearch(sch);
            }

            public bool HasParent(CSearch search)
            {
                foreach (CSearchContainer sc in SearchToContainerMap[search])
                    foreach (CSearch s in SearchDefinitionMap.Keys)
                        if (SearchDefinitionMap[s].Contains(sc))
                            return true;
                return false;
            }

            public void Initialize(List<CSearch> searches)
            {
                SearchIDList.Clear();
                ContainerIDList.Clear();
                ContainerMap.Clear();
                SearchToContainerMap.Clear();
                SearchDefinitionMap.Clear();
                foreach (CSearch s in searches)
                {
                    if (s.IsTest)
                        CreateTestNode(s);
                    else
                    {
                        for (int ctr = 0; ctr < s.ChildList_Load.Count; ctr++)
                            AddChildToSearch(s, LookupSearch(s.ChildList_Load[ctr]));
                    }
                    s.ChildList_Load.Clear();
                }
            }

            public List<CSearch> GetChildren(CSearch search)
            {
                List<CSearch> results = new List<CSearch>();
                foreach (CSearchContainer sc in SearchDefinitionMap[search])
                    results.Add(sc.Search);
                return results;
            }

            public bool RecurseContainer(CSearchContainer sc, String val, String actualVal)
            {
                foreach (CSearchContainer sc1 in ContainerMap[sc])
                {
                    if (sc.Search.IsTest)
                        return sc.Search.Test(val, actualVal);
                    else if (!sc.Search.IsTest)
                    {
                        switch (sc1.Search.ConcatMode)
                        {
                            case ETestConcatination.any:
                                foreach (CSearchContainer sc2 in ContainerMap[sc1])
                                {
                                    if (RecurseContainer(sc, val, actualVal))
                                        return true;
                                    break;
                                }
                                return false;

                            case ETestConcatination.all:
                                foreach (CSearchContainer sc2 in ContainerMap[sc1])
                                {
                                    if (!RecurseContainer(sc, val, actualVal))
                                        return false;
                                }
                                return true;

                            case ETestConcatination.none:
                                foreach (CSearchContainer sc2 in ContainerMap[sc1])
                                {
                                    if (RecurseContainer(sc, val, actualVal))
                                        return false;
                                }
                                return true;
                        }
                    }
                }
                return false;
            }


            
            public bool Test(String val, String actualVal)
            {
                CSearchContainer con = null;
                foreach (List<CSearchContainer> scList in SearchToContainerMap.Values)
                    foreach (CSearchContainer sc in scList)
                        if (sc.Search == Root)
                            con = SearchToContainerMap[Root][0];
                return RecurseContainer(con, val, actualVal);
            }
        }

        public class CSearchContainer
        {
            CSearchContainerCollection Collection = null;
            private int _ID;
            private CSearch _Search;

            public int ID
            {
                get
                {
                    return _ID;
                }
            }

            public CSearch Search
            {
                get
                {
                    return _Search;
                }
            }

            private int GetID()
            {
                if (Collection == null)
                    throw new InvalidOperationException();
                if (!Collection.ContainerIDList.Contains(Collection.ContainerIDList.Count))
                {
                    _ID = Collection.ContainerIDList.Count;
                    Collection.ContainerIDList.Add(_ID);
                }
                else
                {
                    for (int ctr = 0; ctr < Collection.ContainerIDList.Count; ctr++)
                    {
                        if (!Collection.ContainerIDList.Contains(ctr))
                        {
                            _ID = ctr;
                            Collection.ContainerIDList.Add(_ID);
                            break;
                        }
                    }
                }
                return _ID;
            }

            public CSearchContainer(CSearchContainer o, CSearchContainerCollection scc)
            {
                Collection = scc;
                _Search = new CSearch(o.Search, scc);
                _ID = GetID();
            }

            public CSearchContainer(CSearch search, CSearchContainerCollection scc)
            {
                Collection = scc;
                _Search = new CSearch(search, scc);
                _ID = GetID();
            }

            public CSearchContainer(CSearch search, int id, CSearchContainerCollection scc)
            {
                Collection = scc;
                _Search = search;
                _ID = id;
            }

            public void RedefineSearch(CSearch search, List<CSearch> children)
            {
                foreach (CSearchContainer sc in Collection.SearchDefinitionMap[search])
                    sc.Dispose();
                Collection.AddChildrenToSearch(search, children);
            }

            public void Dispose()
            {
                Collection.ContainerIDList.Remove(ID);
                if (Collection.ContainerMap[this] != null)
                {
                    foreach (CSearchContainer sc in Collection.ContainerMap[this])
                        sc.Dispose();
                    Collection.ContainerMap[this].Clear();
                    Collection.ContainerMap.Remove(this);
                }
            }
        }

        public class CSearch : IStoredInXml, INamedXmlSerializable
        {
            private MatchTest TestFunction = null;
            private String _SearchValue = String.Empty;
            private ETestConcatination _ConcatMode = ETestConcatination.unset;
            private CSearchFunctions.EFunction _Function;
            private String _Description = String.Empty;
            private bool bAssignedName = false;
            private bool _IsTest = false;
            private int _ID;
            public List<int> ChildList_Load = new List<int>();
            private CSearchContainerCollection Collection;
            private MatchTest TestEval = null;

            public ETestConcatination ConcatMode
            {
                get  
                {
                    return _ConcatMode;
                }
                set
                {
                    _ConcatMode = value;
                }
            }


            public int ID
            {
                get
                {
                    return _ID;
                }
            }

            public bool IsTest
            {
                get
                {
                    return _IsTest;
                }
            }

            private int GetID()
            {
                if (Collection == null)
                    throw new InvalidOperationException();
                int id = -1;
                if (!Collection.SearchIDList.Contains(Collection.SearchIDList.Count))
                {
                    id = Collection.SearchIDList.Count;
                    Collection.SearchIDList.Add(id);
                }
                else
                {
                    for (int ctr = 0; ctr < Collection.SearchIDList.Count; ctr++)
                        if (!Collection.SearchIDList.Contains(ctr))
                        {
                            id = ctr;
                            Collection.SearchIDList.Add(id);
                            break;
                        }
                }
                return id;
            }

            private CSearch(bool assignID)
            {
                if (assignID)
                {
                    _ID = GetID();
                    Collection.SearchDictionary[ID] = this;
                }
            }


            public void AddChild(CSearch child)
            {
                Collection.AddChildToSearch(this, child);
            }

            public void RemoveChild(CSearch child)
            {
                Collection.RemoveChildFromSearch(this, child);
            }
            
            public void MakeRoot(CSearch search)
            {
                Collection.Root = search;
            }

            public CSearch(CSearchContainerCollection collection, ETestConcatination concatMode, List<CSearch> childSearches)
            {
                Collection = collection;
                _ID = GetID();
                Collection.SearchDictionary[ID] = this;
                ConcatMode = concatMode;
                _IsTest = false;
                collection.AddSearch(this, childSearches);
            }


            public CSearch(CSearchContainerCollection collection, ETestConcatination concatMode)
            {
                Collection = collection;
                _ID = GetID();
                Collection.SearchDictionary[ID] = this;
                _IsTest = false;
                ConcatMode = concatMode;
                _IsTest = false;
                Collection.CreateCompoundNode(concatMode);
            }

            public CSearch(CSearchContainerCollection collection, CSearchFunctions.EFunction fun, String searchVal)
            {
                Collection = collection;
                _ID = GetID();
                Collection.SearchDictionary[ID] = this;
                _Function = fun;
                _IsTest = true;
                collection.CreateTestNode(this);
                _SearchValue = searchVal;
            }

            public static CSearch CreateFromXmlNode(XmlNode node, CSearchContainerCollection collection)
            {
                CSearch search = new CSearch(false);
                search.Collection = collection;
                search.LoadFromXml(node);
                return search;
            }


            public void Dispose()
            {
                Collection.SearchIDList.Remove(ID);
            }

            public String Description
            {
                get
                {
                    if (bAssignedName)
                        return _Description;
                    if (IsTest)
                        return String.Format(CSearchFunctions.FunctionDescriptions[Function], SearchValue);
                    else
                    {
                        String desc = String.Empty;
                        foreach (CSearchContainer sc in Collection.SearchDefinitionMap[this])
                        {
                            if (desc != String.Empty)
                                desc += ", ";
                            desc += sc.Search.Description;
                        }
                        return String.Format(CSearchFunctions.ConcatinationDescriptions[ConcatMode], desc);
                    }
                }
                set
                {
                    _Description = value;
                }
            }

            public String SearchValue
            {
                get
                {
                    return _SearchValue;
                }
            }

            public CSearchFunctions.EFunction Function
            {
                get
                {
                    return _Function;
                }
            }

            private CSearch()
            {
            }

            public CSearch(CSearchContainerCollection scc, XmlNode node)
            {
                Collection = scc;
                LoadFromXml(node);
            }

            public CSearch(CSearchContainerCollection scc, XmlReader reader)
            {
                Collection = scc;
                ReadXml(reader);
            }

            public CSearch(CSearch o, CSearchContainerCollection scc)
            {
                Collection = scc;
                _SearchValue = o._SearchValue;
                _Description = o._Description;
                _IsTest = o._IsTest;
                if (IsTest)
                    TestFunction = CSearchFunctions.CreateDelegate(o.Function);
                _ID = GetID();
                if (o.IsTest)
                    scc.CreateTestNode(this);
                else
                    scc.CreateCompoundNode(o.ConcatMode);
                scc.SearchDictionary[ID] = this;
                
            }

            public int NumChildren
            {
                get
                {
                    if (IsTest)
                        return 0;
                    else
                        return Collection.SearchDefinitionMap[this].Count;
                }
            }


            public bool Test(String testVal, String SearchValue)
            {
                bool bMatch = false;
                if (Collection.SearchDefinitionMap[this].Count == 0)
                    return TestEval(testVal, SearchValue);
                 else
                {
                    switch (ConcatMode)
                    {
                        case ETestConcatination.all:
                            bMatch = true;
                            foreach (CSearchContainer sc in Collection.SearchDefinitionMap[this])
                            {
                                if (!sc.Search.Test(testVal, SearchValue))
                                {
                                    bMatch = false;
                                    break;
                                }
                            }
                            return bMatch;

                        case ETestConcatination.any:
                            bMatch = false;
                            foreach (CSearchContainer sc in Collection.SearchDefinitionMap[this])
                                if (sc.Search.Test(testVal, SearchValue))
                                {
                                    bMatch = true;
                                    break;
                                }
                            return bMatch;

                        case ETestConcatination.none:
                            bMatch = true;
                            foreach (CSearchContainer sc in Collection.SearchDefinitionMap[this])
                                if (sc.Search.Test(testVal, SearchValue))
                                {
                                    bMatch = false;
                                    break;
                                }
                            return bMatch;

                        case ETestConcatination.notAll:
                            bMatch = false;
                            foreach (CSearchContainer sc in Collection.SearchDefinitionMap[this])
                                if (!sc.Search.Test(testVal, SearchValue))
                                {
                                    bMatch = true;
                                    break;
                                }
                            return bMatch;
                    }
                }
                return false;
            }

            public String GetName()
            {
                return "SimpleSearch";
            }

            public void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement("SimpleSearch");
                writer.WriteAttributeString("IsTest", IsTest.ToString());
                writer.WriteElementString("SearchID", ID.ToString());
                List<CSearchContainer> scList = Collection.SearchDefinitionMap[this];
                if (!IsTest)
                {
                    writer.WriteElementString("ConcatinationMode", ConcatMode.ToString());
                    if (_Description == String.Empty)
                        writer.WriteElementString("Description", " ");
                    else
                        writer.WriteElementString("Description", Description);
                    writer.WriteStartElement("Children");
                    writer.WriteAttributeString("NumChildren", scList.Count.ToString());
                    for (int ctr = 0; ctr < scList.Count; ctr++)
                        writer.WriteElementString("ChildID", scList[ctr].Search.ID.ToString());
                }
                else
                {
                    writer.WriteElementString("TestFunction", Function.ToString());
                    writer.WriteElementString("SearchValue", SearchValue);
                    if (_Description == String.Empty)
                        writer.WriteElementString("Description", " ");
                    else
                        writer.WriteElementString("Description", Description);
                }
                writer.WriteEndElement();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("IsTest", IsTest.ToString());
                writer.WriteElementString("SearchID", ID.ToString());
                if (!IsTest)
                {
                    List<CSearch> searches = Collection.GetChildren(this);
                    writer.WriteStartElement("ChildSearches");
                    writer.WriteAttributeString("NumChildren", searches.Count.ToString());
                    foreach (CSearch search in searches)
                        writer.WriteElementString("ChildID", search.ID.ToString());
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteElementString("TestFunction", Function.ToString());
                    writer.WriteElementString("SearhValue", SearchValue);
                }
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                _IsTest = Convert.ToBoolean(reader["IsTest"]);
                reader.ReadStartElement();
                _ID = Convert.ToInt32(reader.ReadElementString());
                String n = reader.ReadElementString();
                ChildList_Load = new List<int>();
                if (!IsTest)
                {
                    int nChildren = Convert.ToInt32(reader["NumChildren"]);
                    reader.ReadStartElement();
                    for (int ctr = 0; ctr < nChildren; ctr++)
                        ChildList_Load.Add(Convert.ToInt32(reader.ReadElementString()));
                    reader.ReadEndElement();
                }
                else
                    _Function = (CSearchFunctions.EFunction)Enum.Parse(typeof(CSearchFunctions.EFunction), reader.ReadElementString());
                reader.ReadEndElement();
            }

            public bool LoadFromXml(XmlNode node)
            {
                ChildList_Load = new List<int>();
                _IsTest = Convert.ToBoolean(node.Attributes[0].Value);
                _ID = Convert.ToInt32(node.ChildNodes[0].Value);
                Collection.SearchDictionary[ID] = this;
                String desc = node.ChildNodes[1].Value;
                if (desc == " ")
                    _Description = String.Empty;
                else
                    _Description = desc;
                if (!IsTest)
                {
                    ConcatMode = (ETestConcatination)Enum.Parse(typeof(ETestConcatination), node.ChildNodes[2].Value);
                    int nChildren = Convert.ToInt32(node.ChildNodes[3].Attributes[0].Value);
                    for (int ctr = 0; ctr < nChildren; ctr++)
                        ChildList_Load.Add(Convert.ToInt32(node.ChildNodes[3].ChildNodes[ctr].Value));
                }
                else
                {
                    _Function = (CSearchFunctions.EFunction)Enum.Parse(typeof(CSearchFunctions.EFunction), node.ChildNodes[3].Value);
                    TestEval = CSearchFunctions.CreateDelegate(Function);
                    _SearchValue = node.ChildNodes[4].Value;
                }
                Collection.SearchDictionary[ID] = this;
                return true;
            }
        }

        public class CSearchFunctions
        {
            public enum EFunction : int { None = 0, Equals, NotEquals, Contains, DoesNotContain, BeginsWith, MatchesRegex, IsBefore, IsAfter, LessThan, MoreThan };

            public static Dictionary<EFunction, String> FunctionDescriptions = new Dictionary<EFunction, String>();
            public static Dictionary<ETestConcatination, String> ConcatinationDescriptions = new Dictionary<ETestConcatination, String>();

            static CSearchFunctions()
            {
                FunctionDescriptions[EFunction.None] = String.Empty;
                FunctionDescriptions[EFunction.Equals] = "Equals \"{0}\"";
                FunctionDescriptions[EFunction.NotEquals] = "Does not equal \"{0}\"";
                FunctionDescriptions[EFunction.IsBefore] = "Falls before {0}";
                FunctionDescriptions[EFunction.IsAfter] = "Falls after {0}";
                FunctionDescriptions[EFunction.LessThan] = "Is less than {0}";
                FunctionDescriptions[EFunction.MoreThan] = "Is greater than {0}";
                FunctionDescriptions[EFunction.Contains] = "Contains \"{0}\"";
                FunctionDescriptions[EFunction.DoesNotContain] = "Does not contain \"{0}\"";
                FunctionDescriptions[EFunction.BeginsWith] = "Begins with \"{0}\"";
                FunctionDescriptions[EFunction.MatchesRegex] = "Contains a match for the regular expression \"{0}\"";
                ConcatinationDescriptions[ETestConcatination.all] = "All are true({0})";
                ConcatinationDescriptions[ETestConcatination.any] = "Any are true({0})";
                ConcatinationDescriptions[ETestConcatination.none] = "None are true({0})";
                ConcatinationDescriptions[ETestConcatination.notAll] = "At least one is false({0})";
            }

            public static MatchTest CreateDelegate(EFunction fun)
            {
                switch (fun)
                {
                    case EFunction.Equals:
                        return new MatchTest(Equals);

                    case EFunction.NotEquals:
                        return new MatchTest(NotEquals);

                    case EFunction.IsBefore:
                        return new MatchTest(IsBefore);

                    case EFunction.IsAfter:
                        return new MatchTest(IsAfter);

                    case EFunction.LessThan:
                        return new MatchTest(LessThan);

                    case EFunction.MoreThan:
                        return new MatchTest(MoreThan);

                    case EFunction.Contains:
                        return new MatchTest(Contains);

                    case EFunction.DoesNotContain:
                        return new MatchTest(DoesNotContain);

                    case EFunction.BeginsWith:
                        return new MatchTest(BeginsWith);

                    case EFunction.MatchesRegex:
                        return new MatchTest(MatchesRegex);

                    case EFunction.None:
                        throw new InvalidOperationException();
                }
                return null;
            }

            static bool Equals(String value, String testValue)
            {
                return value == testValue;
            }

            static bool NotEquals(String value, String testValue)
            {
                return value != testValue;
            }

            static bool IsBefore(String value, String testValue)
            {
                return (DateTime.Parse(value).Date.CompareTo(DateTime.Parse(testValue).Date) < 0);
            }

            static bool IsAfter(String value, String testValue)
            {
                return (DateTime.Parse(value).Date.CompareTo(DateTime.Parse(testValue).Date) > 0);
            }

            static bool LessThan(String value, String testValue)
            {
                return Convert.ToDecimal(value) < Convert.ToDecimal(testValue);
            }

            static bool MoreThan(String value, String testValue)
            {
                return Convert.ToDecimal(value) > Convert.ToDecimal(testValue);
            }

            static bool Contains(String value, String testValue)
            {
                return value.Contains(testValue);
            }

            static bool DoesNotContain(String value, String testValue)
            {
                return (!value.Contains(testValue));
            }

            static bool BeginsWith(String value, String testValue)
            {
                if (value.Length < testValue.Length)
                    return false;
                return (value.Substring(0, testValue.Length) == testValue);
            }

            static bool MatchesRegex(String value, String testValue)
            {
                Regex exp = new Regex(value);
                return exp.IsMatch(testValue);
            }
        }
    }
}
