using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class IATTabPanel : TabControl
    {
        private ResultSetDescriptor Descriptor = null;
        private IResultData ResultSets = null;
        private List<TabPage> Pages = new List<TabPage>();
        private ResultGridPanel ResultGrid = null;
        private ItemSlidePanel ItemSlides = null;
        private SubjectDefinitionPanel SubjectIDPanel = null;
        public IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        public IATTabPanel(ResultSetDescriptor descriptor, IResultData resultSets, Size szThis, EventHandler OnClose)
        {
            this.Size = szThis;
//            this.Selected += new TabControlEventHandler(IATTabPanel_Selected);
            Descriptor = descriptor;
            ResultSets = resultSets;
            TabPage gridPage = new TabPage("IAT Results");
            gridPage.UseVisualStyleBackColor = true;
            gridPage.Location = new Point(4, 22);
            gridPage.Size = szThis - new Size(8, 26);
            ResultGrid = new ResultGridPanel(gridPage.ClientRectangle.Size);
            ResultGrid.Initialize(resultSets, descriptor);
            gridPage.Controls.Add(ItemSlides);
            Pages.Add(gridPage);

            TabPage itemSlidePage = new TabPage("Item Slides");
            itemSlidePage.UseVisualStyleBackColor = true;
            itemSlidePage.Location = new Point(4, 22);
            itemSlidePage.Size = szThis - new Size(8, 26);
     //       ItemSlidePanel slidePanel = new ItemSlidePanel(itemSlidePage.Size);
            ItemSlides.Dock = DockStyle.Fill;
            itemSlidePage.Controls.Add(ItemSlides);
            Pages.Add(itemSlidePage);


            TabPage subjectIDPage = new TabPage("Define Subject ID Method");
            subjectIDPage.UseVisualStyleBackColor = true;
            subjectIDPage.Location = new Point(4, 22);
            subjectIDPage.Size = szThis - new Size(8, 26);
            SubjectIDPanel = new SubjectDefinitionPanel(descriptor, subjectIDPage.ClientRectangle.Width);
            subjectIDPage.Controls.Add(subjectIDPage);
            Pages.Add(subjectIDPage);


            for (int ctr = 0; ctr < Descriptor.BeforeSurveys.Count + Descriptor.AfterSurveys.Count; ctr++)
            {
                TabPage surveyTab = new TabPage();
                Panel surveyPanel = new Panel();
                surveyPanel.Dock = DockStyle.Fill;
                surveyPanel.AutoScroll = true;
                surveyTab.Controls.Add(surveyPanel);
            }
        }
        /*
        void IATTabPanel_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 2)
            {
                MainForm.ResumeItemSlideRetrieval();
                if (!ItemSlides.IsInitialized)
                    ItemSlides.Initialize();
            }
            else
            {
                MainForm.PauseItemSlideRetrieval();
            }
        }
         */
    }
}
