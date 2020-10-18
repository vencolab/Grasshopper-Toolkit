using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Toolkit.Components
{
    public class SurfaceSorting : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public SurfaceSorting()
          : base("SrfIndexSort", "SS", "Sort Surfaces based on sides and split based on index", "Toolkit", "Surface")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddSurfaceParameter("Surfaces", "S", "Surfaces to sort", GH_ParamAccess.list);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Top Index", "Ti", "Surface index on the Top", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Side Index", "Si", "Surface index on the side", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Bottom Index", "Bi", "Surface index on the bottom", GH_ParamAccess.list);
            pManager.AddPointParameter("Top Points", "Tp", "Surfaces on the Top", GH_ParamAccess.list);
            pManager.AddPointParameter("Side Points", "Sp", "Surfaces on the side", GH_ParamAccess.list);
            pManager.AddPointParameter("Bottom Points", "Bp", "Surfaces on the bottom", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Surface> srfs = new List<Surface>();

            if (!DA.GetDataList(0,  srfs)) return;

            List<int> topSrfIndex = new List<int>();
            List<int> sideSrfIndex = new List<int>();
            List<int> bottomSrfIndex = new List<int>();
            
            List<Point3d> topSrfList = new List<Point3d>();
            List<Point3d> sideSrfList = new List<Point3d>();
            List<Point3d> bottomSrfList = new List<Point3d>();
            for (int i = 0; i < srfs.Count; i++)
            {
                Surface srf = srfs[i];
                int z = (int)srf.NormalAt(0.5, 0.5).Z + 1; //get normal of each surface as an int; -1 = bottom, 0 = side, 1 = top
                switch (z)
                {
                    case 0:
                        topSrfList.Add(srf.PointAt(0.5, 0.5));
                        topSrfIndex.Add(i);
                        break;
                    case 1:
                        sideSrfList.Add(srf.PointAt(0.5, 0.5));
                        sideSrfIndex.Add(i);
                        break;
                    case 2:
                        bottomSrfList.Add(srf.PointAt(0.5, 0.5));
                        bottomSrfIndex.Add(i);
                        break;
                    default:
                        sideSrfList.Add(srf.PointAt(0.5, 0.5));
                        sideSrfIndex.Add(i);
                        break;
                }
            }

            //Parallel.ForEach(srfs,
            //    srf =>
            //    {
            //        int z = (int)srf.NormalAt(0.5, 0.5).Z +1;
            //        switch (z)
            //        {
            //            case 0:
            //                topSrfList.Add(srf.PointAt(0.5,0.5));
            //                break;
            //            case 1:
            //                sideSrfList.Add(srf.PointAt(0.5, 0.5));
            //                break;
            //            case 2:
            //                bottomSrfList.Add(srf.PointAt(0.5, 0.5));
            //                break;
            //            default:
            //                sideSrfList.Add(srf.PointAt(0.5, 0.5));
            //                break;
                            

            //        }                       // if normal is facing up place surface in topSrfList
            //    });


            DA.SetDataList(0, topSrfIndex);
            DA.SetDataList(1, sideSrfIndex);
            DA.SetDataList(2, bottomSrfIndex);
            DA.SetDataList(3, topSrfList);
            DA.SetDataList(4, sideSrfList);
            DA.SetDataList(5, bottomSrfList);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("b05174ec-be20-4bef-acfa-0ddab686c8e5"); }
        }
    }
}