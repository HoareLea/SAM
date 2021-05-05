using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using SAM.Geometry.Spatial;
using System;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalTransform : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("41a52d4f-159e-4e30-bb4c-167b259eeb7d");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalTransform()
          : base("SAMAnalytical.Transform", "SAMAnalytical.Transform",
              "WIP!!! Currently Transform only Panel - known issue when Pushing Revit - DO NOT USE WITH REVIT",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_analytical", "_analytical", "SAM Analytical Panel", GH_ParamAccess.item);
            inputParamManager.AddParameter(new GooTransform3DParam(), "_transform", "_transform", "Transform can be SAM or GH", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddGenericParameter("Analytical", "Analytical", "SAM Analytical Object such as Panel, Aperture etc.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            Transform3D transform3D = null;
            if (!dataAccess.GetData(1, ref transform3D))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            SAMObject sAMObject = null;
            if (!dataAccess.GetData(0, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if(sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);
                panel.Transform(transform3D);
                sAMObject = panel;
            }
            else if(sAMObject is Aperture)
            {
                Aperture aperture = new Aperture((Aperture)sAMObject);
                aperture.Transform(transform3D);
                sAMObject = aperture;
            }
            else if(sAMObject is Space)
            {
                Space space = new Space((Space)sAMObject);
                space.Transform(transform3D);
                sAMObject = space;
            }
            else if(sAMObject is AdjacencyCluster)
            {
                AdjacencyCluster adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
                adjacencyCluster.Transform(transform3D);
                sAMObject = adjacencyCluster;
            }
            else if (sAMObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = new AnalyticalModel((AnalyticalModel)sAMObject);
                analyticalModel.Transform(transform3D);
                sAMObject = analyticalModel;
            }

            dataAccess.SetData(0, sAMObject);
        }
    }
}