using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class Space : SAMObject
    {
        private Geometry.Spatial.Point3D location;


        public Space(Space space)
            : base(space)
        {
            this.location = space.Location;
        }

        public Space(Guid guid, Space space)
        : base(guid, space)
        {
            this.location = space.Location;
        }

        public Space(Guid guid, string name, Geometry.Spatial.Point3D location)
            : base(guid, name)
        {
            this.location = location;
        }

        public Space(string name)
            : base(name)
        {
        }

        public Space(string name, Geometry.Spatial.Point3D location)
            : base(name)
        {
            this.location = location;
        }

        public Geometry.Spatial.Point3D Location
        {
            get
            {
                return location;
            }
        }
    }
}
