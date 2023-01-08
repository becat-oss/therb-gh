using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Collections.Generic;
using Rhino.Geometry.Intersect;
using Newtonsoft.Json;
using Model;
using Rhino.Display;
using System.Drawing;
using System.Windows.Forms;
using Grasshopper.GUI;
using GH_IO.Serialization;
using System.Linq;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class THERB : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public THERB()
          : base("THERB", "THERB",
              "Description",
              "THERB-GH", "Modelling")
        {
        }

        const string FONT_SIZE_NAME = "fontsize";
        const int DEFALT_FONT_SIZE = 12;

        public Envelope envelope;
        public List<Construction> constructions;
        private Therb _therb;
        private int _fontsize;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBoxParameter("boxes", "boxes", "list of room boxes", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("_windows", "windows", "list of windows", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("_overhangs", "overhangs", "list of overhangs", GH_ParamAccess.list);
            pManager.AddGenericParameter("_Envelope", "Envelope", "Envelope class", GH_ParamAccess.item); //Envelopeのinputをoptionalにしたい
            pManager.AddGenericParameter("_Constructions", "Constructions", "Construction class", GH_ParamAccess.list); //Envelopeのinputをoptionalにしたい
            pManager.AddNumberParameter("tol", "tolerance", "tolerance", GH_ParamAccess.item, 0.1);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Therb", "Therb", "THERB class", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //var geos = new Brep();
            List<Box> boxes = new List<Box>();
            List<Surface> windows = new List<Surface>();
            List<Surface> overhangs = new List<Surface>();
            constructions = new List<Construction>();
            envelope = new Envelope();

            List<Room> roomList = new List<Room>();
            List<Face> faceList = new List<Face>();

            //TODO:GrasshopperのほうのInitTotalRoomをC#側に移植する
            Room.InitTotalRoom();
            Face.InitTotalFace();


            //DA.GetData("geos", ref geos);
            //tolとwindowsは任意のパラメータとしたい
            double tol = 0.1;
            DA.GetDataList(0, boxes);
            DA.GetDataList(1, windows);
            DA.GetDataList(2, overhangs);
            //Envelopeのinputがなかったら以下の情報でEnvelopeを初期化
            //Envelope defaultEnv = new Envelope(1, 2, 3, 4, 5, 6);
            DA.GetData(3, ref envelope);
            DA.GetDataList(4, constructions);
            DA.GetData(5, ref tol);

            var exBoxes = ExBox.SplitGeometry(boxes, tol);

            //static memberを初期化する
            //new Room().initRoom()

            //Roomに対する処理
            var splitSurfs = new List<Surface>();
            foreach (var exBox in exBoxes)
            {
                Room temp = new Room(exBox.Box.ToBrep());

                splitSurfs.AddRange(exBox.BoxSurfaces);

                foreach (var brepface in exBox.BoxSurfaces)
                {
                    Vector3d normal = brepface.NormalAt(0.5, 0.5);
                    Vector3d tempNormal = reviseNormal(brepface, temp);
                    Face face = new Face(temp, brepface, normal, tempNormal);
                    faceList.Add(face);
                    temp.addFace(face);
                }
                roomList.Add(temp);
            }

            //FaceのboundaryConditionを計算する処理
            List<Face> faceListBC = solveBoundary(faceList, tol);

            //windowがどのwallの上にあるかどうかを判断するロジック
            var messages = new List<string>();
            List<Window> windowList = windowOnFace(faceListBC, windows, tol, ref messages);
            foreach(Window window in windowList)
            {
                if (window.parent == null)
                    messages.Add("親のいない窓があります。window_id : " + window.id);
            }
            foreach (var message in messages)
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, message);
            Window.InitTotalWindow();

            //window情報をfaceにaddする
            List<Face> faceListWindow = new List<Face>();
            foreach (Face face in faceListBC)
            {
                foreach (Window window in windowList)
                {
                    if (window.parentId == face.id)
                    {
                        face.addWindows(window);
                    }
                }
                faceListWindow.Add(face);
            }

            //Roomに属するfaceの属性(wall,ceiling,roof)を集計するロジック
            foreach (Room room in roomList)
            {
                List<Face> faces = room.getFaces();
                foreach (Face face in faces)
                {
                    room.groupChildFaces(face);
                }
            }

            //overhangがどのwallの上にあるかどうかを判断するロジック
            List<Overhang> overhangList = overhangOnWindow(windowList,overhangs);

            //overhang情報をfaceにaddする
            List<Face> faceListOverhang = new List<Face>();
            foreach(Window window in windowList)
            {
                foreach(Overhang overhang in overhangList)
                {
                    if(overhang.parentWindowId == window.id)
                    {
                        window.addOverhangs(overhang);
                    }
                }
            }

            _therb = new Therb(roomList, faceListWindow, windowList, overhangList);

            DA.SetData("Therb", _therb);
        }

        [Obsolete]
        private List<Brep> SplitGeometry(List<Brep> breps, double tol)
        {
            if (breps.Count == 1)
            {
                //if (!breps[0].IsSolid) throw new Exception("開かれたBrepが入れられました");
                return breps;
            }
            List<Brep> splitGeos = new List<Brep>();
            for (int i = 0; i < breps.Count; i = i + 1)
            {
                //if (!breps[i].IsSolid) throw new Exception("開かれたBrepが入れられました");

                List<Brep> cutterBreps = breps.FindAll(geo => geo != breps[i]);
                Brep[] splitGeo = breps[i].Split(cutterBreps, tol);
                Brep jointedBrep = Brep.JoinBreps(splitGeo, tol)[0];

                if (!jointedBrep.IsSolid) throw new Exception("BrepのJointが失敗しました。");

                splitGeos.Add(jointedBrep);
            }
            return splitGeos;
        }

        private enum StateSurfPoint
        {
            Unknown,
            OnFace,
            NotOnFace
        }
        private List<Window> windowOnFace(List<Face> faceList, List<Surface> windows, double tol, ref List<string> messages)
        {
            //TODO:内壁に窓を配置するケースもあるっぽい
            List<Face> externalFaces = faceList.FindAll(face => face.bc == BoundaryCondition.exterior);

            List<Window> windowList = new List<Window>();
            foreach(Surface windowGeo in windows)
            {
                Window window = new Window(windowGeo, envelope,constructions);
                foreach (Face exFace in externalFaces)
                {
                    var state = StateSurfPoint.Unknown;
                    foreach (Point3d coner in windowGeo.ToBrep().DuplicateVertices())
                    {
                        if (exFace.geometry.ClosestPoint(coner, out double u, out double v)){
                            var onPoint = exFace.geometry.PointAt(u, v);
                            if(coner.DistanceTo(onPoint) < tol)
                            {
                                if (state == StateSurfPoint.NotOnFace)
                                {
                                    state = StateSurfPoint.NotOnFace;
                                    messages.Add("壁端に窓が配置されています。window_id : " + window.id);
                                    break;
                                }
                                state = StateSurfPoint.OnFace;
                            }
                            else
                            {
                                if (state == StateSurfPoint.OnFace)
                                {
                                    state = StateSurfPoint.NotOnFace;
                                    messages.Add("壁端に窓が配置されています。wall_idwindow_id : " + window.id);
                                    break;
                                }
                                state = StateSurfPoint.NotOnFace;
                            }
                        }
                        else throw new Exception($"壁-窓判定ができませんでした。face_id : {exFace.id} window_id : " + window.id);

                    }
                    if (state == StateSurfPoint.OnFace)
                        window.addParent(exFace);
                }
                windowList.Add(window);

            }
            return windowList;
        }

        private List<Overhang> overhangOnWindow(List<Window> windowList, List<Surface> overhangs)
        {
            List<Overhang> overhangList = new List<Overhang>();
            foreach (Surface overhangGeo in overhangs)
            {
                Window parent = windowList[0];
                double closestDistance = 10000;
                Overhang overhang = new Overhang(overhangGeo);
                foreach (Window window in windowList)
                {
                    double distance = overhang.centerPt.DistanceTo(window.centerPt);
                    if (distance < closestDistance)
                    {
                        parent = window;
                        closestDistance = distance;
                    }
                }

                if (closestDistance < 10000)
                {
                    overhang.addParentWindow(parent);
                    overhang.addParentFace(parent.parent);
                }
                else
                {
                    //エラー処理を加える
                }
                overhangList.Add(overhang);

            }
            return overhangList;
        }

        private List<Face> solveBoundary(List<Face> faceList, double tol)
        {
            List<Face> faceListBC = new List<Face>();
            for (int i=0;i<faceList.Count; i++)
            {
                //テストするsurfaceを選択する
                Face testFace = faceList[i];

                //違うparentを持つfaceを選択する
                List<Face> targetFaces = faceList.FindAll(face => face.parent != testFace.parent);

                List<Surface> targetSurfaces = new List<Surface>();

                foreach (Face face in targetFaces)
                {
                    targetSurfaces.Add(face.geometry);
                };

                Point3d testPt = new Point3d(testFace.centerPt);
                testPt += testFace.normal * -tol / 2;
                Ray3d testRay = new Ray3d(testPt, testFace.normal);

                if (shootIt(testRay, targetSurfaces, tol, 2))
                {
                    testFace.bc = BoundaryCondition.interior;
                    //どのfaceに接しているのかをチェック
                    //testFace.centerPtから一番近いtargetSurfacesが隣接しているsurfaceだと判断する
                    Face adjacentFace = getClosestFaceFromFace(testFace, targetFaces);

                    //内壁の重複を防ぐロジック
                    
                    testFace.adjacencyRoomId = adjacentFace.parentId;
                    //testFace.adjacencyFaceId = adjacentFace.partId;
                    testFace.adjacencyFace = adjacentFace;

                    //既にadjacentFace.adjacentFaceがtestFaceだったらduplicateフラグ
                    if (adjacentFace.adjacencyFace == testFace)
                    {
                        testFace.unique = false;
                        testFace.partId = adjacentFace.partId;
                    }

                }
                else
                {
                    //z座標<=0であれば、bc=groundとする
                    if(testFace.centerPt.Z <= tol)
                    {
                        testFace.bc = BoundaryCondition.ground;
                    }
                    else
                    {
                        testFace.bc = BoundaryCondition.exterior;
                    }
                    testFace.adjacencyRoomId = 0;
                }
                
                testFace.setElementType();
                testFace.setPartId();
                testFace.setConstructionId(envelope,constructions);
                faceListBC.Add(testFace);
            }
            return faceListBC;
        }

        private bool shootIt(Ray3d ray, List<Surface> srfs, double tol, int bounce)
        {
            Point3d[] intersectPts = Intersection.RayShoot(ray, srfs, 1);
            if (intersectPts.Length > 0)
            {
                if (ray.Position.DistanceTo(intersectPts[0]) <= tol)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private Face getClosestFaceFromFace(Face originFace, List<Face> faces)
        {
            Face closestFace = null;
            double closestDistance = 10000;
            foreach(Face face in faces)
            {
                double distance = 10000;
                //faceがwallであればwallのみでテストする
                if (originFace.surfaceType ==  SurfaceType.Wall && face.surfaceType == SurfaceType.Wall)
                {
                    distance = originFace.centerPt.DistanceTo(face.centerPt);
                }else if(originFace.surfaceType != SurfaceType.Wall && face.surfaceType != SurfaceType.Wall)
                {
                    distance = originFace.centerPt.DistanceTo(face.centerPt);
                }
                if (distance < closestDistance)
                {
                    closestFace = face;
                    closestDistance = distance;
                }
            }
            return closestFace;
        }

        private Vector3d reviseNormal(Surface srf, Room parent)
        {
            //モデルによってはsurfaceのvectorの方向が意図と逆に向いている場合があるのでそれを修正
            Point3d centroid = parent.centroid;
            AreaMassProperties areaMp = AreaMassProperties.Compute(srf);
            Point3d centerPt = areaMp.Centroid;
            Vector3d normal = srf.NormalAt(0.5, 0.5);
            Point3d testPt = centerPt + normal;
            double baseDistance = centroid.DistanceTo(centerPt);
            double testDistance = centroid.DistanceTo(testPt);

            if (baseDistance > testDistance)
            {
                Vector3d revNormal = new Vector3d(-normal.X, -normal.Y, -normal.Z);
                return revNormal;
            }
            else
            {
                return normal;
            }
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
            get { return new Guid("871d367b-8760-4573-b161-304cb7d17bf0"); }
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

        //public override void DrawViewportWires(IGH_PreviewArgs args)
        //{
        //    base.DrawViewportWires(args);
        //    try
        //    {
        //        foreach (var room in therb.rooms)
        //        {
                    //var text3d = new Text3d("RoomId:" + room.id, new Plane(room.centroid, new Vector3d(0, 0, 1)), _fontsize);
                    //args.Display.Draw3dText(text3d, Color.Black);
                    //args.Display.Draw2dText("RoomId:" + room.id, Color.Black, room.centroid, false);
        //            args.Display.Draw2dText(
        //                "RoomId:" + room.id, 
        //                Color.Black,
        //                room.centroid, 
        //                true, 
        //                _fontsize);
        //        }
        //    }
        //    catch { }
        //}
    }

    public class ExBox
    {
        public Box Box { get; private set; }
        public List<Surface> BoxSurfaces { get; private set; }
        public const double STRIC_TOL = 1e-9;

        public ExBox(Box box)
        {
            this.Box = box;
            this.BoxSurfaces = new List<Surface>();
            foreach(var bfl in box.ToBrep().Faces)
            {
                this.BoxSurfaces.Add(bfl.DuplicateSurface());
            }
        }

        public static List<ExBox> SplitGeometry(List<Box> boxes, double tol)
        {
            if (boxes.Count == 1)
            {
                return new List<ExBox>() {new ExBox(boxes[0]) };
            }
            var exBoxes = new List<ExBox>();
            foreach (var box in boxes)
            {
                if (!IsSamePlane(boxes[0], box)) throw new Exception("BoxのPlaneが合いませんでした。");
                exBoxes.Add(new ExBox(box));
            }
            for(int i = 0;i < exBoxes.Count;i++)
            {
                var others = exBoxes.FindAll(exbox => exbox != exBoxes[i]);
                if (others.Count == exBoxes.Count) throw new Exception();
                foreach (var other in others)
                {
                    exBoxes[i].BoxSurfaces = SplitSurface(exBoxes[i].BoxSurfaces, other.BoxSurfaces);
                }
                
            }
            return exBoxes;
        }

        private static bool IsSamePlane(Box box1, Box box2)
        {
            var plane1 = box1.Plane;
            var plane2 = box2.Plane;

            if (IsParallel(plane1.XAxis, plane2.XAxis))
            {
                if (IsParallel(plane1.YAxis, plane2.YAxis)) return true;
                if (IsParallel(plane1.YAxis, plane2.ZAxis)) return true;
            }
            if (IsParallel(plane1.XAxis, plane2.YAxis))
            {
                if (IsParallel(plane1.YAxis, plane2.XAxis)) return true;
                if (IsParallel(plane1.YAxis, plane2.ZAxis)) return true;
            }
            if (IsParallel(plane1.XAxis, plane2.ZAxis))
            {
                if (IsParallel(plane1.YAxis, plane2.XAxis)) return true;
                if (IsParallel(plane1.YAxis, plane2.YAxis)) return true;
            }

            return false;
        }

        private static bool IsOnPlane(Plane plane, Point3d point)
        {
            var closetPoint = plane.ClosestPoint(point);
            return point.DistanceTo(closetPoint) < STRIC_TOL;
        }

        private static bool IsParallel(Vector3d vec1, Vector3d vec2)
        {
            return Vector3d.CrossProduct(vec1, vec2).IsZero;
        }

        private static List<Surface> SplitSurface(List<Surface> curSurfs, List<Surface> otherSurfs)
        {
            foreach (var otherSurf in otherSurfs)
            {
                Plane otherPlane;
                otherSurf.TryGetPlane(out otherPlane);
                for(int i = 0; i < curSurfs.Count; i++)
                {
                    var curSurf = curSurfs[i];
                    Plane curPlane;
                    curSurf.TryGetPlane(out curPlane);
                    if (!IsOnPlane(curPlane, otherPlane.Origin)) continue;
                    if (!IsParallel(curPlane.Normal, otherPlane.Normal)) continue;
                    List<Surface> surfs;
                    var otherMaxU = otherSurf.Domain(0).ParameterAt(1d);
                    var otherMaxV = otherSurf.Domain(1).ParameterAt(1d);
                    if (TrySplit(curSurf, otherSurf.PointAt(0, 0), out surfs))
                    {
                        curSurfs.RemoveAt(i--);
                        curSurfs.AddRange(surfs);
                        continue;
                    }
                    if (TrySplit(curSurf, otherSurf.PointAt(otherMaxU, 0), out surfs))
                    {
                        curSurfs.RemoveAt(i--);
                        curSurfs.AddRange(surfs);
                        continue;
                    }
                    if (TrySplit(curSurf, otherSurf.PointAt(otherMaxU, otherMaxV), out surfs))
                    {
                        curSurfs.RemoveAt(i--);
                        curSurfs.AddRange(surfs);
                        continue;
                    }
                    if (TrySplit(curSurf, otherSurf.PointAt(0, otherMaxV), out surfs))
                    {
                        curSurfs.RemoveAt(i--);
                        curSurfs.AddRange(surfs);
                        continue;
                    }
                }
            }

            return curSurfs;
        }

        private static bool TrySplit(Surface surface, Point3d point, out List<Surface> results)
        {
            results = new List<Surface>();
            double u, v;
            if (!surface.ClosestPoint(point, out u, out v)) return false;
            var surfs = surface.Split(0, u);
            if (surfs.Length == 0) surfs = new Surface[] { surface };
            for (int i = 0; i < surfs.Length; i++)
            {
                var surfs2 = surfs[i].Split(1, v);
                if (surfs2.Length == 0) results.Add(surfs[i]);
                else results.AddRange(surfs2);
            }
            if (results.Count == 1) return false;
            return true;
        }
    }
}
