using System;
using System.Collections.Generic;
using System.Text;

namespace IATClient
{
    class FixedDigitDisplay : OneNumberResponseDisplay
    {
        protected override CResponse GetResponse()
        {
            return new CFixedDigResponse((int)Value1.Value);
        }

        public override void SetResponse(CResponse response)
        {
            Value1.Value = ((CFixedDigResponse)response).NumDigs;
            Invalidate();
        }

        public FixedDigitDisplay()
            : base(Properties.Resources.sFixedDigitLabel, Properties.Resources.sFixedDigitValueLabel)
        {
        }

        protected override void ResponseDisplay_Load(object sender, EventArgs e)
        {
            base.ResponseDisplay_Load(sender, e);
            Value1.Minimum = 1;
            Value1.Maximum = 4000;
            ((QuestionDisplay)Parent).RecalcSize();
        }

        protected override void Value1Changed(object sender, EventArgs e)
        {
            (SurveyItem.Response as CFixedDigResponse).NumDigs = (int)Value1.Value;
        }
    }
}
