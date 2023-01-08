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
    public class Window : BaseFace
    {
        public Face parent;
        public static int _totalWindows;

        static Window()
        {
            _totalWindows = 0;
        }

        public static void InitTotalWindow()
        {
            _totalWindows = 0;
        }

        public Window(Surface geometry,Envelope envelope,List<Construction> constructions) : base(geometry)
        {
            guid = Guid.NewGuid();
            _totalWindows += 1;
            id = _totalWindows;
            this.constructionId = 6;
            this.structureId = constructions.Find(opaque => opaque.id == envelope.roof.id).therbId; ;
        }

        public void addParent(Face parent)
        {
            this.parent = parent;
            //parentId = parent.partId;//注意)part idを使っている
            parentId = parent.id;
            tiltAngle = parent.tiltAngle;
        }
        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += Environment.NewLine;
                preview += " id       :" + id + Environment.NewLine;
                preview += " parentId :" + parent.id;
            }
            catch { }
            return preview;
        }
    }
}
