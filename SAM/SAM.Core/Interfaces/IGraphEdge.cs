using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public interface IGraphEdge
    {
        object GetObject();
        double Weight { get;  }
    }
}
