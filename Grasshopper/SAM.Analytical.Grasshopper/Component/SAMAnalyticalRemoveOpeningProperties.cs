using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalRemoveOpeningProperties : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e8f5f498-22dd-404f-9b6c-dc4aee1867c4");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalRemoveOpeningProperties()
          : base("SAMAnalytical.RemoveOpeningProperties", "SAMAnalytical.RemoveOpeningProperties",
              "RemoveOpeningProperties",
              "SAM", "Analytical03")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures_", NickName = "apertures_", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

                return [.. result];
            }
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Outputs
        {
            get
            {
                List<GH_SAMParam> result = [];
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analytical", NickName = "analytical", Description = "SAM Analytical", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                return [.. result];
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
            int index = -1;

            index = Params.IndexOfInputParam("_analytical");
            IAnalyticalObject analyticalObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Aperture> apertures = [];
            index = Params.IndexOfInputParam("apertures_");
            if(index != -1)
            {
                if(!dataAccess.GetDataList(index, apertures))
                {
                    apertures = [];
                }
            }

            AdjacencyCluster adjacencyCluster = null;

            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)analyticalObject, true);
            }
            else if(analyticalObject is AnalyticalModel)
            {
                AnalyticalModel analyticalModel = (AnalyticalModel)analyticalObject;

                adjacencyCluster = analyticalModel.AdjacencyCluster;
                if(adjacencyCluster != null)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);
                }
            }

            if(adjacencyCluster is null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }


            if (apertures == null || apertures.Count == 0)
            {
                apertures = adjacencyCluster.GetApertures();
            }

            foreach(Aperture aperture in apertures)
            {
                if(adjacencyCluster.GetPanel(aperture) is not Panel panel)
                {
                    continue;
                }

                if(!aperture.RemoveValue(ApertureParameter.OpeningProperties))
                {
                    continue;
                }

                panel = Create.Panel(panel);

                panel.RemoveAperture(aperture.Guid);

                panel.AddAperture(aperture);

                adjacencyCluster.AddObject(panel);
            }

            if(analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if(analyticalObject is AnalyticalModel analyticalModel_Temp)
            {
                analyticalObject = new AnalyticalModel(analyticalModel_Temp, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("analytical");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }
        }
    }
}