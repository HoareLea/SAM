using System.ComponentModel;

namespace SAM.Geometry.Grasshopper
{
    [Description("Mesh Type")]
    public enum MeshType
    {
        [Description("Undefined")] Undefined,
        [Description("Triangle")] Triangle,
        [Description("Quad")] Quad
    }
}