using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Forms;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    interface ISearchCriterion : INamedXmlSerializable
    {
        bool IsSearchMatch(String testVal);
    }

    interface IResultSearch
    {
        bool Test(IResultSet resultSet);
    }


    enum ETestConcatination { unset, all, none, any, notAll };
    public delegate bool MatchTest(String value, String testValue);




    class CSingletonSearch : IResultSearch, INamedXmlSerializable
    {
        ISearchCriterion SearchCriterion;
        CSearchValue TestValueExtractor;

        public CSingletonSearch(ISearchCriterion searchCriterion, CSearchValue testValueExtractor)
        {
            SearchCriterion = searchCriterion;
            TestValueExtractor = testValueExtractor;
        }

        public bool Test(IResultSet resultSet)
        {
            return SearchCriterion.IsSearchMatch(TestValueExtractor.ExtractValue(resultSet));
        }

        public String GetName()
        {
            return "SingletonSearch";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            SearchCriterion.WriteXml(writer);
            TestValueExtractor.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {

        }

    }
    
    class CSimpleSearch
    {
        private IResultSearch Search;

        public CSimpleSearch(IResultSearch search)
        {
            Search = search;
        }

        public List<int> SearchResultSet(IResultData resultSets)
        {
            List<int> matchedIndicies = new List<int>();
            for (int ctr = 0; ctr < resultSets.NumResultSets; ctr++)
                if (Search.Test(resultSets[ctr]))
                    matchedIndicies.Add(ctr);
            return matchedIndicies;
        }
    }
    
    class CSearchLeg : IResultSearch
    {
        private List<IResultSearch> Searches = new List<IResultSearch>();
        private ETestConcatination ConcatMode;


        public CSearchLeg()
        {
            ConcatMode = ETestConcatination.unset;
        }

        public ETestConcatination ConcatinationMode
        {
            get
            {
                return ConcatMode;
            }
            set
            {
                ConcatMode = value;
            }
        }

        public CSearchLeg(ETestConcatination concatMode)
        {
            ConcatMode = concatMode;
        }


        public bool Test(IResultSet resultSet)
        {
            if (Searches.Count == 1)
                return Searches[0].Test(resultSet);
            switch (ConcatMode)
            {
                case ETestConcatination.all:
                    foreach (IResultSearch rs in Searches)
                        if (!rs.Test(resultSet))
                            return false;
                    return true;

                case ETestConcatination.any:
                    foreach (IResultSearch rs in Searches)
                        if (rs.Test(resultSet))
                            return true;
                    return false;

                case ETestConcatination.none:
                    foreach (IResultSearch rs in Searches)
                        if (rs.Test(resultSet))
                            return false;
                    return true;

                case ETestConcatination.notAll:
                    foreach (IResultSearch rs in Searches)
                        if (!rs.Test(resultSet))
                            return true;
                    return false;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
