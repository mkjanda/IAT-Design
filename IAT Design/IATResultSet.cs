using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace IATClient.IATResultSet
{
    [Serializable]
    public class IATResultSetElement : ISerializable
    {
        private int _BlockNumber, _ItemNumber;
        private long _ResponseTime;

        public int BlockNumber
        {
            get
            {
                return _BlockNumber;
            }
            set
            {
                _BlockNumber = value;
            }
        }

        public int ItemNumber
        {
            get
            {
                return _ItemNumber;
            }
            set
            {
                _ItemNumber = value;
            }
        }

        public long ResponseTime
        {
            get
            {
                return _ResponseTime;
            }
            set
            {
                _ResponseTime = value;
            }
        }

        public IATResultSetElement()
        {
            _ItemNumber = -1;
            _ResponseTime = -1;
            _BlockNumber = -1;
        }

        protected IATResultSetElement(SerializationInfo info, StreamingContext context)
        {
            _ItemNumber = info.GetInt32("ItemNumber");
            _ResponseTime = info.GetInt64("ResponseTime");
            _BlockNumber = info.GetInt32("BlockNumber");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public virtual void GetObjectData(SerializationInfo info,  StreamingContext context)
        {
            info.AddValue("ItemNumber", ItemNumber, typeof(Int32));
            info.AddValue("ResponseTime", ResponseTime, typeof(Int32));
            info.AddValue("BlockNumber", BlockNumber, typeof(Int32));
        }
    }

    public class SurveyResponseSet : ISerializable
    {
        private string[] _SurveyResult = null;

        public string[] SurveyResult
        {
            get
            {
                return _SurveyResult;
            }
            set
            {
                _SurveyResult = value;
            }
        }

        protected SurveyResponseSet(SerializationInfo info, StreamingContext context)
        {
            SurveyResult = (string[])info.GetValue("SurveyResults", typeof(string[]));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SurveyResults", SurveyResult, typeof(string[]));
        }
    }

    public class IATResultSet : ISerializable
    {
        private SurveyResponseSet _BeforeSurveyResults, _AfterSurveyResults;
        private IATResultSetElement _IATResults;

        public SurveyResponseSet BeforeSurveyResults
        {
            get
            {
                return _BeforeSurveyResults;
            }
            set
            {
                _BeforeSurveyResults = value;
            }
        }

        public SurveyResponseSet AfterSurveyResults
        {
            get
            {
                return _AfterSurveyResults;
            }
            set
            {
                _AfterSurveyResults = value;
            }
        }

        public IATResultSetElement IATResults
        {
            get
            {
                return _IATResults;
            }
            set
            {
                _IATResults = value;
            }
        }

        public IATResultSet()
        {
            _BeforeSurveyResults = null;
            _AfterSurveyResults = null;
            _IATResults = null;
        }

        protected IATResultSet(SerializationInfo info, StreamingContext context)
        {
            _BeforeSurveyResults = (SurveyResponseSet)info.GetValue("BeforeSurveyResults", typeof(SurveyResponseSet));
            _AfterSurveyResults = (SurveyResponseSet)info.GetValue("AfterSurveyResults", typeof(SurveyResponseSet));
            _IATResults = (IATResultSetElement)info.GetValue("IATResults", typeof(IATResultSetElement));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("BeforeSurveyResults", typeof(SurveyResponseSet));
            info.AddValue("AfterSurveyResults", typeof(SurveyResponseSet));
            info.AddValue("IATResults", typeof(IATResultSetElement));
        }
    }

    public class IATResultSetList : ISerializable
    {
        private IATResultSet[] _IATResultSets = null;
        private int _NumResultSets = -1;

        public IATResultSet[] IATResultSets
        {
            get
            {
                return _IATResultSets;
            }
            set
            {
                _IATResultSets = value;
            }
        }

        public int NumResultSets
        {
            get
            {
                return _NumResultSets;
            }
            set
            {
                _NumResultSets = value;
            }
        }

        protected IATResultSetList(SerializationInfo info, StreamingContext context)
        {
            _IATResultSets = (IATResultSet[])info.GetValue("IATResults", typeof(IATResultSet[]));
            _NumResultSets = info.GetInt32("NumResultSets");
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IATResults", IATResultSets, typeof(IATResultSet[]));
            info.AddValue("NumResultSets", NumResultSets, typeof(Int32));
        }
    }
}
