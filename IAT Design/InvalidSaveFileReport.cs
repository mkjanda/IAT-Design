using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IATClient
{
    public class InvalidSaveFileReport
    {
        public static void Report()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("CorruptedSaveFileReport", new XElement("ProductCode", LocalStorage.Activation[LocalStorage.Field.ProductKey]),
                new XElement("UserName", LocalStorage.Activation[LocalStorage.Field.UserName]),
                new XElement("UserEmail", LocalStorage.Activation[LocalStorage.Field.UserEmail])));
            StringWriter sWriter = new StringWriter();
            xDoc.Save(sWriter);
            MemoryStream memStream = new MemoryStream();
            byte[] bytes = Encoding.UTF8.GetBytes(sWriter.ToString());
            memStream.Write(bytes, 0, bytes.Length);
            WebClient wClient = new WebClient();
            wClient.Headers[HttpRequestHeader.ContentType] = "text/xml";
            wClient.UploadData(Properties.Resources.sInvalidSaveFileReportURL, memStream.ToArray());
            MessageBox.Show("Invalid or corrupted save file.", "IAT Design");
        }
    }
}
