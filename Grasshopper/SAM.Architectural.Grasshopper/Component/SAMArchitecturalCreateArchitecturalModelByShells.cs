﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Architectural.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Architectural.Grasshopper
{
    public class SAMArchitecturalCreateArchitecturalModelByShells : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("2f3330e7-2fe9-41d2-bf4e-a9e557d612cf");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.0";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMArchitecturalCreateArchitecturalModelByShells()
          : base("SAMArchitectural.CreateArchitecturalModelByShells", "SAMArchitectural.CreateArchitecturalModelByShells",
              "SAMArchitectural Create ArchitecturalModel By Shells",
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

                global::Grasshopper.Kernel.Parameters.Param_GenericObject genericObject = new global::Grasshopper.Kernel.Parameters.Param_GenericObject() { Name = "_shells", NickName = "_shells", Description = "SAM Analytical Shells", Access = GH_ParamAccess.list };
                genericObject.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(genericObject, ParamVisibility.Binding));

                GooPartitionParam panelParam = new GooPartitionParam() { Name = "partitions_", NickName = "partitions_", Description = "SAM Architectural Partitions", Access = GH_ParamAccess.list, Optional = true };
                panelParam.DataMapping = GH_DataMapping.Flatten;
                result.Add(new GH_SAMParam(panelParam, ParamVisibility.Voluntary));

                global::Grasshopper.Kernel.Parameters.Param_Number paramNumber;

                paramNumber = new global::Grasshopper.Kernel.Parameters.Param_Number() { Name = "_groundElevation_", NickName = "_groundElevation_", Description = "Ground Elevation", Access = GH_ParamAccess.item };
                paramNumber.SetPersistentData(0);
                result.Add(new GH_SAMParam(paramNumber, ParamVisibility.Voluntary));

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
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "architecturalModel", NickName = "architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
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

            ArchitecturalModel architecturalModel = Create.ArchitecturalModel(shells, partitions, groundElevation, true, 0.01, minArea, maxDistance, maxAngle, silverSpacing, tolerance, Core.Tolerance.Angle);
            if(partitions == null || partitions.Count == 0)
            {
                architecturalModel.UpdateHostPartitionType(
                    curtainWallType: new WallType("Curtain Wall"), 
                    internalWallType: new WallType("Internal Wall"),
                    externalWallType: new WallType("External Wall"),
                    undergroundWallType: new WallType("Underground Wall"),
                    internalFloorType: new FloorType("Internal Floor"),
                    externalFloorType: new FloorType("External Floor"),
                    onGradeFloorType: new FloorType("On Grade Floor"),
                    undergroundFloorType: new FloorType("Underground Floor"),
                    undergroundCeilingFloorType: new FloorType("Underground Ceiling Floor"),
                    roofType: new RoofType("Roof"));
            }


            index = Params.IndexOfOutputParam("architecturalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooArchitecturalModel(architecturalModel));
        }
    }
}