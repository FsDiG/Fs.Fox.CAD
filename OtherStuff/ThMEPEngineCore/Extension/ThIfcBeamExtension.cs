using System;
using ThMEPEngineCore.Model;
using ThMEPEngineCore.Service;


namespace ThMEPEngineCore.Extension
{
    public static class ThIfcBeamExtension
    {
        public static void ExtendBoth(this ThIfcBeam beam, double startExtendLength, double endExtendLength)
        {
            if (beam is ThIfcLineBeam lineBeam)
            {
                var outliner = new ThLineBeamOutliner(lineBeam);
                outliner.ExtendBoth(startExtendLength, endExtendLength);
                lineBeam.StartPoint = outliner.StartPoint;
                lineBeam.EndPoint = outliner.EndPoint;
                lineBeam.Outline = outliner.Outline;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}
