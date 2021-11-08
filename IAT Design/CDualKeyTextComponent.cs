using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    public class CDualKeyTextComponent : CTextDisplayItem
    {
        private CIATDualKey DualKey;

        public CIATDualKey OwningKey
        {
            get
            {
                return DualKey;
            }
            set
            {
                DualKey = value;
            }
        }

        public CDualKeyTextComponent()
            : base(EUsedAs.conjunction)
        {
            DualKey = null;
        }

        public CDualKeyTextComponent(CIATDualKey dualKey) : base(EUsedAs.conjunction) 
        {
            DualKey = dualKey;
        }

        public CDualKeyTextComponent(CDualKeyTextComponent o, CIATDualKey key) : base(o, EType.dualKeyTextComponent)
        {
            DualKey = key;
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }
    }
}
