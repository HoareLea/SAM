using SAM.Core;
using SAM.Geometry.Spatial;
using System.Collections.Generic;

namespace SAM.Analytical
{
    public static partial class Query
    {
        public static Dictionary<IPartition, Architectural.MaterialLayer> InternalMaterialLayerDictionary(this ArchitecturalModel architecturalModel, Space space, double silverSpacing = Tolerance.MacroDistance, double tolerance = Tolerance.Distance)
        {
            Dictionary<IPartition, Vector3D> dictionary = architecturalModel.NormalDictionary(space, out Shell shell, true, silverSpacing, tolerance);
            if(dictionary == null)
            {
                return null;
            }

            Dictionary<IPartition, Architectural.MaterialLayer> result = new Dictionary<IPartition, Architectural.MaterialLayer>();
            foreach(KeyValuePair<IPartition, Vector3D> keyValuePair in dictionary)
            {
                if(keyValuePair.Key == null)
                {
                    continue;
                }

                Architectural.MaterialLayer materialLayer = FirstMaterialLayer(keyValuePair.Key as IHostPartition, keyValuePair.Value);
                result[keyValuePair.Key] = materialLayer;
            }

            return result;
        }
    }
}