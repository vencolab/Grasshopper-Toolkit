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
        public static VariableDivide Divide(Curve crv, List<double> distanceList, int rep)
        {
            VariableDivide vd = new VariableDivide();

            if (distanceList.Count == 0)
                return null;

            if (rep > 2 | rep < 0)
            {
                rep = 0;
            }
            //Dictionary<int, List<double>>.ValueCollection values = dictDistances.Values;

            crv.Domain = new Interval(0, crv.GetLength());
            double t1 = crv.Domain.T1;
            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;

            int i, j;
            i = j = -1;

            List<Curve> crvList = new List<Curve>();
            List<Point3d> ptList = new List<Point3d>() { crv.PointAtStart };
            List<Vector3d> vecList = new List<Vector3d>() { crv.TangentAtStart };
            List<double> paramList = new List<double>() { 0.0 };
            bool tolBool = false;
            bool endPtsBool = false;

            //warning message
            if (crv.PointAtStart.DistanceTo(crv.PointAtEnd) < distanceList.Max())
            {
                endPtsBool = true;
            }

            List<Curve> curves = new List<Curve>();
            curves.Sort((x, y) => x.PointAtStart.Z.CompareTo(y.PointAtStart.Z)); // Sort curve based on Height

            //recursion

            while (true)
            {

                //reset i if distance list end reached but curve is still long.
                if (i >= distanceList.Count - 1)
                {
                    switch (rep)
                    {
                        case 0:
                            i = -1;
                            break;

                        case 1:
                            i = distanceList.Count - 2;
                            break;

                        case 2:
                            goto Finish;
                    }
                }
                // increment
                i++;
                j++;

                //intersection properties for
                double currentParam = paramList[j];
                double currentDistance = distanceList[i];
                NurbsSurface nurbsSrf = new Sphere(crv.PointAt(currentParam), currentDistance).ToNurbsSurface();

                if (currentDistance < tol)  // if current distance is smaller than document tolerance, remove distance from computation
                {
                    tolBool = true;
                    distanceList.RemoveAt(i);
                }

                currentDistance = crv.Split(currentParam)[0].GetLength() + (distanceList[i] * 1.1);

                // sphere-curve intersection
                // using  overload with curve domain to limit intersection area- allows to break while-loop when curve reaches the end.
                CurveIntersections events = Intersection.CurveSurface(crv, new Interval(currentParam, currentDistance), nurbsSrf, tol, tol);

                if (events != null && events.Count > 0)
                {
                    if (events.Last().ParameterA > paramList.Last())
                    {
                        ptList.Add(events.Last().PointA);
                        vecList.Add(crv.TangentAt(events.Last().ParameterA));
                        paramList.Add(events.Last().ParameterA);
                    }
                    else { break; }
                }
                else { break; }
            }

        Finish:
            crvList.Add(crv);
            ptList.Add(crv.PointAtEnd);
            vecList.Add(crv.TangentAtEnd);
            paramList.Add(crv.Domain.Max);

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