using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using Rhino.Geometry.Intersect;
using Newtonsoft.Json;
using Model;
using Utils;
using Rhino;

namespace THERBgh
{
    public class CreateDatData
    {
        public static string CreateADat(Therb therb,double ach)
        {

            List<Room> roomList = therb.rooms;
            string aDat = "*hourly data od forced room air ventilation \r\n";

            roomList.ForEach(room =>
            {
                aDat += "into room" + Converter.FillEmpty(room.id, 3) + "from " + Converter.FillEmpty(0, 3)
                    + "=>  \r\n"
                    + Converter.FillEmpty("from outdoor=0", 25)
                    + Converter.FillEmpty("- ", 6)
                    + string.Join("", Enumerable.Repeat(Converter.FillEmpty(room.volume*ach, 7, 1), 12)) + "\r\n" 
                    + Converter.FillEmpty("quantity (m3/h)", 25)
                    + Converter.FillEmpty("- ", 6) + "\r\n"
                    + Converter.FillEmpty("(-1.:natural vent.)", 25)
                    + Converter.FillEmpty("- ", 6) 
                    + string.Join("", Enumerable.Repeat(Converter.FillEmpty(room.volume*ach, 7, 1), 12)) + "\r\n" + "\r\n"; 
            });

            return aDat;
        }

        public static string CreateBDat(Therb therb)
        {

            string bDat = "";

            List<Room> roomList = therb.rooms;
            List<Face> faceList = therb.faces;
            List<Window> windowList = therb.windows;
            List<Overhang> overhangList = therb.overhangs;


            //TODO:doorの入力UIも作る
            List<Face> doorList = new List<Face>();
            List<Face> shadingList = new List<Face>();
            List<Face> wingList = new List<Face>();

            if (roomList == null) throw new NullReferenceException("ルームリストが読み込めませんでした。");

            List<Face> exteriorWalls = new List<Face>();
            List<Face> interiorWalls = new List<Face>();
            List<Face> exteriorRoofs = new List<Face>();
            List<Face> interiorRoofFloors = new List<Face>();
            List<Face> exteriorFloors = new List<Face>();
            //List<Face> interiorFloors = new List<Face>();
            List<Face> groundFloors = new List<Face>();


            //faceを分類
            faceList.ForEach(face =>
            {
                if (face.elementType == ElementType.exteriorWall)
                {
                    exteriorWalls.Add(face);
                }
                else if (face.elementType == ElementType.interiorWall && face.unique)
                {
                    interiorWalls.Add(face);
                }
                else if (face.elementType == ElementType.exteriorRoof)
                {
                    exteriorRoofs.Add(face);
                }
                else if (face.elementType == ElementType.interiorRoof || face.elementType == ElementType.interiorFloor)
                {
                    if (face.unique)
                    {
                        interiorRoofFloors.Add(face);
                    }

                }
                else if (face.elementType == ElementType.exteriorFloor)
                {
                    exteriorFloors.Add(face);
                }
                else if (face.elementType == ElementType.groundFloor)
                {
                    groundFloors.Add(face);
                }
            });

            //TODO:exteriorFloorの扱いがどうなるかを確認

            //1行目 部屋、壁の数をカウント
            bDat += Converter.FillEmpty(roomList.Count, 4)
                + Converter.FillEmpty(exteriorWalls.Count, 4)
                + Converter.FillEmpty(windowList.Count, 4)
                + Converter.FillEmpty(interiorWalls.Count, 4)
                + Converter.FillEmpty(doorList.Count, 4)
                + Converter.FillEmpty(interiorRoofFloors.Count, 4)
                + Converter.FillEmpty(exteriorRoofs.Count, 4)
                + Converter.FillEmpty(groundFloors.Count, 4)
                + Converter.FillEmpty(shadingList.Count, 4)
                + Converter.FillEmpty(wingList.Count, 4) + " \r\n";

            //室の情報を抽出
            roomList.ForEach(room =>
            {
                bDat += Converter.FillEmpty("Room " + room.id.ToString(), 12)
                + Converter.FillEmpty(room.minPt.X, 9, 3)
                + Converter.FillEmpty(room.minPt.Y, 8, 3)
                + Converter.FillEmpty(room.minPt.Z, 8, 3)
                + Converter.FillEmpty(room.maxPt.X, 8, 3)
                + Converter.FillEmpty(room.maxPt.Y, 8, 3)
                + Converter.FillEmpty(room.maxPt.Z, 8, 3)
                + Converter.FillEmpty(room.volume, 10, 3)
                 + "      18.800   16.7000 \r\n";
                //volumeを抽出する必要
            });

            //外壁の情報を抽出
            exteriorWalls.ForEach(exWall =>
            {
                bDat += Converter.FillEmpty("Ex-wall " + exWall.partId.ToString(), 13)
                + Converter.FillEmpty(exWall.minPt.X, 8, 3)
                + Converter.FillEmpty(exWall.minPt.Y, 8, 3)
                + Converter.FillEmpty(exWall.minPt.Z, 8, 3)
                + Converter.FillEmpty(exWall.maxPt.X, 8, 3)
                + Converter.FillEmpty(exWall.maxPt.Y, 8, 3)
                + Converter.FillEmpty(exWall.maxPt.Z, 8, 3)
                + Converter.FillEmpty(exWall.tiltAngle, 10, 3)
                + Converter.FillEmpty(exWall.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(exWall.structureId, 5)
                + "  overhang No.     0      wing1 No.    0   wing2 No.    0 \r\n      window No. "
                + OutputWindowIds(exWall)
                //TODO:windowIdsの処理を入れ込む必要
                + "\r\n";
            });

            //窓の情報を抽出
            windowList.ForEach(window =>
            {
                bDat += Converter.FillEmpty("Window " + window.partId.ToString(), 13)
                + Converter.FillEmpty(window.minPt.X, 8, 3)
                + Converter.FillEmpty(window.minPt.Y, 8, 3)
                + Converter.FillEmpty(window.minPt.Z, 8, 3)
                + Converter.FillEmpty(window.maxPt.X, 8, 3)
                + Converter.FillEmpty(window.maxPt.Y, 8, 3)
                + Converter.FillEmpty(window.maxPt.Z, 8, 3)
                + Converter.FillEmpty(window.tiltAngle, 10, 3)
                + Converter.FillEmpty(window.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(window.structureId, 5)
                + "  overhang No.     0      wing1 No.    0   wing2 No.    0\r\n";
            });

            string doorIds = "   0   0   0   0   0   0   0   0   0   0   0";
            //内壁の情報を抽出
            interiorWalls.ForEach(inWall =>
            {
                bDat += Converter.FillEmpty("In-wall " + inWall.partId.ToString(), 13)
                + Converter.FillEmpty(inWall.minPt.X, 8, 3)
                + Converter.FillEmpty(inWall.minPt.Y, 8, 3)
                + Converter.FillEmpty(inWall.minPt.Z, 8, 3)
                + Converter.FillEmpty(inWall.maxPt.X, 8, 3)
                + Converter.FillEmpty(inWall.maxPt.Y, 8, 3)
                + Converter.FillEmpty(inWall.maxPt.Z, 8, 3)
                + Converter.FillEmpty(inWall.tiltAngle, 10, 3)
                + Converter.FillEmpty(inWall.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(inWall.structureId, 5)
                + " \r\n     in-door No. " + doorIds + " \r\n";
            });

            //床・天井の情報を抽出
            interiorRoofFloors.ForEach(inMat =>
            {
                bDat += Converter.FillEmpty("flr&cling " + inMat.partId.ToString(), 13)
                + Converter.FillEmpty(inMat.minPt.X, 8, 3)
                + Converter.FillEmpty(inMat.minPt.Y, 8, 3)
                + Converter.FillEmpty(inMat.minPt.Z, 8, 3)
                + Converter.FillEmpty(inMat.maxPt.X, 8, 3)
                + Converter.FillEmpty(inMat.maxPt.Y, 8, 3)
                + Converter.FillEmpty(inMat.maxPt.Z, 8, 3)
                + Converter.FillEmpty(inMat.tiltAngle, 10, 3)
                + Converter.FillEmpty(inMat.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(inMat.structureId, 5) + "\r\n";
            });

            exteriorRoofs.ForEach(roof =>
            {
                bDat += Converter.FillEmpty("Roof " + roof.partId.ToString(), 13)
                + Converter.FillEmpty(roof.minPt.X, 8, 3)
                + Converter.FillEmpty(roof.minPt.Y, 8, 3)
                + Converter.FillEmpty(roof.minPt.Z, 8, 3)
                + Converter.FillEmpty(roof.maxPt.X, 8, 3)
                + Converter.FillEmpty(roof.maxPt.Y, 8, 3)
                + Converter.FillEmpty(roof.maxPt.Z, 8, 3)
                + Converter.FillEmpty(roof.tiltAngle, 10, 3)
                + Converter.FillEmpty(roof.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(roof.structureId, 5) + "\r\n";
            });

            groundFloors.ForEach(floor =>
            {
                bDat += Converter.FillEmpty("Ground " + floor.partId.ToString(), 13)
                + Converter.FillEmpty(floor.minPt.X, 8, 3)
                + Converter.FillEmpty(floor.minPt.Y, 8, 3)
                + Converter.FillEmpty(floor.minPt.Z, 8, 3)
                + Converter.FillEmpty(floor.maxPt.X, 8, 3)
                + Converter.FillEmpty(floor.maxPt.Y, 8, 3)
                + Converter.FillEmpty(floor.maxPt.Z, 8, 3)
                + Converter.FillEmpty(floor.tiltAngle, 10, 3)
                + Converter.FillEmpty(floor.area, 12, 4)
                + "    0\r\n  structure No. "
                + Converter.FillEmpty(floor.structureId, 5) + "\r\n";
            });

            //庇情報を書き出す
            overhangList.ForEach(overhang =>
            {
                //FIXME:今のところ、庇の出はx,yの差分の小さいほうとしている
                double length = Math.Min(Math.Abs(overhang.maxPt.X - overhang.minPt.X), Math.Abs(overhang.maxPt.Y - overhang.minPt.Y));

                bDat += Converter.FillEmpty("Overhang " + overhang.id.ToString(), 13)
                + Converter.FillEmpty(overhang.minPt.X, 8, 3)
                + Converter.FillEmpty(overhang.minPt.Y, 8, 3)
                + Converter.FillEmpty(overhang.minPt.Z, 8, 3)
                + Converter.FillEmpty(overhang.maxPt.X, 8, 3)
                + Converter.FillEmpty(overhang.maxPt.Y, 8, 3)
                //庇の出はparentFaceのnormal directionによって決まる
                + Converter.FillEmpty(length, 8, 3)
                + Converter.FillEmpty(overhang.parentWindow.tiltAngle, 10, 3)
                + Converter.FillEmpty(overhang.area, 12, 4)
                + "    0\r\n";
            });

            return bDat;
        }

        public static string CreateRDat(Therb therb)
        {

            List<Room> roomList = therb.rooms;
            List<Face> faceList = therb.faces;
            List<Window> windowList = therb.windows;

            string rDat = "";

            if (roomList == null) throw new NullReferenceException("ルームリストが読み込めませんでした。");

            var directionDict = new Dictionary<string, int>(){
                {"S",1},
                {"W",2},
                {"N",3},
                {"E",4},
                {"F",5},
                {"CR",6},
            };

            var directionCount = new Dictionary<string, int>(){
                {"S",1},
                {"W",1},
                {"N",1},
                {"E",1},
                {"F",1},
                {"CR",1},
            };

            //Roomデータに対する処理
            roomList.ForEach(room =>
            {
                List<int> directions = room.getDirectionList();
                //idList.Add(room.id);
                rDat += Converter.FillEmpty(room.id, 5)
                + Converter.FillEmpty(directions[0], 5)
                + Converter.FillEmpty(directions[1], 5)
                + Converter.FillEmpty(directions[2], 5)
                + Converter.FillEmpty(directions[3], 5)
                + Converter.FillEmpty(directions[4], 5)
                + Converter.FillEmpty(directions[5], 5)
                + Converter.FillEmpty(directions[6], 5) + " \r\n";
            });


            //Faceデータに対する処理
            roomList.ForEach(room =>
            {
                int id = 1;
                faceList.ForEach(face =>
                {
                    if (room.id == face.parentId)
                    {
                        rDat += Converter.FillEmpty(room.id, 5)
                        + Converter.FillEmpty(id, 5)
                        + Converter.FillEmpty(directionDict[face.direction.ToString()], 5)
                        + Converter.FillEmpty(directionCount[face.direction.ToString()], 5)
                        + Converter.FillEmpty(face.constructionId, 5)
                        + Converter.FillEmpty(face.partId, 5)
                        + Converter.FillEmpty(face.adjacencyRoomId, 5) + " \r\n";

                        directionCount[face.direction.ToString()] += 1;
                        id += 1;

                        //TODO:窓に関する処理を追加
                        
                        face.windows.ForEach(window =>
                        {
                            rDat += Converter.FillEmpty(room.id, 5)
                            + Converter.FillEmpty(id, 5)
                            + Converter.FillEmpty(directionDict[face.direction.ToString()], 5)
                            + Converter.FillEmpty(directionCount[face.direction.ToString()], 5)
                            + Converter.FillEmpty(6, 5)
                            + Converter.FillEmpty(window.id, 5)
                            + Converter.FillEmpty(face.adjacencyRoomId, 5) + " \r\n";

                            id += 1;
                        });
                    }


                });
            });

            return rDat;
        }

        public static string CreateWDat(List<Construction> constructions)
        {
            string wDat = "";

            var elementIdDict = new Dictionary<ElementType, int>(){
                {ElementType.exteriorWall,1},
                {ElementType.interiorWall,2},
                {ElementType.interiorFloor,3},
                {ElementType.exteriorRoof,4},
                {ElementType.groundFloor,5},
                {ElementType.window,6},
            };

            var classificationDict = new Dictionary<ElementType, int>(){
                {ElementType.exteriorWall,1},
                {ElementType.interiorWall,2},
                {ElementType.interiorFloor,3},
                {ElementType.exteriorRoof,4},
                {ElementType.groundFloor,5},
                {ElementType.window,6},
            };

            var opaqueTransparentDict = new Dictionary<ElementType, string>()
            {
                {ElementType.exteriorWall," 0.70 0.90 0.70 0.90 0.000e-09 0.000e-10"},
                {ElementType.interiorWall," 0.70 0.90 0.70 0.90 0.000e-09 0.000e-10"},
                {ElementType.interiorFloor," 0.70 0.90 0.70 0.90 0.000e-09 0.000e-10"},
                {ElementType.exteriorRoof," 0.70 0.90 0.70 0.90 0.000e-09 0.000e-10"},
                {ElementType.groundFloor," 0.70 0.90 0.70 0.90 0.000e-09 0.000e-10"},
                {ElementType.window," 0.00 0.84 0.00 0.84 0.000e-00 0.000e-00"},
            };

            //constructionsのorderをreverseする=>室内側と室外側が逆だった  

            constructions.ForEach(construction =>
            {
                int numMaterials = construction.materials.Count;
                int cavityLayer = 0;
                bool window = false;

                wDat += Converter.FillEmpty(construction.therbId, 3)
                + Converter.FillEmpty(elementIdDict[construction.category], 2)
                + opaqueTransparentDict[construction.category]
                + Converter.FillEmpty(numMaterials, 3) + " \r\n";

                //2行目入力 classification
                int i = 0;
                construction.materials.ForEach(material =>
                {
                    i += 1;
                    wDat += Converter.FillEmpty(material.classification, 10);
                    if (material.classification == 6)
                    {
                        window = true;
                    }

                    if (material.classification == 3 || material.classification == 4|| material.classification == 5)
                    {
                        cavityLayer = i;
                    }
                    
                });
                wDat += FillMultipleZeros(13-numMaterials,10,0);
                wDat += " \r\n";

                //3行目入力 分割数
                //TODO: 厚みとかによってdynamicに分割数が変わるロジック
                construction.materials.ForEach(material =>
                {
                    if (material.classification == 3 || material.classification == 4 || material.classification == 5)
                    {
                        wDat += Converter.FillEmpty(2, 10);
                    }
                    else
                    {
                        wDat += Converter.FillEmpty(1, 10);
                    }
                    
                });
                wDat += FillMultipleZeros(13 - numMaterials, 10,0);
                wDat += " \r\n";

                //4行目入力 厚み
                construction.materials.ForEach(material =>
                {
                    double ceiledThickness = material.thickness / 1000;

                    //厚みが1mm以下のマテリアルはTHERBが読み込めない
                    if (material.thickness <= 1)
                    {
                        ceiledThickness = 0.001;
                    }

                    
                    wDat += Converter.FillEmpty(ceiledThickness, 10, 3);
                });
                wDat += FillMultipleZeros(13 - numMaterials, 10,3);
                wDat += " \r\n";

                //5行目　熱伝導率
                construction.materials.ForEach(material =>
                {
                    wDat += Converter.FillEmpty(material.conductivity, 10,3);
                });
                wDat += FillMultipleZeros(13 - numMaterials, 10, 3);
                wDat += " \r\n";

                //6行目　比熱
                construction.materials.ForEach(material =>
                {
                    wDat += Converter.FillEmpty(material.specificHeat, 10, 1);
                });
                wDat += FillMultipleZeros(13 - numMaterials, 10, 1);
                wDat += " \r\n";

                //7行目　密度
                construction.materials.ForEach(material =>
                {
                    wDat += Converter.FillEmpty(material.density, 10, 1);
                });
                wDat += FillMultipleZeros(13 - numMaterials, 10, 1);
                wDat += " \r\n";

                //8行目　水分伝導率
                construction.materials.ForEach(material =>
                {
                    wDat += Converter.FillEmpty(0, 10);
                });
                wDat += " \r\n";

                //9行目　水分容量
                construction.materials.ForEach(material =>
                {
                    wDat += Converter.FillEmpty(0, 10);
                });
                wDat += " \r\n";

                //TODO: this logic has to be fixed
                if (window)
                {
                    wDat += CavityLayers(numMaterials, cavityLayer, 0.0, 0.0) + "\r\n" //solar absorptance
                    + CavityLayers(numMaterials, cavityLayer, 0.0,0.0) + "\r\n" //solar transmittance
                    + CavityLayers(numMaterials, cavityLayer, 0.84,0.0) + "\r\n" //interior emissivity of glazing & low-e & shade, etc.
                    + CavityLayers(numMaterials, cavityLayer, 0.84, 0.0) + "\r\n" //exterior emissivity of glazing & low-e & shade, etc.
                    + CavityLayers(numMaterials, cavityLayer, 0.0,2.0) + "\r\n" //interior coefficient of cavity convection [W/m2 K]
                    + CavityLayers(numMaterials, cavityLayer, 0.0, 2.0) + "\r\n" //exterior coefficient of cavity convection [W/m2 K]
                    + CavityLayers(numMaterials, cavityLayer, 0, 0) + "\r\n" //velocity of air flow through cavity [m/s]
                    + CavityLayers(numMaterials, cavityLayer, 0, 0) + "\r\n"; //inflow from outdoor or room (= 0 or 1)
                }
                wDat += " \r\n";

            });

            return wDat;
        }

        public static string CreateSDat(Schedule schedule,List<Room> rooms)
        {
            List<int> m = schedule.monthly.hvac;
            List<int> w = schedule.weekly.hvac;
            List<int> dh = schedule.daily.hvac;
            List<int> dht = schedule.daily.heating;
            List<int> dct = schedule.daily.cooling;

            string sDat = "*month data   ----1---2---3---4---5---6---7---8---9--10--11--12 <== (no-con.=0, heating=1, cooling=2, h.c.=3) \r\n"
                    + " air condi.   -" + FillSchedule(m, 4) + "\r\n"
                    + "*week data    - sun mon tue wed thu fri sat <== (no-con.=0, conditioning=1) \r\n"
                    + " air condi.   -" + FillSchedule(w, 4) + "\r\n"
                    + "*hour  data   - ----1 ----2 ----3 ----4 ----5 ----6 ----7 ----8 ----9 ---10 ---11 ---12 ---13 ---14 ---15 ---16 ---17 ---18 ---19 ---20 ---21 ---22 ---23 ---24 \r\n";

            rooms.ForEach(room =>
            {
                sDat += "room ======>" + Converter.FillEmpty(room.id, 3) + FillSchedule(dh, 6) + "\r\n"
                    + " set low  tem.-" + FillSchedule(dht, 6, 1) + "\r\n"
                    + " set high tem.-" + FillSchedule(dct, 6, 1) + "\r\n"
                    + " inner heating-   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0 \r\n"
                    + " set low  R.H.-  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0  40.0 \r\n"
                    + " set high R.H.-  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0  60.0 \r\n"
                    + " moisture gen.-   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0   0.0 \r\n";
            });

            return sDat;
        }

        public static string CreateTDat(int startMonth,int EndMonth,Vector3d northDirection)
        {
            int calcDays = EndMonth * 30;
            //ベクトルから建物方位角への変換


            string tDat = "*** THERB 起動用データ *** \r\n"
                + "------------------ ----------------- （入出力データ）\r\n"
                + "出  力  データ     -o.dat \r\n"
                + "気  象  データ     -Fukuoka.dat \r\n"
                + "壁  体  データ     -w.dat \r\n"
                + "建  物  データ     -b.dat \r\n"
                + "  室    データ     -r.dat \r\n"
                + "室換気  データ     -a.dat \r\n"
                + "ｽｹｼﾞｭｰﾙ データ     -s.dat \r\n"
                + "------------------ ------- i（計算日） \r\n"
                + "計算年             -      2019 \r\n"
                + "計算開始月         -         " + startMonth.ToString() + " \r\n"
                + "計算開始日         -         1 \r\n"
                + "計算日数           -       " + calcDays.ToString() + " \r\n"
                + "予備計算日数       -         5 \r\n"
                + "------------------ ------- i（0：無し，1：簡易（吸放湿無し）2：詳細（吸放湿有り） \r\n"
                + "湿度計算の有無     -         1 \r\n"
                + "------------------ ---.--- （計算地域等の基本データ） \r\n"
                + "緯度      (°)     -  33.60 \r\n"
                + "経度      (°)     - 130.22 \r\n"
                + "建物方位角(°)     -" + Converter.FillEmpty(NorthDegree(northDirection),6,1) + " \r\n"
                + "地表面日射吸収率   -   0.8 \r\n"
                + "地表面長波放射率   -   0.9 \r\n"
                + "------------------ -----.- （計算時間間隔） \r\n"
                + "計算時間間隔(secs) -  3600. \r\n"
                + "------------------ ---.--- （0：毎時計算，W/m2K） \r\n"
                + "対流熱伝達率inside -   4.0 \r\n"
                + "            outside-  19.0 \r\n"
                + "放射熱伝達率inside -   5.0 \r\n"
                + "            outside-   5.0 \r\n"
                + "--------------------1--2--3--4--5--6--7--8--9-10 \r\n"
                + "温湿度出力データ№ - 186 187 188 189 190 191 192 193 194 195 \r\n"
                + "（30点）           - 196 187 198 199 200 201  92  94  96  97 \r\n"
                + "                   -  88  90  87  98 201 201 201 201 201 201 \r\n"
                + "--------------------------------------------------------------- \r\n"
                + "熱負荷出力室№     -   2 \r\n";

            return tDat;

        }

        private static string FillSchedule(List<int> schedule,int eachLength)
        {
            string result = "";
            schedule.ForEach(s =>
            {
                result += Converter.FillEmpty(s, eachLength);
            });
            return result;
        }
        private static string FillSchedule(List<int> schedule, int eachLength,int digit)
        {
            string result = "";
            schedule.ForEach(s =>
            {
                result += Converter.FillEmpty(s, eachLength, digit);
            });
            return result;
        }
        private static string FillMultipleZeros(int repeatNum,int totalLength,int digit)
        {
            string result = "";
            for (int i = 0; i < repeatNum; i++)
            {
                result += Converter.FillEmpty(0, totalLength,digit);
            }
            return result;
        }

        private static string OutputWindowIds(Face face)
        {
            const int MAX_IMPUT_ID_COUNT = 11;

            string windowIdStrs = "";
            for (int windowIdIndex = 0; windowIdIndex < MAX_IMPUT_ID_COUNT; windowIdIndex++)
            {
                if (windowIdIndex < face.windowIds.Count)
                {
                    windowIdStrs += string.Format("{0,4}", face.windowIds[windowIdIndex]);
                }
                else
                {
                    windowIdStrs += Converter.FillEmpty("0", 4);
                }
            }
            return windowIdStrs;
        }

        private static string CavityLayers(int numMaterials, int cavityLayer, double glassNumber, double cavityNumber,int digit=3)
        {

            string windowIdStrs = "";
            for (int windowIdIndex = 1; windowIdIndex < numMaterials+1; windowIdIndex++)
            {
                if (windowIdIndex == cavityLayer)
                {
                    windowIdStrs += Converter.FillEmpty(cavityNumber,10,digit);
                }
                else
                {
                    windowIdStrs += Converter.FillEmpty(glassNumber, 10, digit);
                }
            }
            return windowIdStrs;
        }

        private static double NorthDegree(Vector3d north)
        {
            var northDegree = 0d;

            var angle1 = RhinoMath.ToDegrees(Vector3d.VectorAngle(Vector3d.XAxis, north));
            var angle2 = RhinoMath.ToDegrees(Vector3d.VectorAngle(Vector3d.YAxis, north));

            if (angle1 < 90)
                northDegree = 360 - angle2;
            else
                northDegree = angle2;

            if (double.IsInfinity(northDegree))
                throw new Exception("建物方位角が無限大になりました。");

            return northDegree;
        }
    }

    public class Setting
    {
        public Vector3d northDirection;
        public int startMonth;
        public int endMonth;
        public double ventilationRate;

        public Setting()
        {

        }

        public Setting(Vector3d northDirection,int startMonth,int endMonth,double ventilationRate)
        {
            this.northDirection = northDirection;
            this.startMonth = startMonth;
            this.endMonth = endMonth;
            this.ventilationRate = ventilationRate;
        }
    }
    public class ResEnvelope
    {
        public List<EnvelopePayload> data;
    }

    public class EnvelopePayload
    {
        public string name;
        public string id;
        public Construction exteriorWall;
        public Construction interiorWall;
        public Construction floorCeiling;
        public Construction groundFloor;
        public Construction roof;
        public Construction window;
    }

    public class Envelope
    {
        public string name;
        public string id;
        public Construction exteriorWall;
        public Construction interiorWall;
        public Construction floorCeiling;
        public Construction groundFloor;
        public Construction roof;
        public Construction window;
        public int exteriorWallId;
        public int interiorWallId;
        public int floorCeilingId;
        public int groundFloorId;
        public int roofId;
        public int windowId;

        public Envelope()
        {

        }

        public Envelope(EnvelopePayload payload)
        {
            //EnvelopePayloadの中身が変わる
            name = payload.name;
            id = payload.id;
            exteriorWall = payload.exteriorWall;
            interiorWall = payload.interiorWall;
            floorCeiling = payload.floorCeiling;
            groundFloor = payload.groundFloor;
            roof = payload.roof;
            window = payload.window;
        }

        public Envelope(int exWallId,int inWallId,int floorCeilId, int exRoofId, int groundFlrId, int windowCntId)
        {
            exteriorWallId = exWallId;
            interiorWallId = inWallId;
            floorCeilingId = floorCeilId;
            groundFloorId = groundFlrId;
            roofId = exRoofId;
            windowId = windowCntId;
        }

        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                //preview += " Id         :" + id + Environment.NewLine;
                preview += " Name :" + name + Environment.NewLine;
                preview += " exteriorWallId :" + exteriorWallId.ToString() + Environment.NewLine;
                preview += " roofId :" + roofId.ToString() + Environment.NewLine;
                preview += " groundFloorId :" + groundFloorId.ToString() + Environment.NewLine;
                preview += " interiorWallId :" + interiorWallId.ToString() + Environment.NewLine;
                preview += " floorCeilingId :" + floorCeilingId.ToString() + Environment.NewLine;
                preview += " windowId :" + windowId.ToString() + Environment.NewLine;
            }
            catch { }
            return preview;
        }
    }

    public class ResSchedule
    {
        public List<Schedule> data;
    }

    public class Schedule
    {
        public string name;
        public string description;
        public MonthlySch monthly;
        public WeeklySch weekly;
        public DailySch daily;
        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                preview += " Name :" + name + Environment.NewLine;
            }
            catch { }
            return preview;
        }
    }

    public class WeeklySch
    {
        public List<int> hvac;
    }

    public class MonthlySch
    {
        public List<int> hvac;
    }

    public class DailySch
    {
        public List<int> hvac;
        public List<int> heating;
        public List<int> cooling;
    }

    public class ResConstruction
    {
        public List<Construction> data;
    }

    public class ConstructionPayload
    {
        public string id;
        [JsonProperty(PropertyName = "category")]
        public ElementType category;
        public List<Material> materials;
    }

    public class Construction
    {
        public string id;
        public string therbId;
        [JsonProperty(PropertyName = "category")]
        public ElementType category;
        public List<Material> materials;

        public Construction()
        {

        }


        public Construction(ConstructionPayload opaque, int opaqueCount)
        {
            int idNum = opaqueCount;
            therbId = idNum.ToString();
            id = opaque.id;
            category = opaque.category;
            materials = opaque.materials;
        }

        public Construction(ConstructionPayload translucent, int opaqueCount, int translucentId)
        {
            int idNum = translucentId + opaqueCount;
            therbId = idNum.ToString();
            id = translucent.id;
            category = ElementType.window;
            materials = translucent.materials;
        }

        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                preview += " Id         :" + id + Environment.NewLine;
                preview += " Category :" + category + Environment.NewLine;
                preview += " Materials  :" + string.Join(", ", Material.GetNames(materials)) + Environment.NewLine;
            }
            catch { }
            return preview;
        }

        public bool filterByCategory(ElementType category)
        {
            if (this.category == category)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public class ResTranslucent
    {
        public List<ConstructionPayload> data;
    }


    public class Translucent
    {
        public string id;
        public string therbId;
        [JsonProperty(PropertyName = "category")]
        public ElementType categories;
        public List<Material> materials;


        public Translucent(ConstructionPayload translucent, int opaqueCount,int translucentId)
        {
            int idNum = translucentId + opaqueCount;
            therbId = idNum.ToString();
            id = translucent.id;
            categories = ElementType.window;
            materials = translucent.materials;
        }
    }

    public class ResOpaque
    {
        public List<ConstructionPayload> data;
    }

    public class Opaque
    {
        public string id;
        public string therbId;
        [JsonProperty(PropertyName = "category")]
        public ElementType categories;
        public List<Material> materials;

        public Opaque(ConstructionPayload opaque, int opaqueCount)
        {
            int idNum = opaqueCount;
            therbId = idNum.ToString();
            id = opaque.id;
            categories = opaque.category;
            materials = opaque.materials;
        }
    }



    public class Material
    {
        public string id;
        public string name;
        public double conductivity;
        public double density;
        public double specificHeat;
        public double thickness;
        public int classification;

        public static List<string> GetNames(List<Material> materials)
        {
            var names = new List<string>();
            foreach (var material in materials)
                names.Add(material.name);
            return names;
        }
    }
}


