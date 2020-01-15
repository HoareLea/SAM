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
        private List<Panel> panels;


        public Space(Space space)
            : base(space)
        {
            this.location = space.Location;
            this.panels = space.panels;
        }

        public Space(Guid guid, string name, Geometry.Spatial.Point3D location, IEnumerable<Panel> panels)
            : base(guid, name)
        {
            this.location = location;
            if (panels != null)
                this.panels = new List<Panel>(panels);
        }

        public Space(string name, IEnumerable<Panel> panels)
            : base(name)
        {
            this.panels = new List<Panel>(panels);
        }

        public Space(Guid guid, string name, Geometry.Spatial.Point3D location)
            : base(guid, name)
        {
            this.location = location;
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

        public List<Panel> Panels
        {
            get
            {
                if (panels == null)
                    return null;

                return new List<Panel>(panels);
            }
        }
    }
}
