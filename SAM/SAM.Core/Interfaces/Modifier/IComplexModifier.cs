using System.Collections.Generic;

namespace SAM.Core
{
    public interface IComplexModifier : IModifier
    {
        List<IModifier> Modifiers { get; }
    }
}