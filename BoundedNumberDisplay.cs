using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IATClient
{
    class BoundedNumberDisplay : TwoNumberResponseDisplay
    {
        protected override CResponse GetResponse()
        {
            return new CBoundedNumResponse((int)Value1.Value, (int)Value2.Value);
        }

        public override void SetResponse(CResponse response)
        {
            CBoundedNumResponse bnr = (CBoundedNumResponse)response;
            Value1.Value = bnr.MinValue;
            Value2.Value = bnr.MaxValue;
        }

        public BoundedNumberDisplay()
            : base(Properties.Resources.sBoundedNumLabel, Properties.Resources.sBoundedNumMinLabel, Properties.Resources.sBoundedNumMaxLabel)
        {
            Value1.Minimum = Decimal.MinValue;
            Value1.Maximum = Decimal.MaxValue;
            Value2.Minimum = Decimal.MinValue;
            Value2.Maximum = Decimal.MaxValue;
        }

        protected override void Value1Changed(object sender, EventArgs e)
        {
            (SurveyItem.Response as CBoundedNumResponse).MinValue = Value1.Value;
        }

        protected override void Value2Changed(object sender, EventArgs e)
        {
            (SurveyItem.Response as CBoundedNumResponse).MaxValue = Value2.Value;
        }
    }
}
