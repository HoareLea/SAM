using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifyVentilationProfileByFunction : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("e01a70b5-b59b-4b38-b012-b08728f76dc4");

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
        public SAMAnalyticalModifyVentilationProfileByFunction()
          : base("SAMAnalytical.ModifyVentilationProfileByFunction", "SAMAnalytical.ModifyVentilationProfileByFunction",
              "Modify Ventilation Profile By Function",
              "SAM", "Analytical02")
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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "_analyticalObject", NickName = "_analyticalObject", Description = "SAM AnalyticalModel or AdjacencyCluster", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam { Name = "_spaces/zones", NickName = "_spaces/zones", Description = "Objects such as Point, SAM Analytical InternalCondition etc.", Access = GH_ParamAccess.list, Optional = true}, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_String() { Name = "_function", NickName = "_function", Description = "Function", Access = GH_ParamAccess.item, Optional = false }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "ac/h_", NickName = "ac/h_", Description = "Air changes per hour", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new Param_Number() { Name = "m3/h_", NickName = "m3/h_", Description = "Air flow [m3/h]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

                Param_Number param_Number;

                param_Number = new Param_Number() { Name = "_factor_", NickName = "_factor_", Description = "Factor", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(1.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                param_Number = new Param_Number() { Name = "_setback_", NickName = "_setback_", Description = "Setback", Access = GH_ParamAccess.item, Optional = true };
                param_Number.SetPersistentData(0.0);
                result.Add(new GH_SAMParam(param_Number, ParamVisibility.Binding));

                result.Add(new GH_SAMParam(new Param_String() { Name = "description_", NickName = "description_", Description = "Description", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Binding));

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
                result.Add(new GH_SAMParam(new GooAnalyticalObjectParam() { Name = "AnalyticalObject", NickName = "AnalyticalObject", Description = "SAM AnalyticalObject", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooInternalConditionParam() { Name = "InternalConditions", NickName = "InternalConditions", Description = "SAM InternalConditions", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
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

            IAnalyticalObject analyticalObject = null;
            index = Params.IndexOfInputParam("_analyticalObject");
            if (index == -1 || !dataAccess.GetData(index, ref analyticalObject) || analyticalObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            string function = null;
            index = Params.IndexOfInputParam("_function");
            if (index == -1 || !dataAccess.GetData(index, ref function) || function == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double ach = double.NaN;
            index = Params.IndexOfInputParam("ac/h_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref ach);
            }

            double m3h = double.NaN;
            index = Params.IndexOfInputParam("m3/h_");
            if (index != -1)
            {
                dataAccess.GetData(index, ref m3h);
            }

            double factor = double.NaN;
            index = Params.IndexOfInputParam("_factor_");
            if (index != -1)
            {
                if(!dataAccess.GetData(index, ref factor))
                {
                    factor = 1.0;
                }
            }

            double setback = double.NaN;
            index = Params.IndexOfInputParam("_setback_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref setback))
                {
                    setback = 0.0;
                }
            }

            string description = null;
            index = Params.IndexOfInputParam("description_");
            if (index != -1)
            {
                if (!dataAccess.GetData(index, ref description))
                {
                    description = null;
                }
            }

            AdjacencyCluster adjacencyCluster = null;
            if (analyticalObject is AdjacencyCluster)
            {
                adjacencyCluster = (AdjacencyCluster)analyticalObject;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)analyticalObject).AdjacencyCluster;
            }

            List<Space> spaces = [];

            List<InternalCondition> internalConditions = [];

            if (adjacencyCluster is not null)
            {
                adjacencyCluster = new AdjacencyCluster(adjacencyCluster, true);

                List<IAnalyticalObject> analyticalObjects = [];
                index = Params.IndexOfInputParam("_spaces/zones");
                if (index != -1 && dataAccess.GetDataList(index, analyticalObjects) && analyticalObjects != null)
                {
                    foreach (IAnalyticalObject analyticalObject_Temp in analyticalObjects)
                    {
                        if(analyticalObject_Temp is not SAMObject sAMObject)
                        {
                            continue;
                        }

                        List<Space> spaces_Temp = [];
                        if (analyticalObject_Temp is Space space)
                        {
                            spaces_Temp.Add(adjacencyCluster.GetObject<Space>(sAMObject.Guid));
                        }
                        else if (analyticalObject_Temp is Zone zone)
                        {
                            spaces_Temp = adjacencyCluster.GetSpaces(zone);
                        }

                        spaces.AddRange(spaces_Temp);
                    }
                }

                if(spaces is null || spaces.Count == 0)
                {
                    spaces = adjacencyCluster.GetSpaces();
                }
            }

            if (adjacencyCluster != null && spaces != null && spaces.Count != 0)
            {
                foreach (Space space in spaces)
                {
                    if (space?.InternalCondition is not InternalCondition internalCondition)
                    {
                        continue;
                    }

                    if (function is null)
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunction);
                    }
                    else
                    {
                        Function function_Temp = Analytical.Convert.ToSAM_Function(function);
                        if(function_Temp is not null)
                        {
                            FunctionType functionType = function_Temp.GetFunctionType();

                            if (functionType == FunctionType.tcmvc || functionType == FunctionType.tcmvn || functionType == FunctionType.tmmvn)
                            {
                                int vent_Index = 3;

                                if (!double.IsNaN(ach))
                                {
                                    function_Temp[vent_Index] = ach;
                                }
                                else if (!double.IsNaN(m3h))
                                {
                                    double volume = space.Volume(adjacencyCluster);
                                    if (!double.IsNaN(volume))
                                    {
                                        function_Temp[vent_Index] = volume / m3h;
                                    }
                                }
                            }
                        }

                        internalCondition.SetValue(InternalConditionParameter.VentilationFunction, function_Temp?.ToString() ?? function);
                    }

                    if(description is null)
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionDescription);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionDescription, description);
                    }

                    if(double.IsNaN(factor))
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionFactor);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionFactor, factor);
                    }

                    if (double.IsNaN(setback))
                    {
                        internalCondition.RemoveValue(InternalConditionParameter.VentilationFunctionSetback);
                    }
                    else
                    {
                        internalCondition.SetValue(InternalConditionParameter.VentilationFunctionSetback, setback);
                    }

                    space.InternalCondition = internalCondition;

                    internalConditions.Add(internalCondition);

                    adjacencyCluster.AddObject(space);
                }
            }

            if (analyticalObject is AdjacencyCluster)
            {
                analyticalObject = adjacencyCluster;
            }
            else if (analyticalObject is AnalyticalModel)
            {
                analyticalObject = new AnalyticalModel((AnalyticalModel)analyticalObject, adjacencyCluster);
            }

            index = Params.IndexOfOutputParam("AnalyticalObject");
            if (index != -1)
            {
                dataAccess.SetData(index, analyticalObject);
            }

            index = Params.IndexOfOutputParam("InternalConditions");
            if (index != -1)
            {
                dataAccess.SetDataList(index, internalConditions);
            }
        }
    }
}