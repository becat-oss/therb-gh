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
    public enum ElementTypeForTotalArea
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
        window,
        skylight
    }
    public class BaseFace : BaseGeo
    {
        public int parentId;
        public Surface geometry;
        public int partId;
        public Vector3d normal;
        public Point3d centerPt;
        public double tiltAngle;
        public BrepVertexList vertices;
        public double area;
        public int constructionId; //とりあえず外壁1,内壁2,床(室内）5,屋根4,床（地面）5, 窓6　THERBのelement Idに相当/鈴木君からもらったデータにとりあえずそろえる
        public int structureId; //w.datファイルのidと対応
        public Overhang overhang;
        public int overhangId; //overhangは一つしか指定できない

        public BaseFace(Surface geometry)
        {
            this.geometry = geometry;
            //parentId = parent.id; parentの設定方法がFaceとFenestrationで違う
            AreaMassProperties areaMp = AreaMassProperties.Compute(geometry);
            centerPt = areaMp.Centroid;
            area = areaMp.Area;
            //normal = tempNormal; 今のところ、normalというattributeがない
            BrepVertexList vertices = geometry.ToBrep().Vertices;
            this.vertices = vertices;

            minPt = getMinCoord(vertices);
            maxPt = getMaxCoord(vertices);

            overhangId = 0;

        }
        public void addOverhangs(Overhang overhang)
        {
            this.overhang = overhang;
            overhangId = overhang.id;
        }
        public void Transform(Transform transform)
        {
            this.geometry.Transform(transform);
        }

        public static ElementTypeForTotalArea ToElementTypeForTotalArea(ElementType elementType)
        {
            if (elementType == ElementType.exteriorWall) return ElementTypeForTotalArea.exteriorWall;
            if (elementType == ElementType.interiorWall) return ElementTypeForTotalArea.interiorWall;
            if (elementType == ElementType.exteriorFloor) return ElementTypeForTotalArea.exteriorFloor;
            if (elementType == ElementType.interiorFloor) return ElementTypeForTotalArea.interiorFloor;
            if (elementType == ElementType.interiorRoof) return ElementTypeForTotalArea.interiorRoof;
            if (elementType == ElementType.exteriorRoof) return ElementTypeForTotalArea.exteriorRoof;
            if (elementType == ElementType.interiorCeiling) return ElementTypeForTotalArea.interiorCeiling;
            if (elementType == ElementType.exteriorCeiling) return ElementTypeForTotalArea.exteriorCeiling;
            if (elementType == ElementType.groundFloor) return ElementTypeForTotalArea.groundFloor;
            if (elementType == ElementType.groundRoof) return ElementTypeForTotalArea.groundRoof;
            if (elementType == ElementType.groundWall) return ElementTypeForTotalArea.groundWall;
            if (elementType == ElementType.groundCeiling) return ElementTypeForTotalArea.groundCeiling;
            throw new Exception("error GetElementTypeForTotalArea");
        }
    }
}
