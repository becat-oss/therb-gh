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
    //overhangはwallに紐づく
    public class Overhang : BaseFace
    {
        public Face parentFace; //overhangの場合は、parentFace,parentWindowどちらも持つ
        public Window parentWindow;
        public int parentWindowId;
        public static int _totalOverhangs;

        static Overhang()
        {
            _totalOverhangs = 0;
        }

        public static void InitTotalOverhang()
        {
            _totalOverhangs = 0;
        }

        public Overhang(Surface geometry) : base(geometry)
        {
            guid = Guid.NewGuid();
            _totalOverhangs += 1;
            id = _totalOverhangs;
            this.constructionId = 1; //とりあえず外壁に揃えて1とする
        }

        public void addParentFace(Face parentFace)
        {
            this.parentFace = parentFace;
            parentId = parentFace.partId;
            tiltAngle = parentFace.tiltAngle;
        }

        public void addParentWindow(Window parentWindow)
        {
            this.parentWindow = parentWindow;
            parentWindowId = parentWindow.id;
        }

        public override string ToString()
        {
            string preview = base.ToString();
            try
            {
                preview += " id       :" + id + Environment.NewLine;
                preview += " parentId :" + parentFace.id + Environment.NewLine;
                preview += " parentWindowId :" + parentWindowId;
            }
            catch { }
            return preview;
        }
    }
}
