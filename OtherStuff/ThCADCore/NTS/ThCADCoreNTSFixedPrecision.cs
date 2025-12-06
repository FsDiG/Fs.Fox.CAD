using System;

namespace ThCADCore.NTS
{
    public class ThCADCoreNTSFixedPrecision : IDisposable
    {
        public ThCADCoreNTSFixedPrecision()
        {
            ThCADCoreNTSService.Instance.PrecisionReduce = true;
        }

        public void Dispose()
        {
            ThCADCoreNTSService.Instance.PrecisionReduce = false;
        }
    }
}
