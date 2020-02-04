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

        public Level(Level level)
            : base(level)
        {
            elevation = level.elevation;
        }

        public Level(double elevation)
            : base()
        {
            this.elevation = elevation;
        }

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

        public Geometry.Spatial.Plane GetPlane()
        {
            return new Geometry.Spatial.Plane(new Geometry.Spatial.Point3D(0, 0, elevation), Geometry.Spatial.Vector3D.BaseZ);
        }
    }
}
