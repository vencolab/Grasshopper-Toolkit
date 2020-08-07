using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Hayball
{
    public class VariableDivide : GH_Component
    {

        // initialise new instance  of variableDivide Class
        public VariableDivide() : base("Divide Variable Distance", "VarDist", "Divide curves using a list of distances between points", "Hayball", "Curve")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves to Divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "List of Distances", GH_ParamAccess.list);
            pManager.HideParameter(0);
        }

        //register all output parameters
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves with new domain", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "division Parameters", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parameters", "t", "division Parameters", GH_ParamAccess.list);
            pManager.HideParameter(0);
        }


        // method that does work
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            // 1. Collect
            Curve crv = null;
            List<double> divisions = new List<double>();

            if (!DA.GetData(0, ref crv)) { return; }
            if (!DA.GetDataList(1, divisions)) { return; }

            // 2. Compute
            Compute result = ComputeDivs(crv, divisions);

            // 3. Set
            if (result != null)
            {
                if (result.Parameters != null)
                {
                    DA.SetDataList(0, result.Curves);
                    DA.SetDataList(1, result.Points);
                    DA.SetDataList(2, result.Parameters);
                }
            }
        }

        public class Compute
        {
            public List<Curve> Curves { get; set; }
            public List<Point3d> Points { get; set; }
            public List<double> Parameters { get; set; }
        }

        private Compute ComputeDivs(Curve crv, List<double> distanceList)
        {
            Compute result = new Compute();

            if (distanceList.Count == 0)
            {
                return null;
            }

            if (crv.PointAtStart.DistanceTo(crv.PointAtEnd) < distanceList.Max())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " Distance between start and end points of curve(s) may be too small." +
                    " If it yields undesirable results, consider increasing distance between them to max value in the supplied distances.");
            }

            crv.Domain = new Interval(0, crv.GetLength());
            double t1 = crv.Domain.T1;
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            int i, j;
            i = j = -1;

            List<Curve> crvList = new List<Curve>();
            List<Point3d> ptList = new List<Point3d>() { crv.PointAtStart };
            List<double> paramList = new List<double>() { 0.0 };

            while (true)
            {
                //reset i if distance list end reached and curve still exists.
                if (i >= distanceList.Count - 1)
                {
                    i = -1;
                }

                // increment
                i++;
                j++;

                if (distanceList[i] < tol)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " Distances less than or equal to rhino model tolerance will be ignored");
                    distanceList.RemoveAt(i);
                }

                // sphere-curve intersection
                // using  overload with curve domain to limit intersection area- allows to break while-loop when curve reaches the end.
                CurveIntersections events = Intersection.CurveSurface(crv, new Interval(paramList[j], t1), new Sphere(crv.PointAt(paramList[j]), distanceList[i]).ToNurbsSurface(), tol, tol);

                if (events != null && events.Count > 0)
                {
                    paramList.Add(events.Last().ParameterA);
                    ptList.Add(events.Last().PointA);
                }
                else { break; }
            }

            crvList.Add(crv);
            ptList.Add(crv.PointAtEnd);
            paramList.Add(crv.Domain.Max);

            result.Curves = crvList;
            result.Points = ptList;
            result.Parameters = paramList;

            return result;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;
        //You can add image files to your project resources and access them like this:
        // return Resources.IconForThisComponent


        public override Guid ComponentGuid => new Guid("aa4d0bde-fd4c-43e0-89b9-166a03f0564f");
    }
}
