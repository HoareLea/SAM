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
    public class SAMAnalyticalGeometry : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("6628ec6c-84ba-4f2b-97cf-f2ccdbe8599a");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.2";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalGeometry()
          : base("SAMAnalytical.Geometry", "SAMAnalytical.Geometry",
              "Convert SAM Analytical to GH Geometry ie. Panel to Surface",
              "SAM", "Analytical01")
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

                result.Add(new GH_SAMParam(new GooJSAMObjectParam<SAMObject>() { Name = "_SAMAnalytical", NickName = "_SAMAnalytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean;

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_cutApertures", NickName = "_cutApertures", Description = "Cut Apertures If Applicable", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_includeFrame", NickName = "_includeFrame", Description = "Include Frame", Access = GH_ParamAccess.item };
                param_Boolean.SetPersistentData(false);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Number param_Number;
                param_Number = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_tolerance_", NickName = "_tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                param_Number.SetPersistentData(0.00001);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Geometry() { Name = "Geometry", NickName = "Geometry", Description = "Geometry in GH ie.Surface", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_SAMAnalytical");
            SAMObject sAMObject = null;
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool cutApertures = false;
            index = Params.IndexOfInputParam("_cutApertures");
            if (index == -1 || !dataAccess.GetData(index, ref cutApertures))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            bool includeFrame = false;
            index = Params.IndexOfInputParam("_includeFrame");
            if (index == -1 || !dataAccess.GetData(index, ref includeFrame))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double tolerance = 0.00001;
            index = Params.IndexOfInputParam("_tolerance_");
            if (index == -1 || !dataAccess.GetData(index, ref tolerance))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<IGH_Goo> result = new List<IGH_Goo>();

            if (sAMObject is Panel)
            {
                result.AddRange(((Panel)sAMObject).ToGrasshopper(cutApertures, tolerance));
            }
            else if (sAMObject is Aperture)
            {
                result.AddRange(((Aperture)sAMObject).ToGrasshopper(includeFrame));
            }
            else if (sAMObject is PlanarBoundary3D)
            {
                result.Add(((PlanarBoundary3D)sAMObject).ToGrasshopper());
            }
            else if (sAMObject is Space)
            {
                result.Add(((Space)sAMObject).ToGrasshopper());
            }
            else if (sAMObject is AdjacencyCluster)
            {
                List<GH_Brep> breps = ((AdjacencyCluster)sAMObject).ToGrasshopper(cutApertures, includeFrame, tolerance);
                if (breps != null)
                    result.AddRange(breps);

            }
            else if (sAMObject is AnalyticalModel)
            {
                List<GH_Brep> breps = ((AnalyticalModel)sAMObject)?.AdjacencyCluster?.ToGrasshopper(cutApertures, includeFrame, tolerance);
                if (breps != null)
                    result.AddRange(breps);
            }

            index = Params.IndexOfOutputParam("Geometry");
            if (index != -1)
            {
                dataAccess.SetDataList(index, result);
            }
        }
    }
}
