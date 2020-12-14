using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public interface ISystemType : ISAMObject
    {
        string Description { get; }
    }
}