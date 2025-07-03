using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThMEPEngineCore.BeamInfo.Model;

namespace ThMEPEngineCore.Interface
{
    public interface ICalculateBeam
    {
        List<Beam> GetBeamInfo(DBObjectCollection dbObjs);
    }
}
