using Grasshopper.Kernel;
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
    public class ReadEnvelope : GH_Component
    {
        const string ENVELOPE_URL = "https://z3ekk9rish.execute-api.ap-northeast-1.amazonaws.com/dev/";
        //const string OPAQUE_URL = "https://n4lws74mn3.execute-api.ap-northeast-1.amazonaws.com/dev/";

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadEnvelope()
          : base("ReadEnvelope", "ReadEnvelope",
              "read envelope data",
              "THERB-GH", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Read envelope json data", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Envelopes", "Envelopes", "envelope list", GH_ParamAccess.list);
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

            string text = wc.DownloadString(ENVELOPE_URL);
            //string text2 = wc.DownloadString(OPAQUE_URL);

            List<EnvelopePayload> envelopePayload = JsonConvert.DeserializeObject<ResEnvelope>(text).data;

            //List<ConstructionPayload> opaques = JsonConvert.DeserializeObject<ResOpaque>(text2).data;

            //int opaqueCnt = opaques.Count;

            //windowのidを変える必要あり
            List<Envelope> envelopes = new List<Envelope>();

            foreach(EnvelopePayload e in envelopePayload)
            {
                envelopes.Add(new Envelope(e));
            }

            DA.SetDataList("Envelopes", envelopes);
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
            get { return new Guid("71427f32-11d1-4f4f-9637-f132949b328a"); }
        }
    }
}
