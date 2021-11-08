using System;

namespace IATClient
{
    public partial class ItemSlideManifest
    {
        public uint[] GetItemIDs(String filename)
        {
            for (int ctr = 0; ctr < ItemSlideEntries.Length; ctr++)
                if (filename == ItemSlideEntries[ctr].SlideFileName)
                    return ItemSlideEntries[ctr].Items;
            return null;
        }

        public String GetSlideFile(int id)
        {
            for (int ctr = 0; ctr < ItemSlideEntries.Length; ctr++)
                for (int ctr2 = 0; ctr2 < ItemSlideEntries[ctr].Items.Length; ctr2++)
                    if (id == ItemSlideEntries[ctr].Items[ctr2])
                        return ItemSlideEntries[ctr].SlideFileName;
            return String.Empty;
        }
    }
}