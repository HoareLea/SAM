using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public interface ISystemType : IJSAMObject
    {
        string Description { get; }
        string Name { get; }
    }
}