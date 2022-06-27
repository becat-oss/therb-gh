﻿using Grasshopper.Kernel;
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
    public class ReadProperty : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadProperty()
          : base("ReadProperty", "ReadProperty",
              "Read properties from class",
              "THERB-GH", "Modelling")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "Therb", "Therb data", GH_ParamAccess.item);
            //pManager.AddTextParameter("property", "property", "property to extract", GH_ParamAccess.item);
            //pManager.AddTextParameter("class", "class", "room or face or window", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //TODO: RegisterOutputParamsをdynamicにしたい
            //pManager.AddSurfaceParameter("surface", "surface", "extracted surface", GH_ParamAccess.list);
            pManager.AddBrepParameter("surface", "surface", "extracted surface", GH_ParamAccess.list);
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

            List<Face> faceList = therb.faces;
            //List<Face> faceList = new List<Face>();
            //DA.GetDataList(0, faceList);

            //string property = "";
            //DA.GetData(1, ref property);
            List<Surface> surfaceList = new List<Surface>();

            faceList.ForEach(face =>
            {
                surfaceList.Add(face.geometry);
            });

            //TODO:型の問題を解決
            DA.SetDataList("surface", surfaceList);
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
            get { return new Guid("01bf9c59-9cce-48b5-b00b-0c0ecb858f63"); }
        }
    }
}
