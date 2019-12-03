using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Spatial;

namespace SAM.Geometry.Grasshopper
{
    //public class TemplateCompoment : GH_Component
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the SAM_point3D class.
    //    /// </summary>
    //    public TemplateCompoment()
    //      : base("SAM_point3D", "pt3d",
    //          "Create SAM point3d",
    //          "SAM", "Geometry")
    //    {
    //    }

    //    /// <summary>
    //    /// Registers all the input parameters for this component.
    //    /// </summary>
    //    protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
    //    {
    //        inputParamManager.AddPointParameter("Point", "pt", "GH Point", GH_ParamAccess.item);
    //    }

    //    /// <summary>
    //    /// Registers all the output parameters for this component.
    //    /// </summary>
    //    protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
    //    {
    //        outputParamManager.AddGenericParameter("Point3D", "pt3d", "SAM Point3D", GH_ParamAccess.item);
    //    }

    //    /// <summary>
    //    /// This is the method that actually does the work.
    //    /// </summary>
    //    /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
    //    protected override void SolveInstance(IGH_DataAccess dataAccess)
    //    {
    //        GH_Point point = null;

    //        if (!dataAccess.GetData(0, ref point))
    //        {
    //            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
    //            return;
    //        }


    //        dataAccess.SetData(0, point.ToSAM());
    //    }

    //    /// <summary>
    //    /// Provides an Icon for the component.
    //    /// </summary>
    //    protected override System.Drawing.Bitmap Icon
    //    {
    //        get
    //        {
    //            //You can add image files to your project resources and access them like this:
    //            // return Resources.IconForThisComponent;
    //            return null;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the unique ID for this component. Do not change this ID after release.
    //    /// </summary>
    //    public override Guid ComponentGuid
    //    {
    //        get { return new Guid("060a9d71-9c97-4502-9e30-e9a7d51d21db"); }
    //    }
    //}
}