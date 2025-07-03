using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThMEPEngineCore.Model
{
    public class ThIfcSpace : ThIfcSpatialStructureElement
    {
        public List<ThIfcSpace> SubSpaces { get; set; }
        public ThIfcSpace()
        {
            SubSpaces = new List<ThIfcSpace>();
            Uuid = Guid.NewGuid().ToString();
        }
    }
}
