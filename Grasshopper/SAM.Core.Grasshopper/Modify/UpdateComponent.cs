using Grasshopper.Kernel;
using Rhino;
using System;

namespace SAM.Core.Grasshopper
{
    public static partial class Modify
    {
        public static void UpdateComponent(GH_SAMComponent gH_SAMComponent)
        {
            GH_Document gH_Document = gH_SAMComponent?.OnPingDocument();
            if (gH_Document == null)
            {
                return;
            }

            GH_SAMComponent gH_SAMComponent_New = Activator.CreateInstance(gH_SAMComponent.GetType()) as GH_SAMComponent;
            if (gH_SAMComponent_New == null)
            {
                RhinoApp.WriteLine("Failed to create new component.");
                return;
            }

            gH_Document.AddObject(gH_SAMComponent_New, false);

            gH_SAMComponent_New.Attributes.Pivot = gH_SAMComponent.Attributes.Pivot;

            Modify.CopyParameteres(GH_ParameterSide.Output, gH_SAMComponent, gH_SAMComponent_New);
            Modify.CopyParameteres(GH_ParameterSide.Input, gH_SAMComponent, gH_SAMComponent_New);

            gH_Document.RemoveObject(gH_SAMComponent, true);
            gH_SAMComponent_New.ExpireSolution(true);
        }
    }
}