using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Model;
using System.Windows.Forms;

// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace THERBgh
{
    public class FilterConstruction : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public FilterConstruction()
          : base("FilterConstruction", "FilterConstruction",
              "Filter construction by properties",
              "THERB-GH", "Modelling")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Constructions", "Constructions", "Construction data", GH_ParamAccess.list);
            pManager.AddIntegerParameter("category", "category", "category to filter", GH_ParamAccess.item, -1);
            var categoryInput = (Param_Integer)pManager[1];
            categoryInput.AddNamedValue("exteriorWall", (int)ElementType.exteriorWall);
            categoryInput.AddNamedValue("interiorWall", (int)ElementType.interiorWall);
            categoryInput.AddNamedValue("interiorFloor", (int)ElementType.interiorFloor);
            categoryInput.AddNamedValue("exteriorRoof", (int)ElementType.exteriorRoof);
            categoryInput.AddNamedValue("groundFloor", (int)ElementType.groundFloor);
            categoryInput.AddNamedValue("window", (int)ElementType.window);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Constructions", "Constructions", "Construction data", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Construction> constructionList = new List<Construction>();       
            DA.GetDataList(0, constructionList);

            int category = -1;
            DA.GetData(1, ref category);

            //keyが正しくなかったらエラーを返すようにする  
            //Enum.TryParse(bc, out BoundaryCondition boundaryCondition);
            if (category < -1 | Enum.GetNames(typeof(ElementType)).Length < category)
            {
                throw new ArgumentException("範囲外の数字が入れられました。", "category");
            }

            List<Construction> trueConstructionList = new List<Construction>();

            foreach (Construction construction in constructionList)
            {
                if(category == -1 | construction.filterByCategory((ElementType)category)) 
                {
                    trueConstructionList.Add(construction);
                }
            }

            DA.SetDataList("Constructions", trueConstructionList);
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
            get { return new Guid("696a31b5-f362-4122-989d-a072fc87a0b1"); }
        }
    }
}
