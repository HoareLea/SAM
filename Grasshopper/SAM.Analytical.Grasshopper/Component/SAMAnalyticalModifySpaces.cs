using Grasshopper.Kernel;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalModifySpaces : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("f3105082-dd14-4b4e-bd79-49de1497548f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalModifySpaces()
          : base("SAMAnalytical.ModifySpaces", "SAMAnalytical.ModifySpaces",
              "Modify Spaces in AdjacencyCluster or AnalyticalModel \n You can ONLY apply one value to input object or list of objects",
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_analytical", NickName = "_analytical", Description = "SAM Analytical AdjacencyCluster or AnalyticalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooSpaceParam() { Name = "_spaces_", NickName = "_spaces_", Description = "Spaces to be modified. All spaces will be modified if input not provided", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "occupancy_", NickName = "occupancy_", Description = "Space occupancy [p]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "designCoolingLoad_", NickName = "designCoolingLoad_", Description = "Design Cooling Load [W]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "designHeatingLoad_", NickName = "designHeatingLoad_", Description = "Design Heating Load [W]", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ventilationRiserName_", NickName = "ventilationRiserName_", Description = "Ventilation Riser Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "heatingRiserName_", NickName = "heatingRiserName_", Description = "Heating Riser Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "coolingRiserName_", NickName = "coolingRiserName_", Description = "Cooling Riser Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "daylightFactor_", NickName = "daylightFactor_", Description = "Daylight Factor", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "supplyAirFlow_", NickName = "supplyAirFlow_", Description = "Supply Air Flow", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "exhaustAirFlow_", NickName = "exhaustAirFlow_", Description = "Exhaust Air Flow", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "ventilationSystemTypeName_", NickName = "ventilationSystemTypeName_", Description = "Ventilation System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "coolingSystemTypeName_", NickName = "coolingSystemTypeName_", Description = "Cooling System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "heatingSystemTypeName_", NickName = "heatingSystemTypeName_", Description = "Heating System Type Name", Access = GH_ParamAccess.item, Optional = true }, ParamVisibility.Voluntary));
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
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "analytical", NickName = "analytical", Description = "SAM Analytical Object", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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
            
            SAMObject sAMObject = null;
            index = Params.IndexOfInputParam("_analytical");
            if (index == -1 || !dataAccess.GetData(index, ref sAMObject) || sAMObject == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            AdjacencyCluster adjacencyCluster = null;
            if (sAMObject is AnalyticalModel)
            {
                adjacencyCluster = ((AnalyticalModel)sAMObject).AdjacencyCluster;
            }
            else if (sAMObject is AdjacencyCluster)
            {
                adjacencyCluster = new AdjacencyCluster((AdjacencyCluster)sAMObject);
            }
            else
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            if (adjacencyCluster != null)
            {
                List<Space> spaces = new List<Space>();

                index = Params.IndexOfInputParam("_spaces_");
                if (index != -1)
                {
                    dataAccess.GetDataList(index, spaces);
                }

                if (spaces == null || spaces.Count == 0)
                {
                    spaces = adjacencyCluster.GetSpaces();
                }

                if (spaces != null && spaces.Count != 0)
                {
                    double occupancy = double.NaN;
                    double designCoolingLoad = double.NaN;
                    double designHeatingLoad = double.NaN;
                    string ventilationRiserName = null;
                    string heatingRiserName = null;
                    string coolingRiserName = null;
                    double daylightFactor = double.NaN;
                    double supplyAirFlow = double.NaN;
                    double exhaustAirFlow = double.NaN;
                    string ventilationSystemTypeName = null;
                    string coolingSystemTypeName = null;
                    string heatingSystemTypeName = null;

                    double @double = double.NaN;
                    string @string = null;

                    index = Params.IndexOfInputParam("occupancy_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        occupancy = @double;
                    }

                    index = Params.IndexOfInputParam("designCoolingLoad_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        designCoolingLoad = @double;
                    }

                    index = Params.IndexOfInputParam("designHeatingLoad_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        designHeatingLoad = @double;
                    }

                    index = Params.IndexOfInputParam("ventilationRiserName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        ventilationRiserName = @string;
                    }

                    index = Params.IndexOfInputParam("heatingRiserName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        heatingRiserName = @string;
                    }

                    index = Params.IndexOfInputParam("coolingRiserName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        coolingRiserName = @string;
                    }

                    index = Params.IndexOfInputParam("daylightFactor_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        daylightFactor = @double;
                    }

                    index = Params.IndexOfInputParam("supplyAirFlow_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        supplyAirFlow = @double;
                    }

                    index = Params.IndexOfInputParam("exhaustAirFlow_");
                    if (index != -1 && dataAccess.GetData(index, ref @double))
                    {
                        exhaustAirFlow = @double;
                    }

                    index = Params.IndexOfInputParam("ventilationSystemTypeName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        ventilationSystemTypeName = @string;
                    }

                    index = Params.IndexOfInputParam("coolingSystemTypeName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        coolingSystemTypeName = @string;
                    }

                    index = Params.IndexOfInputParam("heatingSystemTypeName_");
                    if (index != -1 && dataAccess.GetData(index, ref @string))
                    {
                        heatingSystemTypeName = @string;
                    }

                    for(int i =0; i < spaces.Count;i++)
                    {
                        if(spaces[i] == null)
                        {
                            continue;
                        }

                        Space space = adjacencyCluster.GetObject<Space>(spaces[i].Guid);
                        if (space == null)
                        {
                            continue;
                        }

                        space = new Space(space);

                        if (!double.IsNaN(occupancy))
                        {
                            space.SetValue(SpaceParameter.Occupancy, occupancy);
                        }

                        if (!double.IsNaN(designCoolingLoad))
                        {
                            space.SetValue(SpaceParameter.DesignCoolingLoad, designCoolingLoad);
                        }

                        if (!double.IsNaN(designHeatingLoad))
                        {
                            space.SetValue(SpaceParameter.DesignHeatingLoad, designHeatingLoad);
                        }

                        if (ventilationRiserName != null)
                        {
                            space.SetValue(SpaceParameter.VentilationRiserName, ventilationRiserName);
                        }

                        if (coolingRiserName != null)
                        {
                            space.SetValue(SpaceParameter.CoolingRiserName, coolingRiserName);
                        }

                        if (heatingRiserName != null)
                        {
                            space.SetValue(SpaceParameter.HeatingRiserName, heatingRiserName);
                        }

                        if (!double.IsNaN(daylightFactor))
                        {
                            space.SetValue(SpaceParameter.DaylightFactor, daylightFactor);
                        }

                        if (!double.IsNaN(supplyAirFlow))
                        {
                            space.SetValue(SpaceParameter.SupplyAirFlow, supplyAirFlow);
                        }

                        if (!double.IsNaN(exhaustAirFlow))
                        {
                            space.SetValue(SpaceParameter.ExhaustAirFlow, exhaustAirFlow);
                        }

                        InternalCondition internalCondition = space.InternalCondition;
                        if (internalCondition != null)
                        {
                            if (ventilationSystemTypeName != null)
                            {
                                internalCondition.SetValue(InternalConditionParameter.VentilationSystemTypeName, ventilationSystemTypeName);
                            }

                            if (coolingSystemTypeName != null)
                            {
                                internalCondition.SetValue(InternalConditionParameter.CoolingSystemTypeName, coolingSystemTypeName);
                            }

                            if (heatingSystemTypeName != null)
                            {
                                internalCondition.SetValue(InternalConditionParameter.HeatingSystemTypeName, heatingSystemTypeName);
                            }

                            space.InternalCondition = internalCondition;
                        }

                        adjacencyCluster.AddObject(space);
                    }
                }
            }

            if (sAMObject is AnalyticalModel)
            {
                sAMObject = new AnalyticalModel((AnalyticalModel)sAMObject, adjacencyCluster);
            }
            else if (sAMObject is AdjacencyCluster)
            {
                sAMObject = adjacencyCluster;
            }

            index = Params.IndexOfOutputParam("analytical");
            if(index != -1)
                dataAccess.SetData(index, sAMObject);
        }
    }
}