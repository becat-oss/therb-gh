using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Rhino.Geometry.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.ObjectModel;
using static Model.Room;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Windows.Forms;
using THERBgh;

namespace Model
{
    public class AreaData
    {
        public double exteriorWall;
        public double interiorWall;
        public double exteriorFloor;
        public double interiorFloor;
        public double interiorRoof;
        public double exteriorRoof;
        public double interiorCeiling;
        public double exteriorCeiling;
        public double groundFloor;
        public double groundRoof;
        public double groundWall;
        public double groundCeiling;
        public double window;
        public double skylight;
    }
    public class SendData
    {
        public AreaData area;
        public string envelope_id;
    }
    public class Therb
    {
        const string GEOM_DATA_POST_URL = "https://oyster-app-8jboe.ondigitalocean.app/geometry/";

        public List<Room> rooms;
        public List<Face> faces;
        public List<Window> windows;
        public List<Overhang> overhangs;

        public Dictionary<ElementTypeForTotalArea, double> TotalAreaForElementType 
            = new Dictionary<ElementTypeForTotalArea, double>();
        //shadingとかものちのちつけていく

        public Envelope envelope;

        public Therb(List<Room> rooms, List<Face> faces, List<Window> windows, List<Overhang> overhangs, Envelope envelope)
        {
            this.rooms = rooms;
            this.faces = faces;
            this.windows = windows;
            this.overhangs = overhangs;
            this.envelope = envelope;
        }

        public void CheckTherb(out List<string> messages)
        {
            messages = new List<string>();
            bool existOnGround = false, 
                 existFloating = false, 
                 existBuried   = false, 
                 existInGround = false;
            foreach (Room room in rooms)
            {
                var roomStatus = room.CheckRoom();
                if (roomStatus == RoomStatus.OnGround)
                    existOnGround = true;
                else if(roomStatus == RoomStatus.Floating)
                    existFloating = true;
                else if (roomStatus == RoomStatus.Buried)
                    existBuried = true;
                else existInGround = true;
            }

            if (!existOnGround &
                 !existFloating &
                 !existBuried &
                 existInGround)
                messages.Add("モデルが全部地面に埋まっています。");

            if (!existOnGround &
                 existFloating &
                 !existBuried &
                 !existInGround)
                messages.Add("モデルが全部宙に浮いています。");

            if (existBuried || existOnGround & existInGround)
                messages.Add("モデルが一部地面に埋まっています。");

        }
        public void TransformForTherbAnarysis()
        {
            double tol = 0.001;

            List<BoundingBox> boundingBoxes = new List<BoundingBox>();
            var minX = double.MaxValue;
            var minY = double.MaxValue;/*
            foreach (var room in rooms)
                boundingBoxes.Add(room.geometry.GetBoundingBox(false));
            foreach (var overhang in overhangs)
                boundingBoxes.Add(overhang.geometry.GetBoundingBox(false));*/
            foreach (var room in rooms)
            {
                var bb = room.geometry.GetBoundingBox(false);
                if (bb.Min.X < minX) minX = bb.Min.X;
                if (bb.Min.Y < minY) minY = bb.Min.Y;
            }
            foreach (var overhang in overhangs)
            {
                BoundingBox bb = overhang.geometry.GetBoundingBox(false);
                if (bb.Min.X < minX) minX = bb.Min.X;
                if (bb.Min.Y < minY) minY = bb.Min.Y;
            }

            var tr = Transform.Translation(-minX + tol, -minY + tol, 0);

            foreach (var room in rooms)
                room.Transform(tr);
            foreach (var face in faces)
                face.Transform(tr);
            foreach (var window in windows)
                window.Transform(tr);
            foreach (var overhang in overhangs)
                overhang.Transform(tr);

        }

        public bool TryCalcTotalArea()
        {
            try
            {
                TotalAreaForElementType = new Dictionary<ElementTypeForTotalArea, double>();
                foreach (var key in Enum.GetValues(typeof(ElementTypeForTotalArea)))
                    TotalAreaForElementType.Add((ElementTypeForTotalArea)key, 0d);

                foreach (var face in faces)
                    TotalAreaForElementType[BaseFace.ToElementTypeForTotalArea(face.elementType)] 
                        += face.area;

                foreach(var window in windows)
                {
                    if(window.parent.surfaceType == SurfaceType.Ceiling)
                        TotalAreaForElementType[ElementTypeForTotalArea.skylight]
                            += window.area;
                    else
                        TotalAreaForElementType[ElementTypeForTotalArea.window]
                            += window.area;
                }

                return true;
            }
            catch(Exception ex)
            {
                TotalAreaForElementType = null;
                return false;
            }
        }
        public void PostArea(string urlAddNum, Envelope envelope)
        {
            if (this == null | !this.TryCalcTotalArea()) 
                throw new ArgumentException("ジオメトリデータ送信できませんでした。", "therb");

            var totalAreaForElementType = this.TotalAreaForElementType;
            var sendData = new SendData();
            sendData.area = new AreaData();
            sendData.area.exteriorWall = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.exteriorWall],2);
            sendData.area.interiorWall = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.interiorWall], 2);
            sendData.area.exteriorFloor = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.exteriorFloor], 2);
            sendData.area.interiorFloor = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.interiorFloor], 2);
            sendData.area.interiorRoof = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.interiorRoof], 2);
            sendData.area.exteriorRoof = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.exteriorRoof], 2);
            sendData.area.interiorCeiling = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.interiorCeiling], 2);
            sendData.area.exteriorCeiling = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.exteriorCeiling], 2);
            sendData.area.groundFloor = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.groundFloor], 2);
            sendData.area.groundRoof = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.groundRoof], 2);
            sendData.area.groundWall = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.groundWall], 2);
            sendData.area.groundCeiling = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.groundCeiling], 2);
            sendData.area.window = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.window], 2);
            sendData.area.skylight = Math.Round(totalAreaForElementType[ElementTypeForTotalArea.skylight], 2);
            if (envelope != null)
                sendData.envelope_id = envelope.id;
            var rowdata = JsonConvert.SerializeObject(sendData);

            try
            {
                var content = new StringContent(rowdata, Encoding.UTF8, "application/json");

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, GEOM_DATA_POST_URL + urlAddNum);
                request.Content = content;
                var responseTask = new HttpClient().SendAsync(request);
                responseTask.Wait();
                var response = responseTask.Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {

                    Debug.WriteLine(response.Content);
                    MessageBox.Show("面積送信できました。");
                    return;
                }
                Debug.WriteLine(response.StatusCode);
            }
            catch (Exception e)
            {
                Debug.WriteLine("error: " + e.Message);
            }

        }
    }
}
