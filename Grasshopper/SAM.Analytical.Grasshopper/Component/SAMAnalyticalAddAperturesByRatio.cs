﻿using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalAddAperturesByRatio : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("12e7038b-df21-44dd-aebd-d47a13147ead");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
                protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalAddAperturesByRatio()
          : base("SAMAnalytical.AddAperturesByRatio", "SAMAnalytical.AddAperturesByRatio",
              "Add Apertures to SAM Analytical Object: ie Panel, AdjacencyCluster or Analytical Model",
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

                GooAnalyticalObjectParam analyticalObjectParam = new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM Analytical Object such as AdjacencyCluster, Panel or AnalyticalModel", Access = GH_ParamAccess.item };
                result.Add(new GH_SAMParam(analyticalObjectParam, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number number = null;

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_ratio_", NickName = "_ratio_", Description = "Ratio", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean boolean = null;

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_subdivide_", NickName = "_subdivide_", Description = "Subdivide", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_apertureHeight_", NickName = "_apertureHeight_", Description = "Default aperture Height", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(2);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_sillHeight_", NickName = "_sillHeight_", Description = "Default sill Height", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(0.8);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_horizontalSeparation_", NickName = "_horizontalSeparation_", Description = "Horizontal Separation", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(3);
                result.Add(new GH_SAMParam(number, ParamVisibility.Binding));

                GooApertureConstructionParam apertureConstructionParam = new GooApertureConstructionParam() { Name = "_apertureConstruction_", NickName = "_apertureConstruction_", Description = "SAM Analytical Aperture Construction", Access = GH_ParamAccess.item, Optional = true};
                result.Add(new GH_SAMParam(apertureConstructionParam, ParamVisibility.Binding));

                boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_keepSeparationDistance_", NickName = "_keepSeparationDistance_", Description = "Keep horizontal separation distance between apertures", Access = GH_ParamAccess.item, Optional = true };
                boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(boolean, ParamVisibility.Voluntary));

                number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_offset_", NickName = "_offset_", Description = "Minimal Ofsset between wall and apertures", Access = GH_ParamAccess.item, Optional = true };
                number.SetPersistentData(0.1);
                result.Add(new GH_SAMParam(number, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "analyticalObject", NickName = "analyticalObject", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooApertureParam() { Name = "apertures", NickName = "apertures", Description = "SAM Analytical Apertures", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "successful", NickName = "successful", Description = "Successful", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            int index_Successful = Params.IndexOfOutputParam("successful");
            if(index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, false);
            }

            int index = -1;


            ApertureConstruction apertureConstruction = null;

            index = Params.IndexOfInputParam("_apertureConstruction_");
            if(index != -1)
            {
                dataAccess.GetData(index, ref apertureConstruction);
            }

            double ratio = double.NaN;
            index = Params.IndexOfInputParam("_ratio_");
            if (index == -1 || !dataAccess.GetData(index, ref ratio))
            {
                ratio = double.NaN;
            }

            bool subdivide = true;
            index = Params.IndexOfInputParam("_subdivide_");
            if (index == -1 || !dataAccess.GetData(index, ref subdivide))
            {
                subdivide = true;
            }

            double apertureHeight = 2;
            index = Params.IndexOfInputParam("_apertureHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref apertureHeight))
            {
                apertureHeight = 2;
            }

            double sillHeight = 0.8;
            index = Params.IndexOfInputParam("_sillHeight_");
            if (index == -1 || !dataAccess.GetData(index, ref sillHeight))
            {
                sillHeight = 0.8;
            }

            double horizontalSeparation = 3;
            index = Params.IndexOfInputParam("_horizontalSeparation_");
            if (index == -1 || !dataAccess.GetData(index, ref horizontalSeparation))
            {
                horizontalSeparation = 3;
            }

            double offset = 0.1;
            index = Params.IndexOfInputParam("_offset_");
            if (index == -1 || !dataAccess.GetData(index, ref offset))
            {
                offset = 0.1;
            }

            bool keepSeparationDistance = false;
            index = Params.IndexOfInputParam("_keepSeparationDistance_");
            if (index == -1 || !dataAccess.GetData(index, ref keepSeparationDistance))
            {
                keepSeparationDistance = false;
            }

            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analyticalObject");
            if (!dataAccess.GetData(index, ref sAMObject))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (sAMObject is Panel)
            {
                Panel panel = Create.Panel((Panel)sAMObject);

                List<Aperture> apertures = null;

                if (!double.IsNaN(ratio))
                {
                    ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                    if (apertureConstruction_Temp == null)
                        apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel, ApertureType.Window);

                    apertures = panel.AddApertures(apertureConstruction_Temp, ratio, subdivide, apertureHeight, sillHeight, horizontalSeparation, offset, keepSeparationDistance);
                }

                index = Params.IndexOfOutputParam("analyticalObject");
                if (index != -1)
                {
                    dataAccess.SetData(index, panel);
                }

                index = Params.IndexOfOutputParam("apertures");
                if (index != -1)
                {
                    dataAccess.SetDataList(index, apertures?.ConvertAll(x => new GooAperture(x)));
                }

                if(index_Successful != -1)
                {
                    dataAccess.SetData(index_Successful, apertures != null && apertures.Count != 0);
                }

                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            AnalyticalModel analyticalModel = null;
            if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else if(sAMObject is AnalyticalModel)
            {
                analyticalModel = ((AnalyticalModel)sAMObject);
                adjacencyCluster = analyticalModel.AdjacencyCluster;
            }

            if (adjacencyCluster == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Tuple<Panel, Aperture>> tuples_Result = null;

            List<Panel> panels = adjacencyCluster.GetPanels();
            if (panels != null && !double.IsNaN(ratio))
            {
                tuples_Result = new List<Tuple<Panel, Aperture>>();

                foreach (Panel panel in panels)
                {
                    if (panel.PanelType != PanelType.WallExternal || panel.Adiabatic())
                    {
                        continue;
                    }

                    Panel panel_New = Create.Panel(panel);

                    ApertureConstruction apertureConstruction_Temp = apertureConstruction;
                    if (apertureConstruction_Temp == null)
                        apertureConstruction_Temp = Analytical.Query.DefaultApertureConstruction(panel_New, ApertureType.Window);

                    List<Aperture> apertures = panel_New.AddApertures(apertureConstruction_Temp, ratio, subdivide, apertureHeight, sillHeight, horizontalSeparation, offset, keepSeparationDistance);
                    if (apertures == null)
                        continue;

                    apertures.ForEach(x => tuples_Result.Add(new Tuple<Panel, Aperture>(panel_New, x)));
                    adjacencyCluster.AddObject(panel_New);
                }
            }

            index = Params.IndexOfOutputParam("analyticalObject");
            if(index != -1)
            {
                if (analyticalModel != null)
                {
                    AnalyticalModel analyticalModel_Result = new AnalyticalModel(analyticalModel, adjacencyCluster);
                    dataAccess.SetData(index, analyticalModel_Result);
                }
                else
                {
                    dataAccess.SetData(index, adjacencyCluster);
                }
            }


            index = Params.IndexOfOutputParam("apertures");
            if (index != -1)
            {
                dataAccess.SetDataList(index, tuples_Result?.ConvertAll(x => new GooAperture(x.Item2)));
            }


            if (index_Successful != -1)
            {
                dataAccess.SetData(index_Successful, tuples_Result != null && tuples_Result.Count > 0);
            }
        }
    }
}