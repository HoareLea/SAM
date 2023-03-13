using System;

namespace SAM.Core
{
    public interface IEnumFilter : IFilter
    {
        public Enum Enum { get; set; }
    }
}