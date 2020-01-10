using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Surface : IClosed3D
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

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            return boundary.GetBoundingBox(offset);
        }

        public IClosed3D GetBoundary()
        {
            return boundary.Clone() as IClosed3D;
        }
    }
}
