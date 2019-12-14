using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Extrusion : IBoundable3D
    {
        private Face face;
        private Vector3D vector;

        public Extrusion(Face face, double height)
        {
            this.face = new Face(face);
            vector = new Vector3D(0, 0, height);
        }

        public Extrusion(Face face, Vector3D vector)
        {
            this.face = face;
            this.vector = vector;
        }

        public Extrusion(Extrusion extrusion)
        {
            face = new Face(extrusion.face);
            vector = new Vector3D(extrusion.vector);
        }

        public IGeometry Clone()
        {
            return new Extrusion(this);
        }

        public BoundingBox3D GetBoundingBox(double offset = 0)
        {
            throw new NotImplementedException();
        }
    }
}
