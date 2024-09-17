using System.Collections.Generic;

namespace SAM.Core
{
    public interface IComplexModifier : IModifier
    {

    }

    public interface IComplexModifier<T> : IComplexModifier where T : IModifier
    {
        List<T> Modifiers { get; }
    }
}