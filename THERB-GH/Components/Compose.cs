using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Collections.Generic;
using Rhino.Geometry.Intersect;
using Newtonsoft.Json;
using Model;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class Compose : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public Compose()
          : base("Compose", "Compose",
              "compose therb class from rooms, faces, windows and overhangs",
              "THERB-GH", "Modelling")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rooms", "Rooms", "Room classes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Faces", "Faces", "Face classes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Windows", "Windows", "Window classes", GH_ParamAccess.list);
            pManager.AddGenericParameter("Overhangs", "Overhangs", "Overhang classes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "Therb", "Therb class", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Room> roomList = new List<Room>();
            DA.GetDataList(0, roomList);
            List<Face> faceList = new List<Face>();
            DA.GetDataList(1, faceList);
            List<Window> windowList = new List<Window>();
            DA.GetDataList(2, windowList);
            List<Overhang> overhangList = new List<Overhang>();
            DA.GetDataList(3, overhangList);

            Therb therb = new Therb(roomList, faceList, windowList, overhangList);

            DA.SetData("Therb",therb);
        }

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
            get { return new Guid("b32956ef-fed0-4bb1-9492-191b2aa040a0"); }
        }
    }
}
