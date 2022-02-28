using System;

namespace IATClient
{
    public class Enumeration : IComparable
    {
        public readonly int ID;
        public readonly String Name;

        protected Enumeration() { }
        public Enumeration(int id, String name)
        {
            this.ID = id;
            this.Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != GetType())
                return false;
            if ((ID == (obj as Enumeration).ID) && (Name == (obj as Enumeration).Name))
                return true;
            return false;
        }

        public int CompareTo(object obj)
        {
            return ID.CompareTo((obj as Enumeration).ID);
        }
    }
}
