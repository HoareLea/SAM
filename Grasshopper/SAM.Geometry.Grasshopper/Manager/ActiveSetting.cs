using Rhino.Geometry;
using SAM.Core;
using System.Reflection;

namespace SAM.Geometry.Grasshopper
{
    public static partial class ActiveSetting
    {
        public static class Name
        {

        }

        private static Setting setting = Load();

        private static MeshingParameters meshingParameters;
        
        private static Setting Load()
        {
            Setting setting = ActiveManager.GetSetting(Assembly.GetExecutingAssembly());
            if (setting == null)
                setting = GetDefault();

            return setting;
        }

        public static Setting Setting
        {
            get
            {
                return setting;
            }
        }

        public static Setting GetDefault()
        {
            Setting setting = new Setting(Assembly.GetExecutingAssembly());

            return setting;
        }

        public static MeshingParameters GetMeshingParameters()
        {
            if (meshingParameters == null)
            {
                meshingParameters = new MeshingParameters
                {
                    SimplePlanes = false,
                    RefineGrid = true,
                    GridMinCount = 16,
                    GridMaxCount = 400,
                    GridAspectRatio = 20,
                    MaximumEdgeLength = 0,
                    RefineAngle = 20,
                    MinimumEdgeLength = 0,
                    //SimplePlanes = true,
                    //MinimumEdgeLength = 0.6,
                    //Tolerance = 0.2
                };
            }

            return meshingParameters;
        }
    }
}
