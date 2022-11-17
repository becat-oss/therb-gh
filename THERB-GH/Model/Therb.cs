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

namespace Model
{
    public class Therb
    {
        public List<Room> rooms;
        public List<Face> faces;
        public List<Window> windows;
        public List<Overhang> overhangs;
        //shadingとかものちのちつけていく

        public Therb(List<Room> rooms, List<Face> faces, List<Window> windows, List<Overhang> overhangs)
        {
            this.rooms = rooms;
            this.faces = faces;
            this.windows = windows;
            this.overhangs = overhangs;
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
    }
}
