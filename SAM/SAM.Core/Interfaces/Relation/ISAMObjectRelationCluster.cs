using System.Collections.Generic;

namespace SAM.Core
{
    public interface ISAMObjectRelationCluster : IRelationCluster
    {
        public bool TryGetValues(IJSAMObject @object, IComplexReference complexReference, out List<object> values);

        public List<object> GetValues(IComplexReference complexReference);
    }
}
