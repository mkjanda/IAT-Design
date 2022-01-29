/*using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    abstract class SpecifierPanel : Panel
    {
        public abstract List<DynamicSpecifier> GetDefinedSpecifiers();
        public DynamicIATPanel.RetrieveIATItemWithNdx IATItemRetriever = null;
        protected static Font DisplayFont = new Font("Arial", 9.75F);

        public SpecifierPanel()
        {
            this.MouseMove += new MouseEventHandler(SpecifierPanel_MouseMove);
        }

        private void SpecifierPanel_MouseMove(object sender, EventArgs e)
        {
            
        }
    }
}
*/