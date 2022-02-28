using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace IATClient
{
    public class ContentsList : IList<IContentsItem>
    {
        private List<Tuple<ContentsItemType, Uri>> ItemUris = new List<Tuple<ContentsItemType, Uri>>();

        public IContentsItem this[int ndx]
        {
            get
            {
                return ItemUris[ndx].Item1.Fetch(ItemUris[ndx].Item2);
            }
            set
            {
                ItemUris[ndx] = new Tuple<ContentsItemType, Uri>(value.Type, value.URI);
            }
        }

        public int IndexOf(IContentsItem ci)
        {
            Tuple<ContentsItemType, Uri> tuple = ItemUris.Where(tup => tup.Item2.Equals(ci.URI)).FirstOrDefault();
            if (tuple == null)
                return -1;
            return ItemUris.IndexOf(tuple);
        }

        public void RemoveAt(int ndx)
        {
            ItemUris.RemoveAt(ndx);
        }

        public void Insert(int ndx, IContentsItem ci)
        {
            ItemUris.Insert(ndx, new Tuple<ContentsItemType, Uri>(ci.Type, ci.URI));
        }

        public void Insert(int ndx, ContentsItemType ciType, Uri uri)
        {
            ItemUris.Insert(ndx, new Tuple<ContentsItemType, Uri>(ciType, uri));
        }

        public IEnumerator<IContentsItem> GetEnumerator()
        {
            return ItemUris.Select(tup => tup.Item1.Fetch(tup.Item2)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Remove(IContentsItem ci)
        {
            try
            {
                ItemUris.Remove(ItemUris.Where(tup => tup.Item2.Equals(ci.URI)).First());
                return true;
            }
            catch (ArgumentException)
            {
                return false;

            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int Count
        {
            get
            {
                return ItemUris.Count;
            }
        }

        public void CopyTo(IContentsItem[] ary, int i)
        {
            throw new NotImplementedException();
        }

        public bool Contains(IContentsItem ci)
        {
            return ItemUris.Select(tup => tup.Item2).Contains(ci.URI);
        }

        public void Clear()
        {
            ItemUris.Clear();
        }

        public void Add(IContentsItem ci)
        {
            ItemUris.Add(new Tuple<ContentsItemType, Uri>(ci.Type, ci.URI));
        }

        public void Add(ContentsItemType type, Uri uri)
        {
            ItemUris.Add(new Tuple<ContentsItemType, Uri>(type, uri));
        }
    }
}
