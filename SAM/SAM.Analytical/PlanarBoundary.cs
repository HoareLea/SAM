using SAM.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Analytical
{
    public class PlanarBoundary : SAMObject
    {
        private List<PlanarBoundary> boundaries;

        public PlanarBoundary(IEnumerable<PlanarBoundary> boundaries)
            : base()
        {
            this.boundaries = new List<PlanarBoundary>(boundaries);
        }
    }
}
