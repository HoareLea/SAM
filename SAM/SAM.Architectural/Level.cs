using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SAM.Core;

namespace SAM.Architectural
{
    public class Level : SAMObject
    {
        private double elevation;

        public Level(string name, double elevation)
            : base(name)
        {
            this.elevation = elevation;
        }

        public double Elevation
        {
            get
            {
                return elevation;
            }
        }
    }
}
