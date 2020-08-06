using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.UI;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using SAM.Geometry.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalApertureConstructions : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e379534a-795e-46b7-a160-41c13cd6a7f7");

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalApertureConstructions()
          : base("SAMAnalytical.ApertureConstructions", "SAMAnalytical.ApertureConstructions",
              "Get Analytical Constructions from AdjacencyCluster or AnalyticalModel",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            int index;

            inputParamManager.AddParameter(new GooSAMObjectParam<SAMObject>(), "_SAMAnalytical", "_SAMAnalytical", "SAM Analytical Object ie.Panel, AdjacencyCluster", GH_ParamAccess.item);
            
            index = inputParamManager.AddGenericParameter("panelType_", "panelType_", "PanelType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;

            index = inputParamManager.AddGenericParameter("apertureType_", "apertureType_", "ApertureType", GH_ParamAccess.item);
            inputParamManager[index].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooApertureConstructionParam(), "ApertureConstructions", "ApertureConstructions", "SAM Analytical Aperture Constructions", GH_ParamAccess.list);
            outputParamManager.AddBooleanParameter("Successful", "Successful", "Successful", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            dataAccess.SetData(0, false);

            SAMObject sAMObject = null;
            if(!dataAccess.GetData(0, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = null;
            if(sAMObject is Panel)
            {
                panels = new List<Panel>() { (Panel)sAMObject };
            }
            else if(sAMObject is AdjacencyCluster)
            {
                panels = ((AdjacencyCluster)sAMObject).GetPanels();
            }
            else if(sAMObject is AnalyticalModel)
            {
                panels = ((AnalyticalModel)sAMObject).AdjacencyCluster?.GetPanels();
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            GH_ObjectWrapper objectWrapper; ;

            objectWrapper = null;
            dataAccess.GetData(1, ref objectWrapper);

            PanelType panelType = PanelType.Undefined;

            if(objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            objectWrapper = null;
            dataAccess.GetData(2, ref objectWrapper);

            ApertureType apertureType = ApertureType.Undefined;

            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    apertureType = Analytical.Query.ApertureType(((GH_String)objectWrapper.Value).Value);
                else
                    apertureType = Analytical.Query.ApertureType(objectWrapper.Value);
            }


            List<ApertureConstruction> apertureConstructions = Analytical.Query.ApertureConstructions(panels, apertureType, panelType);

            dataAccess.SetDataList(1, apertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
            dataAccess.SetData(0, apertureConstructions != null);
        }
    }
}