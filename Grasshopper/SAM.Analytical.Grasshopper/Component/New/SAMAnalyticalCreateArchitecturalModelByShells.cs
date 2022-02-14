using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMAnalyticalCreateBuildingModelByShells : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2f3330e7-2fe9-41d2-bf4e-a9e557d612cf");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.3";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMAnalyticalCreateBuildingModelByShells()
          : base("SAMAnalytical.CreateBuildingModelByShells", "SAMAnalytical.CreateBuildingModelByShells",
              "Creates BuildingModel By Shells",
              "SAM WIP", "Analytical")
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shells", NickName = "_shells", Description = "SAM Analytical Shells", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                GooPartitionParam partitionParam = new GooPartitionParam() { Name = "partitions_", NickName = "partitions_", Description = "SAM Architectural Partitions", Access = GH_ParamAccess.list, Optional = true };
                partitionParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(partitionParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_groundElevation_", NickName = "_groundElevation_", Description = "Ground Elevation", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                GooLocationParam locationParam = new GooLocationParam() { Name = "_location_", NickName = "_location_", Description = "SAM Core Location", Access = GH_ParamAccess.item};
                locationParam.SetPersistentData(Core.Query.DefaultLocation());
                result.Add(new GH_SAMParam(partitionParam, ParamVisibility.Voluntary));

                GooAddressParam addressParam = new GooAddressParam() { Name = "_address_", NickName = "_address_", Description = "SAM Core Address", Access = GH_ParamAccess.item };
                addressParam.SetPersistentData(Core.Query.DefaultAddress());
                result.Add(new GH_SAMParam(addressParam, ParamVisibility.Voluntary));

                GooMaterialLibraryParam materialLibraryParam = new GooMaterialLibraryParam() { Name = "materialLibrary_", NickName = "materialLibrary_", Description = "SAM Core MaterialLibrary", Access = GH_ParamAccess.item, Optional = true };
                result.Add(new GH_SAMParam(materialLibraryParam, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxDistance_", NickName = "maxDistance_", Description = "Max Distance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.01);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "maxAngle_", NickName = "maxAngle_", Description = "Max Angle", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0.0872664626);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "silverSpacing_", NickName = "silverSpacing_", Description = "Silver Spacing for computation of Spaces", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "minArea_", NickName = "minArea_", Description = "Minimal Area", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.MacroDistance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "tolerance_", NickName = "tolerance_", Description = "Tolerance", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(Core.Tolerance.Distance);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Binding));


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
                result.Add(new GH_SAMParam(new GooBuildingModelParam() { Name = "buildingModel", NickName = "buildingModel", Description = "SAM Architectural BuildingModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            index = Params.IndexOfInputParam("_shells");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Shell> shells = new List<Shell>();
            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
                if (Geometry.Grasshopper.Query.TryGetSAMGeometries(objectWrapper, out List<Shell> shells_Temp) && shells_Temp != null)
                    shells.AddRange(shells_Temp);

            index = Params.IndexOfInputParam("partitions_");
            List<IPartition> partitions = null;
            if(index != -1)
            {
                partitions = new List<IPartition>();
                dataAccess.GetDataList(index, partitions);
            }

            index = Params.IndexOfInputParam("maxDistance_");
            double maxDistance = 0.01;
            if (index != -1)
                dataAccess.GetData(index, ref maxDistance);

            index = Params.IndexOfInputParam("maxAngle_");
            double maxAngle = 0.0872664626;
            if (index != -1)
                dataAccess.GetData(index, ref maxAngle);

            index = Params.IndexOfInputParam("silverSpacing_");
            double silverSpacing = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref silverSpacing);

            index = Params.IndexOfInputParam("_groundElevation_");
            double groundElevation = 0;
            if (index != -1)
                dataAccess.GetData(index, ref groundElevation);

            index = Params.IndexOfInputParam("minArea_");
            double minArea = Core.Tolerance.MacroDistance;
            if (index != -1)
                dataAccess.GetData(index, ref minArea);

            index = Params.IndexOfInputParam("tolerance_");
            double tolerance = Core.Tolerance.Distance;
            if (index != -1)
                dataAccess.GetData(index, ref tolerance);

            index = Params.IndexOfInputParam("_location_");
            Core.Location location = Core.Query.DefaultLocation();
            if (index != -1)
                dataAccess.GetData(index, ref location);

            index = Params.IndexOfInputParam("_address_");
            Core.Address address = Core.Query.DefaultAddress();
            if (index != -1)
                dataAccess.GetData(index, ref address);

            index = Params.IndexOfInputParam("materialLibrary_");
            Core.MaterialLibrary materialLibrary = null;
            if (index != -1)
                dataAccess.GetData(index, ref materialLibrary);

            BuildingModel buildingModel = Create.BuildingModel(shells, partitions, groundElevation, true, materialLibrary, 0.01, minArea, maxDistance, maxAngle, silverSpacing, tolerance, Core.Tolerance.Angle);
            if(buildingModel != null)
            {
                if (partitions == null || partitions.Count == 0)
                {
                    HostPartitionTypeLibrary hostPartitionTypeLibrary = Analytical.Query.DefaultHostPartitionTypeLibrary();
                    
                    buildingModel.UpdateHostPartitionType(
                        curtainWallType: hostPartitionTypeLibrary.GetHostPartitionType<WallType>(PartitionAnalyticalType.CurtainWall),
                        internalWallType: hostPartitionTypeLibrary.GetHostPartitionType<WallType>(PartitionAnalyticalType.InternalWall),
                        externalWallType: hostPartitionTypeLibrary.GetHostPartitionType<WallType>(PartitionAnalyticalType.ExternalWall),
                        undergroundWallType: hostPartitionTypeLibrary.GetHostPartitionType<WallType>(PartitionAnalyticalType.UndergroundWall),
                        internalFloorType: hostPartitionTypeLibrary.GetHostPartitionType<FloorType>(PartitionAnalyticalType.InternalFloor),
                        externalFloorType: hostPartitionTypeLibrary.GetHostPartitionType<FloorType>(PartitionAnalyticalType.ExternalFloor),
                        onGradeFloorType: hostPartitionTypeLibrary.GetHostPartitionType<FloorType>(PartitionAnalyticalType.OnGradeFloor),
                        undergroundFloorType: hostPartitionTypeLibrary.GetHostPartitionType<FloorType>(PartitionAnalyticalType.UndergroundFloor),
                        undergroundCeilingFloorType: hostPartitionTypeLibrary.GetHostPartitionType<FloorType>(PartitionAnalyticalType.UndergroundCeiling),
                        roofType: hostPartitionTypeLibrary.GetHostPartitionType<RoofType>(PartitionAnalyticalType.Roof),
                        materialLibrary: materialLibrary);
                }

                buildingModel.Location = location;
                buildingModel.Address = address;

            }

            index = Params.IndexOfOutputParam("buildingModel");
            if (index != -1)
                dataAccess.SetData(index, new GooBuildingModel(buildingModel));
        }
    }
}