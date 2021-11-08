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

        protected override Bitmap Generate()
        {
            if (IsDisposed || Broken)
                return null;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FromDIType(Type));
            Graphics g = Graphics.FromImage(bmp);
            Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            Pen pen = new Pen(Color.LimeGreen, 3);
            g.FillRectangle(backBr, new Rectangle(new Point(0, 0), CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Size));
            g.DrawRectangle(pen, new Rectangle(0, 0, CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Width - 3, CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Height - 3));
            pen.Dispose();
            CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            backBr.Dispose();
            g.Dispose();
            return bmp;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rImageId", rImageId), new XElement("InvalidationState", InvalidationState.ToString()),
                new XElement("KeyedDirection", KeyedDir.ToString())));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            rImageId = xDoc.Root.Attribute("rImageId").Value;
            SetImage(rImageId);
            var iStateAttr = xDoc.Root.Attribute("InvalidationState");
            if (iStateAttr != null)
            {
                InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
                if (!InvalidationState.Equals(InvalidationStates.InvalidationReady))
                {
                    InvalidationState = InvalidationStates.InvalidationReady;
                    ScheduleInvalidation();
                }
            }
            else
            {
                InvalidationState = InvalidationStates.InvalidationReady;
            }
        }
    }
}
