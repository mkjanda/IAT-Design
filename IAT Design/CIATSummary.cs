using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace IATClient
{
    public class CIATSummary
    {
        private String _IATLink = String.Empty;
        private String IATName;
        private bool Is7Block;
        private List<int> NumStimuli;
        private List<int> NumPresentations;
        private IATConfig.ConfigFile.ERandomizationType RandomizationType;
        private List<String> _Surveys;
        private String _DataRetrievalPassword = String.Empty;
        private String _AdminPassword = String.Empty;

        public String AdminPassword
        {
            get
            {
                return _AdminPassword;
            }
            set
            {
                _AdminPassword = value;
            }
        }

        public String DataRetrievalPassword
        {
            get
            {
                return _DataRetrievalPassword;
            }
            set
            {
                _DataRetrievalPassword = value;
            }
        }

        public List<String> Surveys
        {
            get
            {
                return _Surveys;
            }
        }

        public String IATLink
        {
            get
            {
                return _IATLink;
            }
            set
            {
                _IATLink = value;
            }
        }

        public CIATSummary(IATConfig.ConfigFile cf)
        {
            NumStimuli = new List<int>();
            NumPresentations = new List<int>();
            _Surveys = new List<String>();
            IATName = cf.Name;
            Is7Block = cf.Is7Block;
            int ctr = 0;
            while (ctr < cf.EventList.Count)
            {
                if (cf.EventList[ctr].EventType == IATConfig.IATEvent.EEventType.BeginIATBlock)
                {
                    NumPresentations.Add(((IATConfig.BeginIATBlock)cf.EventList[ctr]).NumPresentations);
                    NumStimuli.Add(((IATConfig.BeginIATBlock)cf.EventList[ctr]).NumItems);
                }
                ctr++;
            }
            RandomizationType = cf.RandomizationType;
        }

        public void PrintPage(object sender, PrintPageEventArgs e)
        {
            Font f = System.Drawing.SystemFonts.DefaultFont;
            SizeF szText;
            float currY = 0;
            float linePadding = f.Height / 10;
            szText = e.Graphics.MeasureString("IAT Summary", f);
            e.Graphics.DrawString("IAT Summary", f, Brushes.Black, new PointF((e.PageBounds.Width - szText.Width) / 2, currY));
            currY += szText.Height + linePadding;
            e.Graphics.DrawLine(Pens.Black, new PointF(0, currY), new PointF(e.PageBounds.Width, currY));
            currY += linePadding;
            e.Graphics.DrawString(String.Format("IAT Name: {0}", IATName), f, Brushes.Black, new PointF(0, currY));
            currY += f.Height + linePadding;
            if (Is7Block)
                e.Graphics.DrawString("Standard 7 Block IAT", f, Brushes.Black, new PointF(0, currY));
            else
                e.Graphics.DrawString("Custom IAT", f, Brushes.Black, new PointF(0, currY));
            currY += f.Height + linePadding;
            e.Graphics.DrawString(String.Format("IAT Link: {0}", IATLink), f, Brushes.Black, new PointF(0, currY));
            currY += f.Height + linePadding;
            e.Graphics.DrawString(String.Format("Admin Password:  {0}", AdminPassword), f, Brushes.Black, new PointF(0, currY));
            currY += f.Height + linePadding;
            e.Graphics.DrawString(String.Format("Data Retrieval Password:  {0}", DataRetrievalPassword), f, Brushes.Black, new PointF(0, currY));
            currY += f.Height + linePadding;
            for (int ctr = 0; ctr < NumStimuli.Count; ctr++)
            {
                e.Graphics.DrawString(String.Format("Block #{0}: {1} stimuli, {2} presentations", ctr + 1, NumStimuli[ctr], NumPresentations[ctr]),
                    f, Brushes.Black, new PointF(0, currY));
                currY += f.Height + linePadding;
            }
            if (Surveys.Count > 0)
            {
                szText = e.Graphics.MeasureString("Surveys", f);
                e.Graphics.DrawString("Surveys", f, Brushes.Black, new PointF(0, currY));
                currY += f.Height + 1;
                e.Graphics.DrawLine(Pens.Black, new PointF(0, currY), new PointF(szText.Width, currY));
                currY += linePadding;
                for (int ctr = 0; ctr < Surveys.Count; ctr++)
                {
                    e.Graphics.DrawString(Surveys[ctr], f, Brushes.Black, new PointF(0, currY));
                    currY += f.Height + linePadding;
                }
            }
        }
    }
}
