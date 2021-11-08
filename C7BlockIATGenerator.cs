using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IATClient
{
    class C7BlockIATGenerator
    {
        private CIAT IAT;
        private const int stagger = 0;
        private readonly Uri ConjunctionUri = new DIConjunction().URI;

        public C7BlockIATGenerator(CIAT iat)
        {
            IAT = iat;
        }

        /// <summary>
        /// Generates a response key for the specified block in a seven block IAT.  Note: this function might fail if it it not
        /// called with for each value in order, meaning it should be called with arguments 3, 4, 5, 6, and 7 for nBlock in that order and
        /// in that order only
        /// </summary>
        /// <param name="nBlock">the IAT block to generate a response key for</param>
        /// <returns></returns>
        private CIATKey GenerateResponseKeyForBlock(int nBlock)
        {
            // find a unique name for the response key
            int n = 1;
            var keyNames = CIAT.SaveFile.GetAllIATKeyUris().Select(u => CIAT.SaveFile.GetIATKey(u)).Select(k => k.Name);
            while (keyNames.Contains(String.Format("Generated Key {0}", n)))
                n++;
            String keyName = String.Format("Generated Key {0}", n);
            CIATKey key = null;
            CIATDualKey dk = null;
            switch (nBlock)
            {
                case 3:
                    dk = new CIATDualKey() { Name = keyName };
                    dk.SuspendLayout();
                    dk.BaseKey1Uri = IAT.Blocks[0].Key.URI;
                    dk.BaseKey2Uri = IAT.Blocks[1].Key.URI;
                    dk.ConjunctionUri = ConjunctionUri;
                    dk.ResumeLayout(true);
                    return dk;

                case 4:
                    dk = new CIATDualKey() { Name = keyName };
                    dk.SuspendLayout();
                    dk.BaseKey1Uri = IAT.Blocks[0].Key.URI;
                    dk.BaseKey2Uri = IAT.Blocks[1].Key.URI;
                    dk.ConjunctionUri = ConjunctionUri;
                    dk.ResumeLayout(true);
                    return dk;

                case 5:
                    key = new CIATReversedKey(keyName, IAT.Blocks[1].Key.URI);
                    return key;

                case 6:
                    dk = new CIATDualKey() { Name = keyName };
                    dk.SuspendLayout();
                    dk.BaseKey1Uri = IAT.Blocks[0].Key.URI;
                    dk.BaseKey2Uri = IAT.Blocks[4].Key.URI;
                    dk.ConjunctionUri = ConjunctionUri;
                    dk.ResumeLayout(true);
                    return dk;

                case 7:
                    dk = new CIATDualKey() { Name = keyName };
                    dk.SuspendLayout();
                    dk.BaseKey1Uri = IAT.Blocks[0].Key.URI;
                    dk.BaseKey2Uri = IAT.Blocks[4].Key.URI;
                    dk.ConjunctionUri = ConjunctionUri;
                    dk.ResumeLayout(true);
                    return dk;
            }
            return null;
        }

        private void CopyItemsToBlock(CIATBlock dest, CIATBlock src, bool reverse)
        {
            for (int ctr = 0; ctr < src.NumItems; ctr++)
            {
                if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.None)
                    dest.AddItem(src[ctr], KeyedDirection.None);
                else if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.Left)
                    dest.AddItem(src[ctr], reverse ? KeyedDirection.Right : KeyedDirection.Left);
                else if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.Right)
                    dest.AddItem(src[ctr], reverse ? KeyedDirection.Left : KeyedDirection.Right);
                else if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.DynamicLeft)
                    dest.AddItem(src[ctr], reverse ? KeyedDirection.DynamicRight : KeyedDirection.DynamicLeft);
                else if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.DynamicRight)
                    dest.AddItem(src[ctr], reverse ? KeyedDirection.DynamicLeft : KeyedDirection.DynamicRight);
                else if (src[ctr].GetKeyedDirection(src.URI) == KeyedDirection.DynamicNone)
                    dest.AddItem(src[ctr], KeyedDirection.DynamicNone);
            }
        }
       

        public bool Generate(bool bAlternate)
        {

            IContentsItem cItem;

            // get least after-survey index
            int insertionNdx = int.MaxValue;
            if (IAT.AfterSurvey.Count == 0)
                insertionNdx = IAT.Contents.Count;
            else
                for (int ctr = 0; ctr < IAT.AfterSurvey.Count; ctr++)
                    if (insertionNdx > IAT.Contents.IndexOf(IAT.AfterSurvey[ctr]))
                        insertionNdx = IAT.Contents.IndexOf(IAT.AfterSurvey[ctr]);

            CIATBlock b3 = new CIATBlock(IAT);
            b3.Key = GenerateResponseKeyForBlock(3);
            CopyItemsToBlock(b3, IAT.Blocks[0], false);
            CopyItemsToBlock(b3, IAT.Blocks[1], false);
            b3.AddToIAT(insertionNdx++);

            CIATBlock b4 = new CIATBlock(IAT);
            b4.Key = GenerateResponseKeyForBlock(4);
            CopyItemsToBlock(b4, IAT.Blocks[0], false);
            CopyItemsToBlock(b4, IAT.Blocks[1], false);
            b4.AddToIAT(insertionNdx++);

            CIATBlock b5 = new CIATBlock(IAT);
            b5.Key = GenerateResponseKeyForBlock(5);
            CopyItemsToBlock(b5, IAT.Blocks[1], true);
            b5.AddToIAT(insertionNdx++);

            CIATBlock b6 = new CIATBlock(IAT);
            b6.Key = GenerateResponseKeyForBlock(6);
            CopyItemsToBlock(b6, IAT.Blocks[0], false);
            CopyItemsToBlock(b6, IAT.Blocks[1], true);
            b6.AddToIAT(insertionNdx++);

            // generate block #7
            CIATBlock b7 = new CIATBlock(IAT);
            b7.Key = GenerateResponseKeyForBlock(7);
            CopyItemsToBlock(b7, IAT.Blocks[0], false);
            CopyItemsToBlock(b7, IAT.Blocks[1], true);
            b7.AddToIAT(insertionNdx++);

            if (bAlternate)
            {
                new AlternationGroup(b3, b6);
                new AlternationGroup(b4, b7);
            }
            return true;
        }
    }
}
