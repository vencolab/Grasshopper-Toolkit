using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;


namespace Hayball
{
    public class VariableDivideMTVarComp : GH_TaskCapableComponent<VariableDivideMTVarComp.SolveResults>, IGH_VariableParameterComponent
    {

        // initialise new instance  of variableDivide Class
        public VariableDivideMTVarComp() : base("Divide Variable Distance", "VarDist", "Divide curves using a list of distances between points(MT & VarParam)", "Hayball", "Curve")
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
            pManager.AddCurveParameter("Curve", "C", "Reparameterized curves", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "division Parameters", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parameters", "t", "division Parameters", GH_ParamAccess.list);
            //pManager.AddNumberParameter("test", "t", "division Parameters", GH_ParamAccess.list);
            pManager.HideParameter(0);
        }

        public class SolveResults
        {
            
            public List<Curve> Curves { get; set; }
            public List<Point3d> Points { get; set; }
            public List<double> Parameters { get; set; }

            //public List<double> div2 { get; set; }
        }

        private SolveResults ComputeDivs(Curve crv, ConcurrentDictionary <int, List<double>> distanceList)
        {
           distanceList.Values[1]
           
            if (distanceList.Count == 0)
            {
                return null;
            }

            if (crv.PointAtStart.DistanceTo(crv.PointAtEnd) < distanceList.Max())
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " Distance between start and end points of curve(s) may be too small." +
                    " If it yields undesirable results, consider increasing distance between them to max value in the supplied distances.");
            }

            // if parameter is within region A, use items from dictionary list A )
            // 
            SolveResults result = new SolveResults();

            crv.Domain = new Interval(0, crv.GetLength());

            double t1 = crv.Domain.T1;
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
            int i, j;
            i = j = -1;

            List<Curve> crvList = new List<Curve>();
            List<Point3d> ptList = new List<Point3d>() { crv.PointAtStart };
            List<double> paramList = new List<double>() { 0.0 };
            //List<double> div2 = distanceList2;


            // TODO: Fix intersection issues with curves that have their start and End points too close to each other; eg:closed curves.
            
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
            //result.div2 = div2;

            return result;
        }

        // method that does work
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            if (InPreSolve)
            {
                Curve crv = null;
                List<double> distances = new List<double>();
                //List<double> divisions2 = new List<double>();
                Task<SolveResults> tsk = null;

                if (DA.GetData(0, ref crv) && DA.GetDataList(1, distances))
                {
                    tsk = Task.Run(() => ComputeDivs(crv, distances), CancelToken);
                }
                TaskList.Add(tsk);
                return;
            }
            if (!GetSolveResults(DA, out SolveResults results))
            {
                // 1. Collect
                Curve crv = null;
                //List<double> distances = new List<double>();
                List<double> distances2 = new List<double>();
                Dictionary <int, distances> dict = new Dictionary <int, distances> ();

                for (int i = 1; i < Params.Input.Count; i++) 
                {
                    List<double>distances= new List<double>();

                    if (!DA.GetData(i, distances)) { return; }
                    dict.Add(i,distances);

                }
                if (!DA.GetData(0, ref crv)) { return; }
                if (!DA.GetDataList(1, distances)) { return; }
                if (!DA.GetDataList(2, distances2)) { return; }

                // 2. Compute
                results = ComputeDivs(crv, distances);
            }
            // 3. Set
            if (results != null)
            {
                if (results.Parameters != null)
                {
                    DA.SetDataList(0, results.Curves);
                    DA.SetDataList(1, results.Points);
                    DA.SetDataList(2, results.Parameters);
                    //DA.SetDataList(3, results.div2);
                }
            }
        }
        
        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input) { return true; }
            else { return false; }
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input) { return true; }
            else { return false; }
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number input = new Param_Number();
            return input;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return Params.UnregisterInputParameter(Params.Input[index]);
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 1; i < Params.Input.Count; i++) 
            {
                IGH_Param param = Params.Input[i];
                param.Name = GH_ComponentParamServer.InventUniqueNickname("Distance" + (i+1), Params.Input);
                param.NickName = "D" + (i+1);
                param.Description = "Optional list of distances";
                param.Access = GH_ParamAccess.list;
                param.Optional = true;
            }

        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        public override Guid ComponentGuid => new Guid("6D99AD86-ECDB-45F8-928B-6446C30F92F2");
    }
}
