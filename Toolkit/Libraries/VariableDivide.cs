using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Linq;

namespace Toolkit
{
    public class VariableDivide
    {
        public List<Curve> Curves { get; set; }
        public List<Point3d> Points { get; set; }
        
        public List<Vector3d> Tangents { get; set; }
        public List<double> Parameters { get; set; }

        public bool WarningMessageTol { get; set; }
        public bool WarningMessageEndPts { get; set; }

        //public VariableDivide() { }
        public static VariableDivide Divide(Curve crvs, List<double> distances, int rep, Dictionary<int,List<double>> dictDistances)
        {
            VariableDivide vd = new VariableDivide();

            if (distances.Count == 0)
            {
                return null;
            }

            Dictionary<int, List<double>>.ValueCollection values = dictDistances.Values;

            crvs.Domain = new Interval(0, crvs.GetLength());
            double t1 = crvs.Domain.T1;
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            int i, j;
            i = j = -1;

            List<Curve> crvList = new List<Curve>();
            List<Point3d> ptList = new List<Point3d>() { crvs.PointAtStart };
            List<Vector3d> vecList = new List<Vector3d>() { crvs.TangentAtStart};
            List<double> paramList = new List<double>() { 0.0 };
            bool tolBool = false;
            bool endPtsBool = false;

            //warning message
            if (crvs.PointAtStart.DistanceTo(crvs.PointAtEnd) < distances.Max())
            {
                endPtsBool = true;
            }

            List<Curve> crv = new List<Curve>();
            crv.Sort((x, y) => x.PointAtStart.Z.CompareTo(y.PointAtStart.Z));

            //recursion

            while (true)
            {
                //reset i if distance list end reached but curve is still long.
                if (i >= distances.Count - 1)
                {
                    switch (rep)
                    {
                        case 0:
                            i = -1;
                            break;
                        case 1:
                            i = distances.Count - 1;
                            break;
                        case 2:
                            goto Finish;
                    }
                }

                // increment
                i++;
                j++;

                if (distances[i] < tol)
                {
                    tolBool = true;
                    distances.RemoveAt(i);
                }

                // sphere-curve intersection
                // using  overload with curve domain to limit intersection area- allows to break while-loop when curve reaches the end.
                CurveIntersections events = Intersection.CurveSurface(crvs, new Interval(paramList[j], t1), new Sphere(crvs.PointAt(paramList[j]), distances[i]).ToNurbsSurface(), tol, tol);

                if (events != null && events.Count > 0)
                {
                    if (events.Last().ParameterA > paramList.Last())
                    {
                        ptList.Add(events.Last().PointA);
                        vecList.Add(crvs.TangentAt(events.Last().ParameterA));
                        paramList.Add(events.Last().ParameterA);

                    }

                    else { break; }
                }
                else { break; }
            }

            Finish:
            crvList.Add(crvs);
            ptList.Add(crvs.PointAtEnd);
            vecList.Add(crvs.TangentAtEnd);
            paramList.Add(crvs.Domain.Max);

            vd.Curves = crvList;
            vd.Points = ptList;
            vd.Tangents = vecList;
            vd.Parameters = paramList;
            vd.WarningMessageTol = tolBool;
            vd.WarningMessageEndPts = endPtsBool;
            
            return vd;
        }
    }
}