using Newtonsoft.Json.Linq;

namespace SAM.Core
{
    public interface IMaterial: IJSAMObject
    {
        string Name { get; }
        System.Guid Guid { get; }
    }
}