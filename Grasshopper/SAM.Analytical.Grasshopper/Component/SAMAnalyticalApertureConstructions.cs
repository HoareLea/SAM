// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalApertureConstructions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e379534a-795e-46b7-a160-41c13cd6a7f7");

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
        public SAMAnalyticalApertureConstructions()
          : base("SAMAnalytical.ApertureConstructions", "SAMAnalytical.ApertureConstructions",
              "Get Analytical Constructions from AdjacencyCluster or AnalyticalModel",
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_SAMAnalytical", NickName = "_SAMAnalytical", Description = "SAM Analytical Object ie.Panel, AdjacencyCluster", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "panelType_", NickName = "panelType_", Description = "PanelType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "apertureType_", NickName = "apertureType_", Description = "ApertureType", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooApertureConstructionParam() { Name = "ApertureConstructions", NickName = "ApertureConstructions", Description = "SAM Analytical Aperture Constructions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "Successful", NickName = "Successful", Description = "Successful", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, false);
            }

            index = Params.IndexOfInputParam("_SAMAnalytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Panel> panels = null;
            if (sAMObject is Panel)
            {
                panels = new List<Panel>() { (Panel)sAMObject };
            }
            else if (sAMObject is AdjacencyCluster)
            {
                panels = ((AdjacencyCluster)sAMObject).GetPanels();
            }
            else if (sAMObject is AnalyticalModel)
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
            index = Params.IndexOfInputParam("panelType_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }

            PanelType panelType = PanelType.Undefined;

            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    panelType = Analytical.Query.PanelType(((GH_String)objectWrapper.Value).Value);
                else
                    panelType = Analytical.Query.PanelType(objectWrapper.Value);
            }

            objectWrapper = null;
            index = Params.IndexOfInputParam("apertureType_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref objectWrapper);
            }

            ApertureType apertureType = ApertureType.Undefined;

            if (objectWrapper != null)
            {
                if (objectWrapper.Value is GH_String)
                    apertureType = Analytical.Query.ApertureType(((GH_String)objectWrapper.Value).Value);
                else
                    apertureType = Analytical.Query.ApertureType(objectWrapper.Value);
            }


            List<ApertureConstruction> apertureConstructions = Analytical.Query.ApertureConstructions(panels, apertureType, panelType);

            index = Params.IndexOfOutputParam("ApertureConstructions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, apertureConstructions?.ConvertAll(x => new GooApertureConstruction(x)));
            }

            index = Params.IndexOfOutputParam("Successful");
            if (index != -1)
            {
                dataAccess.SetData(index, apertureConstructions != null);
            }
        }
    }
}
