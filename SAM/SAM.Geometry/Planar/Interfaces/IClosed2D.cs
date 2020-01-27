using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Geometry.Planar
{
    public interface IClosed2D : IGeometry2D
    {
        //closed2D inside this
        bool Inside(IClosed2D closed2D);

        bool Inside(Point2D point2D);

        double GetArea();
    }
}
