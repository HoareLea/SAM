using Grasshopper.Kernel;
using SAM.Core.Grasshopper.Properties;
using System;

namespace SAM.Core.Grasshopper
{
    public class SAMCoreCreateLocation : GH_SAMComponent
    {
        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid => new Guid("3b78dc09-962a-48a4-9631-ef5858eb623f");

        /// <summary>
        /// The latest version of this component
        /// </summary>
        public override string LatestComponentVersion => "1.0.1";

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.SAM_Small3;

        /// <summary>
        /// Initializes a new instance of the SAM_point3D class.
        /// </summary>
        public SAMCoreCreateLocation()
          : base("SAMCore.CreateLocation", "SAMCore.CreateLocation",
              "Create Location, , default London Heathrow",
              "SAM", "Core")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager inputParamManager)
        {
            Location location = Core.Query.DefaultLocation();
            
            inputParamManager.AddTextParameter("_name", "_name", "Name", GH_ParamAccess.item, location.Name);
            inputParamManager.AddNumberParameter("_elevation", "_elevation", "Elevation", GH_ParamAccess.item, location.Elevation);
            inputParamManager.AddNumberParameter("_latitude", "_latitude", "Latitude", GH_ParamAccess.item, location.Latitude);
            inputParamManager.AddNumberParameter("_longitude", "_longitude", "Longitude", GH_ParamAccess.item, location.Longitude);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_OutputParamManager outputParamManager)
        {
            outputParamManager.AddParameter(new GooLocationParam(), "Location", "Location", "SAM Location", GH_ParamAccess.item);
            //outputParamManager.AddGenericParameter("Points", "Pts", "Snap points", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="dataAccess">
        /// The DA object is used to retrieve from inputs and store in outputs.
        /// </param>
        protected override void SolveInstance(IGH_DataAccess dataAccess)
        {
            string name = string.Empty;
            if (!dataAccess.GetData(0, ref name))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double elevation = double.NaN;
            if (!dataAccess.GetData(1, ref elevation))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double latitude = double.NaN;
            if (!dataAccess.GetData(2, ref latitude))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            double longitude = double.NaN;
            if (!dataAccess.GetData(3, ref longitude))
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
                return;
            }

            dataAccess.SetData(0, new GooLocation(new Location(name, longitude, latitude, elevation)));
        }
    }
}