using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    class TrueFalseDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(23, 2, 23, 3);

        protected override Padding GetChoiceEditPadding()
        {
            return ChoiceEditPadding;
        }

        protected override CResponse GetResponse()
        {
            return new CBoolResponse(ChoiceEdits[0].Text, ChoiceEdits[1].Text);
        }

        protected override String GetChoiceDefaultText(TextBox sender)
        {
            if (sender == ChoiceEdits[0])
                return "True";
            else
                return "False";
        }

        public override void SetResponse(CResponse response)
        {
            SuspendLayout();
            CBoolResponse br = (CBoolResponse)response;
            ClearChoices();
            CreateChoiceEdit(br.TrueStatement);
            CreateChoiceEdit(br.FalseStatement);
            LayoutControl();
            ResumeLayout();
        }

        public TrueFalseDisplay()
            : base(false)
        {
        }

        protected override void AddChoiceButton_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void RemoveChoiceFromResponse(int ndx)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateChoiceText(int ndx, string text)
        {
            if (ndx == 0)
                (SurveyItem.Response as CBoolResponse).TrueStatement = text;
            else if (ndx == 1)
                (SurveyItem.Response as CBoolResponse).FalseStatement = text;
        }

        public override async void LayoutControl()
        {
            Action a = () =>
            {
                if (!Monitor.TryEnter(lockObj))
                    return;
                LayoutEvent.WaitOne();
                LayoutEvent.Reset();
                Monitor.Exit(lockObj);
                LayoutChoices();
                this.Invoke(new Action(() =>
                {
                    SuspendLayout();
                    this.Size = new Size(ClientSize.Width, ChoicesSize.Height + InteriorPadding.Vertical);
                }));
                LayoutEvent.Set();
                ((QuestionDisplay)Parent).RecalcSize(false);
            };
            await Task.Run(a);
        }

        protected override void ResponseDisplay_Load(object sender, EventArgs e)
        {
            base.ResponseDisplay_Load(sender, e);
            if (ChoiceEdits.Count == 0)
            {
                CreateChoiceEdit("True");
                ChoiceEdits[0].Leave += new EventHandler(TrueFalse_Leave);
                CreateChoiceEdit("False");
                ChoiceEdits[1].Leave += new EventHandler(TrueFalse_Leave);
            }
            LayoutControl();
        }

        private void TrueFalse_Leave(object sender, EventArgs e)
        {
            TextBox box = (TextBox)sender;
            if (box.Text == String.Empty)
            {
                if (ChoiceEdits.IndexOf(box) == 0)
                    box.Text = "True";
                else
                    box.Text = "False";
            }
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
