using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Model;
using Grasshopper.Kernel.Parameters;
using System.Windows.Forms;
using GH_IO.Serialization;
using Grasshopper.GUI;
using System.Drawing;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public enum VisualizeType
    {
        partId,
        elementType,
        structureId,
        direction,
        None
    }
    public class ReadFaceProperty : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ReadFaceProperty()
          : base("ReadFaceProperty", "ReadFaceProperty",
              "Read face properties from class",
              "THERB-GH", "Utility")
        {
        }

        const string FONT_SIZE_NAME = "fontsize";
        const int DEFALT_FONT_SIZE = 12;

        private List<Face> _faces;
        private int _fontsize;
        private VisualizeType _visualizeType;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddGenericParameter("Therb", "Therb", "Therb data", GH_ParamAccess.item);
            pManager.AddGenericParameter("Faces", "Faces", "list of Face", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager.AddIntegerParameter("Type", "Type", "Visualize id type.", GH_ParamAccess.item, -1);
            var bcInput = (Param_Integer)pManager[1];
            bcInput.AddNamedValue("partId", (int)VisualizeType.partId);
            bcInput.AddNamedValue("elementType", (int)VisualizeType.elementType);
            bcInput.AddNamedValue("structureId", (int)VisualizeType.structureId);
            bcInput.AddNamedValue("direction", (int)VisualizeType.direction);
            bcInput.AddNamedValue("None", -1);
            //pManager[1].Optional = true;
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
            pManager.AddIntegerParameter("roomId", "roomId", "room id", GH_ParamAccess.list);
            pManager.AddIntegerParameter("partId", "partId", "id", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("surface", "surface", "extracted surface", GH_ParamAccess.list);
            pManager.AddTextParameter("elementType", "element type", "element type", GH_ParamAccess.list);
            pManager.AddNumberParameter("normal", "normal", "normal direction", GH_ParamAccess.list);
            pManager.AddPointParameter("centerPt","centerPt","center point", GH_ParamAccess.list);
            pManager.AddTextParameter("direction", "direction", "direction", GH_ParamAccess.list);
            pManager.AddIntegerParameter("constructionId", "constructionId", "construction id", GH_ParamAccess.list);
            pManager.AddIntegerParameter("structureId", "structureId", "structure id", GH_ParamAccess.list);
            pManager.AddIntegerParameter("AdjacencyRoomId", "AdjacencyRoomId", "adjacency room id", GH_ParamAccess.list);
            pManager.AddGenericParameter("Windows", "Windows", "list of Window", GH_ParamAccess.list);
            pManager.AddIntegerParameter("WindowIds", "WindowIds", "list of WindowIds", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //Therb therb = null;
            List<Face> faceList = new List<Face>();
            int visualizeType = -1;
            //DA.GetData(0, ref therb);
            DA.GetDataList(0, faceList);
            DA.GetData(1, ref visualizeType);
            //faceList.AddRange(therb.faces);
            //List<Face> faceList = therb.faces;

            _faces = faceList;
            if (visualizeType < -1 | Enum.GetNames(typeof(VisualizeType)).Length < visualizeType)
            {
                throw new ArgumentException("範囲外の数字が入れられました。", "Type");
            }
            _visualizeType = (VisualizeType)visualizeType;

            List<int> roomIdList = new List<int>();
            List<int> partIdList = new List<int>();
            List<Surface> surfaceList = new List<Surface>();
            List<string> elementTypeList = new List<string>();
            List<Vector3d> normalList = new List<Vector3d>();
            List<Point3d> centerPtList = new List<Point3d>();
            List<string> directionList = new List<string>();
            List<int> constructionIdList = new List<int>();
            List<string> structureIdList = new List<string>();
            List<int> adjacencyRoomIdList = new List<int>();
            List<List<Window>> windowList = new List<List<Window>>();
            List<int> windowIdList = new List<int>();

            faceList.ForEach(face =>
            {
                roomIdList.Add(face.parentId);
                partIdList.Add(face.partId);
                surfaceList.Add(face.geometry);
                elementTypeList.Add(face.elementType.ToString());
                normalList.Add(face.tempNormal);
                centerPtList.Add(face.centerPt);
                directionList.Add(face.direction.ToString());
                constructionIdList.Add(face.constructionId);
                structureIdList.Add(face.structureId);
                adjacencyRoomIdList.Add(face.adjacencyRoomId);
                windowList.Add(face.windows);
                windowIdList.AddRange(face.windowIds);
            });

            DA.SetDataList("roomId", roomIdList);
            DA.SetDataList("partId", partIdList);
            DA.SetDataList("surface", surfaceList);
            DA.SetDataList("elementType", elementTypeList);
            DA.SetDataList("normal", normalList);
            DA.SetDataList("centerPt", centerPtList);
            DA.SetDataList("direction", directionList);
            DA.SetDataList("constructionId", constructionIdList);
            DA.SetDataList("structureId", structureIdList);
            DA.SetDataList("AdjacencyRoomId", adjacencyRoomIdList);
            DA.SetDataList("Windows", windowList);
            DA.SetDataList("WindowIds", windowIdList);
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

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            base.AppendAdditionalComponentMenuItems(menu);

            Menu_AppendItem(menu, FONT_SIZE_NAME);
            ToolStripTextBox textBox = Menu_AppendTextItem(menu,
                    this._fontsize.ToString(),
                    (obj, e) => { },
                    Menu_TextBoxChanged,
                    true);
            textBox.Name = FONT_SIZE_NAME;

        }

        protected void Menu_TextBoxChanged(GH_MenuTextBox sender, string newText)
        {
            try
            {
                this._fontsize = int.Parse(newText);
                this.ExpireSolution(true);
            }
            catch
            {
                this._fontsize = DEFALT_FONT_SIZE;
            }
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetInt32(FONT_SIZE_NAME, _fontsize);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            try { _fontsize = reader.GetInt32(FONT_SIZE_NAME); }
            catch { this._fontsize = DEFALT_FONT_SIZE; }

            return base.Read(reader);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            base.DrawViewportWires(args);
            try
            {
                if (_visualizeType == VisualizeType.None)
                    return;
                foreach (var face in _faces)
                {
                    //var text3d = new Text3d("RoomId:" + room.id, new Plane(room.centroid, new Vector3d(0, 0, 1)), _fontsize);
                    //args.Display.Draw3dText(text3d, Color.Black);
                    //args.Display.Draw2dText("RoomId:" + room.id, Color.Black, room.centroid, false);
                    var writeText = "";
                    if (_visualizeType == VisualizeType.partId)
                        writeText = "partId:" + face.partId;
                    else if (_visualizeType == VisualizeType.elementType)
                        writeText = "elementType:" + face.elementType;
                    else if (_visualizeType == VisualizeType.structureId)
                        writeText = "structureId:" + face.structureId;
                    else if (_visualizeType == VisualizeType.direction)
                        writeText = "direction:" + face.direction;

                    args.Display.Draw2dText(
                        writeText,
                        Color.Black,
                        face.centerPt,
                        true,
                        _fontsize);
                }
            }
            catch { }
        }
    }
}
