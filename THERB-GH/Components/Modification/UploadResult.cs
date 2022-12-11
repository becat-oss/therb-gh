using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Model;
using Newtonsoft.Json;
using Rhino.Geometry;
using THERBgh;

namespace Components.Modification
{
    public class UploadResult : GH_Component
    {
        const string UPLOAD_FILE_O = "o.dat";

        const int MAX_SERVER_TRY_COUNT = 6;
        //readonly static string[] POST_URLS = new string[5] {
        //    "https://oyster-app-8jboe.ondigitalocean.app/therb/result",
        //    "https://oyster-app-8jboe.ondigitalocean.app/therb/result",
        //    "https://oyster-app-8jboe.ondigitalocean.app/therb/result",
        //    "https://oyster-app-8jboe.ondigitalocean.app/therb/result",
        //    "https://oyster-app-8jboe.ondigitalocean.app/therb/result"
        //};
        string POST_URL = "https://oyster-app-8jboe.ondigitalocean.app/therb/result";

        /// <summary>
        /// Initializes a new instance of the UploadResult class.
        /// </summary>
        public UploadResult()
          : base("UploadResult", "UploadResult",
              "upload result",
              "THERB-GH", "IO")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "therb", "THERB class", GH_ParamAccess.item);
            pManager.AddGenericParameter("_Envelope", "Envelope", "Envelope class", GH_ParamAccess.item);
            pManager.AddTextParameter("o_dat_file_path", "o_dat_file_path", "o.dat file path", GH_ParamAccess.item);
            pManager.AddBooleanParameter("run", "run", "run upload result", GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("result","result","result data", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var done = false;
            DA.GetData("run", ref done);
            if (!done) return;

            Therb therb = null;
            DA.GetData(0, ref therb);

            Envelope envelope = null;
            DA.GetData(1, ref envelope);

            var path = "";
            DA.GetData("o_dat_file_path", ref path);

            if (string.IsNullOrEmpty(path)) throw new Exception("pathが読み取れませんでした。");

            char[] seps = { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
            var paths = path.Split(seps);
            var name = paths[paths.Length - 2];// RunSimulationのnameと同じ

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    MultipartFormDataContent content = new MultipartFormDataContent();

                    StreamContent streamContentOFile = new StreamContent(fs);
                    streamContentOFile.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "data",
                                FileName = UPLOAD_FILE_O
                            };

                    StringContent stringContentName = new StringContent(name);
                    stringContentName.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "name",
                            };

                    content.Add(streamContentOFile);
                    content.Add(stringContentName);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, POST_URL);
                    request.Content = content;
                    var responseTask = new HttpClient().SendAsync(request);
                    responseTask.Wait();
                    var response = responseTask.Result;

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseContentStr = "";
                        Debug.WriteLine(response.Content);
                        MessageBox.Show("oファイル送信できました。");
                        using (Stream responseStream = response.Content.ReadAsStreamAsync().Result)
                        {
                            using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                            {
                                responseContentStr = sr.ReadToEnd();
                                DA.SetData("result", responseContentStr);
                            }
                        }

                        var responseContent = JsonConvert.DeserializeObject<ResponseContent>(responseContentStr);
                        therb.PostArea(responseContent.data.project_id, envelope);

                    }
                    Debug.WriteLine(response.StatusCode);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("error: " + e.Message);
                    MessageBox.Show("送信できませんでした。");
                    throw new Exception("送信できませんでした。");
                    //Task.Delay(1000);
                }
            }
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
            get { return new Guid("5D5ACFE6-B114-4F48-9A35-0E57826680C5"); }
        }
    }
    public class ResponseContent
    {

        public Data data;
        public string message;
        public string status;
        public class Data
        {
            public string api;
            public string project_id;
            public string url;
        }
    }
}