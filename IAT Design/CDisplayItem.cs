using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace IATClient
{
    [Serializable]
    public abstract class CDisplayItem : IDisposable, IComponentImageSource
    {
        public abstract bool IsValid { get; }
        public abstract void Validate();
        public abstract void Invalidate();
        public abstract CComponentImage.ESourceType SourceType { get; }
        /// <summary>
        /// An enumeration of the types of display items
        /// </summary>
        public enum EType { stimulusImage, responseKeyImage, text, dualKey, memoryImage, multiLineTextDisplayItem, responseKeyText, dualKeyTextComponent };

        // constant strings to represent the item types
        protected const String sImage = "Image";
        protected const String sText = "Text";
        protected const String sDualKey = "DualKey";
        protected const String sMemoryImage = "MemoryImage";
        protected const String sResponseKeyImage = "ResponseKeyImage";

        private readonly object lockObject = new object();

        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }

        protected void Lock()
        {
            Monitor.Enter(lockObject);
        }

        protected void Unlock()
        {
            Monitor.PulseAll(lockObject);
            Monitor.Exit(lockObject);
        }

        protected bool TryLock()
        {
            return Monitor.TryEnter(lockObject);
        }

        public virtual bool ImageExists
        {
            get
            {
                return true;
            }
        }

        public abstract IIATImage IATImage { get; }

        // the type of the display item
        private EType _Type;

        /// <summary>
        /// gets the type of display item
        /// </summary>
        public EType Type
        {
            get
            {
                return _Type;
            }
        }

        public Size ItemSize
        {
            get
            {
                return GetItemSize();
            }
        }
/*
        // the bounding rectangle of the display item
        private Rectangle _Bounds;

        /// <summary>
        /// gets or sets the bounding rectangle of the display item
        /// </summary>
        public Rectangle Bounds
        {
            get
            {
                return _Bounds;
            }
            set
            {
                _Bounds = value;
            }
        }
*/
        
        public CDisplayItem(EType type)
        {
            _Type = type;
        }

        /// <summary>
        /// Determines if the object's data is valid
        /// </summary>
        /// <returns>"true" if the object's data is valid, otherwise "false"</returns>
        public abstract bool IsDefined();

        public abstract void Load(XElement elem);
        public abstract void WriteToXml(XmlTextWriter writer);
        public abstract bool LoadFromXml(XmlNode node);

        /// <summary>
        /// Displays the object at the specified location using the given graphics context
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="location">The location to display the object</param>
        /// <returns>"true" on success, otherwise "false"</returns>
//        public abstract bool Display(Graphics g, Point location);

        
        
        /// <summary>
        /// Calculates the size of the item and returns it
        /// </summary>
        /// <returns>The size of the item when displayed</returns>
        protected abstract Size GetItemSize();

        public Size UnscaledItemSize
        {
            get
            {
                Size sz = GetItemSize();
                return new Size((int)((double)sz.Width / CIATLayout.XScale), (int)(double)(sz.Height / CIATLayout.YScale));
            }
        }
        

        /// <summary>
        /// Disposes of any resources used by the object
        /// </summary>
        public virtual void Dispose()
        {
            Lock();
            if (IATImageID != -1)
            {
                if (IATImage.IsUserImage)
                    ((IUserImage)IATImage).Dispose();
                else
                    ((INonUserImage)IATImage).Dispose((ImageManager.INonUserImageSource)this);
                _IATImage = -1;
            }
            Unlock();
        }

        /// <summary>
        /// Creates a display item from the passed XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>a new CDisplayItem object on success, otherwise "null"</returns>
        public static CDisplayItem CreateFromXml(XmlNode node, Type senderType)
        {
            String sType = node.Attributes["Type"].Value;
            CDisplayItem displayItem = null;
            
            switch (sType)
            {
                case sImage:
                    displayItem = new CStimulusImageItem();
                    break;

                case sText:
                    if (senderType == typeof(CIATItem))
                        displayItem = new CTextDisplayItem(CTextDisplayItem.EUsedAs.stimulus);
                    else if (senderType == typeof(CIATDualKey))
                        displayItem = new CDualKeyTextComponent();
                    else if (senderType == typeof(CIATKey))
                        displayItem = new CResponseKeyTextItem(CTextDisplayItem.EUsedAs.responseKey);
                    break;

                case sResponseKeyImage:
                    displayItem = new CResponseKeyImage();
                    break;

                default:
                    return null;
            }
            displayItem.LoadFromXml(node);
            return displayItem;
        }

        public static void WriteEmptyElement(XmlTextWriter writer)
        {
            writer.WriteStartElement("DisplayItem");
            writer.WriteAttributeString("Type", "None");
            writer.WriteEndElement();
        }
    }
}
