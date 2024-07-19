using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static List<GH_SAMComponent> UpdateComponents(GH_Document gH_Document, out Log log)
        {
            log = new Log();

            if (gH_Document == null)
            {
                return null;
            }

            IList<IGH_DocumentObject> gH_DocumentObjects = gH_Document.Objects;
            if (gH_DocumentObjects == null || gH_DocumentObjects.Count == 0)
            {
                return null;
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
                return null;
            }

            return UpdateComponents(gH_SAMComponents, out log);
        }

        public static List<GH_SAMComponent> UpdateComponents(IEnumerable<GH_SAMComponent> gH_SAMComponents, out Log log)
        {
            log = new Log();
            if (gH_SAMComponents == null || gH_SAMComponents.Count() == 0)
            {
                return null;
            }

            List<Tuple<GH_SAMComponent, GH_SAMComponent>> tuples = new List<Tuple<GH_SAMComponent, GH_SAMComponent>>();
            foreach (GH_SAMComponent gH_SAMComponent in gH_SAMComponents)
            {
                GH_SAMComponent gH_SAMComponent_New = DuplicateComponent(gH_SAMComponent, out Log log_Temp);
                if(gH_SAMComponent_New == null)
                {
                    continue;
                }

                tuples.Add(new Tuple<GH_SAMComponent, GH_SAMComponent>(gH_SAMComponent, gH_SAMComponent_New));

                if (log_Temp != null)
                {
                    log.AddRange(log_Temp);
                }
            }

            List<GH_SAMComponent> result = new List<GH_SAMComponent>();
            foreach(Tuple<GH_SAMComponent, GH_SAMComponent> tuple in tuples)
            {
                //tuple.Item2.ExpireSolution(false);
                result.Add(tuple.Item2);
                tuple.Item1.OnPingDocument().RemoveObject(tuple.Item1, false);
            }

            return result;
        }
    }
}