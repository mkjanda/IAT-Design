using System;
using System.Collections.Generic;
using System.Drawing;

using System.Text;
using System.Xml;

namespace IATClient
{
    [Serializable]
    class CStimulusImageItem : CImageDisplayItem, IStimulus
    {
        private bool bComponentUpdated = false;

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.image;
            }
        }

        public override bool IsDefined()
        {
            return true;
        }

        public CStimulusImageItem()
            : base(EType.stimulusImage)
        {
            StretchToFit = false;
        }

        public CStimulusImageItem(IIATImage image)
            : base(EType.stimulusImage)
        {
            if (image == null)
                return;
            _IATImage = image;
            if ((IATImage.OriginalImageSize.Width > CIAT.Layout.StimulusSize.Width)
                || (IATImage.OriginalImageSize.Height > CIAT.Layout.StimulusSize.Height))
            IATImage.Resize(CIAT.Layout.StimulusSize);
            StretchToFit = true;
        }

        public CStimulusImageItem(CStimulusImageItem o)
            : base(o)
        {
            _IATImage = o.IATImage;
            StretchToFit = true;
        }

        public void SetImage(IIATImage image)
        {
            Lock();
            if (IATImage != null)
                IATImage.Dispose();
            _IATImage = image;
//            image.CreateCopy();
            Unlock();
            bComponentUpdated = true;
        }

        public override bool IsValid
        {
            get
            {
                return !bComponentUpdated;
            }
        }

        public override void Validate()
        {
            bComponentUpdated = false;
        }

        public override void Invalidate()
        {
            bComponentUpdated = true;
            IATImage.Resize(CIAT.Layout.StimulusSize);
        }

        public override void Dispose()
        {
            Lock();
            base.Dispose();
            bComponentUpdated = false;
            Unlock();
        }

        public override String FullFilePath
        {
            get
            {
                Lock();
                String str = base.FullFilePath;
                Unlock();
                return str;
            }
            set
            {
                Lock();
                base.FullFilePath = value;
                if (StretchToFit || (IATImage.OriginalImageSize.Width > CIAT.Layout.StimulusSize.Width) || (IATImage.OriginalImageSize.Height > CIAT.Layout.StimulusSize.Height))
                    IATImage.Resize(CIAT.Layout.StimulusSize);
                bComponentUpdated = true;
                Unlock();
            }
        }

        protected override Size GetItemSize()
        {
            Size szResult = new Size();
            if (StretchToFit || (IATImage.OriginalImageSize.Width > CIAT.Layout.StimulusSize.Width) || (IATImage.OriginalImageSize.Height > CIAT.Layout.StimulusSize.Height))
            {
                double AspectRatio = (double)IATImage.OriginalImageSize.Width / (double)IATImage.OriginalImageSize.Height;
                double SizedAR = (double)CIAT.Layout.StimulusSize.Width / (double)CIAT.Layout.StimulusSize.Height;
                if (SizedAR > AspectRatio)
                {
                    szResult.Width = (int)(CIAT.Layout.StimulusSize.Height * AspectRatio);
                    szResult.Height = CIAT.Layout.StimulusSize.Height;
                }
                else
                {
                    szResult.Width = CIAT.Layout.StimulusSize.Width;
                    szResult.Height = (int)((double)CIAT.Layout.StimulusSize.Width / (double)AspectRatio);
                }
            }
            else
                szResult = IATImage.OriginalImageSize;
            return szResult;
        }

        /// <summary>
        /// Writes the object's data to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("DisplayItem");
            writer.WriteStartAttribute("Type");
            writer.WriteString(sImage);
            writer.WriteEndAttribute();
            writer.WriteElementString("SaveFileNdx", IATImageID.ToString());
            writer.WriteElementString("Description", Description);
            writer.WriteElementString("StretchToFit", StretchToFit.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Load's the object's data from an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // ensure that node has the correct number of children
            if (node.ChildNodes.Count != 3)
                return false;

            // load the file name and directory
            int nodeCtr = 0;
            _IATImage = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            Description = node.ChildNodes[nodeCtr++].InnerText;
            _StretchToFit = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            ((IUserImage)CIAT.ImageManager[_IATImage]).SetSizeCallback(new ImageManager.ImageSizeCallback(CalcImageSize));
            if ((IATImage.OriginalImageSize.Width > CIAT.Layout.StimulusSize.Width)  || (IATImage.OriginalImageSize.Height > CIAT.Layout.StimulusSize.Height))
                CIAT.ImageManager[_IATImage].Resize(CIAT.Layout.StimulusSize);
            // success
            return true;
        }

        public static Size CalcImageSize(Size originalImageSize, bool SizeToFit)
        {
            Size szResult = new Size();
            if (SizeToFit || (originalImageSize.Width > CIAT.Layout.StimulusSize.Width) || (originalImageSize.Height > CIAT.Layout.StimulusSize.Height))
            {
                double AspectRatio = (double)originalImageSize.Width / (double)originalImageSize.Height;
                double SizedAR = (double)CIAT.Layout.StimulusSize.Width / (double)CIAT.Layout.StimulusSize.Height;
                if (SizedAR > AspectRatio)
                {
                    szResult.Width = (int)(CIAT.Layout.StimulusSize.Height * AspectRatio);
                    szResult.Height = CIAT.Layout.StimulusSize.Height;
                }
                else
                {
                    szResult.Width = CIAT.Layout.StimulusSize.Width;
                    szResult.Height = (int)((double)CIAT.Layout.StimulusSize.Width / (double)AspectRatio);
                }
            }
            else
                szResult = originalImageSize;
            return szResult;
        }

        protected override IATClient.ImageManager.ImageSizeCallback GetSizeCallback()
        {
            return CalcImageSize;
        }

    }
}
