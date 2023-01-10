using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using THERBgh;

namespace THERB_GH
{
    public class ReadWeather : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReadWeather class.
        /// </summary>
        public ReadWeather()
          : base("ReadWeather", "ReadWeather",
              "read weather data",
              "THERB-GH", "IO")
        {
        }
        [Obsolete]
        static readonly List<string> CITY_LIST = new List<string>()
        {
            "Fukuoka", "Kumamoto"
        };

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Weather", "Weather", "weather list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var weatherList = new List<Weather>();
            CITY_LIST.ForEach(city => { weatherList.Add(new Weather(city)); });
            DA.SetDataList("Weather", weatherList);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("313BF1DD-133C-4315-95B6-A9C68B4152C0"); }
        }
    }
}