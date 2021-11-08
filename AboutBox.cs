using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Threading;

namespace IATClient
{
    partial class AboutBox : Form
    {
        class InstructionManualDownload
        {
            private MemoryStream DownloadedData;
            private String SaveFilePath;
            private HttpWebRequest Request;
            private HttpWebResponse Response;
            private bool _TimedOut = false;
            private byte[] _ReadBuffer = new byte[65536];
            private object lockObject = new object();
            private Stream _ReadStream;

            public bool TimedOut
            {
                get
                {
                    lock (lockObject)
                    {
                        return _TimedOut;
                    }
                }
                set
                {
                    lock (lockObject)
                    {
                        _TimedOut = value;
                    }
                }
            }

            public byte[] ReadBuffer
            {
                get
                {
                    return _ReadBuffer;
                }
            }

            public Stream ReadStream
            {
                get
                {
                    return _ReadStream;
                }
            }

            public InstructionManualDownload(String saveFilePath, HttpWebRequest request)
            {
                SaveFilePath = saveFilePath;
                DownloadedData = new MemoryStream();
                Request = request;
            }

            public void CommitToFile()
            {
                FileStream fStream = new FileStream(SaveFilePath, FileMode.OpenOrCreate);
                fStream.Write(DownloadedData.ToArray(), 0, (int)DownloadedData.Length);
                fStream.Close();
            }

            public void BeginRead(IAsyncResult r)
            {
                Response = (HttpWebResponse)Request.EndGetResponse(r);
                _ReadStream = Response.GetResponseStream();
            }

            public void CommitRead(int nBytes)
            {
                DownloadedData.Write(ReadBuffer, 0, nBytes);
            }
        }


        private System.Windows.Forms.Timer ProgressTimer = new System.Windows.Forms.Timer();
        private PictureBox ProgressImageBox = new PictureBox();
        private Label DownloadLabel = new Label();
        private List<Image> ProgressWheelImages = new List<Image>();
        private int nProgressImage = 0;        
        public AboutBox()
        {
            InitializeComponent();
            this.VersionLabel.Text = LocalStorage.Activation[LocalStorage.Field.Version];
            this.ProductKeyEdit.Text = LocalStorage.Activation[LocalStorage.Field.ProductKey];
            ProgressTimer.Interval = 50;
            ProgressTimer.Tick += new EventHandler(ProgressTimer_Tick);
            ProgressImageBox.Size = new Size(20, 20);
            ProgressImageBox.Location = new Point(0, 0);
            ProgressPanel.Controls.Add(ProgressImageBox);
            DownloadLabel.TextAlign = ContentAlignment.MiddleLeft;
            DownloadLabel.Margin = new Padding(0, ProgressImageBox.Width >> 1, 0, 0);
            DownloadLabel.Location = new Point(ProgressImageBox.Right + 1, 0);
            DownloadLabel.Size = new Size(TextRenderer.MeasureText("Downloading Manual . . . ", DownloadLabel.Font).Width, ProgressPanel.Height);
            ProgressPanel.Controls.Add(DownloadLabel);
            this.Shown += new EventHandler(AboutBox_Shown);
            BuildDescription();
            this.okButton.Click += new EventHandler(OK_Click);
            /*
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.labelProductName.Text = AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyVersion);
            this.labelCopyright.Text = AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyCompany;
            this.textBoxDescription.Text = AssemblyDescription;
            */
        }

        private void BuildDescription()
        {
            ProductDescription.LinkClicked += new LinkClickedEventHandler(ProductDescription_LinkClicked);
            List<String> Description = new List<String>();
            Description.Add("For documentation on the use of this product, please click on the link below to download an instruction manual.");
            Description.Add(String.Empty);
            Description.Add(Properties.Resources.sInstructionManualLink);
            for (int ctr = 0; ctr < Description.Count; ctr++)
                ProductDescription.Text += Description[ctr] + "\r\n";
        }

        private void ProductDescription_LinkClicked(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF files|*.pdf|All Files|*.*";
            saveDialog.InitialDirectory = Directory.GetCurrentDirectory();
            saveDialog.FileName = "IAT Design Manual.pdf";
            if (saveDialog.ShowDialog() != DialogResult.OK)
                return;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Properties.Resources.sInstructionManualLink);
            InstructionManualDownload download = new InstructionManualDownload(saveDialog.FileName, request);
            request.Method = "GET";
            IAsyncResult result = request.BeginGetResponse(new AsyncCallback(BeginWebRead), download);
            ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, new WaitOrTimerCallback(DownloadTimeout), download, 30000, true);
            ProgressTimer.Start();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (nProgressImage >= ProgressWheelImages.Count)
                nProgressImage = 0;
            ProgressImageBox.Image = ProgressWheelImages[nProgressImage++];
            DownloadLabel.Text = String.Format("Downloading Manual {0} {1} {2}", (nProgressImage < (ProgressWheelImages.Count >> 2)) ? String.Empty : ".", 
                (nProgressImage < (ProgressWheelImages.Count >> 1)) ? String.Empty : ".", (nProgressImage < (3 * ProgressWheelImages.Count >> 2)) ? String.Empty : ".");
        }

        private void BeginWebRead(IAsyncResult result)
        {
            InstructionManualDownload theDownload = (InstructionManualDownload)result.AsyncState;
            if (theDownload.TimedOut)
                return;
            theDownload.BeginRead(result);
            IAsyncResult readResult = theDownload.ReadStream.BeginRead(theDownload.ReadBuffer, 0, 65536, new AsyncCallback(OnWebRead), theDownload);
            ThreadPool.RegisterWaitForSingleObject(readResult.AsyncWaitHandle, new WaitOrTimerCallback(DownloadTimeout), theDownload, 30000, true);
        }

        private void OnWebRead(IAsyncResult result)
        {
            InstructionManualDownload theDownload = (InstructionManualDownload)result.AsyncState;
            if (theDownload.TimedOut)
                return;
            int nBytesRead = theDownload.ReadStream.EndRead(result);
            if (nBytesRead == 0)
            {
                theDownload.CommitToFile();
                ProgressTimer.Stop();
                this.Invoke(new Action<String, System.Drawing.Color>(DisplayDownloadMessage), "Download Completed", Color.Black);
                return;
            }
            theDownload.CommitRead(nBytesRead);
            IAsyncResult readResult = theDownload.ReadStream.BeginRead(theDownload.ReadBuffer, 0, 65536, new AsyncCallback(OnWebRead), theDownload);
            ThreadPool.RegisterWaitForSingleObject(readResult.AsyncWaitHandle, new WaitOrTimerCallback(DownloadTimeout), theDownload, 30000, true);
        }

        void AboutBox_Shown(object sender, EventArgs e)
        {
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate1, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate2, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate3, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate4, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate5, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate6, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate7, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate8, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate9, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate10, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate11, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate12, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate13, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate14, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate15, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate16, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate17, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate18, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate19, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate19, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate21, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate22, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate23, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate24, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate19, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate26, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate27, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate28, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate29, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate30, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate31, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate32, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate33, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate34, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate35, 19, 19));
            ProgressWheelImages.Add(new Bitmap(Properties.Resources.progress_rotate36, 19, 19));
        }

        private void DisplayDownloadMessage(String s, System.Drawing.Color c)
        {
            DownloadLabel.ForeColor = c;
            DownloadLabel.Text = s;
        }

        private void DownloadTimeout(object download, bool timeout)
        {
            if (timeout)
            {
                InstructionManualDownload theDownload = (InstructionManualDownload)download;
                theDownload.TimedOut = true;
                ProgressTimer.Stop();
                this.Invoke(new Action<String, System.Drawing.Color>(DisplayDownloadMessage), "Download failed.  If this problem persists, contact us at admin@iatsoftware.net", System.Drawing.Color.Red);
            }
        }

        private void OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UnregisterButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(Properties.Resources.sDeactivateConfirmation, Properties.Resources.sDeactivationCaption, MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            LocalStorage.Deactivate();
            MessageBox.Show(Properties.Resources.sDeactivationCompleteMessage, Properties.Resources.sDeactivationCompleteCaption);
            this.Close();
            Application.OpenForms[Properties.Resources.sMainFormName].Close();
        }

        
        #region Assembly Attribute Accessors
/*
        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }
  */
      #endregion

    }
}
