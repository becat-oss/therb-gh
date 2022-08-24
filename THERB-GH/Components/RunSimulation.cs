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

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class RunSimulation : GH_Component
    {
        const string THERB_FILE_NAME = "therb.exe";
        const string THERB_FOLDER_PATH = @"C:\therb";
        const string CREATE_FILE_B = "b.dat";
        const string CREATE_FILE_R = "r.dat";
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
            pManager.AddTextParameter("name", "name", "simulation case name", GH_ParamAccess.item);
            //pManager.AddBooleanParameter("cloud", "cloud", "run simulation in cloud", GH_ParamAccess.item);
            pManager.AddBooleanParameter("run", "run", "run THERB simulation", GH_ParamAccess.item);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("result", "result", "Room class", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string namePath = "";

            bool done = false;
            Therb therb = null;
            DA.GetData(0, ref therb);

            if (therb == null) return;

            List<Construction> constructionList = new List<Construction>();
            DA.GetDataList(1, constructionList);

            DA.GetData("name", ref namePath);
            DA.GetData("run", ref done);
            if (!done) return;

            var bDat = CreateDatData.CreateBDat(therb);
            var rDat = CreateDatData.CreateRDat(therb);
            //TODO: CreateADat,CreateWDatも呼ぶ
            

            if (string.IsNullOrEmpty(namePath)) throw new Exception("nameが読み取れませんでした。");
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

            namePath = Path.Combine(THERB_FOLDER_PATH, namePath);
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
            using (FolderBrowserDialog FBD = new FolderBrowserDialog()
            {
                Description = "フォルダを開く", 
                SelectedPath = initDir,
                ShowNewFolderButton = false
            })
            {
                if(FBD.ShowDialog() == DialogResult.OK)
                {
                    //MessageBox.Show(FBD.SelectedPath);
                    dirPath = FBD.SelectedPath;
                    if (string.IsNullOrEmpty(dirPath)) {
                        MessageBox.Show("パスが読み取れませんでした。");
                        return;
                    }
                    if(!File.Exists(Path.Combine(dirPath, THERB_FILE_NAME)))
                    {
                        MessageBox.Show("パス内にtherb.exeがありませんでした。" + Environment.NewLine + "中止します。");
                        return;
                    }
                }
                else　return;
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

            //処理3. コマンドラインを立ち上げ、therb.exeファイルを呼び出す
            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(namePath, THERB_FILE_NAME),
                WorkingDirectory = namePath
            };
            process.Start();
            process.WaitForExit();
#if DEBUG
            Debug.WriteLine("END");
            Debug.WriteLine("EXITCODE:" + process.ExitCode.ToString());
            Debug.WriteLine("EXITTIME" + process.ExitTime.ToString());
#endif

            //処理3. zipファイルを作成し、https://stingray-app-vgak2.ondigitalocean.app/therb/run にfrom-dataのキーdatasetに対応するファイルとして添付し、POSTする

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
