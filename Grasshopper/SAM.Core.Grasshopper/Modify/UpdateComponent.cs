using Grasshopper.Kernel;
using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static GH_SAMComponent DuplicateComponent(GH_SAMComponent gH_SAMComponent, out Log log)
        {
            log = new Log();

            GH_Document gH_Document = gH_SAMComponent?.OnPingDocument();
            if (gH_Document == null)
            {
                log.Add(new LogRecord("Could not access document or component", LogRecordType.Error));
                return null;
            }

            GH_SAMComponent result = Activator.CreateInstance(gH_SAMComponent.GetType()) as GH_SAMComponent;
            if (result == null)
            {
                log.Add(new LogRecord("Failed to create new component: {0}", LogRecordType.Error, gH_SAMComponent.Name));
                return null;
            }

            bool add = gH_Document.AddObject(result, false);
            if(!add)
            {
                log.Add(new LogRecord("Could not add component to document: {0}", LogRecordType.Error, gH_SAMComponent.Name));
                return null;
            }

            log.Add(new LogRecord("Component {0} updating. Old version: {1} New version: {2}", LogRecordType.Message, gH_SAMComponent.Name, gH_SAMComponent.ComponentVersion, gH_SAMComponent.LatestComponentVersion));

            result.Attributes.Pivot = gH_SAMComponent.Attributes.Pivot;

            CopyParameteres(GH_ParameterSide.Output, gH_SAMComponent, result);
            CopyParameteres(GH_ParameterSide.Input, gH_SAMComponent, result);

            //gH_Document.RemoveObject(gH_SAMComponent, false);

            //result.ExpireSolution(true);
            return result;
        }

        public static GH_SAMComponent DuplicateComponent(GH_SAMComponent gH_SAMComponent)
        {
            return DuplicateComponent(gH_SAMComponent, out Log log);
        }
    }
}