using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino.Geometry.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;

namespace Model
{
    public class Room : BaseGeo
    {
        public Brep geometry;
        public Point3d centroid;
        private List<Face> _faceList;
        public static int _totalRooms;
        public BrepVertexList vertices;
        public List<Face> sWalls;
        public List<Face> wWalls;
        public List<Face> nWalls;
        public List<Face> eWalls;
        public List<Face> floors;
        public List<Face> roofs;
        public double volume;
        static Room()
        {
            _totalRooms = 0;
        }

        public static void InitTotalRoom()
        {
            _totalRooms = 0;
        }

        public Room(Brep geometry)
        {
            guid = Guid.NewGuid();
            _totalRooms += 1;
            id = _totalRooms;
            //displayName = _totalRooms;
            _faceList = new List<Face>();
            this.geometry = geometry;
            BrepVertexList vertices = geometry.Vertices;
            this.vertices = vertices;
            minPt = getMinCoord(vertices);
            maxPt = getMaxCoord(vertices);
            sWalls = new List<Face>();//これらは紐づくwindowの数も加える  
            wWalls = new List<Face>();
            nWalls = new List<Face>();
            eWalls = new List<Face>();
            floors = new List<Face>();
            roofs = new List<Face>();
            VolumeMassProperties vmp = VolumeMassProperties.Compute(geometry);
            centroid = vmp.Centroid;
            //brepのボリュームを計算する
            volume = geometry.GetVolume();
        }

        public void addFace(Face face)
        {
            _faceList.Add(face);
        }

        public void groupChildFaces(Face face)
        {
            if (face.surfaceType == SurfaceType.Wall)
            {
                switch (face.direction)
                {
                    case Direction.S:
                        sWalls.Add(face);
                        break;
                    case Direction.N:
                        nWalls.Add(face);
                        break;
                    case Direction.W:
                        wWalls.Add(face);
                        break;
                    case Direction.E:
                        eWalls.Add(face);
                        break;
                }
            }
            else if (face.surfaceType == SurfaceType.Roof)
            {
                roofs.Add(face);
            }
            else if (face.surfaceType == SurfaceType.Floor)
            {
                floors.Add(face);
            }

            //窓も追加する
            face.windows.ForEach(window =>
            {
                switch (face.direction)
                {
                    case Direction.S:
                        sWalls.Add(face);
                        break;
                    case Direction.N:
                        nWalls.Add(face);
                        break;
                    case Direction.W:
                        wWalls.Add(face);
                        break;
                    case Direction.E:
                        eWalls.Add(face);
                        break;
                }
            });
        }

        public List<int> getDirectionList()
        {
            List<int> directionList = new List<int>();
            directionList.Add(sWalls.Count);
            directionList.Add(wWalls.Count);
            directionList.Add(nWalls.Count);
            directionList.Add(eWalls.Count);
            directionList.Add(floors.Count);
            directionList.Add(roofs.Count);
            int total = sWalls.Count + wWalls.Count + nWalls.Count + eWalls.Count + floors.Count + roofs.Count;
            directionList.Add(total);

            return directionList;//[S,W,N,E,F,CR,total]
        }

        public List<Face> getFaces()
        {
            return _faceList;
        }

        //public static string generateDisplayName{
        //}

        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                preview += " Id          :" + id + Environment.NewLine;
                preview += " centroid    :" + centroid + Environment.NewLine;
                preview += " faceListIds :" + string.Join(", ", Face.GetFaceIds(_faceList)) + Environment.NewLine;
                preview += " vertices    :" + vertices + Environment.NewLine;
                preview += " volume      :" + volume;
            }
            catch { }
            return preview;
        }

    }
}
