using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Spatial
{
    public class Extrusion
    {
        private Face face;
        private Vector3D vector;

        public Extrusion(Face face, double height)
        {
            this.face = face;
            vector = new Vector3D(0, 0, height);
        }

        public Extrusion(Face face, Vector3D vector)
        {
            this.face = face;
            this.vector = vector;
        }
    }
}
