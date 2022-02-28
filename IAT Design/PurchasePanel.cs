using mshtml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
namespace IATClient
{
    [System.Runtime.InteropServices.ComVisibleAttribute(true)]
    public class PurchasePanel : UserControl
    {

        public class ResourcePrice
        {
            public enum EResourceType { Administration, DiskSpace, Iat };
            private EResourceType _Resource;
            private int _Quantity, _Price;

            public EResourceType Resource
            {
                get
                {
                    return _Resource;
                }
            }

            public int Quantity
            {
                get
                {
                    return _Quantity;
                }
            }

            public int Price
            {
                get
                {
                    return _Price;
                }
            }

            public ResourcePrice() { }

            public void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                _Resource = (EResourceType)Enum.Parse(typeof(EResourceType), reader.ReadElementString("Resource"));
                _Quantity = Convert.ToInt32(reader.ReadElementString("Quantity"));
                _Price = Convert.ToInt32(reader.ReadElementString("Price"));
                reader.ReadEndElement();
            }
        }

        public class Pricing
        {
            private List<ResourcePrice> _ResourcePrices = new List<ResourcePrice>();

            public Pricing() { }

            public List<ResourcePrice> AdministrationPrices
            {
                get
                {
                    return new List<ResourcePrice>(_ResourcePrices.FindAll(resource => resource.Resource == ResourcePrice.EResourceType.Administration).OrderBy(Resource => Resource.Quantity));
                }
            }

            public List<ResourcePrice> IATPrices
            {
                get
                {
                    return new List<ResourcePrice>(_ResourcePrices.FindAll(resource => resource.Resource == ResourcePrice.EResourceType.Iat).OrderBy(Resource => Resource.Quantity));
                }
            }

            public List<ResourcePrice> DiskSpacePrices
            {
                get
                {
                    return new List<ResourcePrice>(_ResourcePrices.FindAll(resource => resource.Resource == ResourcePrice.EResourceType.DiskSpace).OrderBy(Resource => Resource.Quantity));
                }
            }

            public void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                while (reader.Name == "ResourcePrice")
                {
                    ResourcePrice rp = new ResourcePrice();
                    rp.ReadXml(reader);
                    _ResourcePrices.Add(rp);
                }
            }
        }


        private WebBrowser browserWindow = null;
        private Pricing pricing;
        private static Size PurchasePanelSize = new Size(1010, 645);
        private String JSESSIONID;
        private bool PaymentSubmitted;
        private WebBrowserDocumentCompletedEventHandler hPageLoaded;
        private int adminPrice = 0, iatPrice = 0, diskSpacePrice = 0;
        private Button SubmitButton, CancelButton;
        private enum EPurchasePhase { initializing = 1, order = 2, payment = 3, finalize = 4, confirmation = 5, other = 6 };
        private Nullable<EPurchasePhase> PurchasePhase = EPurchasePhase.initializing;

        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        public PurchasePanel()
        {
            this.Load += new EventHandler(PurchasePanel_Load);
            this.Size = PurchasePanelSize;
        }

        private void PurchasePanel_Load(object sender, EventArgs e)
        {
            SuspendLayout();
            SubmitButton = new Button();
            SubmitButton.Text = "Submit";
            CancelButton = new Button();
            CancelButton.Text = "Close";
            Controls.Add(SubmitButton);
            SubmitButton.Left = (this.Width >> 1) - SubmitButton.Width;
            SubmitButton.Top = this.Height - SubmitButton.Height;
            SubmitButton.Click += new EventHandler(Submit_Click);
            SubmitButton.Enabled = false;
            Controls.Add(CancelButton);
            CancelButton.Left = (this.Width >> 1) + CancelButton.Width;
            CancelButton.Top = this.Height - CancelButton.Height;
            CancelButton.Click += new EventHandler(Cancel_Click);
            PurchaseInitiation initPurchase = new PurchaseInitiation();
            MemoryStream memStream = new MemoryStream();
            XmlWriter xWriter = new XmlTextWriter(memStream, Encoding.UTF8);
            xWriter.WriteStartDocument();
            initPurchase.WriteXml(xWriter);
            xWriter.WriteEndDocument();
            xWriter.Flush();
            if (browserWindow != null)
                this.Controls.Add(browserWindow);
            browserWindow = new WebBrowser();
            browserWindow.Dock = DockStyle.Top;
            browserWindow.Width = this.Width;
            browserWindow.Location = new Point(0, 0);
            browserWindow.Height = this.Height - SubmitButton.Height;
            browserWindow.WebBrowserShortcutsEnabled = false;
            browserWindow.IsWebBrowserContextMenuEnabled = false;
            browserWindow.AllowWebBrowserDrop = false;
            browserWindow.ScriptErrorsSuppressed = true;
            browserWindow.ObjectForScripting = this;
            hPageLoaded = new WebBrowserDocumentCompletedEventHandler(WebPage_Loaded);
            Controls.Add(browserWindow);
            HttpWebRequest request = WebRequest.CreateHttp(Properties.Resources.sPurchaseResourcesURI);
            request.Method = "POST";
            request.CookieContainer = new CookieContainer();
            request.ContentType = "text/xml";
            request.ContentLength = memStream.Length;
            request.KeepAlive = false;
            request.Timeout = 30000;
            try
            {
                Stream s = request.GetRequestStream();
                s.Write(memStream.ToArray(), 0, (int)memStream.Length);
                request.GetResponseAsync().ContinueWith(new Action<Task<WebResponse>>(PurchasePageReturned));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to IAT Software server. Please try again later.");
                MainForm.Controls.Remove(this);
                MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
            }
        }

        private void Submit_Click(object sender, EventArgs e)
        {
            if (this.PurchasePhase == EPurchasePhase.order)
            {
                if (adminPrice + diskSpacePrice + iatPrice == 0)
                {
                    MessageBox.Show("Please select the resources you wish to purchase before clicking Submit", "No Purchase Selected");
                    return;
                }
                InsertProductKey();
            }
            if (this.PurchasePhase == EPurchasePhase.payment)
                if (!ValidatePaymentInfo())
                    return;
            if (this.PurchasePhase == EPurchasePhase.finalize)
            {
                mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
                if (((mshtml.IHTMLInputElement)doc.getElementById("newEMailCheck")).@checked)
                {
                    String email = ((mshtml.IHTMLInputElement)doc.getElementById("newEMail")).value;
                    Regex exp = new Regex(@".+@.+\..+");
                    if (!exp.IsMatch(email))
                    {
                        MessageBox.Show("Please enter a valid email address.");
                        return;
                    }
                    else
                    {
                        DialogResult dlgResult = MessageBox.Show("Do you wish to update the email address your product is registered to with this address?", "Update Email?", MessageBoxButtons.YesNoCancel);
                        if (dlgResult == DialogResult.Yes)
                            ((mshtml.IHTMLInputElement)doc.getElementById("updateEMail")).value = "yes";
                        else if (dlgResult == DialogResult.Cancel)
                            return;
                    }
                }
            }
            this.SubmitButton.Enabled = false;
            foreach (HtmlElement form in browserWindow.Document.Forms)
                form.InvokeMember("submit");
        }

        private void InsertProductKey()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            mshtml.IHTMLInputElement elem = (mshtml.IHTMLInputElement)doc.getElementById("productKey");
            elem.value = LocalStorage.Activation[LocalStorage.Field.ProductKey];
        }

        private bool ValidatePaymentInfo()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            String fName = doc.getElementsByName("firstName").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String lName = doc.getElementsByName("lastName").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String address1 = doc.getElementsByName("address1").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String city = doc.getElementsByName("city").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String state = doc.getElementsByName("state").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String zip = doc.getElementsByName("zip").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String cc = doc.getElementsByName("countryCode").OfType<IHTMLSelectElement>().First<IHTMLSelectElement>().value;
            bool noCard = ((from elem in doc.getElementsByName("card").OfType<IHTMLInputElement>() where elem.@checked select elem).Count() == 0);
            String cardNumber = doc.getElementsByName("cardNumber").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String cvv2 = doc.getElementsByName("cvv2").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String cardExpMonth = doc.getElementsByName("cardExpMonth").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            String cardExpYear = doc.getElementsByName("cardExpYear").OfType<IHTMLInputElement>().First<IHTMLInputElement>().value;
            if ((fName == null) || (lName == null))
            {
                MessageBox.Show("Please enter a valid first and last name");
                return false;
            }
            if ((fName.Trim() == String.Empty) || (lName.Trim() == String.Empty))
            {
                MessageBox.Show("Please enter a valid first and last name");
                return false;
            }
            if (address1 == null)
            {
                MessageBox.Show("Please enter a street address");
                return false;
            }
            if (address1.Trim() == String.Empty)
            {
                MessageBox.Show("Please enter a street address");
                return false;
            }
            if (city == null)
            {
                MessageBox.Show("Please enter your city");
                return false;
            }
            if (city.Trim() == String.Empty)
            {
                MessageBox.Show("Please enter your city");
                return false;
            }
            if (cc == null)
            {
                MessageBox.Show("Please select your country");
                return false;
            }
            if (cc.Trim() == String.Empty)
            {
                MessageBox.Show("Please select your country");
                return false;
            }
            if (noCard)
            {
                MessageBox.Show("Please select a credit card");
                return false;
            }
            if ((cardNumber == null) || (cvv2 == null))
            {
                MessageBox.Show("Please enter your credit card number and CVV2 code, which appears on the rear of the card.");
                return false;
            }
            if ((cardNumber.Trim() == String.Empty) || (cvv2.Trim() == String.Empty))
            {
                MessageBox.Show("Please enter your credit card number and CVV2 code, which appears on the rear of the card.");
                return false;
            }
            if ((cardExpMonth == null) || (cardExpYear == null))
            {
                MessageBox.Show("Please enter your card expiration date");
                return false;
            }
            if ((cardExpMonth.Trim() == String.Empty) || (cardExpYear.Trim() == String.Empty))
            {
                MessageBox.Show("Please enter your card expiration date");
                return false;
            }
            return true;
        }

        private void WebPage_Loaded(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Regex exp = new Regex("EnableSubmit=(true|false)");
            String cookies = ((mshtml.IHTMLDocument2)browserWindow.Document.DomDocument).cookie;
            if (cookies == null)
            {
                this.SubmitButton.Enabled = true;
            }
            else
            {
                Match m = exp.Match(cookies);
                if (Convert.ToBoolean(m.Groups[1].Value))
                    this.SubmitButton.Enabled = true;
            }
            exp = new Regex("Page=(order|payment|finalize|confirmation|other)");
            if (cookies != null)
            {
                Match m = exp.Match(cookies);
                if (m.Success)
                    PurchasePhase = Enum.Parse(typeof(EPurchasePhase), m.Groups[1].Value) as Nullable<EPurchasePhase>;
                else
                    PurchasePhase = EPurchasePhase.other;
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void PurchasePageReturned(Task<WebResponse> finishedTask)
        {
            HttpWebResponse resp = (HttpWebResponse)finishedTask.Result;
            MemoryStream memStream = new MemoryStream();
            Stream s = resp.GetResponseStream();
            long contentLength = resp.ContentLength;
            long totalBytesRead = 0;
            int nBytesRead;
            byte[] buff = new byte[65536];
            while ((nBytesRead = s.Read(buff, 0, 65536)) != 0)
            {
                memStream.Write(buff, 0, nBytesRead);
                totalBytesRead += nBytesRead;
            }

            browserWindow.DocumentCompleted += hPageLoaded;
            this.BeginInvoke(new Action<String>(SetBrowserWindowContent), Encoding.UTF8.GetString(memStream.ToArray()));
            JSESSIONID = resp.Cookies["JSESSIONID"].Value;
            PurchasePhase = EPurchasePhase.order;
            HttpWebRequest request = WebRequest.CreateHttp(Properties.Resources.sRetrieveResourcePricesURI);
            request.Method = "GET";
            request.KeepAlive = false;
            request.Accept = "text/xml";
            request.Timeout = 30000;
            try
            {
                request.GetResponseAsync().ContinueWith(new Action<Task<WebResponse>>(PricesReturned));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to IAT Software server. Please try again later.");
                MainForm.Controls.Remove(this);
                MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
            }
        }

        public void AdminQuantityChanged()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            adminPrice = Convert.ToInt32(((mshtml.IHTMLInputElement)doc.getElementById("AdministrationsDrop_" + ((mshtml.IHTMLSelectElement)doc.getElementById("AdministrationsDrop")).selectedIndex.ToString())).value);
            UpdateTotal();
        }

        public void IATQuantityChanged()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            iatPrice = Convert.ToInt32(((mshtml.IHTMLInputElement)doc.getElementById("IATsDrop_" + ((mshtml.IHTMLSelectElement)doc.getElementById("IATsDrop")).selectedIndex.ToString())).value);
            UpdateTotal();
        }

        public void DiskSpaceChanged()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            diskSpacePrice = Convert.ToInt32(((mshtml.IHTMLInputElement)doc.getElementById("DiskSpaceDrop_" + ((mshtml.IHTMLSelectElement)doc.getElementById("DiskSpaceDrop")).selectedIndex.ToString())).value);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            ((mshtml.IHTMLElement)doc.getElementById("total")).innerHTML = "$" + (adminPrice + iatPrice + diskSpacePrice).ToString();
        }

        public void EnableNewEMail()
        {
            mshtml.IHTMLDocument3 doc = (mshtml.IHTMLDocument3)browserWindow.Document.DomDocument;
            if (((mshtml.IHTMLInputElement)doc.getElementById("newEMailCheck")).@checked)
            {
                ((mshtml.IHTMLInputElement)doc.getElementById("newEMail")).disabled = false;
                ((mshtml.IHTMLInputElement)doc.getElementById("newEMailCheck")).value = "true";
            }
            else
            {
                ((mshtml.IHTMLInputElement)doc.getElementById("newEMail")).value = "";
                ((mshtml.IHTMLInputElement)doc.getElementById("newEMailCheck")).value = "false";
                ((mshtml.IHTMLInputElement)doc.getElementById("newEMail")).disabled = true;
            }
        }

        private void PricesReturned(Task<WebResponse> finishedTask)
        {
            WebResponse response = finishedTask.Result;
            MemoryStream memStream = new MemoryStream();
            Stream s = response.GetResponseStream();
            byte[] buff = new byte[65536];
            int nBytesRead = 0;
            while ((nBytesRead = s.Read(buff, 0, 65536)) != 0)
                memStream.Write(buff, 0, nBytesRead);
            memStream.Seek(0, SeekOrigin.Begin);
            TextReader txtReader = new StreamReader(memStream, Encoding.UTF8);
            XmlReader xReader = new XmlTextReader(txtReader);
            pricing = new Pricing();
            pricing.ReadXml(xReader);
        }

        private void SetBrowserWindowContent(String s)
        {
            browserWindow.DocumentText = s;

        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        public void SendPurchase(PurchaseOrder pOrder)
        {
            HttpWebRequest request = WebRequest.CreateHttp(Properties.Resources.sSubmitPurchaseOrderURI);
            request.Method = "POST";
            request.KeepAlive = false;
            request.ContentType = "text/xml; charset=utf-8";
            MemoryStream memStream = new MemoryStream();
            XmlWriter xWriter = new XmlTextWriter(memStream, Encoding.UTF8);
            xWriter.WriteStartDocument();
            pOrder.WriteXml(xWriter);
            xWriter.Flush();
            request.ContentLength = memStream.Length;
            try
            {
                Stream s = request.GetRequestStream();
                s.Write(memStream.ToArray(), 0, (int)memStream.Length);
                request.GetResponseAsync().ContinueWith((Action<Task<WebResponse>>)PurchaseOrderSent);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to connect to IAT Software server. Please try again later.");
                MainForm.Controls.Remove(this);
                MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
            }
        }

        private void PurchaseOrderSent(Task<WebResponse> respTask)
        {
            MemoryStream memStream = new MemoryStream();
            try
            {
                WebResponse resp = respTask.Result;
                Stream s = resp.GetResponseStream();
                long contentLength = resp.ContentLength;
                long totalBytesRead = 0;
                int nBytesRead;
                byte[] buff = new byte[65536];
                while ((nBytesRead = s.Read(buff, 0, 65536)) != 0)
                {
                    memStream.Write(buff, 0, nBytesRead);
                    totalBytesRead += nBytesRead;
                }
                JSESSIONID = resp.Headers["JSESSIONID"];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Due to a server error, your purchase could not be completed. You have not been billed. Please try again later. If this problem persists, contact us at admin@iatsoftware.net.");
                MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
            }
            this.BeginInvoke(new Action<String>(SetBrowserWindowContent), Encoding.UTF8.GetString(memStream.ToArray()));
            browserWindow.Navigating += new WebBrowserNavigatingEventHandler(BrowserWindow_Navigating);
        }

        private void BrowserWindow_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (PaymentSubmitted)
                e.Cancel = true;
            PaymentSubmitted = true;
        }
    }
}
