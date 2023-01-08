using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino.Geometry.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using THERBgh;

namespace Model
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BoundaryCondition
    {
        exterior,
        interior,
        ground
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SurfaceType
    {
        Wall,
        Roof,
        Ceiling,
        Floor
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ElementType
    {
        exteriorWall,
        interiorWall,
        exteriorFloor,
        interiorFloor,
        interiorRoof,
        exteriorRoof,
        interiorCeiling,
        exteriorCeiling,
        groundFloor,
        groundRoof,
        groundWall,
        groundCeiling,
        window
    }

    public enum FaceKeys
    {
        bc,
        elementType,
        direction,
    }

    public enum Direction
    {
        N,
        S,
        W,
        E,
        F,
        CR
    }
    public class Face : BaseFace
    {
        public SurfaceType surfaceType { get; private set; }
        public BoundaryCondition bc { get; set; }
        public ElementType elementType { get; set; }
        public Room parent;
        public Vector3d tempNormal;
        public Direction direction;
        public int adjacencyRoomId; //隣接しているRoomのId 外気に接している場合には0
        public bool unique;
        public List<Window> windows { get; private set; }
        public List<int> windowIds;
        public static int _totalFaces;
        public static int _totalExWalls;
        public static int _totalInWalls;
        public static int _totalFlrCeilings;
        public static int _totalRoofs;
        public static int _totalGrounds;
        public Face adjacencyFace;

        static Face()
        {
            _totalFaces = 0;
            _totalExWalls = 0;
            _totalInWalls = 0;
            _totalFlrCeilings = 0;
            _totalRoofs = 0;
            _totalGrounds = 0;
        }

        public static void InitTotalFace()
        {
            _totalFaces = 0;
            _totalExWalls = 0;
            _totalInWalls = 0;
            _totalFlrCeilings = 0;
            _totalRoofs = 0;
            _totalGrounds = 0;
        }

        public Face(Room parent, Surface geometry, Vector3d normal, Vector3d tempNormal) : base(geometry)
        {
            guid = Guid.NewGuid();
            _totalFaces += 1;
            id = _totalFaces;
            //全体を通してのidとelementごとのidをつける
            this.parent = parent;
            parentId = parent.id;
            //方角、床、天井を判別するためにはtempNormalを使う
            this.surfaceType = getFaceType(tempNormal);
            direction = defineDirection(tempNormal);

            this.normal = tempNormal;
            this.unique = true;

            //TODO: this logic has to be elaborated
            if (surfaceType == SurfaceType.Wall)
            {
                tiltAngle = 90;
            }
            else
            {
                tiltAngle = 0;
            }
            windows = new List<Window>();
            windowIds = new List<int>();
        }

        /*
        public bool filterByKey<T>(T key,string value)
        {
            if (this.key == value)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        */
        public bool filterByBc(BoundaryCondition bc)
        {
            if (this.bc == bc)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //TODO: 他のFaceKeysに関しても適用できるよう汎用性を持たせたい（上みたいな感じ）
        public bool filterByDirection(Direction direction)
        {
            if (this.direction == direction)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool filterBySurfaceType(SurfaceType surfT)
        {
            if (this.surfaceType == surfT)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private Direction defineDirection(Vector3d normal)
        {
            if (surfaceType == SurfaceType.Floor)
            {
                return Direction.F;
            }
            else if (surfaceType == SurfaceType.Roof)
            {
                return Direction.CR;
            }

            Vector3d northAngle = new Vector3d(0, 1, 0);
            Vector3d westAngle = new Vector3d(-1, 0, 0);

            double angle1 = (180 / Math.PI) * (Vector3d.VectorAngle(normal, northAngle));
            double angle2 = (180 / Math.PI) * (Vector3d.VectorAngle(normal, westAngle));

            //Print("{0}:{1}", normal, angle1);

            if (angle1 < 45)
            {
                return Direction.N;
            }
            else if (angle1 == 45 && angle2 == 45)
            {
                return Direction.N;
            }
            else if (angle1 > 135)
            {
                return Direction.S;
            }
            else if (angle1 == 135 && angle2 == 135)
            {
                return Direction.S;
            }
            else if (angle2 <= 45)
            {
                return Direction.W;
            }
            else
            {
                return Direction.E;
            }
        }

        public SurfaceType getFaceType(Vector3d normal)
        {

            //better to add filter logic for internal wall
            double Z = normal.Z;
            int roofAngle = 10;
            if (Z > Math.Cos((Math.PI / 180) * roofAngle))
            {
                return SurfaceType.Roof;
            }
            else if (Z < -Math.Cos((Math.PI / 180) * 85))
            {
                return SurfaceType.Floor;
            }
            else
            {
                //string direction = defineDirection(normal);
                //Print("{0}:{1}", normal, direction);
                return SurfaceType.Wall;
            }
        }

        public void addWindows(Window window)
        {
            windows.Add(window);
            windowIds.Add(window.id);
        }

        public void setPartId()
        {
            if (unique)
            {
                switch (elementType)
                {
                    case ElementType.exteriorWall:
                        partId = _totalExWalls;
                        break;
                    case ElementType.interiorWall:
                        partId = _totalInWalls;
                        break;
                    case ElementType.interiorRoof:
                        partId = _totalFlrCeilings;
                        break;
                    case ElementType.interiorFloor:
                        partId = _totalFlrCeilings;
                        break;
                    case ElementType.exteriorRoof:
                        partId = _totalRoofs;
                        break;
                    case ElementType.groundFloor:
                        partId = _totalGrounds;
                        break;
                }
            }
        }

        public void setElementType()
        {
            string strElementType = bc.ToString() + surfaceType.ToString();
            elementType = (ElementType)Enum.Parse(typeof(ElementType), strElementType);

            if (!unique) return;

            switch (elementType)
            {
                case ElementType.exteriorWall:
                    _totalExWalls += 1;
                    break;
                case ElementType.interiorWall:
                    _totalInWalls += 1;
                    break;
                case ElementType.interiorRoof:
                    _totalFlrCeilings += 1;
                    break;
                case ElementType.interiorFloor:
                    _totalFlrCeilings += 1;
                    break;
                case ElementType.exteriorRoof:
                    _totalRoofs += 1;
                    break;
                case ElementType.groundFloor:
                    _totalGrounds += 1;
                    break;
            }
        }

        public void setConstructionId(Envelope envelope,List<Construction> constructions)
        {
            //Print("{0}", elementType);
            switch (elementType)
            {
                //construction側でtherb用の通しidは管理している
               
                case ElementType.exteriorWall:
                    constructionId = 1;
                    structureId = constructions.Find(opaque => opaque.id == envelope.exteriorWall.id).therbId;
                    break;
                case ElementType.interiorWall:
                    constructionId = 2;
                    structureId = constructions.Find(opaque => opaque.id == envelope.interiorWall.id).therbId;
                    break;
                case ElementType.interiorRoof:
                    constructionId = 3;
                    structureId = constructions.Find(opaque=>opaque.id == envelope.floorCeiling.id).therbId;
                    break;
                case ElementType.interiorFloor:
                    constructionId = 3;
                    structureId = constructions.Find(opaque => opaque.id == envelope.floorCeiling.id).therbId;
                    break;
                case ElementType.exteriorRoof:
                    constructionId = 4;
                    structureId = constructions.Find(opaque => opaque.id == envelope.roof.id).therbId;
                    break;
                case ElementType.groundRoof:
                    constructionId = 5;
                    structureId = constructions.Find(opaque => opaque.id == envelope.groundFloor.id).therbId;
                    break;
                case ElementType.groundFloor:
                    constructionId = 5;
                    structureId = constructions.Find(opaque => opaque.id == envelope.groundFloor.id).therbId;
                    break;
            }
        }
        public void OverrideConstruction(Construction construction)
        {
            this.structureId = construction.therbId;
        }

        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                preview += " id                :" + id + Environment.NewLine;
                preview += " BoundaryCondition :" + bc + Environment.NewLine;
                preview += " elementType       :" + elementType + Environment.NewLine;
                preview += " parentId          :" + parent.id + Environment.NewLine;
                preview += " direction         :" + direction + Environment.NewLine;
                preview += " adjacencyRoomId   :" + adjacencyRoomId + Environment.NewLine;
                preview += " unique            :" + unique + Environment.NewLine;
                preview += " windowIds         :" + string.Join(", ", windowIds) + Environment.NewLine;
                if (adjacencyFace is null)
                {
                    preview += " adjacencyFaceId   :null";
                }
                else
                {
                    preview += " adjacencyFaceId"+adjacencyFace.id;
                }
                
            }
            catch { }
            return preview;
        }

        public static List<int> GetFaceIds(List<Face> faces)
        {
            var ids = new List<int>();
            foreach (var face in faces)
                ids.Add(face.id);
            return ids;
        }
    }
}
