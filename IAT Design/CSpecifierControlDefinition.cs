using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    public class CSpecifierControlDefinition
    {
        private CDynamicSpecifier.ESpecifierType _SpecifierType;
        private List<String> _Values;
        private List<String> _Statements;

        public CDynamicSpecifier.ESpecifierType SpecifierType
        {
            get
            {
                return _SpecifierType;
            }
            set
            {
                _SpecifierType = value;
            }
        }

        public List<String> Statements
        {
            get
            {
                return _Statements;
            }
        }

        public List<String> Values
        {
            get
            {
                return _Values;
            }
        }

        public CSpecifierControlDefinition(CDynamicSpecifier.ESpecifierType type)
        {
            _SpecifierType = type;
            _Statements = new List<String>();
            _Values = new List<String>();
        }
    }
}
