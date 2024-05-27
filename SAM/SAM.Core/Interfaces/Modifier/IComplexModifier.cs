using System.Collections.Generic;

namespace SAM.Core
{
    public interface IComplexModifier : ISimpleModifier
    {
        public List<ISimpleModifier> GetModifiers();
    }
}