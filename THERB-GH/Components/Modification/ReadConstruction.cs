﻿using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Model;
using System.Windows.Forms;
using Rhino.Collections;
using System.Reflection;
using System.Net;
using Newtonsoft.Json;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class ReadConstruction : GH_Component
    {
        const string OPAQUE_URL = "https://stingray-app-vgak2.ondigitalocean.app/constructions";
        const string TRANSLUCENT_URL = "https://stingray-app-vgak2.ondigitalocean.app/windows";

        //const string OPAQUE_URL = "http://localhost:5000/constructions";
        //const string TRANSLUCENT_URL = "http://localhost:5000/windows";

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadConstruction()
          : base("ReadConstruction", "ReadConstruction",
              "read construction data",
              "THERB-GH", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Read construction json data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //TODO: RegisterOutputParamsをdynamicにしたい
            pManager.AddGenericParameter("Construction", "Construction", "construction list", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var flag = false;
            DA.GetData("Run", ref flag);
            if (!flag) return;

            //jsonデータを読み込む（データフォーマットはnotionを参照）
            var wc = new WebClient();

            string text = wc.DownloadString(OPAQUE_URL);
            string text2 = wc.DownloadString(TRANSLUCENT_URL);

            //List<Construction> constructions = JsonConvert.DeserializeObject<ResConstruction>(text).data;
            List<Opaque> opaques = JsonConvert.DeserializeObject<ResOpaque>(text).data;

            List<TranslucentPayload> translucentsPayload = JsonConvert.DeserializeObject<ResTranslucent>(text2).data;

            int opaqueCnt = opaques.Count;

            List<Translucent> translucents = new List<Translucent>();

            foreach(TranslucentPayload t in translucentsPayload)
            {
                translucents.Add(new Translucent(t, opaqueCnt));
            }

            List<Construction> constructions = new List<Construction>();

            foreach(Opaque opaque in opaques)
            {
                constructions.Add(new Construction(opaque));
            }

            foreach(Translucent t in translucents)
            {
                constructions.Add(new Construction(t));
            }

            DA.SetDataList("Construction", constructions);
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
            get { return new Guid("0454e523-af16-472e-a9df-b9a546e68c04"); }
        }
    }
}
