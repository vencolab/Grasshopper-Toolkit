using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hayball
{
    public class VariableDivideMT : GH_TaskCapableComponent<VariableDivideFunctions.SolveResults>
    {
        // initialise new instance  of variableDivide Class
        public VariableDivideMT() : base("Divide Variable Distance", "VarDist", "Divide curves using a list of distances between points", "Hayball", "Curve")
        {
        }

        //register all the input parameters
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

        public class SolveResults
        {
            public List<Curve> Curves { get; set; }
            public List<Point3d> Points { get; set; }
            public List<double> Parameters { get; set; }
        }

        private SolveResults ComputeDivs(Curve crv, List<double> distanceList)
        {
            SolveResults result = new SolveResults();

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

        // method that does work
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                Curve crv = null;
                List<double> divisions = new List<double>();
                Task<SolveResults> tsk = null;

                if (DA.GetData(0, ref crv) && DA.GetDataList(1, divisions))
                {
                    tsk = Task.Run(() => ComputeDivs(crv, divisions), CancelToken);
                }
                TaskList.Add(tsk);
                return;
            }
            if (!GetSolveResults(DA, out SolveResults results))
            {
                // 1. Collect
                Curve crv = null;
                List<double> divisions = new List<double>();

                if (!DA.GetData(0, ref crv)) { return; }
                if (!DA.GetDataList(1, divisions)) { return; }

                // 2. Compute
                results = ComputeDivs(crv, divisions);
            }
            // 3. Set
            if (results != null)
            {
                if (results.Parameters != null)
                {
                    DA.SetDataList(0, results.Curves);
                    DA.SetDataList(1, results.Points);
                    DA.SetDataList(2, results.Parameters);
                }
            }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DivideDistance;

        public override Guid ComponentGuid => new Guid("0C85412A-A5CF-4767-941B-A84C19F57AF1");
    }
}