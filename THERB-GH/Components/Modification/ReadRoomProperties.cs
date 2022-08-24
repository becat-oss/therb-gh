using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Model;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class ReadRoomProperty : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadRoomProperty()
          : base("ReadRoomProperty", "ReadRoomProperty",
              "Read room properties from class",
              "THERB-GH", "Utility")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Rooms", "Rooms", "list of Room", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //TODO: RegisterOutputParamsをdynamicにしたい
            //pManager.AddSurfaceParameter("surface", "surface", "extracted surface", GH_ParamAccess.list);
            pManager.AddIntegerParameter("id", "id", "id", GH_ParamAccess.list);
            pManager.AddBrepParameter("geometry","geometry","geometry", GH_ParamAccess.list);
            //pManager.AddPointParameter("centroid", "centroid", "centroid", GH_ParamAccess.list);
            pManager.AddNumberParameter("volume", "volume", "volume", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Therb therb = null;
            List<Room> roomList = new List<Room>();
            //DA.GetData(0, ref therb);
            DA.GetDataList(0, roomList);
            //faceList.AddRange(therb.faces);
            //List<Face> faceList = therb.faces;
            List<int> idList = new List<int>();
            List<Brep> geometryList = new List<Brep>();
            //List<Point3d> centroidList = new List<Point3d>();
            List<double> volumeList = new List<double>();

            roomList.ForEach(room =>
            {
                idList.Add(room.id);
                geometryList.Add(room.geometry);
                //centroidList.Add(room.centroid);
                volumeList.Add(room.volume);
            });

            DA.SetDataList("id", idList);
            DA.SetDataList("geometry", geometryList);
            //DA.SetDataList("centroid", centroidList);
            DA.SetDataList("volume", volumeList);
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
            get { return new Guid("d663f604-c8d0-42cc-8281-22e7b5a54f5e"); }
        }
    }
}
