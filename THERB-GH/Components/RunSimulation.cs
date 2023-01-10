using Grasshopper.Kernel;
using Grasshopper.GUI;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using Newtonsoft.Json;
//using Microsoft.WindowsAPICodePack.Dialogs;
using Model;
using System.Threading;

using System.Web;
using System.Net;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class RunSimulation : GH_Component
    {
        const string THERB_FILE_NAME = "Therb.exe";
        const string THERB_FOLDER_PATH = @"C:\therb";
        const string CREATE_FILE_B = "b.dat";
        const string CREATE_FILE_R = "r.dat";
        const string CREATE_FILE_T = "t.dat";
        const string CREATE_FILE_W = "w.dat";
        const string CREATE_FILE_A = "a.dat";
        const string CREATE_FILE_S = "s.dat";
        const string CREATED_FILE_O = "o.dat";

        const int MAX_SERVER_TRY_COUNT = 6;
        //const string POST_URL = "https://stingray-app-vgak2.ondigitalocean.app/therb/run";
        readonly static string[] POST_URLS = new string[5] {
            "https://oyster-app-8jboe.ondigitalocean.app/therb/run",
            "https://oyster-app-8jboe.ondigitalocean.app/therb/run",
            "https://oyster-app-8jboe.ondigitalocean.app/therb/run",
            "https://oyster-app-8jboe.ondigitalocean.app/therb/run",
            "https://oyster-app-8jboe.ondigitalocean.app/therb/run"
        };

        /*
        readonly string[] FILES_TO_COPY = new[]
        {
            "b.dat",
            "r.dat"
        };
        readonly List<string> CREATE_FILES = new List<string>()
        {
            "b.dat",
            "r.dat"
        };
        readonly List<string> FILES_TO_COPY = new List<string>()
        {
            "b.dat",
            "r.dat"
        };*/

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public RunSimulation()
          : base("RunSimulation", "Run simulation",
              "Run THERB simulation",
              "THERB-GH", "Simulation")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "therb", "THERB class", GH_ParamAccess.item);
            pManager.AddGenericParameter("Constructions", "Constructions", "Construction data", GH_ParamAccess.list);
            pManager.AddGenericParameter("Setting", "Setting", "Setting data", GH_ParamAccess.item);
            pManager.AddGenericParameter("Schedule", "Schedule", "Schedule data", GH_ParamAccess.item);
            pManager.AddTextParameter("name", "name", "simulation case name", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("cloud", "cloud", "run simulation in cloud", GH_ParamAccess.item);
            pManager.AddBooleanParameter("run", "run", "run THERB simulation", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("result", "result", "Room class", GH_ParamAccess.item);
            pManager.AddTextParameter("o_dat_file_path", "o_dat_file_path", "o.dat file path", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string name = "", namePath = "";

            bool cloudRun = false, done = false;
            Therb therb = null;
            DA.GetData(0, ref therb);

            therb.CheckTherb(out List<string> messages);
            foreach(string s in messages)
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, s);

            if (therb == null) return;

            List<Construction> constructionList = new List<Construction>();
            DA.GetDataList(1, constructionList);

            Schedule schedule = new Schedule();
            DA.GetData("Schedule", ref schedule);
            var hasSchedule = false;
            if (schedule.name != null) hasSchedule = true;

            //settingのinputをoptionalにしたい
            Setting setting = new Setting();
            DA.GetData("Setting", ref setting);

            DA.GetData("name", ref name);
            //DA.GetData("cloud", ref cloudRun);
            DA.GetData("run", ref done);
            if (!done) return;

            foreach (string s in messages)
                MessageBox.Show(s);

            var bDat = CreateDatData.CreateBDat(therb);
            var rDat = CreateDatData.CreateRDat(therb);

            //Vector3d northDirection = new Vector3d(0, 0, 0);
            var tDat = CreateDatData.CreateTDat(setting.startMonth,setting.endMonth,setting.northDirection, setting.weather);
            //TODO: settingが入力されていなかったら、デフォルト値を入れて計算
            var wDat = CreateDatData.CreateWDat(constructionList);
            var aDat = CreateDatData.CreateADat(therb,setting.ventilationRate);

            List<Room> rooms = therb.rooms;
            var sDat = "";
            if (hasSchedule)
                sDat = CreateDatData.CreateSDat(schedule,rooms);


            if (string.IsNullOrEmpty(name)) throw new Exception("nameが読み取れませんでした。");
            //if (!File.Exists(THERB_FILE_PATH)) throw new Exception("therb.exeが見つかりませんでした。");

            if (!Directory.Exists(THERB_FOLDER_PATH))
            {
                if (MessageBox.Show(THERB_FOLDER_PATH + "のフォルダが見つかりませんでした。" + Environment.NewLine + "作成しますか？"
                    , "", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Directory.CreateDirectory(THERB_FOLDER_PATH);
                }
                else
                {
                    throw new Exception("フォルダ作成がキャンセルされました。");
                }
            }

            namePath = Path.Combine(THERB_FOLDER_PATH, name);
            if (Directory.Exists(namePath)){
                if(MessageBox.Show(namePath + Environment.NewLine + "と同じフォルダが見つかりました。" + Environment.NewLine + "上書きしますか？"
                    , "", MessageBoxButtons.OKCancel) != DialogResult.OK) return;
            }
            else
            {
                Directory.CreateDirectory(namePath);
            }

            //TODO: 入力はtherbクラスとし、therbクラスからb.dat,r.datファイルを生成するロジックをこのコンポーネントの中で走らせる 


            //処理1. example/test/THERB_formatの中にあるデータをまるごとc://therb/{name}フォルダにコピー 
            string initDir = "";
            try
            {
                initDir = Directory.GetCurrentDirectory();
                if (string.IsNullOrEmpty(initDir)) throw new Exception();
            }
            catch
            {
                initDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            string dirPath = "";

            if (Directory.Exists(Path.Combine(initDir, "THERB")))
            {
                dirPath = Path.Combine(initDir, "THERB");
            }
            else
            {
                using (FolderBrowserDialog FBD = new FolderBrowserDialog()
                {
                    Description = "フォルダを開く",
                    SelectedPath = initDir,
                    ShowNewFolderButton = false
                })
                {
                    if (FBD.ShowDialog() == DialogResult.OK)
                    {
                        //MessageBox.Show(FBD.SelectedPath);
                        dirPath = FBD.SelectedPath;
                        if (string.IsNullOrEmpty(dirPath))
                        {
                            MessageBox.Show("パスが読み取れませんでした。");
                            return;
                        }
                        if (!File.Exists(Path.Combine(dirPath, THERB_FILE_NAME)) && !cloudRun)
                        {
                            MessageBox.Show("パス内にtherb.exeがありませんでした。" + Environment.NewLine + "中止します。");
                            return;
                        }
                    }
                    else return;
                }
            }

            #region TRY CommonOpenFileDialog
            /*
            using (CommonOpenFileDialog COFD = new CommonOpenFileDialog()
            { 
                Title = "フォルダを開く",
                InitialDirectory = initDir,
                IsFolderPicker = true
            }) {
                if (COFD.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    MessageBox.Show(COFD.FileName);
                }
            }
            */
            #endregion

            foreach (string pathFrom in Directory.EnumerateFiles(dirPath, "*", SearchOption.AllDirectories))
            {
                string pathTo = pathFrom.Replace(dirPath, namePath);

                string targetFolder = Path.GetDirectoryName(pathTo);
                if (Directory.Exists(targetFolder) == false)
                {
                    Directory.CreateDirectory(targetFolder);
                }
                File.Copy(pathFrom, pathTo, true);
            }

            //処理2. inputのb.dat,r.datデータをc://therb/{name}フォルダにb.dat,r.datファイルとして書き込み  

            using (StreamWriter writer = File.CreateText(Path.Combine(namePath, CREATE_FILE_B)))
            {
                writer.Write(bDat);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(namePath, CREATE_FILE_R)))
            {
                writer.Write(rDat);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(namePath, CREATE_FILE_W)))
            {
                writer.Write(wDat);
            }

            using (StreamWriter writer = File.CreateText(Path.Combine(namePath, CREATE_FILE_A)))
            {
                writer.Write(aDat);
            }

            //Schedule inputがあるときには、以下のロジックを回す。ないときには回さない
            if (hasSchedule)
            {
                using (StreamWriter writer = File.CreateText(Path.Combine(namePath, CREATE_FILE_S)))
                {
                    writer.Write(sDat);
                }
            }
            if(!File.Exists(Path.Combine(namePath, CREATE_FILE_S)))
            {
                MessageBox.Show("s.datファイルが読み取れませんでした。" + Environment.NewLine + 
                    "file path : " + Path.Combine(namePath, CREATE_FILE_S));
                throw new Exception("s.datファイルが読み取れませんでした。 - file path : " + Path.Combine(namePath, CREATE_FILE_S));
            }

            //t.datだけはshift-JISで書き出す
            
            using (StreamWriter sw = new StreamWriter(Path.Combine(namePath, CREATE_FILE_T), false, Encoding.GetEncoding("shift-jis")))
            {
                sw.Write(tDat);
            };


            if (cloudRun)
            {
                //処理3. cloud=trueのときには、zipファイルを作成し、https://oyster-app-8jboe.ondigitalocean.app/therb/run にfrom-dataのキーdatasetに対応するファイルとして添付し、POSTする

                var zipfile = namePath + ".zip";
                if (File.Exists(zipfile))
                {
                    if (MessageBox.Show($"{zipfile}がありましたが上書きされます。{Environment.NewLine}実行しますか？", "", MessageBoxButtons.OKCancel) != DialogResult.OK)
                        return;
                    File.Delete(zipfile);
                }
                ZipFile.CreateFromDirectory(namePath, zipfile);

                bool post_done = false;
                for (int i = 0; i < MAX_SERVER_TRY_COUNT; i++)
                {
                    foreach (var url in POST_URLS)
                    {
                        using (FileStream fs = new FileStream(zipfile, FileMode.Open, FileAccess.Read))
                        {
                            try
                            {
                                MultipartFormDataContent content = new MultipartFormDataContent();

                                StreamContent streamContent = new StreamContent(fs);
                                streamContent.Headers.ContentDisposition =
                                       new ContentDispositionHeaderValue("form-data")
                                       {
                                           Name = "dataset",
                                           FileName = name + ".zip"
                                       };

                                content.Add(streamContent);
                                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);
                                request.Content = content;
                                var responseTask = new HttpClient().SendAsync(request);
                                responseTask.Wait();
                                var response = responseTask.Result;

                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    Debug.WriteLine(response.Content);
                                    MessageBox.Show("解析できました。");
                                    post_done = true;
                                    break;
                                }
                                Debug.WriteLine(response.StatusCode);
                            }
                            catch (Exception e)
                            {
                                Debug.WriteLine("error: " + e.Message);
                                Task.Delay(1000);
                            }
                        }
                    }
                    if (post_done)
                    {
                        break;
                    }
                }
                if (post_done)
                {
                    File.Delete(zipfile);
                }
                else
                {
                    MessageBox.Show("解析できませんでした。");
                    throw new Exception("解析できませんでした。");
                }
            }
            else
            {
                //処理3. cloud=falseのときには、コマンドラインを立ち上げ、therb.exeファイルを呼び出す
                var process = new Process();
                process.StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Combine(namePath, THERB_FILE_NAME),
                    WorkingDirectory = namePath
                };
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    MessageBox.Show("解析がうまく行きませんでした。" + Environment.NewLine + 
                        "EXITCODE : " + process.ExitCode);
                    throw new Exception("解析がうまく行きませんでした。 - EXITCODE : " + process.ExitCode);
                }

                var path = Path.Combine(namePath, CREATED_FILE_O);

                DA.SetData("o_dat_file_path", path);

            }
            MessageBox.Show("以下の計算条件でシミュレーションを行いました。" + Environment.NewLine +
                $"気象データ：{setting.weather.name}" + Environment.NewLine +
                $"Envelope : {therb.envelope.name}" + Environment.NewLine +
                $"Schedule : {schedule.name}");
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
            get { return new Guid("b56ae3c7-9860-4f35-951e-0d6d427f5a2e"); }
        }
    }
}
