using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace IATClient
{
    public class CIATBlock : IStoredInXml, IContentsItem, IValidatedItem, IDisposable
    {
        protected CIAT IAT;
        private bool _IsDisposed = false;
        private int _NumPresentations;
        protected AlternationGroup AltGroup = null;
        protected String _Name = String.Empty;
        protected bool _IsPracticeBlock;
        protected int _IndexInContents = -1;
        private bool _IsDynamicallyKeyed = false;
        private Dictionary<CIATItem, CCompositeImage> CompositeImageDictionary;
        private CIATItem CurrentlyOpenItem = null;
        private Size CompositeImageSize = Size.Empty;
        public delegate int BlockIndexRetriever(CIATBlock block);
        public delegate bool IsPracticeBlockResolver(CIATBlock block);
        public BlockIndexRetriever GetIndex;
        public delegate IATConfigFileNamespace.ConfigFile.ERandomizationType RandomizationTypeResolver();
        public RandomizationTypeResolver GetRandomizationType;
        private CIATItem dummyItem = new CIATItem();

        public bool IsHeaderItem { get { return true; } }

        public int NumItems
        {
            get
            {
                return Items.Count;
            }
        }
        public bool IsDynamicallyKeyed
        {
            get
            {
                return _IsDynamicallyKeyed;
            }
            set
            {
                _IsDynamicallyKeyed = value;
            }
        }


        public bool IsPracticeBlock
        {
            get
            {
                return _IsPracticeBlock;
            }
        }

        /// <summary>
        /// gets or sets the block that is administered alternately with this block
        /// </summary>
        public CIATBlock AlternateBlock
        {
            get
            {
                if (!HasAlternateItem)
                    return null;
                if (AlternationGroup.GroupMembers[0] == this)
                    return (CIATBlock)AlternationGroup.GroupMembers[1];
                else
                    return (CIATBlock)AlternationGroup.GroupMembers[0];
            }
        }

        /// <summary>
        /// gets or sets the number of presentations for this block -- note this value only has meaning if the randomization type of 
        /// the IAT is set to "SetNumberOfPresentations"
        /// </summary>
        public int NumPresentations
        {
            get
            {
                if (GetRandomizationType() == IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations)
                {
                    if (_NumPresentations == -1)
                        _NumPresentations = _Items.Count;
                    return _NumPresentations;
                }
                else
                    return _Items.Count;
            }
            set
            {
                _NumPresentations = value;
            }
        }

        // a list of keys for the IAT Block
        private String KeyName;

        /// <summary>
        /// gets the key for the IAT block
        /// </summary>
        public CIATKey Key
        {
            get
            {
                if (KeyName == String.Empty)
                    return null;
                return CIATKey.KeyDictionary[KeyName];
            }
            set
            {
                if (value == null)
                    KeyName = String.Empty;
                else
                    KeyName = value.Name;
            }
        }

        // a list of items for the IAT block
        private List<CIATItem> _Items;

        /// <summary>
        /// gets the list of items for the IAT block
        /// </summary>
        protected List<CIATItem> Items
        {
            get
            {
                return _Items;
            }
        }

        public CIATItem this[int ndx]
        {
            get
            {

                if ((ndx < 0) || (ndx >= Items.Count))
                    return null;
                return Items[ndx];
            }
        }

        public bool Contains(CIATItem i)
        {
            return Items.Contains(i);
        }

        // the instructions for the IAT block
        private CMultiLineTextDisplayItem _Instructions;

        public CMultiLineTextDisplayItem Instructions
        {
            get
            {
                return _Instructions;
            }
            set
            {
                _Instructions = value;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CIATBlock(CIAT iat, bool isPracticeBlock)
        {
            _Items = new List<CIATItem>();
            Key = null;
            _Instructions = new CMultiLineTextDisplayItem(CMultiLineTextDisplayItem.EUsedAs.iatBlockInstructions);
            _NumPresentations = -1;
            GetIndex = new BlockIndexRetriever(iat.GetBlockIndex);
            GetRandomizationType = new RandomizationTypeResolver(iat.GetRandomizationType);
            IAT = iat;
            _IsPracticeBlock = isPracticeBlock;
            dummyItem.ParentBlock = this;
            ConstructCompositeImageDictionary(CIAT.Layout.InteriorSize);
        }

        public void SizeCompositeImages(CIATLayout layout)
        {
            foreach (CCompositeImage ci in CompositeImageDictionary.Values)
            {
                ci.SetFinalSize(layout.InteriorSize);
            }
            foreach (CIATItem i in Items)
                i.ResizeStimulus();
        }
        /*
        public CIATBlock(CIATBlock o)
        {
            _Items = new List<CIATItem>();
            foreach (CIATItem i in o._Items)
                _Items.Add(new CIATItem(i));
            _Key = o._Key;
            _Instructions = new CMultiLineTextDisplayItem(o._Instructions);
            _NumPresentations = o._NumPresentations;
            GetIndex = new BlockIndexRetriever(o.IAT.GetBlockIndex);
            GetRandomizationType = new RandomizationTypeResolver(o.IAT.Packager.GetRandomizationType);
            IAT = o.IAT;
            _IsPracticeBlock = o.IsPracticeBlock;
            _Name = o.Name;
            foreach (CIATItem i in Items)
                i.ParentBlock = this;
            dummyItem.ParentBlock = this;
        }
/*
        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="o">The object to be copied</param>
        public CIATBlock(CIATBlock o)
        {
            _Items = new List<CIATItem>();
            for (int ctr = 0; ctr < o.Items.Count; ctr++)
            {
                CIATItem i = new CIATItem(o.Items[ctr]);
                _Items.Add(i);
            }
            _Key = o.Key;
            _Instructions = new CMultiLineTextDisplayItem(o.Instructions);
            _NumPresentations = o._NumPresentations;
            _AlternateBlock = o._AlternateBlock;
            GetIndex = o.GetIndex;
        }
*/
        /// <summary>
        /// Resizes the instructions in the event of a layout change
        /// </summary>
        /// 
        
        public void ResizeInstructions()
        {
            _Instructions.InvalidateComponentImage();
        }
        
        /// <summary>
        /// Determines if the object's data is valid
        /// </summary>
        /// <returns>"true" if the object contains valid data, otherwise "false"</returns>
      /*
        public bool IsValid()
        {
            if (_Key == null)
            {
                throw new ValidationException(Properties.Resources.sBlockLacksResponseKey, 1, ValidationException.EType.Block);
            }
            for (int ctr = 0; ctr < Items.Count; ctr++)
            {
                try 
                {
                    Items[ctr].IsValid();
                }
                catch (ValidationException ex)
                {
                    ex.SetArg(0, (ctr + 1).ToString());
                    throw ex;
                }
            }
            return true;
        }
        */
        public void Validate()
        {
            if (Key == null)
                throw new Exception(Properties.Resources.sNoKeyAssignedToBlockException);
            for (int ctr = 0; ctr < Items.Count; ctr++)
                Items[ctr].Validate(ctr);
        }

        public void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            CLocationDescriptor loc = new CIATBlockLocationDescriptor(this, null);
            if (Key == null)
                ErrorDictionary.Add(this, new CValidationException(EValidationException.BlockResponseKeyUndefined, loc));
            foreach (CIATItem i in Items)
                i.ValidateItem(ErrorDictionary);
        }

        /// <summary>
        /// Writes the object's data to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public void WriteToXml(XmlTextWriter writer)
        {
            // write the start of the block element
            writer.WriteStartElement("IATBlock");

            // write the key name
            if (Key != null)
                writer.WriteElementString("KeyName", Key.Name);
            else
                writer.WriteElementString("KeyName", String.Empty);

            writer.WriteElementString("IsDynamicallyKeyed", IsDynamicallyKeyed.ToString());

            // write the instructions
            Instructions.WriteToXml(writer);

            // write the items
            writer.WriteStartElement("Items");
            writer.WriteAttributeString("NumItems", Items.Count.ToString());
            for (int ctr = 0; ctr < Items.Count; ctr++)
                Items[ctr].WriteToXml(writer);
            writer.WriteEndElement();

            // write contents index
            writer.WriteElementString("ContentsIndex", IndexInContents.ToString());

            writer.WriteElementString("Name", Name);

            // close the "IATBlock" element
            writer.WriteEndElement();
        }

        public bool LoadFromXml(XmlNode node)
        {
            ClearCompositeImageDictionary();
            // ensure the correct type of node
            if (node.Name != "IATBlock")
                return false;
            int nodeCtr = 0;
            // ensure the correct number of child nodes

            // get the key 
            KeyName = node.ChildNodes[nodeCtr++].InnerText;

            if (node.ChildNodes.Count == 6)
                _IsDynamicallyKeyed = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            else
                _IsDynamicallyKeyed = false;

            // get the instructions
            _Instructions.LoadFromXml(node.ChildNodes[nodeCtr++]);

            // get the items
            _Items.Clear();
            for (int ctr = 0; ctr < node.ChildNodes[nodeCtr].ChildNodes.Count; ctr++)
            {
                CIATItem item = new CIATItem();
                item.LoadFromXml(node.ChildNodes[nodeCtr].ChildNodes[ctr]);
                AddItem(item);
            }
            nodeCtr++;

            _IndexInContents = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            _Name = node.ChildNodes[nodeCtr++].InnerText;
            return true;
        }
/*
        public void ResolveAlternateBlock(List<CIATBlock> BlockList)
        {
            if (AlternateBlockIndex == -1)
                AlternateBlock = null;
            else
                AlternateBlock = BlockList[AlternateBlockIndex - 1];
        }
*/
        public ContentsItemType Type
        {
            get
            {
                if (!IsPracticeBlock)
                    return ContentsItemType.IATBlock;
                return ContentsItemType.IATPracticeBlock;
            }
        }

        public bool HasAlternateItem
        {
            get
            {
                return (AltGroup != null);
            }
        }

        public AlternationGroup AlternationGroup
        {
            get
            {
                return AltGroup;
            }
            set
            {
                if ((AltGroup != null) && (value != null))
                    throw new Exception("Dispose of the alternation group and instantiate a new one.  Do not try to change the value of a IContentsItem alternation group.");
                AltGroup = value;
            }
        }

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public int IndexInContainer
        {
            get
            {
                if (Type == ContentsItemType.IATPracticeBlock)
                    return IAT.PracticeBlocks.IndexOf(this);
                else
                    return IAT.Blocks.IndexOf(this);
            }
        }

        public void DeleteFromIAT()
        {
            IAT.Contents.Remove(this);
            if (Type == ContentsItemType.IATPracticeBlock)
                IAT.PracticeBlocks.Remove(this);
            else
                IAT.Blocks.Remove(this);
        }

        public void AddToIAT(int InsertionNdx)
        {
            int containerNdx = 0;
            for (int ctr = 0; ctr < InsertionNdx; ctr++)
                if (IAT.Contents[ctr].Type == Type)
                    containerNdx++;
            if (Type == ContentsItemType.IATPracticeBlock)
            {
                Name = "IAT Practice Block #" + (containerNdx + 1).ToString();
                IAT.PracticeBlocks.Insert(containerNdx, this);
            }
            else
            {
                IAT.Blocks.Insert(containerNdx, this);
                Name = "IAT Block #" + (containerNdx + 1).ToString();
            }
            IAT.Contents.Insert(InsertionNdx, this);
        }

        public int IndexInContents
        {
            get
            {
                if (IAT.Contents.Contains(this))
                    return IAT.Contents.IndexOf(this);
                return _IndexInContents;
            }
        }

        public void ResolveSpecifiers()
        {
            foreach (CIATItem i in Items)
                if (i.KeySpecifierID != -1)
                    CDynamicSpecifier.GetSpecifier(i.KeySpecifierID).AddIATItem(i, i.SpecifierArg);
        }

        private void ConstructCompositeImageDictionary(Size compositeImageSize)
        {
            CompositeImageSize = compositeImageSize;
            CompositeImageDictionary = new Dictionary<CIATItem, CCompositeImage>();
            foreach (CIATItem i in Items)
            {
                CCompositeImage ci = new CCompositeImage(CIAT.Layout.InteriorSize, compositeImageSize, false, i);
                ci.InvalidateSource(false);
                CompositeImageDictionary[i] = ci;
            }
            CCompositeImage dummyCI = new CCompositeImage(CIAT.Layout.InteriorSize, compositeImageSize, false, dummyItem);
            dummyCI.ForceGenerate();
            CompositeImageDictionary[dummyItem] = dummyCI;

        }

        public void FinalizeCompositeImageDictionary()
        {
            ParameterizedThreadStart proc = new ParameterizedThreadStart(FinalizeCompositeImageDictionaryProc);
            List<CCompositeImage> cImages = new List<CCompositeImage>(CompositeImageDictionary.Values);
            Thread th = new Thread(proc);
            th.Start(cImages);
        }

        private void FinalizeCompositeImageDictionaryProc(object arg)
        {
            List<CCompositeImage> cImages = (List<CCompositeImage>)arg;
            foreach (CCompositeImage ci in cImages)
                ci.ForceGenerateProc();
        }

        private void ClearCompositeImageDictionary()
        {
            if (CompositeImageDictionary == null)
                return;
            CloseItemForEditing();
            foreach (CIATItem i in CompositeImageDictionary.Keys)
            {
                if (i != dummyItem)
                {
                    CompositeImageDictionary[i].Dispose();
                }
            }
            CCompositeImage ci = CompositeImageDictionary[dummyItem];
            CompositeImageDictionary.Clear();
            CompositeImageDictionary[dummyItem] = ci;
        }

        public void OpenItemForEditing(CIATItem i, Control PreviewWin, CCompositeImage.ImageGeneratedHandler callback)
        {
            if (CurrentlyOpenItem != null)
                CloseItemForEditing();
            CurrentlyOpenItem = i;
            CompositeImageDictionary[i].OpenForEditing(PreviewWin, callback);
        }

        public void CloseItemForEditing()
        {
            if (CurrentlyOpenItem == null)
                return;
            CompositeImageDictionary[CurrentlyOpenItem].CloseForEditing();
/*            CompositeImageDictionary[CurrentlyOpenItem].UpdateLock();
            while (!CompositeImageDictionary[CurrentlyOpenItem].Halted)
                CompositeImageDictionary[CurrentlyOpenItem].UpdateWait();
            CompositeImageDictionary[CurrentlyOpenItem].UpdateUnlock();
 */
            CurrentlyOpenItem = null;
        }

        public void InvalidateCompositeImages(bool bForceGenerate)
        {
            foreach (CCompositeImage ci in CompositeImageDictionary.Values)
                ci.InvalidateSource(true);
            if (bForceGenerate)
                CompositeImageDictionary[dummyItem].ForceGenerate();
        }

        public void RemoveItemAt(int ndx)
        {
            CIATItem i = Items[ndx];
            if (CompositeImageDictionary != null)
            {
                if (CurrentlyOpenItem == i)
                    CloseItemForEditing();
                CompositeImageDictionary.Remove(i);
            }
            Items.Remove(i);
        }

        public void RemoveItem(CIATItem i)
        {
            if (CompositeImageDictionary != null)
            {
                if (CurrentlyOpenItem == i)
                    CloseItemForEditing();
                CompositeImageDictionary.Remove(i);
            }
            Items.Remove(i);
        }

        public void AddItem(CIATItem i)
        {
            i.ParentBlock = this;
            if (CompositeImageDictionary != null)
            {
                CCompositeImage ci = new CCompositeImage(CIAT.Layout.InteriorSize, CompositeImageSize, false, i);
                CompositeImageDictionary[i] = ci;
            }
            Items.Add(i);    
        }

        public void MoveItem(int startNdx, int endNdx)
        {
            CIATItem i = Items[startNdx];
            Items.Remove(i);
            if (startNdx < endNdx)
                Items.Insert(endNdx, i);
            else
                Items.Insert(endNdx, i);
        }


        public void InsertItem(int ndx, CIATItem item)
        {
            item.ParentBlock = this;
            if (CompositeImageDictionary != null)
            {
                CCompositeImage ci = new CCompositeImage(CIAT.Layout.InteriorSize, CompositeImageSize, false, item);
                CompositeImageDictionary[item] = ci;
            }
            Items.Insert(ndx, item);
        }

        public void ClearItems()
        {
            ClearCompositeImageDictionary();
            Items.Clear();
        }

        public int GetItemIndex(CIATItem item)
        {
            return Items.IndexOf(item);
        }

        public void GeneratePreview(Panel previewPanel)
        {
            if (!CompositeImageDictionary[dummyItem].IsValid)
                CompositeImageDictionary[dummyItem].ForceGenerateProc();
            Image img = CompositeImageDictionary[dummyItem].CloneCompositeImage();
            Graphics g = Graphics.FromImage(img);
            Font stimFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18);
            String str = String.Empty;
            if (Items.Count == 0)
                str = "No Stimuli";
            else if (Items.Count == 1)
                str = "1 Stimulus";
            else
                str = String.Format("{0} Stimuli", Items.Count);
            SizeF szStr = g.MeasureString(str, stimFont);
            PointF ptDraw = new PointF((img.Width - szStr.Width) / 2, (img.Height - szStr.Height) / 2);
            double ar = (double)previewPanel.Width / (double)previewPanel.Height;
            g.DrawString(str, stimFont, Brushes.White, ptDraw);
            str = Name;
            szStr = g.MeasureString(str, stimFont);
            if (ar > 1)
                ptDraw.Y += (float)(szStr.Height / ar);
            ptDraw.X = (float)(img.Width - szStr.Width);
            g.DrawString(str, stimFont, Brushes.White, new PointF(((img.Width - szStr.Width) / 2), ptDraw.Y) - new SizeF(0, (2 * szStr.Height)));
            stimFont.Dispose();
            g.Dispose();
            previewPanel.BackgroundImage = img;
        }

        public void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = this;
            mainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
        }

        public List<IPreviewableItem> SubContentsItems
        {
            get
            {
                List<IPreviewableItem> result = new List<IPreviewableItem>();
                result.AddRange(Items.ToArray());
                return result;
            }
        }

        public Image GetItemPreview(CIATItem item)
        {
            CompositeImageDictionary[item].UpdateLock();
            if (!CompositeImageDictionary[item].IsValid)
                CompositeImageDictionary[item].ForceGenerateProc();
            Image img = CompositeImageDictionary[item].CloneCompositeImage();
            CompositeImageDictionary[item].UpdateUnlock();
            return img;
        }

        public String PreviewText
        {
            get
            {
                return Name;
            }
        }

        public void Dispose()
        {
            ClearCompositeImageDictionary();
            CompositeImageDictionary[dummyItem].Dispose();
            dummyItem.Dispose();
            foreach (CIATItem i in Items)
                i.Dispose();
            Items.Clear();
            _IsDisposed = true;
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }

        private Button _GUIButton = null;

        public Button GUIButton
        {
            get
            {
                return _GUIButton;
            }
            set
            {
                _GUIButton = value;
            }
        }
    }
}
