using System;

namespace IATClient
{
    class BoundedLengthDisplay : TwoNumberResponseDisplay
    {
        protected override CResponse GetResponse()
        {
            return new CBoundedLengthResponse(Convert.ToInt32(Value1.Value), Convert.ToInt32(Value2.Value));
        }

        public override void SetResponse(CResponse response)
        {
            CBoundedLengthResponse blr = (CBoundedLengthResponse)response;
            Value1.Value = blr.MinLength;
            Value2.Value = blr.MaxLength;
            Invalidate();
        }

        protected override void Value1Changed(object sender, EventArgs e)
        {
            (SurveyItem.Response as CBoundedLengthResponse).MinLength = (int)Value1.Value;
        }

        protected override void Value2Changed(object sender, EventArgs e)
        {
            (SurveyItem.Response as CBoundedLengthResponse).MaxLength = (int)Value2.Value;
        }

        public BoundedLengthDisplay()
            : base(Properties.Resources.sBoundedLengthLabel, Properties.Resources.sBoundedLengthMinLabel, Properties.Resources.sBoundedLengthMaxLabel)
        {
            Value1.Minimum = 0;
            Value1.Maximum = 4000;
            Value2.Minimum = 0;
            Value2.Maximum = 4000;
        }
    }
}
