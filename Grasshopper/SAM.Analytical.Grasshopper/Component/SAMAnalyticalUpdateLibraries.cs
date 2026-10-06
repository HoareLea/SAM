// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalUpdateLibraries : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("0300beb1-0074-4544-8ad1-1b12bff777fd");

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
        public SAMAnalyticalUpdateLibraries()
          : base("SAMAnalytical.UpdateLibraries", "SAMAnalytical.UpdateLibraries",
              "Update Libraries in analyticalModel",
              "SAM", "Analytical04")
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

                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "_analyticalModel", NickName = "_analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_libraries", NickName = "_libraries", Description = "SAM Libraries (MaterialLibraries or/and ProfileLibraries)", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_Boolean param_Boolean = new global::Grasshopper.Kernel.Parameters.Param_Boolean() { Name = "_missingOnly", NickName = "_missingOnly", Description = "Copy only missing objects from library", Access = GH_ParamAccess.item, Optional = true };
                param_Boolean.SetPersistentData(true);
                result.Add(new GH_SAMParam(param_Boolean, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalModelParam() { Name = "analyticalModel", NickName = "analyticalModel", Description = "SAM Analytical Model", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_analyticalModel");
            AnalyticalModel analyticalModel = null;
            if (index == -1 || !dataAccess.GetData(index, ref analyticalModel) || analyticalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_libraries");
            List<IJSAMObject> jSAMObjects = new List<IJSAMObject>();
            if (index == -1 || !dataAccess.GetDataList(index, jSAMObjects) || jSAMObjects == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<ISAMLibrary> sAMLibraries = jSAMObjects.FindAll(x => x is ISAMLibrary).ConvertAll(x => (ISAMLibrary)x);

            index = Params.IndexOfInputParam("_missingOnly");
            bool missingOnly = true;
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref missingOnly))
                {

                }
            }

            foreach (ISAMLibrary sAMLibrary in sAMLibraries)
            {
                if (sAMLibrary is MaterialLibrary)
                {
                    IEnumerable<IMaterial> materials = missingOnly ? Analytical.Query.Materials(analyticalModel.AdjacencyCluster, (MaterialLibrary)sAMLibrary) : ((MaterialLibrary)sAMLibrary).GetMaterials();
                    if (materials != null)
                    {
                        analyticalModel = new AnalyticalModel(analyticalModel);
                        materials?.ToList().ForEach(x => analyticalModel.AddMaterial(x));
                    }
                }
                else if (sAMLibrary is ProfileLibrary)
                {

                    IEnumerable<Profile> profiles = missingOnly ? Analytical.Query.Profiles(analyticalModel.AdjacencyCluster, (ProfileLibrary)sAMLibrary) : ((ProfileLibrary)sAMLibrary).GetProfiles();
                    if (profiles != null)
                    {
                        analyticalModel = new AnalyticalModel(analyticalModel);
                        profiles?.ToList().ForEach(x => analyticalModel.AddProfile(x));
                    }
                }
                else if (sAMLibrary is InternalConditionLibrary)
                {

                    IEnumerable<InternalCondition> internalConditions = ((InternalConditionLibrary)sAMLibrary).GetInternalConditions();
                    if (internalConditions != null)
                    {
                        analyticalModel = new AnalyticalModel(analyticalModel);
                        internalConditions?.ToList().ForEach(x => analyticalModel.AddInternalCondition(x));
                    }
                }
            }



            index = Params.IndexOfOutputParam("analyticalModel");
            if (index != -1)
            {
                dataAccess.SetData(index, new GooAnalyticalModel(analyticalModel));
            }
        }
    }
}
