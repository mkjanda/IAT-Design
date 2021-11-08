using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
/*
namespace IATClient
{
    partial class PackagerForm : Form
    {
        public enum EChildControlResult { ok, cancel };
        public delegate void ChildControlCompleteEventHandler(EChildControlResult result);
        public static Size ChildControlSize = new Size(325, 345);

        private NumPresentationsControl numPresentationsControl;
        private IATPackageControl packageControl;
        public CIAT theIAT;
        private IATConfigMainForm MainForm;

        public PackagerForm(IATConfigMainForm mainForm)
        {
            InitializeComponent();
            numPresentationsControl = new NumPresentationsControl(mainForm.IAT);
            numPresentationsControl.Size = ChildControlSize;
            numPresentationsControl.Location = new Point(0, 0);
            packageControl = new IATPackageControl();
            packageControl.Size = ChildControlSize;
            packageControl.Location = new Point(0, 0);
            theIAT = mainForm.IAT;
            MainForm = mainForm;
        }

        private void PackagerForm_Load(object sender, EventArgs e)
        {
            packageControl.OnControlComplete += new ChildControlCompleteEventHandler(PackageControl_OnComplete);
            Controls.Add(packageControl);
        }

        private void PackageControl_OnComplete(EChildControlResult result)
        {
            if (result == EChildControlResult.cancel)
            {
                Close();
                return;
            }
            if (packageControl.RandomizationType == IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations)
            {
                numPresentationsControl.OnControlComplete += new ChildControlCompleteEventHandler(NumPresentationsControl_Complete);
                Controls.Remove(packageControl);
                Controls.Add(numPresentationsControl);
                return;
            }
            theIAT.Packager.IATName = packageControl.theIATName;
            theIAT.Packager.RedirectionURL = packageControl.theRedirectionURL;
            theIAT.Packager.LeftResponseChar = packageControl.LeftResponseChar;
            theIAT.Packager.RightResponseChar = packageControl.RightResponseChar;
            theIAT.Packager.RandomizationType = packageControl.RandomizationType;
            CDynamicSpecifier.CompactSpecifierDictionary(theIAT);
            DataPasswordForm progressWindow = new DataPasswordForm();
            if (theIAT.Packager.ProcessIAT(MainForm, System.IO.Path.GetDirectoryName(packageControl.thePackageFileName),
                packageControl.theIATName, System.IO.Path.GetFileName(packageControl.thePackageFileName),
                progressWindow) == true)
                progressWindow.ShowDialog(this);
            Close();
        }

        private void NumPresentationsControl_Complete(EChildControlResult result)
        {
            if (result == EChildControlResult.cancel)
            {
                Close();
                return;
            }
            theIAT.Packager.IATName = packageControl.theIATName;
            theIAT.Packager.RedirectionURL = packageControl.theRedirectionURL;
            theIAT.Packager.LeftResponseChar = packageControl.LeftResponseChar;
            theIAT.Packager.RightResponseChar = packageControl.RightResponseChar;
            theIAT.Packager.RandomizationType = IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations;
            int[] numPresentations = numPresentationsControl.NumPresentations;
            int blockNdx = 0;
            for (int ctr = 0; ctr < theIAT.Contents.Count; ctr++)
            {
                if ((theIAT.Contents[ctr].Type == ContentsItemType.IATBlock) || (theIAT.Contents[ctr].Type == ContentsItemType.IATPracticeBlock))
                    ((CIATBlock)theIAT.Contents[ctr]).NumPresentations = numPresentations[blockNdx++];
            }
            DataPasswordForm progressWindow = new DataPasswordForm();
            if (theIAT.Packager.ProcessIAT(MainForm, System.IO.Path.GetDirectoryName(packageControl.thePackageFileName),
                packageControl.theIATName, System.IO.Path.GetFileName(packageControl.thePackageFileName),
                progressWindow) == true)
                progressWindow.ShowDialog(this);
            Close();
        }
    }
}
*/