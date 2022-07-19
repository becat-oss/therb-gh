using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using Rhino.Geometry.Intersect;
using Newtonsoft.Json;
using Model;
using Utils;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class ExportA : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExportA()
          : base("exportA", "exportA",
              "export a.dat",
              "THERB-GH", "Modelling")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "therb", "THERB class", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("a_dat", "a_dat", "a.dat file", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Therb therb = null;
            DA.GetData(0, ref therb);

            if (therb == null) return;

            List<Room> roomList = therb.rooms;
            string aDat = "*hourly data od forced room air ventilation \r\n";

            roomList.ForEach(room =>
            {
                aDat += "into room" + Converter.FillEmpty(room.id, 3) + "from " + Converter.FillEmpty(0, 3)
                    + "=>  \r\n"
                    + Converter.FillEmpty("from outdoor=0", 25)
                    + Converter.FillEmpty("- ", 6)
                    + string.Join("", Enumerable.Repeat(Converter.FillEmpty(room.volume / 2, 7, 1), 12)) + "\r\n" //12回繰り返して呼ぶようにしたい
                    + Converter.FillEmpty("quantity (m3/h)", 25)
                    + Converter.FillEmpty("- ", 6) + "\r\n"
                    + Converter.FillEmpty("(-1.:natural vent.)", 25)
                    + Converter.FillEmpty("- ", 6)
                    + string.Join("", Enumerable.Repeat(Converter.FillEmpty(room.volume / 2, 7, 1), 12)) + "\r\n"; //12回繰り返して呼ぶようにしたい
            });

            DA.SetData("a_dat", aDat);
        }

        //TODO:Utilsモジュールにうつす

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4d940719-485a-4f3f-96ba-58a40632dbd4"); }
        }
    }
}
