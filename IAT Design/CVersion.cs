using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace IATClient
{
    public class CVersion
    {
        private int Release, Major, Minor, Trivial;

        private void ParseVersion(String str)
        {
            Regex regEx = new Regex("([0-9]+)\\.([0-9]+)\\.([0-9]+)\\.([0-9]+)");
            Match match = regEx.Match(str);
            Release = Convert.ToInt32(match.Groups[1].Value);
            Major = Convert.ToInt32(match.Groups[2].Value);
            Minor = Convert.ToInt32(match.Groups[3].Value);
            Trivial = Convert.ToInt32(match.Groups[4].Value);
        }

        public CVersion()
        {
            String version = LocalStorage.Activation[LocalStorage.Field.Version];
            ParseVersion(version);
        }

        public CVersion(String version)
        {
            ParseVersion(version);
        }

        static public int Compare(CVersion v1, CVersion v2)
        {
            if (v1.Release != v2.Release)
                return v2.Release - v1.Release;
            if (v1.Major != v2.Major)
                return v2.Major - v1.Major;
            if (v1.Minor != v2.Minor)
                return v2.Minor - v1.Minor;
            if (v1.Trivial != v2.Trivial)
                return v2.Trivial - v1.Trivial;
            return 0;
        }

        public int CompareTo(CVersion v)
        {
            if (Release != v.Release)
                return Release - v.Release;
            if (Major != v.Major)
                return Major - v.Major;
            if (Minor != v.Minor)
                return Minor - v.Minor;
            if (Trivial != v.Trivial)
                return Trivial - v.Trivial;
            return 0;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}.{3}", Release, Major, Minor, Trivial);
        }
    }
}
