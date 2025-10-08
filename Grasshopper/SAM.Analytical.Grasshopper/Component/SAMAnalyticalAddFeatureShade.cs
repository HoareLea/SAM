using Grasshopper.Kernel;
using Rhino.UI;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddFeatureShade : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new ("19b85b9c-ec97-4a14-9c79-c9f9ce76d3df");

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
        public SAMAnalyticalAddFeatureShade()
          : base("SAMAnalytical.AddFeatureShade", "SAMAnalytical.AddFeatureShade",
              "Add FeatureShade",
              "SAM", "Analytical")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override GH_SAMParam[] Inputs
        {
            get
            {
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject_", NickName = "_analyticalObject_", Description = "SAM Analytical Object such as AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooFeatureShadeParam { Name = "_featureShades", NickName = "_featureShades", Description = "SAM Analytical FeatureShades", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObjects_", NickName = "_analyticalObjects_", Description = "SAM Analytical Objects such as Panels or Apertures", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticalObjects", NickName = "analyticalObjects", Description = "SAM Analytical Objects such as Panels or Apertures", Access = GH_ParamAccess.list}, ParamVisibility.Binding));
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
            int index = -1;

            index = Params.IndexOfInputParam("_featureShades");
            List<FeatureShade> featureShades = [];
            if (index == -1 || !dataAccess.GetDataList(index, featureShades))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_analyticalObject_");
            IAnalyticalObject analyticalObject = null;
            if (index != -1)
            {
                dataAccess.GetData(index, ref analyticalObject);
            }

            AdjacencyCluster adjacencyCluster = null;
            
            if(analyticalObject is not null)
            {
                if(analyticalObject is AdjacencyCluster adjacencyCluster_Temp)
                {
                    adjacencyCluster = new AdjacencyCluster(adjacencyCluster_Temp, true);
                }
                else if(analyticalObject is AnalyticalModel analytcialModel)
                {
                    adjacencyCluster = analytcialModel.AdjacencyCluster;
                    if(adjacencyCluster is not null)
                    {
                        adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);
                    }
                }
            }

            index = Params.IndexOfInputParam("_analyticalObjects_");
            List<IAnalyticalObject> analyticalObjects = [];
            if (index != -1)
            {
                dataAccess.GetDataList(index, analyticalObjects);
            }

            if(analyticalObjects is null || analyticalObjects.Count == 0)
            {
                if(adjacencyCluster is null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                    return;
                }

                adjacencyCluster.GetApertures()?.ForEach(analyticalObjects.Add);
            }

            List<IAnalyticalObject> result = [];

            if (featureShades.Count > 0)
            {
                while (featureShades.Count < analyticalObjects.Count)
                {
                    featureShades.Add(featureShades[0]);
                }

                for (int i = 0; i < analyticalObjects.Count; i++)
                {
                    IAnalyticalObject analyticalObject_Temp = analyticalObjects[i];

                    if (analyticalObject_Temp is Panel panel)
                    {
                        if (adjacencyCluster is not null)
                        {
                            panel = adjacencyCluster.GetObject<Panel>(panel.Guid);
                        }

                        if (panel is null)
                        {
                            continue;
                        }

                        panel.SetValue(PanelParameter.FeatureShade, new FeatureShade(featureShades[i]));
    
                        if (adjacencyCluster is not null)
                        {
                            adjacencyCluster.AddObject(panel);
                        }

                        result.Add(panel);
                    }
                    else if (analyticalObject_Temp is Aperture aperture)
                    {
                        Panel panel_Temp = null;

                        if (adjacencyCluster is not null)
                        {
                            panel_Temp = adjacencyCluster.GetPanel(aperture);
                            if (panel_Temp is null)
                            {
                                continue;
                            }

                            aperture = panel_Temp.GetAperture(aperture.Guid);
                        }

                        if (aperture is null)
                        {
                            continue;
                        }

                        aperture.SetValue(ApertureParameter.FeatureShade, new FeatureShade(featureShades[i]));

                        if (adjacencyCluster is not null)
                        {
                            panel_Temp.RemoveAperture(aperture.Guid);
                            panel_Temp.AddAperture(aperture);

                            adjacencyCluster.AddObject(panel_Temp);
                        }

                        result.Add(aperture);
                    }
                }
            }

            if(adjacencyCluster != null)
            {
                if(analyticalObject is AnalyticalModel analyticalModel)
                {
                    analyticalObject = new AnalyticalModel(analyticalModel, adjacencyCluster);
                }
                else if(analyticalObject is AdjacencyCluster)
                {
                    analyticalObject = adjacencyCluster;
                }
            }

            index = Params.IndexOfOutputParam("analyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("analyticalObjects");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result?.ConvertAll(x => new GooAnalyticalObject(x)));
            }

        }
    }
}