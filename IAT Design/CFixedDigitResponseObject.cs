using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.IO;
using IATClient.ResultData;

namespace IATClient
{
    class CFixedDigitResponseObject : CResponseObject
    {
        private List<CFixedSpecifier> CompressionSpecifiers = new List<CFixedSpecifier>();
        private TextBox ValueBox = null;
        private ListView ValueView = null;
        private CheckBox OneUseCheck = null;
        private GroupBox ImportGroup = null;
        private Button AddValueButton = null, DeleteButton = null, BrowseButton = null;
        private Label AddValueInstructions = null, BrowseLabel = null;
        private Func<int> GetNumDigs = null;
        private List<String> Values = new List<String>();

        public override void DisposeOfControls()
        {
            if ((Type == EType.actual) || (Type == EType.dummy))
            {
                if (ValueBox != null)
                {
                    ValueBox.Dispose();
                    ValueBox = null;
                }
            }
            if (ImportGroup != null)
            {
                foreach (Control c in ImportGroup.Controls)
                    c.Dispose();
                ImportGroup = null;
            }
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            UpdateResponseObject();
            Font font = new Font(fontFamily, fontSize);
            Panel PreviewPanel = new Panel();
            PreviewPanel.BackColor = backColor;
            PreviewPanel.ForeColor = foreColor;
            if ((Type == EType.dummy) || (Type == EType.actual))
            {
                if (ValueBox == null)
                    ValueBox = new TextBox();
                if (Type == EType.actual)
                {
                    ValueBox.ReadOnly = true;
                    ValueBox.Enabled = false;
                }
                ValueBox.BackColor = backColor;
                ValueBox.ForeColor = foreColor;
                ValueBox.Font = font;
                ValueBox.Width = 3 * clientWidth / 5;
                ValueBox.Location = new Point(0, 0);
                PreviewPanel.Controls.Add(ValueBox);
                PreviewPanel.Size = new Size(clientWidth, ValueBox.Bottom + 20);
                return PreviewPanel;
            }
            String[] correctButtonLabels = { "Add Value", "Delete Selected", "Browse" };
            String[] searchButtonLabels = { "Add Search Criteria", "Clear Selection", "Browse" };
            int ButtonWidth = 0;
            int nWidth = 0;
            if (Type == EType.correct)
            {
                for (int ctr = 0; ctr < correctButtonLabels.Length; ctr++)
                {
                    nWidth = TextRenderer.MeasureText(correctButtonLabels[ctr], font).Width + 8;
                    if (nWidth > ButtonWidth)
                        ButtonWidth = nWidth;
                }
            }
            else if (Type == EType.search)
            {
                for (int ctr = 0; ctr < searchButtonLabels.Length; ctr++)
                {
                    nWidth = TextRenderer.MeasureText(searchButtonLabels[ctr], font).Width + 8;
                    if (nWidth > ButtonWidth)
                        ButtonWidth = nWidth;
                }
            }
            int ButtonSpaceWidth = clientWidth;
            ImportGroup = new GroupBox();
            ImportGroup.Font = font;
            ImportGroup.BackColor = backColor;
            ImportGroup.ForeColor = foreColor;
            ImportGroup.Location = new Point(ElementPadding.Left, 0);
            BrowseLabel = new Label();
            BrowseLabel.Font = font;
            BrowseLabel.BackColor = backColor;
            BrowseLabel.ForeColor = foreColor;
            BrowseLabel.Text = "Browse for file to import values from:";
            BrowseLabel.Size = TextRenderer.MeasureText(BrowseLabel.Text, font);
            ButtonSpaceWidth -= BrowseLabel.Width;
            ValueView = new ListView();
            ValueView.Dock = DockStyle.Top;
            ValueView.Height = (int)(font.Height / .975) * 10;
            ValueView.CheckBoxes = true;
            ValueView.View = View.List;
            ValueView.Scrollable = true;
            ValueView.LabelEdit = false;
            ValueView.MultiSelect = true;
            ImportGroup.Controls.Add(ValueView);
            ValueBox = new TextBox();
            ValueBox.Font = font;
            ValueBox.BackColor = backColor;
            ValueBox.ForeColor = foreColor;
            ImportGroup.Controls.Add(ValueBox);
            ValueBox.Width = TextRenderer.MeasureText("88888888888", font).Width + 8;
            int ctrlVertCenter = (ValueBox.Height >> 1) + ValueView.Bottom + 8;
            ValueBox.Location = new Point(10, ctrlVertCenter - (ValueBox.Height >> 1));
            ValueBox.Text = String.Empty;
            ButtonSpaceWidth -= ValueBox.Width - ValueBox.Left;
            int ButtonCenterOffset = ValueBox.Right + (ButtonSpaceWidth / 6);
            AddValueButton = new Button();
            AddValueButton.Font = font;
            AddValueButton.BackColor = backColor;
            AddValueButton.ForeColor = foreColor;
            AddValueButton.Width = ButtonWidth;
            ImportGroup.Controls.Add(AddValueButton);
            AddValueButton.Location = new Point(ButtonCenterOffset - (ButtonWidth >> 1), ctrlVertCenter - (AddValueButton.Height >> 1));
            ButtonCenterOffset += (ButtonSpaceWidth / 3);
            if (Type == EType.correct)
            {
                AddValueButton.Click += new EventHandler(AddValueButton_Click);
                AddValueButton.Text = "Add Value";
            }
            else if (Type == EType.search)
            {
                AddValueButton.Text = "Add Search Criteria";
                AddValueButton.Click += new EventHandler(AddSearchValueButton_Click);
            }
            AddValueButton.Width = ButtonWidth;
            if (Type == EType.search)
            {
                AddValueInstructions = new Label();
                AddValueInstructions.Text = "You may either enter a literal value (12345) or use the question mark as a wildcard (12???).";
                AddValueInstructions.Size = TextRenderer.MeasureText(AddValueInstructions.Text, font);
                AddValueInstructions.Location = new Point(20, ValueBox.Bottom + 10);
                AddValueInstructions.Font = font;
                ImportGroup.Controls.Add(AddValueInstructions);
            }
            DeleteButton = new Button();
            DeleteButton.Font = font;
            DeleteButton.BackColor = backColor;
            DeleteButton.ForeColor = foreColor;
            DeleteButton.Width = ButtonWidth;
            DeleteButton.Location = new Point(ButtonCenterOffset - (ButtonWidth >> 1), ctrlVertCenter - (DeleteButton.Height >> 1));
            ButtonCenterOffset += (ButtonSpaceWidth / 3);
            if (Type == EType.correct)
            {
                DeleteButton.Text = "Delete Selected";
                DeleteButton.Click += new EventHandler(DeleteButton_Click);
            }
            else if (Type == EType.search)
            {
                DeleteButton.Text = "Select All";
                DeleteButton.Click += new EventHandler(SelectAllButton_Click);
            }
            ImportGroup.Controls.Add(DeleteButton);
            BrowseLabel.Location = new Point(ButtonCenterOffset - (ButtonWidth >> 1), ctrlVertCenter - (BrowseLabel.Height >> 1));
            ImportGroup.Controls.Add(BrowseLabel);
            BrowseButton = new Button();
            BrowseButton.Font = font;
            BrowseButton.BackColor = backColor;
            BrowseButton.ForeColor = foreColor;
            ImportGroup.Controls.Add(BrowseButton);
            BrowseButton.Width = ButtonWidth;
            BrowseButton.Location = new Point(BrowseLabel.Right + (ButtonWidth >> 1), ctrlVertCenter - (BrowseButton.Height >> 1));
            if (Type == EType.correct)
                BrowseButton.Click += new EventHandler(BrowseButton_Click);
            else if (Type == EType.search)
                BrowseButton.Click += new EventHandler(BrowseButton_Click);
            BrowseButton.Text = "Browse";
            if (Type == EType.correct)
            {
                OneUseCheck = new CheckBox();
                OneUseCheck.Font = font;
                OneUseCheck.BackColor = backColor;
                OneUseCheck.ForeColor = foreColor;
                ImportGroup.Controls.Add(OneUseCheck);
                OneUseCheck.Text = "Allow each value to be used only once, for example to uniquely identify the test taker";
                OneUseCheck.Width = TextRenderer.MeasureText(OneUseCheck.Text, font).Width + 25;
                OneUseCheck.Location = new Point((clientWidth - OneUseCheck.Width) >> 1, BrowseButton.Bottom + 10);
                OneUseCheck.Click += new EventHandler(OneUseCheck_Click);
            }
            ImportGroup.Text = "Valid Responses";
            ImportGroup.Size = new Size(clientWidth - ElementPadding.Horizontal, AddValueInstructions.Bottom + 20);
            PreviewPanel.Controls.Add(ImportGroup);
            return PreviewPanel;
        }

        public int NumDigs
        {
            get
            {
                return GetNumDigs();
            }
        }

        void OneUseCheck_Click(object sender, EventArgs e)
        {
            IsOneUse = OneUseCheck.Checked;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            foreach (int n in ValueView.CheckedIndices)
            {
                if ((Type == EType.search) || (Type == EType.correct))
                    ValueView.Items.RemoveAt(n);
            }
        }

        private List<String> ExtrapolateValues(String values)
        {
            List<String> strList = new List<String>();
            String str = values;
            while (str != String.Empty)
            {
                int commaNdx = str.IndexOf(",");
                int hyphenNdx = str.IndexOf("-");
                if (hyphenNdx >= commaNdx)
                    hyphenNdx = -1;
                if (hyphenNdx == -1)
                {
                    if (commaNdx > -1)
                        strList.Add(str.Substring(0, commaNdx).Trim());
                    else
                        strList.Add(str.Trim());
                }
                else 
                {
                    strList.Add(str.Substring(0, str.IndexOf("-")).Trim());
                    strList.Add(str.Substring(str.IndexOf("-") + 1).Trim());
                }
                if (commaNdx == -1)
                    str = String.Empty;
                else
                    str = str.Substring(commaNdx + 1);
            }
            return strList;
        }

        private void AddValueButton_Click(object sender, EventArgs e)
        {
            bool bValid = true;
            String val = ValueBox.Text.Trim();
            for (int ctr = 0; ctr < val.Length; ctr++)
                if (!Char.IsDigit(val[ctr]))
                {
                    bValid = false;
                    break;
                }
            if (bValid)
                if (val.Length != NumDigs)
                    bValid = false;
            if (!bValid)
            {
                MessageBox.Show(String.Format("You must enter a numerical value of exactly {0} digits.", NumDigs), "Invalid Input");
                return;
            }
            for (int ctr = 0; ctr < ValueView.Items.Count; ctr++)
                if (ValueView.Items[ctr].Text == val)
                {
                    MessageBox.Show(String.Format("The defined list of acceptable answers for this item already contains {0}.", val), "Duplicate Entry");
                    return;
                }

            ListViewItem lvi = new ListViewItem(ValueBox.Text);
            if ((Type == EType.correct) || (Type == EType.correct))
                Values.Add(ValueBox.Text);
            ValueView.Items.Add(lvi);
        }

        private void AddSearchValueButton_Click(object sender, EventArgs e)
        {
        }

        private void SelectAllButton_Click(object sender, object o)
        {
            for (int ctr = 0; ctr < ValueView.Items.Count; ctr++)
                ValueView.Items[ctr].Selected = true;
        }


        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                int nValuesImported = 0;
                try
                {
                    ValueView.SuspendLayout();
                    Regex exp = new Regex("([^0-9]*[0-9]{" + NumDigs.ToString() + "}|^[0-9]{" + NumDigs.ToString() + "})[^0-9]*");
                    FileStream inStream = File.Open(dlg.FileName, FileMode.Open);
                    TextReader reader = new StreamReader(inStream, true);
                    String str = String.Empty;
                    while ((str = reader.ReadLine()) != null)
                    {
                        MatchCollection matches = exp.Matches(str);
                        foreach (Match m in matches)
                        {
                            if (!ValueView.Items.ContainsKey(m.Value))
                            {
                                ValueView.Items.Add(m.Value);
                                if ((Type == EType.correct) || (Type == EType.search))
                                    Values.Add(m.Value);
                                nValuesImported++;
                            }
                        }
                    }
                    if (nValuesImported == 0)
                    {
                        MessageBox.Show(String.Format("No numerical values of {0} digits were located within the file.", NumDigs), "No Values Imported");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("The file you specified could not be processed as a text file.", "Unrecognized Text Encoding");
                }
                finally
                {
                    ValueView.Sort();
                    ValueView.ResumeLayout(false);
                }
            }
        }

        private List<String> ValueList = new List<String>();
        private bool _OneUse = false;

        public bool IsOneUse
        {
            get
            {
                return _OneUse;
            }
            set
            {
                _OneUse = value;
            }
        }

        public CFixedDigitResponseObject(EType type, ResultSetDescriptor rsb)
            : base(type, rsb) { }

        public CFixedDigitResponseObject(EType type, Response resp) : base(type, resp)
        {
            FixedDigit theResp = (FixedDigit)resp;
            GetNumDigs = new Func<int>(theResp.GetNumDigits);
        }

        public CFixedDigitResponseObject(CFixedDigitResponseObject obj, Response resp)
            : base(obj.Type, resp)
        {
            GetNumDigs = new Func<int>(((FixedDigit)resp).GetNumDigits);
            CompressionSpecifiers.AddRange(obj.CompressionSpecifiers);
            Values.AddRange(obj.Values);
        }

        public CFixedDigitResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            GetNumDigs = new Func<int>(((CFixedDigResponse)csi.Response).GetNumDigs);
        }

        public void AddValue(String val)
        {
            ValueList.Add(val);
        }

        public void AddRange(IEnumerable<String> vals)
        {
            foreach (String s in vals)
                ValueList.Add(s);
        }

        public void Remove(String val)
        {
            ValueList.Remove(val);
        }

        public void Remove(IEnumerable<String> vals)
        {
            foreach (String s in vals)
                ValueList.Remove(s);
        }

        public bool Contains(String val)
        {
            return ValueList.Contains(val);
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
                    ValueBox.Text = value.Value;
                    Values.Clear();
                    Values.Add(value.Value);
                }
                else
                    ValueBox.Text = String.Empty;
            }
        }

        public int NumValues
        {
            get
            {
                return ValueList.Count;
            }
        }

        public String this[int n]
        {
            get
            {
                return ValueList[n];
            }
        }

        public override bool IsSearchMatch(string val)
        {
            if (Type == EType.search)
                return ValueList.Contains(val);
            throw new InvalidOperationException();
        }

        protected override List<CResponseObject.CResponseSpecifier> ResponseSpecifiers
        {
            get { throw new NotImplementedException(); }
        }

        abstract class CFixedSpecifier : IStoredInXml, INamedXmlSerializable        
        {
            public enum EType { singleton, range, wildcard };
            protected int _NumDigs;
            private EType _Type;

            public int NumDigs
            {
                get
                {
                    return _NumDigs;
                }
                set
                {
                    _NumDigs = value;
                }
            }

            public EType Type
            {
                get
                {
                    return _Type;
                }
                set
                {
                    _Type = value;
                }
            }


            public CFixedSpecifier(int numDigs, EType type)
            {
                _NumDigs = numDigs;
            }

            static public CFixedSpecifier CreateFromXml(XmlReader reader, int numDigs)
            {
                EType type = (EType)Enum.Parse(typeof(EType), reader["Type"]);
                CFixedSpecifier spec = null;
                switch (type)
                {
                    case EType.singleton:
                        spec = new CFixedSingleton(numDigs);
                        break;

                    case EType.range:
                        spec = new CFixedRange(numDigs);
                        break;

                    case EType.wildcard:
                        spec = new CFixedWildcard(numDigs);
                        break;
                }
                spec.ReadXml(reader);
                return spec;
            }

            static public CFixedSpecifier CreateFromXml(XmlNode node, int numDigs)
            {
                EType type = (EType)Enum.Parse(typeof(EType), node.Attributes["Type"].Value);
                CFixedSpecifier spec = null;
                switch (type)
                {
                    case EType.singleton:
                        spec = new CFixedSingleton(numDigs);
                        break;

                    case EType.range:
                        spec = new CFixedRange(numDigs);
                        break;

                    case EType.wildcard:
                        spec = new CFixedWildcard(numDigs);
                        break;
                }
                spec.LoadFromXml(node);
                return spec;
            }


            private static ulong fact(ulong n)
            {
                if (n == 1)
                    return 1;
                return n * fact(n - 1);
            }

            private static bool []GetWildSlots(int nDigits, int nCards, int nPermutation)
            {
                bool []wildAry = new bool[nDigits];
                System.Array.Clear(wildAry, 0, nDigits);
                for (int ctr = 0; ctr < nCards; ctr++)
                    wildAry[ctr] = true;
                int []posAry = new int[nCards];
                for (int ctr = 0; ctr < nCards; ctr++)
                    posAry[ctr] = ctr;
                int movingBit = nCards - 1;
                int permNum = 0;
                bool bMovedBit = false;
                while (permNum < nPermutation)
                {
                    if (movingBit == nCards - 1)
                    {
                        if (posAry[movingBit] == nDigits - 1)
                        {
                            int ctr = 0;
                            while (posAry[movingBit - ctr] == nDigits - 1 - ctr)
                                ctr++;
                            movingBit = movingBit - ctr;
                            posAry[movingBit]++;
                        }
                        else
                            posAry[movingBit]++;
                    } else
                    {
                        bMovedBit = false;
                        if (posAry[movingBit] == 1 + posAry[movingBit + 1])
                        {
                            int ctr = 1;
                            while (ctr + movingBit < nCards)
                            {
                                if (posAry[movingBit + ctr] != posAry[movingBit] + ctr)
                                    break;
                                else
                                    ctr++;
                            }
                            if (ctr + movingBit != nCards)
                            {
                                movingBit = ctr + movingBit;
                                if ((movingBit != posAry.Length - 1) || (posAry[movingBit] != nCards - 1))
                                {
                                    posAry[movingBit]++;
                                    bMovedBit = true;
                                }
                            }
                        }
                        else
                        {
                            posAry[movingBit]++;
                            bMovedBit = true;
                        }
                    }
                        if (!bMovedBit)
                        {
                            int ctr = 1;
                            while (posAry[movingBit - ctr] == posAry[movingBit - ctr - 1] + 1)
                                ctr++;
                            movingBit -= ctr;
                            posAry[movingBit]++;
                            for (int ctr1 = movingBit + 1; ctr1 < nCards - 1; ctr1++)
                                posAry[ctr1] = posAry[movingBit] + ctr - movingBit;
                        }
                    permNum++;
                }
                return wildAry;
            }
            
            private static String GetDigits(String val, int[] digs)
            {
                String result = String.Empty;
                for (int ctr = 0; ctr < digs.Length; ctr++)
                    result += val[digs[ctr]];
                return result;
            }

            private static int[] GetWildIndexes(bool[] bWildSlots, int nStart, int nLen)
            {
                int nBitsRead = 0;
                int ctr = 0;
                List<int> wildIndicies = new List<int>();
                 while (nBitsRead < nStart)
                {
                    if (bWildSlots[ctr])
                        nBitsRead++;
                    ctr++;
                }
                for (int ctr2 = ctr; ctr2 < ctr + nLen; ctr2++)
                {
                    nBitsRead = 0;
                    while (!bWildSlots[ctr2])
                        nBitsRead++;
                    wildIndicies.Add(ctr2);
                }
                return wildIndicies.ToArray();
            }


            private static int GetZeroValSet(List<String> values, List<int>[] foundVals, int []wildIndexes, int[] digitIndexes)
            {
                int valSet = 0;
                String val = "-1";
                while (valSet < foundVals.Length)
                {
                    while ((valSet < foundVals.Length) && (foundVals[valSet].Count == 0))
                        valSet++;
                    if (valSet >= foundVals.Length)
                        break;
                    val = GetDigits(values[foundVals[valSet][0]], wildIndexes);
                    if (Convert.ToInt32(val) == 0)
                        break;
                    valSet++;
                }
                if (Convert.ToInt32(val) != 0)
                    return -1;
                return valSet;
            }


            private static List<CFixedWildcard> FindWildcards(List<String> values)
            {
                int nDigs = values[0].Length;
                char[] wildstring = new char[nDigs];
                int[] wildIndexes;
                for (int ctr = 0; ctr < wildstring.Length; ctr++)
                    wildstring[ctr] = '#';

                List<CFixedWildcard> resultList = new List<CFixedWildcard>();
                List<List<String>> WildSpecifiers = new List<List<String>>();
                List<List<bool[]>> WildSlotsList = new List<List<bool[]>>();
                int valNdx;
                bool bDone;
                for (int ctr1 = 1; ctr1 <= nDigs; ctr1++)
                {
                    WildSlotsList.Add(new List<bool[]>());
                    int upperBound = (int)(fact((ulong)nDigs) / (fact((ulong)ctr1) * fact((ulong)(nDigs - ctr1))));
                    for (int ctr2 = 0; ctr2 < upperBound; ctr2++)
                        WildSlotsList[ctr2].Add(GetWildSlots(nDigs, ctr1, ctr2));
                }

                for (int ctr1 = 0; ctr1 < WildSlotsList.Count; ctr1++)
                {
                    List<int>[] FoundVals, LargeFoundVals;
                    int nFound = 0;
                    int ctr2, ctr3, ctr4, ctr5, ctr6, ctr7;
                    for (int ctr15 = 0; ctr15 < WildSlotsList[ctr1].Count; ctr15++)
                    {
                        WildSpecifiers[ctr1] = new List<String>();
                        ctr2 = 0;
                        Dictionary<int, List<String>> WildcardCandidates = new Dictionary<int, List<String>>();
                        bDone = false;

                        if (ctr1 <= 6)
                            wildIndexes = GetWildIndexes(WildSlotsList[ctr1][ctr15], 0, ctr1);
                        else
                            wildIndexes = GetWildIndexes(WildSlotsList[ctr1][ctr15], 0, 6);
                        for (int ctr25 = 0; ctr25 < 6; ctr25++)
                        {
                            int nBitsRead = 0;
                            ctr3 = 0;
                            while (nBitsRead < ctr25 + 1)
                            {
                                if (WildSlotsList[ctr1][ctr15][ctr3])
                                    nBitsRead++;
                                ctr3++;
                            }
                            wildIndexes[ctr25] = ctr3;
                        }

                        FoundVals = new List<int>[(int)Math.Pow(10, wildIndexes.Length)];
                        System.Array.Clear(FoundVals, 0, (int)Math.Pow(10, wildIndexes.Length));
                        nFound = 0;
                        for (ctr3 = 0; ctr3 < values.Count; ctr3++)
                        {
                            String strVal = String.Empty;
                            for (ctr4 = 0; ctr4 < wildIndexes.Length; ctr4++)
                                strVal += values[ctr3][wildIndexes[ctr4]];
                            int nVal = Convert.ToInt32(strVal);
                            if (FoundVals[nVal] == null)
                            {
                                nFound++;
                                FoundVals[nVal] = new List<int>();
                            }
                            FoundVals[nVal].Add(ctr3);
                        }
                        if (nFound != Math.Pow(10, wildIndexes.Length))
                            continue;

                        if (ctr1 > 6)
                        {
                            wildIndexes = GetWildIndexes(WildSlotsList[ctr1][ctr15], 6, ctr1 - 6);
                            LargeFoundVals = new List<int>[(int)Math.Pow(10, wildIndexes.Length)];
                            System.Array.Clear(LargeFoundVals, 0, (int)Math.Pow(10, wildIndexes.Length));
                            for (ctr3 = 0; ctr3 < Math.Pow(10, wildIndexes.Length); ctr3++)
                            {
                                bool bFound = false;
                                for (ctr4 = 0; ctr4 < values.Count; ctr4++)
                                {
                                    String strVal = GetDigits(values[ctr4], wildIndexes);
                                    if (Convert.ToUInt64(strVal) == (ulong)ctr3)
                                    {
                                        if (LargeFoundVals[ctr3] == null)
                                            LargeFoundVals[ctr3] = new List<int>();
                                        LargeFoundVals[ctr3].Add(ctr4);
                                        bFound = true;
                                        break;
                                    }
                                }
                                if (!bFound)
                                {
                                    break;
                                }
                            }

                            for (ctr5 = 0; ctr5 < FoundVals.Length; ctr5++)
                            {
                                for (ctr3 = 0; ctr3 < LargeFoundVals.Length; ctr3++)
                                {
                                    ctr4 = 0;
                                    ctr7 = 0;
                                    ctr6 = 0;
                                    bool[] ExistsInBoth = new bool[FoundVals[ctr2].Count];
                                    System.Array.Clear(ExistsInBoth, 0, ExistsInBoth.Length);
                                    while (ctr4 < FoundVals[ctr2].Count)
                                    {
                                        if (LargeFoundVals[ctr3].Contains(FoundVals[ctr2][ctr4]))
                                        {
                                            ExistsInBoth[ctr4] = true;
                                            if (LargeFoundVals[ctr3].Count > 1)
                                            {
                                                int largeValNdx = LargeFoundVals[ctr3].IndexOf(FoundVals[ctr2][ctr4]);
                                                ctr6 = 0;
                                                while (ctr5 < LargeFoundVals[ctr3].Count)
                                                {
                                                    if (!FoundVals[ctr2].Contains(LargeFoundVals[ctr3][ctr7]))
                                                        LargeFoundVals[ctr3].RemoveAt(ctr7);
                                                    else
                                                        ctr6++;
                                                }
                                            }
                                        }
                                        ctr4++;
                                    }

                                    ctr4 = 0;
                                    ctr5 = 0;
                                    while (ctr4 < FoundVals[ctr2].Count)
                                    {
                                        if (!ExistsInBoth[ctr5])
                                            FoundVals[ctr2].RemoveAt(ctr4);
                                        else
                                            ctr4++;
                                        ctr5++;
                                    }


                                    ExistsInBoth = new bool[LargeFoundVals[ctr3].Count];
                                    ctr4 = 0;
                                    while (ctr4 < LargeFoundVals[ctr2].Count)
                                    {
                                        if (FoundVals[ctr2].Contains(LargeFoundVals[ctr3][ctr4]))
                                        {
                                            if (FoundVals[ctr2].Count > 1)
                                            {
                                                valNdx = FoundVals[ctr2].IndexOf(LargeFoundVals[ctr3][ctr4]);
                                                ctr5 = 0;
                                                while (ctr5 < FoundVals[ctr2].Count)
                                                {
                                                    if (!LargeFoundVals[ctr3].Contains(FoundVals[ctr2][ctr4]))
                                                        FoundVals[ctr3].RemoveAt(ctr5);
                                                    else
                                                        ctr5++;
                                                }
                                            }
                                        }
                                        ctr4++;
                                    }


                                    ctr4 = 0;
                                    ctr5 = 0;
                                    while (ctr4 < LargeFoundVals[ctr2].Count)
                                    {
                                        if (!ExistsInBoth[ctr5])
                                            LargeFoundVals[ctr3].RemoveAt(ctr4);
                                        else
                                            ctr4++;
                                        ctr5++;
                                    }
                                }
                            }
                        }
                        wildIndexes = GetWildIndexes(WildSlotsList[ctr1][ctr15], 0, WildSlotsList[ctr1][ctr15].Length);

                        ctr2 = 0;
                        ctr3 = 0;
                        ctr4 = 0;
                        String RowVal = String.Empty;
                        uint valSet = 0;
                        int zeroValSet = 0;
                        uint valRow = 0;
                        valNdx = 0;
                        bool bFoundMatch;
                        bDone = false;
                        bool bFoundWildcard;
                        uint[] MarkedRows = new uint[(int)Math.Pow(10, wildIndexes.Length)];
                        uint[] MarkedCols = new uint[(int)Math.Pow(10, wildIndexes.Length)];
                        String matchValue;
                        int[] digitIndexes = new int[nDigs - wildIndexes.Length];
                        while (ctr2 + ctr3 < digitIndexes.Length)
                        {
                            if (ctr2 == wildIndexes[ctr3])
                                ctr3++;
                            else
                            {
                                digitIndexes[ctr2] = ctr2 + ctr3;
                                ctr2++;
                            }
                        }
                        while ((zeroValSet = GetZeroValSet(values, FoundVals, wildIndexes, digitIndexes)) != -1)
                        {
                            matchValue = GetDigits(values[FoundVals[zeroValSet][0]], digitIndexes);
                            valSet = 1;
                            valNdx = 0;
                            bFoundWildcard = true;
                            valRow = 0;
                            while (valSet < FoundVals.Length)
                            {
                                bDone = false;
                                while (bDone)
                                {
                                    while ((valRow < FoundVals.Length) && (FoundVals[valRow].Count == 0))
                                        valRow++;
                                    if (Convert.ToUInt32(GetDigits(String.Format("{0:D" + nDigs.ToString() + "}", FoundVals[valRow][valNdx]), wildIndexes)) == valSet)
                                        bDone = true;
                                }
                                bFoundMatch = true;
                                while ((valRow = Convert.ToUInt32(GetDigits(String.Format("{0:D" + nDigs.ToString() + "}", FoundVals[valRow][valNdx]), digitIndexes))) != Convert.ToInt32(matchValue))
                                {
                                    valNdx++;
                                    if (valNdx > FoundVals[valRow].Count)
                                    {
                                        bDone = false;
                                        while (!bDone)
                                        {
                                            while ((valRow < FoundVals.Length) && (FoundVals[valRow].Count == 0))
                                                valRow++;
                                            if (Convert.ToInt32(GetDigits(values[FoundVals[valRow][0]], wildIndexes)) == valSet)
                                                bDone = true;
                                        }
                                        if (valRow == FoundVals.Length)
                                        {
                                            bFoundMatch = false;
                                            break;
                                        }
                                    }
                                    valRow = Convert.ToUInt32(GetDigits(String.Format("{0:D" + nDigs.ToString() + "}", FoundVals[valRow][valNdx]), digitIndexes));
                                    if (Convert.ToUInt32(matchValue) == valRow)
                                    {
                                        MarkedRows[valRow] = valRow;
                                        MarkedCols[valRow] = (uint)valNdx;
                                        valSet++;
                                    }
                                    else
                                    {
                                        valNdx++;
                                    }
                                }
                                if (!bFoundMatch)
                                {
                                    bFoundWildcard = false;
                                    break;
                                }
                            }
                            if (!bFoundWildcard)
                                FoundVals[zeroValSet].RemoveAt(0);
                            else
                            {
                                String resultWild = values[FoundVals[0][0]];
                                char[] resultWildAry = resultWild.ToCharArray();
                                for (int ctr = 0; ctr < wildIndexes.Length; ctr++)
                                    resultWildAry[wildIndexes[ctr]] = '?';
                                resultWild = new String(resultWildAry);
                                resultList.Add(new CFixedWildcard(nDigs, resultWild));
                                FoundVals[zeroValSet].RemoveAt(0);
                                for (int n = 1; n < Math.Pow(10, wildIndexes.Length); n++)
                                    FoundVals[MarkedRows[n]].Remove((int)MarkedCols[n]);
                            }
                        }
                    }
                }
                return resultList;
            }

            public static List<CFixedRange> FindRanges(List<String> values)
            {
                ulong lowVal = ulong.MaxValue;
                ulong highVal = ulong.MaxValue;
                ulong rangeLen = ulong.MaxValue;
                ulong val = ulong.MaxValue;
                int nDigs = values[0].Length;
                int ctr = 0;
                List<CFixedRange> results = new List<CFixedRange>();

                while (ctr < values.Count)
                {
                    if (lowVal == ulong.MaxValue)
                    {
                        lowVal = Convert.ToUInt64(values[ctr++]);
                        rangeLen = 1;
                    }
                    else
                    {
                        val = Convert.ToUInt64(values[ctr++]);
                        if (val != lowVal + rangeLen)
                        {
                            if (rangeLen < 3)
                            {
                                lowVal = ulong.MaxValue;
                                continue;
                            }
                            else
                            {
                                highVal = val;
                                CFixedRange fRange = new CFixedRange(values[0].Length, String.Format("{0:D" + nDigs.ToString() + "}", lowVal), String.Format("{0:D" + nDigs.ToString() + "}", highVal));
                                results.Add(fRange);
                                lowVal = ulong.MaxValue;
                            }
                        }
                    }
                }
                return results;
             }

            public static List<CFixedSpecifier> GenerateCompressionSpecifiers(List<String> values, List<CFixedRange> ranges, List<CFixedWildcard> wildcards)
            {
                List<List<CFixedRange>> Ranges = new List<List<CFixedRange>>();
                List<List<CFixedWildcard>> Wildcards = new List<List<CFixedWildcard>>(); 
                List<CFixedSpecifier> results = new List<CFixedSpecifier>();


                ulong rMin, rMax, wildMin, wildMax;
                CFixedRange range;
                CFixedWildcard wild;
                bool bFound = false;

                for (int ctr1 = 0; ctr1 < ranges.Count; ctr1++)
                {
                    Ranges.Add(new List<CFixedRange>());
                    range = ranges[ctr1];
                    wild = wildcards[ctr1];
                    for (int ctr2 = 0; ctr2 < wildcards.Count; ctr2++)
                    {
                        if (Wildcards.Count < ctr2)
                            Wildcards.Add(new List<CFixedWildcard>());
                        rMin = Convert.ToUInt64(range.MinValue);
                        rMax = Convert.ToUInt64(range.MaxValue);
                        wildMin = Convert.ToUInt64(wild.Value.Replace("?", "0"));
                        wildMax = Convert.ToUInt64(wild.Value.Replace("?", "9"));
                        if (((rMax >= wildMin) && (rMax <= wildMax)) || ((rMin >= wildMin) && (rMin <= wildMax)))
                        {
                            bFound = false;
                            int ctr3 = 0;
                            while ((!bFound) && (ctr3 < Wildcards.Count))
                            {
                                if (Wildcards[ctr3].Contains(wild))
                                    bFound = true;
                                else
                                    ctr3++;
                            }
                            if (bFound)
                                Ranges[Wildcards[ctr3].IndexOf(wild)].Add(range);
                            else
                            {
                                Wildcards[ctr1].Add(wild);
                                Ranges[ctr1].Add(range);
                            }
                        }
                    }
                }

                ulong nRangeWeight;
                ulong nWildWeight;

                for (int ctr1 = 0; ctr1 < ranges.Count; ctr1++)
                {
                    nRangeWeight = 0;
                    foreach (CFixedRange r in Ranges[ctr1])
                        nRangeWeight += (Convert.ToUInt64(r.MaxValue) - Convert.ToUInt64(r.MinValue) + 1) / 2UL;
                    nWildWeight = 0;
                    foreach (CFixedWildcard w in Wildcards[ctr1])
                        nWildWeight += (ulong)Math.Pow(10, w.WildChars);
                    if (nRangeWeight > nWildWeight)
                    {
                        foreach (CFixedRange r in Ranges[ctr1])
                            results.Add(r);
                    }
                    else
                    {
                        foreach (CFixedWildcard w in Wildcards[ctr1])
                            results.Add(w);
                    }
                }


                foreach (CFixedRange r in ranges)
                {
                    bFound = false;
                    foreach (List<CFixedRange> l in Ranges)
                        if (l.Contains(r))
                        {
                            bFound = true;
                            break;
                        }
                    if (!bFound)
                        results.Add(r);
                }

                foreach (CFixedWildcard w in wildcards)
                {
                    bFound = false;
                    foreach (List<CFixedWildcard> l in Wildcards)
                        if (l.Contains(w))
                        {
                            bFound = true;
                            break;
                        }
                    if (!bFound)
                        results.Add(w);
                }

                List<CFixedSingleton> singletons = new List<CFixedSingleton>();
                for (int ctr1 = 0; ctr1 < values.Count; ctr1++)
                    for (int ctr2 = 0; ctr2 < results.Count; ctr2++)
                        if (!results[ctr2].Contains(values[ctr1]))
                            singletons.Add(new CFixedSingleton(values[ctr1].Length, values[ctr1]));
                results.AddRange(singletons.ToArray());
                
                return results;
            }


            public static List<CFixedSpecifier> ProcessValues(List<String> values)
            {
                if (values.Count == 0)
                    return null;
                List<CFixedSpecifier> results = new List<CFixedSpecifier>();
                List<CFixedWildcard> wildcards = FindWildcards(values);
                List<CFixedRange> ranges = FindRanges(values);
                return GenerateCompressionSpecifiers(values, ranges, wildcards);
            }

            public static List<String> ProcessSpecifiers(List<CFixedSpecifier> specs)
            {
                List<String> values = new List<String>();
                foreach (CFixedSpecifier f in specs)
                    values.AddRange(f.GetValues());
                values.Sort();
                return values;
            }

            public String GetName()
            {
                return "FixedSpecifier";
            }

            public abstract bool Contains(String val);
            public abstract void WriteXml(XmlWriter writer);
            public abstract void ReadXml(XmlReader reader);
            public abstract bool LoadFromXml(XmlNode node);
            public abstract void WriteToXml(XmlTextWriter writer);
            public abstract List<String> GetValues();
        }

        class CFixedSingleton : CFixedSpecifier
        {
            private String _Value;

            public String Value
            {
                get
                {
                    return _Value;
                }
                set
                {
                    _Value = value;
                }
            }


            public CFixedSingleton(int nDigs)
                : base(nDigs, EType.singleton)
            {
            }

            public CFixedSingleton(int nDigs, String val)
                : base(nDigs, EType.singleton)
            {
                _Value = val;
            }

            public CFixedSingleton(int nDigs, ulong nVal)
                : base(nDigs, EType.singleton)
            {
                _Value = String.Format("{0:D" + nDigs.ToString() + "}", nVal);
            }

            public override List<String> GetValues()
            {
                List<String> strList = new List<String>();
                strList.Add(Value);
                return strList;
            }

            public override bool Contains(string val)
            {
                if (val == _Value)
                    return true;
                return false;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteString(Value);
                writer.WriteEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteString(Value);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                Value = reader.ReadElementString();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                Value = node.Value;
                return true;
            }
        }

        class CFixedRange : CFixedSpecifier
        {
            private String _MinValue, _MaxValue;

            public String MinValue
            {
                get
                {
                    return _MinValue;
                }
                set
                {
                    _MinValue = value;
                }
            }

            public String MaxValue
            {
                get
                {
                    return _MaxValue;
                }
                set
                {
                    _MaxValue = value;
                }
            }

            public CFixedRange(int nDigs)
                : base(nDigs, EType.range)
            {
                _MinValue = String.Empty;
                _MinValue = String.Empty;
            }

            public CFixedRange(int nDigs, String minValue, String maxValue)
                : base(nDigs, EType.range)
            {
                _MinValue = minValue;
                _MaxValue = maxValue;
            }

            public CFixedRange(int nDigs, ulong minValue, ulong maxValue)
                : base(nDigs, EType.range)
            {
                MinValue = String.Format("{0:D" + nDigs.ToString() + "}", minValue);
                MaxValue = String.Format("{0:D" + nDigs.ToString() + "}", maxValue);
            }

            public override List<String> GetValues()
            {
                List<String> strList = new List<String>();
                ulong val = Convert.ToUInt64(MinValue);
                ulong maxVal = Convert.ToUInt64(MaxValue);
                while (val < maxVal)
                    strList.Add(String.Format("{0:D" + MinValue.Length + "}", val++));
                return strList;
            }

            public override bool Contains(string val)
            {
                ulong nVal = Convert.ToUInt64(val);
                if ((Convert.ToUInt64(MinValue) >= nVal) && (Convert.ToUInt64(MaxValue) <= nVal))
                    return true;
                return false;
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                MinValue = reader.ReadElementString();
                MaxValue = reader.ReadElementString();
                reader.ReadEndElement();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                MinValue = node.ChildNodes[0].Value;
                MaxValue = node.ChildNodes[1].Value;
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteElementString("MinValue", MinValue);
                writer.WriteElementString("MaxValue", MaxValue);
                writer.WriteEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteElementString("MinValue", MinValue);
                writer.WriteElementString("MaxValue", MaxValue);
                writer.WriteEndElement();
            }
        }

        class CFixedWildcard : CFixedSpecifier
        {
            private String _Value;

            public String Value
            {
                get
                {
                    return _Value;
                }
                set
                {
                    _Value = value;
                }
            }

            public int WildChars
            {
                get
                {
                    String str = _Value;
                    int nChars = 0;
                    int ndx;
                    while ((ndx = str.IndexOf("?")) != -1)
                    {
                        nChars++;
                        str = str.Substring(ndx + 1);
                    }
                    return nChars;
                }
            }

            public CFixedWildcard(int nDigs)
                : base(nDigs, EType.wildcard)
            {
                _Value = String.Empty;
            }

            public CFixedWildcard(int nDigs, String val)
                : base(nDigs, EType.wildcard)
            {
                _Value = val;
            }

            public override List<String> GetValues()
            {
                int[] ndxs = new int[WildChars];
                List<String> result = new List<String>();
                ulong ctr1 = 0, ctr2 = 0;
                while (ctr1 < (ulong)ndxs.Length)
                {
                    if (Value[(int)ctr2] == '?')
                        ndxs[ctr1++] = (int)ctr2;
                    ctr2++;
                }
                for (ctr1 = 0; ctr1 < Math.Pow(10, ndxs.Length); ctr1++)
                {
                    String wilds = String.Format("{0:D" + ndxs.Length.ToString() + "}", ctr1);
                    char[] strVal = Value.ToCharArray();
                    for (ctr2 = 0; ctr2 < (ulong)ndxs.Length; ctr2++)
                        strVal[ndxs[ctr2]] = wilds[(int)ctr2];
                    result.Add(strVal.ToString());
                }
                return result;
            }

            public override bool Contains(String val)
            {
                for (int ctr1 = 0; ctr1 < Value.Length; ctr1++)
                    if (Value[ctr1] != '?')
                        if (Value[ctr1] != val[ctr1])
                            return false;
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteString(Value);
                writer.WriteEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Type", Type.ToString());
                writer.WriteString(Value);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                Value = reader.ReadElementString();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                Value = node.Value;
                return true;
            }
        }
    }
}
