using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    public class ResultDetailsPanelContainer : SplitContainer
    {
        private Control StartPanel;

        public ResultDetailsPanelContainer(Control startPanel)
        {
            Panel2Collapsed = true;
            Panel1.Controls.Add(startPanel);
            Orientation = Orientation.Horizontal;
            StartPanel = startPanel;
            this.SplitterMoved += new SplitterEventHandler(ResultDetailsContainer_SplitterMoved);
        }
        
        private void ResultDetailsContainer_SplitterMoved(Object sender, SplitterEventArgs e) {
            if (!Panel1Collapsed)
                Panel1.Invalidate();
            if (!Panel2Collapsed)
                Panel2.Invalidate();
        }
        
        public void PanelClose(Panel p)
        {
            SuspendLayout();
            if (Panel2Collapsed)
            {
                Panel1.Controls.Clear();
                Panel1.Controls.Add(StartPanel);
                ResumeLayout(false);
                return;
            }
            if (Panel1.Controls.Contains(p))
            {
                Panel1.Controls.Clear();
                foreach (Control c in Panel2.Controls)
                    Panel1.Controls.Add(c);
            }
            Panel2.Controls.Clear();
            Panel2Collapsed = true;
            ResumeLayout(false);
        }

        public void PanelSplit()
        {
            if (!Panel2Collapsed)
                return;
            SuspendLayout();
            foreach (Control c in Panel1.Controls)
                Panel2.Controls.Add(c);
            Panel1.Controls.Clear();
            Panel1.Controls.Add(StartPanel);
            Panel2Collapsed = false;
            ResumeLayout(false);
        }

        public void SetPanel(Panel p, Control callingPanel)
        {
            p.Dock = DockStyle.Fill;
            p.AutoScroll = false;
            if (Panel1.Controls.Contains(callingPanel))
            {
                Panel1.Controls.Clear();
                Panel1.Controls.Add(p);
            }
            else if (Panel2.Controls.Contains(callingPanel))
            {
                Panel2.Controls.Clear();
                Panel2.Controls.Add(p);
            }
        }
    }
}
