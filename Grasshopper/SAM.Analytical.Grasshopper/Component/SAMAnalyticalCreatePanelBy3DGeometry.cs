using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreatePanelBy3DGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("35ef8f3e-1cf2-407d-b2ed-33bf371ea161");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreatePanelBy3DGeometry()
          : base("SAMAnalytical.CreatePanelBy3DGeometry", "SAMAnalytical.CreatePanelBy3DGeometry",
              "Create SAM Analytical Panel by 3D Geometry",
              "SAM", "Analytical")
        {
        }

        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();

                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panel_", NickName = "panel_", Description = "Source SAM Analytical Panel", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "3Dgeometry_", NickName = "3Dgeometry_", Description = "3D Geometry", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "panelType_", NickName = "panelType_", Description = "SAM Analytical PanelType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooConstructionParam() { Name = "construction_", NickName = "construction_", Description = "SAM Analytical Construction", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "simplify_", NickName = "simplify_", Description = "Simplify", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Acceptable area of Aperture", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooPanelParam() { Name = "panel", NickName = "panel", Description = "SAM Analytical Panel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                return result.ToArray();
            }
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            int index;

            PanelType? panelType = null;
            index = Params.IndexOfInputParam("panelType_");
            if (index != -1)
            {
                string panelType_String = null;
                if(dataAccess.GetData(index, ref panelType_String))
                {
                    panelType = Analytical.Query.PanelType(panelType_String);
                }
            }

            Construction construction = null;
            index = Params.IndexOfInputParam("construction_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref construction);
            }

            double tolerance = Core.Tolerance.Distance;
            index = Params.IndexOfInputParam("tolerance_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref tolerance);
            }

            double minArea = Core.Tolerance.MacroDistance;
            index = Params.IndexOfInputParam("minArea_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref minArea);
            }


            bool simplify = true;
            index = Params.IndexOfInputParam("simplify_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref simplify);
            }

            List<ISAMGeometry3D> geometry3Ds = null;
            index = Params.IndexOfInputParam("3Dgeometry_");
            if(index != -1)
            {
                object @object = null;
                if(dataAccess.GetData(index, ref @object))
                {
                    if(!Query.TryConvertToPanelGeometries(@object, out geometry3Ds, simplify))
                    {
                        geometry3Ds = null;
                    }
                }
            }

            index = Params.IndexOfInputParam("panel_");
            Panel panel = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref panel);
            }

            List<Panel> panels = null;

            if(panel == null)
            {
                if(geometry3Ds == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Data");
                    return;
                }

                PanelType panelType_Temp = PanelType.Undefined;
                if(panelType != null && panelType.HasValue)
                {
                    panelType_Temp = panelType.Value;
                }

                panels = Create.Panels(geometry3Ds, panelType_Temp, construction, minArea, tolerance);

                if(panels != null && (panelType == null || !panelType.HasValue))
                {
                    for (int i = 0; i < panels.Count; i++)
                    {
                        panelType_Temp = Analytical.Query.PanelType(panels[i]?.GetFace3D()?.GetPlane()?.Normal);
                        if(panelType_Temp != PanelType.Undefined)
                        {
                            panels[i] = Create.Panel(panels[i], panelType_Temp);
                        }
                    }
                }
            }
            else
            {
                panels = new List<Panel> {  Create.Panel(Guid.NewGuid(), panel) };
            }

            if (panels == null || panels.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Invalid data for panel");
                return;
            }

            index = Params.IndexOfOutputParam("panel");
            if(index != -1)
            {
                if (panels.Count == 1)
                {
                    dataAccess.SetData(0, new GooPanel(panels[0]));
                }
                else
                {
                    dataAccess.SetDataList(0, panels.ConvertAll(x => new GooPanel(x)));
                }
            }
        }
    }
}