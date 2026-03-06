// SPDX-License-Identifier: LGPL-3.0-or-later
// Copyright (c) 2020–2026 Michal Dengusiak & Jakub Ziolkowski and contributors

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalSetInternalConditionByInternalConditionLibrary : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("defb11a7-4605-42d0-8d64-481df23dec7e");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Core.Convert.ToBitmap(Resources.SAM_Small);

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalSetInternalConditionByInternalConditionLibrary()
          : base("SAMAnalytical.SetInternalConditionByInternalConditionLibrary", "SAMAnalytical.SetInternalConditionByInternalConditionLibrary",
              "Set Internal Condition By InternalConditionLibrary",
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
                List<GH_SAMParam> result = new List<GH_SAMParam>();
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analyticals", NickName = "_analyticals", Description = "SAM Analytical Objects such as AdjacencyCluster or AnalyticalModel or Space", Access = GH_ParamAccess.list }, ParamVisibility.Binding));

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_names_", NickName = "_ICnames_", Description = "InternalCondition Names or SAM InternalCondition Object \n*use .GetDefaultLibrary and SelectByName \n to select requred IC ", Access = GH_ParamAccess.list };
                genericObject.SetPersistentData(new List<string>() { "S39_OfficeOpen" });
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "spaces_", NickName = "spaces_", Description = "SAM Analytical Spaces \n*if none all spaces from model will be included", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionLibraryParam { Name = "internalConditionLibrary_", NickName = "internalConditionLibrary_", Description = "SAM Analytical InternalConditionLibrary \n*optional", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "Analyticals", NickName = "Analyticals", Description = "SAM Analytical Objects such as AdjacencyCluster or AnalyticalModel or Space", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam { Name = "Spaces", NickName = "Spaces", Description = "SAM Analytical Spaces", Access = GH_ParamAccess.list }, ParamVisibility.Voluntary));
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

            index = Params.IndexOfInputParam("_analyticals");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<SAMObject> sAMObjects = new List<SAMObject>();
            if (!dataAccess.GetDataList(index, sAMObjects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_names_");
            if (index == -1)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<object> objects = [];
            if (!dataAccess.GetDataList(index, objects))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Space> spaces_Input = new List<Space>();
            index = Params.IndexOfInputParam("spaces_");
            if (index == -1 || !dataAccess.GetDataList(index, spaces_Input))
                spaces_Input = null;

            InternalConditionLibrary internalConditionLibrary = null;
            index = Params.IndexOfInputParam("internalConditionLibrary_");
            if (index != -1)
                dataAccess.GetData(index, ref internalConditionLibrary);

            if (internalConditionLibrary == null)
                internalConditionLibrary = ActiveSetting.Setting.GetValue<InternalConditionLibrary>(AnalyticalSettingParameter.DefaultInternalConditionLibrary);

            List<InternalCondition> internalConditions = [];
            for(int i=0; i < objects.Count; i++)
            {
                object @object = objects[i];

                if (@object is IGH_Goo)
                    @object = ((dynamic)@object).Value;

                InternalCondition internalCondition = null;

                if (@object is string)
                {
                    internalCondition = internalConditionLibrary.GetInternalConditions((string)@object)?.FirstOrDefault();
                }
                else if (@object is InternalCondition)
                {
                    internalCondition = new InternalCondition((InternalCondition)@object);
                }

                internalConditions.Add(internalCondition);
            }

            List<Space> spaces = new List<Space>();

            List<Space> spaces_Output = new List<Space>();

            List<SAMObject> result = new List<SAMObject>();
            for(int i=0; i < sAMObjects.Count; i++)
            {
                SAMObject sAMObject = sAMObjects[i];

                if (sAMObject is Space)
                {
                    spaces.Add((Space)sAMObject);
                }
                else if (sAMObject is AdjacencyCluster)
                {
                    AdjacencyCluster adjacencyCluster = (AdjacencyCluster)sAMObject;
                    List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
                    if (spaces_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        
                        for (int j = 0; j < spaces_Temp.Count; j++)
                        {
                            Space space = spaces_Temp[j];

                            InternalCondition internalCondition = internalConditions[Core.Query.Clamp(j, 0, internalConditions.Count - 1)];

                            if (space == null)
                                continue;

                            if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                                continue;

                            Space space_New = new Space(space);

                            space_New.InternalCondition = internalCondition;
                            spaces_Output.Add(space_New);
                            adjacencyCluster.AddObject(space_New);
                        }
                    }
                    adjacencyCluster.AssignSpaceColors();
                    result.Add(adjacencyCluster);
                }
                else if (sAMObject is AnalyticalModel)
                {
                    AdjacencyCluster adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
                    List<Space> spaces_Temp = adjacencyCluster.GetSpaces();
                    if (spaces_Temp != null)
                    {
                        adjacencyCluster = (AdjacencyCluster)adjacencyCluster.Clone();
                        for (int j = 0; j < spaces_Temp.Count; j++)
                        {
                            Space space = spaces_Temp[j];

                            InternalCondition internalCondition = internalConditions[Core.Query.Clamp(j, 0, internalConditions.Count - 1)];
                            
                            if (space == null)
                                continue;

                            if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                                continue;

                            Space space_New = new Space(space);

                            space_New.InternalCondition = internalCondition;
                            spaces_Output.Add(space_New);
                            adjacencyCluster.AddObject(space_New);
                        }
                        adjacencyCluster.AssignSpaceColors();
                    }

                    result.Add(new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster));
                }
            }

            if (spaces != null && spaces.Count != 0)
            {
                for (int i = 0; i < spaces.Count; i++)
                {
                    Space space = spaces[i];

                    InternalCondition internalCondition = internalConditions[Core.Query.Clamp(i, 0, internalConditions.Count - 1)];

                    result.Add(space);

                    if (spaces_Input != null && spaces_Input.Find(x => x.Guid == space.Guid) == null)
                        continue;

                    Space space_New = new Space(space);

                    space_New.InternalCondition = internalCondition;
                    spaces_Output.Add(space_New);
                }
            }

            index = Params.IndexOfOutputParam("Analyticals");
            if (index != -1)
                dataAccess.SetDataList(index, result.ConvertAll(x => new GooJSAMObject<SAMObject>(x)));

            index = Params.IndexOfOutputParam("Spaces");
            if (index != -1)
                dataAccess.SetDataList(index, spaces_Output?.ConvertAll(x => new GooSpace(x)));
        }
    }
}
