using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

using SAM.Geometry.Grasshopper.Properties;
using SAM.Geometry.Spatial;


namespace SAM.Geometry.Grasshopper
{
    //public class SAMGeometryByGHGeometry : GH_Component
    //{
    //    /// <summary>
    //    /// Initializes a new instance of the SAMGeometryByGHGeometry class.
    //    /// </summary>
    //    public SAMGeometryByGHGeometry()
    //      : base("SAMGeometryByGHGeometry", "SAMgeo",
    //          "Description SAMGeometryByGHGeometry",
    //          "SAM", "Geometry")
    //    {
    //    }

    //    /// <summary>
    //    /// Registers all the input parameters for this component.
    //    /// </summary>
    //    protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
    //    {
    //        inputParamManager.AddGenericParameter("GHGeometry", "GHgeo", "Grashopper Geometry", GH_ParamAccess.item);
    //    }


    //    /// <summary>
    //    /// Registers all the output parameters for this component.
    //    /// </summary>
    //    protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
    //    {
    //        outputParamManager.AddGenericParameter("SAMGeometry", "SAMgeo", "SAM Geometry", GH_ParamAccess.item);
    //    }

    //    /// <summary>
    //    /// This is the method that actually does the work.
    //    /// </summary>
    //    /// <param name="dataAccess">The DA object is used to retrieve from inputs and store in outputs.</param>
    //    protected override void SolveInstance(IGH_DataAccess dataAccess)
    //    {
    //        object obj = null;
    //        if (!dataAccess.GetData(0, ref obj))
    //        {
    //            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot get data");
    //            return;
    //        }

    //        GH_Point point = obj as GH_Point;
    //        if (point != null)
    //        { 
    //            dataAccess.SetData(0, point.ToSAM());
    //            return;
    //        }

    //        GH_Line line = obj as GH_Line;
    //        if (line != null)
    //        {
    //            dataAccess.SetData(0, line.ToSAM());
    //            return;
    //        }

    //        GH_Curve curve = obj as GH_Curve;
    //        if (curve != null)
    //        {
    //            IGeometry geometry = null;

    //            if (curve.Value is Rhino.Geometry.LineCurve)
    //                geometry = ((Rhino.Geometry.LineCurve)curve.Value).Line.ToSAM();
    //            else
    //                geometry = curve.ToSAM();

    //            if(geometry != null)
    //            {
    //                dataAccess.SetData(0, geometry);
    //                return;
    //            }
    //        }

    //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Cannot convert geometry");
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
    //            return Resources.SAM_Small;
    //        }
    //    }

    //    /// <summary>
    //    /// Gets the unique ID for this component. Do not change this ID after release.
    //    /// </summary>
    //    public override Guid ComponentGuid
    //    {
    //        get { return new Guid("1c813e64-b8f4-41c1-8e86-bbabe76434d0"); }
    //    }
    //}
}