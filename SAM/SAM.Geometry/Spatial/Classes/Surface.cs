using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Surface : IGeometry3D
    {
        private IClosed3D boundary;

        public Surface(IClosed3D boundary)
        {
            this.boundary = boundary.Clone() as IClosed3D;
        }
        public Surface(Surface surface)
        {
            boundary = surface.boundary.Clone() as IClosed3D;
        }

        public Face ToFace()
        {
            if (boundary is IClosedPlanar3D)
                return new Face(boundary as IClosedPlanar3D);

            return null;
        }

        public IGeometry Clone()
        {
            return new Surface(this);
        }
    }
}
