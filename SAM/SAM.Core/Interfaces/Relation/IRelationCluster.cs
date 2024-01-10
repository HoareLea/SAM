using System;
using System.Collections.Generic;

namespace SAM.Core
{
    public interface IRelationCluster: IJSAMObject
    {
        public List<Type> GetTypes();

        public RelationCluster<T> Cast<T>();
    }
}
