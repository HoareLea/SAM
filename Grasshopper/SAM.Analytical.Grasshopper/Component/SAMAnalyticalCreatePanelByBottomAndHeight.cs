using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelByBottomAndHeight : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("1a5fe397-5876-4d53-bef5-29c1a220ba0a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanelByBottomAndHeight()
          : base("SAMAnalytical.CreatePanelByBottomAndHeight", "SAMAnalytical.CreatePanelByBottomAndHeight",
              "Create SAM Analytical Panel by Bottom Edge And Height",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            index = inputParamManager.AddGenericParameter("_bottom", "_bottom", "Bottom Edge Geometry", GH_ParamAccess.item);
            inputParamManager[index].DataMapping = GH_DataMapping.Flatten;

            index = inputParamManager.AddGenericParameter("panelType_", "panelType_", "PanelType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddParameter(new GooConstructionParam(), "construction_", "construction_", "SAM Analytical Construction", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            inputParamManager.AddNumberParameter("_height", "_height", "Panel Height", GH_ParamAccess.item);

            index = inputParamManager.AddNumberParameter("_minElevation", "_minElevation", "Min Elevation", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooPanelParam(), "Panels", "Panels", "SAM Analytical Panels", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            GH_ObjectWrapper @objectWrapper = null;
            if (!dataAccess.GetData(0, ref @objectWrapper))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISegmentable3D> segmentable3Ds = null;
            if (!Geometry.Grasshopper.Query.TryGetSAMGeometries(@objectWrapper, out segmentable3Ds) || segmentable3Ds == null || segmentable3Ds.Count() == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double height = double.NaN;
            if (!dataAccess.GetData(3, ref height))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            PanelType panelType = PanelType.Undefined;

            objectWrapper = null;
            dataAccess.GetData(1, ref objectWrapper);
            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            Construction construction = null;
            dataAccess.GetData(2, ref construction);

            double minElevation = double.NaN;
            if(dataAccess.GetData(4, ref minElevation))
            {
                for(int i =0; i < segmentable3Ds.Count; i++)
                {
                    BoundingBox3D boundingBox3D = segmentable3Ds[i].GetBoundingBox();

                    segmentable3Ds[i] = segmentable3Ds[i].GetMoved(new Vector3D(0, 0, minElevation - boundingBox3D.Min.Z)) as ISegmentable3D;
                }
            }

            List<Panel> panels = Create.Panels(segmentable3Ds, height, panelType, construction);

            dataAccess.SetDataList(0, panels?.ConvertAll(x => new GooPanel(x)));
        }
    }
}