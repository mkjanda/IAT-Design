using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IATClient
{
    public class DIKeyValueOutline : DIGenerated
    {
        private KeyedDirection KeyedDir = KeyedDirection.None;

        public DIKeyValueOutline(KeyedDirection keyedDir)
        {
            if ((keyedDir != KeyedDirection.Left) && (keyedDir != KeyedDirection.Right))
                throw new ArgumentException("Invalid keyed direction specified for layout rectangle");
            KeyedDir = keyedDir;
            ScheduleInvalidation();
        }

        public DIKeyValueOutline(Uri uri) : base(uri)
        {
        }

        protected override void Invalidate()
        {
            base.Invalidate();
        }

        public override object Clone()
        {
            DIKeyValueOutline o = new DIKeyValueOutline(this.KeyedDir);
            return o;
        }

        public override Rectangle AbsoluteBounds
        {
            get
            {
                return (KeyedDir == KeyedDirection.Left) ? CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle : CIAT.SaveFile.Layout.RightKeyValueOutlineRectangle;
            }
        }

        public override IUri IUri
        {
            get
            {
                if (KeyedDir == KeyedDirection.Left)
                    return CIATLayout.ILeftKeyValueOutlineUri;
                else
                    return CIATLayout.IRightKeyValueOutlineUri;
            }
        }

        protected override bool Generate()
        {
            if (IsDisposed || Broken)
                return true;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FromDIType(Type));
            Graphics g = Graphics.FromImage(bmp);
            Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            Pen pen = new Pen(Color.LimeGreen, 3);
            g.FillRectangle(backBr, new Rectangle(new Point(0, 0), CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Size));
            g.DrawRectangle(pen, new Rectangle(0, 0, CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Width - 3, CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Height - 3));
            pen.Dispose();
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            backBr.Dispose();
            g.Dispose();
            SetImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }

        public override void Save()
        {
            String ImageRelId = (this.IImage != null) ? (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, this.IImage.BaseType).First().Id) : String.Empty;
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rId", ImageRelId), new XElement("KeyedDirection", KeyedDir.ToString())));
            xDoc.Root.Add(new XElement("Size", new XElement("Width", ItemSize.Width.ToString()), new XElement("Height", ItemSize.Height.ToString())));
            using (Stream s = CIAT.SaveFile.GetWriteStream(this))
                xDoc.Save(s);
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            String ImageRelId = xDoc.Root.Attribute("rId").Value;
            if (ImageRelId == String.Empty)
                Generate();
            else
            {
                try
                {
                    SetImage(CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri);
                }
                catch (Exception ignored) { }
            }
            KeyedDir = KeyedDirection.FromString(xDoc.Root.Element("KeyedDirection").Value);
            ItemSize = new Size(Convert.ToInt32(xDoc.Root.Element("Size").Element("Width").Value), Convert.ToInt32(xDoc.Root.Element("Size").Element("Height").Value));
        }
    }
}
