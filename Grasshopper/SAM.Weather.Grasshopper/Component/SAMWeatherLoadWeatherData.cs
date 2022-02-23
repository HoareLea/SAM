using Grasshopper.Kernel;
using SAM.Weather.Grasshopper.Properties;
using SAM.Core.Grasshopper;
using System;
using System.Collections.Generic;

namespace SAM.Weather.Grasshopper
{
    //public class SAMWeatherLoadWeatherData : GH_SAMVariableOutputParameterComponent
    //{
    //    /// <summary>
    //    /// Gets the unique ID for this component. Do not change this ID after release.
    //    /// </summary>
    //    public override Guid ComponentGuid => new Guid("8b7dd18b-b3d9-45ba-bccf-f1f51cdfe604");

    //    /// <summary>
    //    /// The latest version of this component
    //    /// </summary>
    //    public override string LatestComponentVersion => "1.0.1";

    //    /// <summary>
    //    /// Provides an Icon for the component.
    //    /// </summary>
    //    protected override System.Drawing.Bitmap Icon => Resources.SAM_Small;

    //    /// <summary>
    //    /// Initializes a new instance of the SAM_point3D class.
    //    /// </summary>
    //    public SAMWeatherLoadWeatherData()
    //      : base("SAMWeather.LoadWeatherData", "SAMWeather.LoadWeatherData",
    //          "Load SAM Weather WeatherData",
    //          "SAM", "Weather")
    //    {
    //    }

    //    protected override GH_SAMParam[] Inputs
    //    {
    //        get
    //        {
    //            List<GH_SAMParam> result = new List<GH_SAMParam>();

    //            global::Grasshopper.Kernel.Parameters.Param_String @string = new global::Grasshopper.Kernel.Parameters.Param_String() { Name = "_epwPath", NickName = "_epwPath", Description = "EPW File Path", Access = GH_ParamAccess.item, Optional = false };
    //            result.Add(new GH_SAMParam(@string, ParamVisibility.Binding));

    //            return result.ToArray();
    //        }
    //    }

    //    /// <summary>
    //    /// Registers all the output parameters for this component.
    //    /// </summary>
    //    protected override GH_SAMParam[] Outputs
    //    {
    //        get
    //        {
    //            List<GH_SAMParam> result = new List<GH_SAMParam>();
    //            result.Add(new GH_SAMParam(new GooWeatherDataParam() { Name = "weatherData", NickName = "weatherData", Description = "SAM Weather Data", Access = GH_ParamAccess.item }, ParamVisibility.Binding));
    //            return result.ToArray();
    //        }
    //    }

    //    /// <summary>
    //    /// This is the method that actually does the work.
    //    /// </summary>
    //    /// <param name="dataAccess">
    //    /// The DA object is used to retrieve from inputs and store in outputs.
    //    /// </param>
    //    protected override void SolveInstance(IGH_DataAccess dataAccess)
    //    {
    //        int index = -1;

    //        index = Params.IndexOfInputParam("_epwPath");

    //        string path = null;
    //        if (index == -1 || !dataAccess.GetData(index, ref path) || string.IsNullOrEmpty(path))
    //        {
    //            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid data");
    //            return;
    //        }

    //        WeatherData weatherData = Convert.ToSAM(path);
    //        if (weatherData == null)
    //        {
    //            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Could not read EPW file");
    //            return;
    //        }



    //        dataAccess.SetData(0, new GooWeatherData(weatherData));
    //    }
    //}
}