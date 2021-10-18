using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using SAM.Analytical.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using SAM.Geometry.Spatial;
using System;
using System.Collections.Generic;

namespace SAM.Analytical.Grasshopper
{
    public class SAMArchitecturalAddAirPartitions : GH_SAMVariableOutputParameterComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("31344240-ad1f-48dc-8700-f5df3dd4fb90");

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
        public SAMArchitecturalAddAirPartitions()
          : base("SAMArchitectural.AddAirPartitions", "SAMArchitectural.AddAirPartitions",
              "Add AirPartitions",
              "SAM", "Architectural")
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
                result.Add(new GH_SAMParam(new GooArchitecturalModelParam() { Name = "_architecturalModel", NickName = "_architecturalModel", Description = "SAM Architectural ArchitecturalModel", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new global::Grasshopper.Kernel.Parameters.Param_GenericObject { Name = "_planes", NickName = "_planes", Description = "SAM Geometry Planes", Access = GH_ParamAccess.list }, ParamVisibility.Binding));
                result.Add(new GH_SAMParam(new GooRoomParam() { Name = "rooms_", NickName = "rooms_", Description = "SAM Architectural Rooms", Access = GH_ParamAccess.list, Optional = true }, ParamVisibility.Voluntary));
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

            ArchitecturalModel architecturalModel = null;
            index = Params.IndexOfInputParam("_architecturalModel");
            if (index == -1 || !dataAccess.GetData(index, ref architecturalModel) || architecturalModel == null)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            index = Params.IndexOfInputParam("_planes");
            List<GH_ObjectWrapper> objectWrappers = new List<GH_ObjectWrapper>();
            if (index == -1 || !dataAccess.GetDataList(index, objectWrappers) || objectWrappers == null || objectWrappers.Count == 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            List<Plane> planes = new List<Plane>();

            foreach (GH_ObjectWrapper objectWrapper in objectWrappers)
            {
                Plane plane = null;

                object @object = objectWrapper.Value;
                if (@object is IGH_Goo)
                {
                    @object = (@object as dynamic).Value;
                }

                if (@object is double)
                {
                    plane = Geometry.Spatial.Create.Plane((double)@object);
                }
                else if (@object is string)
                {
                    if (double.TryParse((string)@object, out double elevation_Temp))
                    {
                        plane = Geometry.Spatial.Create.Plane(elevation_Temp);
                    }
                }
                else if (@object is Rhino.Geometry.Plane)
                {
                    plane = Geometry.Grasshopper.Convert.ToSAM(((Rhino.Geometry.Plane)@object));
                }
                else if (@object is Plane)
                {
                    plane = (Plane)@object;
                }

                if (plane == null)
                {
                    continue;
                }

                planes.Add(plane);
            }

            List<Room> rooms = null;
            index = Params.IndexOfInputParam("spaces_");
            if (index != -1)
            {
                List<Room> rooms_Temp = new List<Room>();

                if (dataAccess.GetDataList(index, rooms_Temp) && rooms_Temp != null && rooms_Temp.Count != 0)
                {
                    rooms = rooms_Temp;
                }
            }

            architecturalModel = new ArchitecturalModel(architecturalModel);

            architecturalModel.AddAirPartitions(planes, rooms);

            index = Params.IndexOfOutputParam("architecturalModel");
            if (index != -1)
                dataAccess.SetData(index, new GooArchitecturalModel(architecturalModel));
        }
    }
}