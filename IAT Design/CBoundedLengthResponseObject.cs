using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CBoundedLengthResponseObject : CResponseObject
    {
        private Panel PreviewPanel = null;
        private String _Answer = String.Empty;
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
        private GroupBox SimpleGroup = null;
        private Font PreviewFont = null;
        private TextBox SimpleSearchBox = null;
        private Button SimpleSearchAddButton = null, SimpleSearchDeleteButton = null, SimpleSearchRenameButton = null;
        private Button CreateCompoundSearchButton = null, ModifyCompoundSearchButton = null, DeleteCompoundSearchButton = null;
        private RadioButton AllConcatRadio = null, AnyConcatRadio = null, NoneConcatRadio = null;
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

        public override bool IsSearchMatch(string val)
        {
            foreach (CResponseSpecifier rs in ResponseSpecifiers)
                if (rs.IsSearchMatch(val))
                    return true;
            return false;
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = true;
            UpdateResponseObject();
            PreviewPanel = new Panel();
            AnswerBox = new TextBox();
            Font PreviewFont = new Font(fontFamily, fontSize);
            AnswerBox.BackColor = backColor;
            AnswerBox.ForeColor = foreColor;
            AnswerBox.Font = PreviewFont;
            AnswerBox.Location = new Point((int)(.15 * clientWidth), 0);
            AnswerBox.Width = (int)(.2 * clientWidth);
            if (Type == EType.actual)
            {
                AnswerBox.ReadOnly = true;
                AnswerBox.BackColor = backColor;
            }
            PreviewPanel.Size = new Size(clientWidth, AnswerBox.Height);
            PreviewPanel.Controls.Add(AnswerBox);
            return PreviewPanel;
        }



        public override void DisposeOfControls()
        {
            if (AnswerBox != null)
            {
                AnswerBox.Dispose();
                AnswerBox = null;
            }
        }

    }
}