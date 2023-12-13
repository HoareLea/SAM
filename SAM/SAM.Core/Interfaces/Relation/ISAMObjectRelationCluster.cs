using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAM.Core
{
    public interface ISAMObjectRelationCluster :IRelationCluster
    {
        public bool TryGetValues(IJSAMObject @object, IComplexReference complexReference, out List<object> values);
    }
}
