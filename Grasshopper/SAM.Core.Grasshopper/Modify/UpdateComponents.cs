using Grasshopper.Kernel;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static void UpdateComponents(GH_Document gH_Document)
        {

            if (gH_Document == null)
            {
                return;
            }

            IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
            if (gH_DocumentObjects == null || gH_DocumentObjects.Count == 0)
            {
                return;
            }

            List<GH_SAMComponent> gH_SAMComponents = new List<GH_SAMComponent>();
            foreach (IGH_DocumentObject gH_DocumentObject in gH_DocumentObjects)
            {
                GH_SAMComponent gH_SAMComponent = gH_DocumentObject as GH_SAMComponent;

                if (!(gH_DocumentObject is GH_SAMComponent))
                {
                    continue;
                }

                if (gH_SAMComponent.Obsolete)
                {
                    gH_SAMComponents.Add(gH_SAMComponent);
                }
            }

            if (gH_SAMComponents == null || gH_SAMComponents.Count == 0)
            {
                return;
            }

            UpdateComponents(gH_SAMComponents);
        }

        public static void UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents)
        {
            if (gH_SAMComponents == null || gH_SAMComponents.Count() == 0)
            {
                return;
            }

            foreach (GH_SAMComponent gH_SAMComponent in gH_SAMComponents)
            {
                UpdateComponent(gH_SAMComponent);
            }
        }
    }
}