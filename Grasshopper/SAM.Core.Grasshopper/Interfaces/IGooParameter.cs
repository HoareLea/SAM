using Grasshopper.Kernel.Types;

namespace SAM.Core.Grasshopper
{
    public interface IGooParameter: IGH_Goo
    {
        string Name { get; }
    }
}