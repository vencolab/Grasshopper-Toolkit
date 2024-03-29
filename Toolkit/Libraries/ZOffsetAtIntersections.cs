﻿using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Linq;
using KDTree;

namespace Hayball.Libraries
{
    class ZOffsetAtIntersection
    {
        public List<Curve> OffsetedCurves { get; set; }

        

        public static ZOffsetAtIntersection OffsetAtIntersection(List<Curve> crvs, double LayerWidth, double LayerHeight, double ResolutionMultiplier)
        {
            List<Curve> _crvs = new List<Curve>() { crvs[0]};
            for (int i = 1; i < crvs.Count; i++)
            {
                CurveIntersections ci = Intersection.CurveCurve(crvs[i], crvs[i-1], 0.01, 0.01);

                crvs[i].DivideByLength(LayerWidth, true, out Point3d[] pts);
                PointCloud pointClouds = new PointCloud(pts);
                foreach (Curve c in _crvs)
                {
                    c.ClosestPoints((IEnumerable<GeometryBase>)pointClouds,out Point3d point3D, out _,out int whichGeo, LayerWidth + 1);
                }
                foreach (Point3d p in pts)
                {

                }
            }
            return null;
        }
    }
}
