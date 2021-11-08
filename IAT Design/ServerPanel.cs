using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ServerPanel : UserControl
    {
        public static Size ServerPanelSize = new Size(787, 505);

        public String IATName
        {
            get
            {
                return PackagePanel.IATName;
            }
            set
            {
                PackagePanel.IATName = value;
            }
        }
        

        public IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Parent;
            }
        }

        public ServerPanel()
        {
            InitializeComponent();
            PackagePanel.OnDataRetrievalPasswordChanged += new TestPackagerPanel.TextBoxChangedEventHandler(DataPanel.DataRetrievalPasswordChangedInPackagerPanel);
            PackagePanel.OnHostURLChanged += new TestPackagerPanel.TextBoxChangedEventHandler(DataPanel.HostURLChangedInPackagerPanel);
            PackagePanel.OnIATNameChanged += new TestPackagerPanel.TextBoxChangedEventHandler(DataPanel.IATNameChangedInPackagerPanel);
        }

        private void ServerPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                PackagePanel.InitFromIAT(MainForm.IAT);
                DataPanel.theIAT = MainForm.IAT;
            }
        }
    }
}
