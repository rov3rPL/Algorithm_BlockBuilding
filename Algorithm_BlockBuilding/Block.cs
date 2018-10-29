using System.Collections.Generic;

namespace Algorithm_BlockBuilder
{
    internal class Block
    {
        //Keep array?
        public ServiceVariantSlot[] ServiceVariantSlots { get; set; }

        private int _spareTime = int.MinValue;
        public int SpareTime { 
            get {
                if (_spareTime >= 0) return _spareTime;

                int cumulatedSpareTime = 0;

                for(int i=1; i<ServiceVariantSlots.Length; ++i)
                {
                    cumulatedSpareTime += ServiceVariantSlots[i].StartTime - ServiceVariantSlots[i - 1].EndTime;                    
                }

                _spareTime = cumulatedSpareTime;
                return _spareTime;
            }
        }


    }
}
