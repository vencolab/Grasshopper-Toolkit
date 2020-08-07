using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

//TO-DO:
//- Icon
// 
/*pseudocode

 get geometry outline
    project geometry outline to projection geometry 
        if projection is not closed
            get projection geometry outline 
            trim projection geometry outline using projection 
            join all curves
    split projection geometry with new closed curve

    get surface area of remaining geometry 
    output geometry and area
*/

namespace Toolkit
{
    public class Shadow : GH_Component
    {
        public Shadow() : base("ShadowOutline", "SO", "Project mesh outlines on another mesh in a given direction", "Hayball", "Mesh")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Mesh", "M", "Meshes whose outline to project", GH_ParamAccess.item);
            pManager.AddVectorParameter("Direction", "D", "Vector Direction to project", GH_ParamAccess.item);
            pManager.AddMeshParameter("PMesh", "PM", "Meshes to project on", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Outlines", "Out", "Porjected Outlines", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            Vector3d vec = new Vector3d();
            List<Mesh> pMesh = new List<Mesh>();

            if (!DA.GetData(0, ref mesh)) { return; }
            if (!DA.GetData(1, ref vec)) { return; }
            if (!DA.GetData(2, ref pMesh)) { return; }

            List<Curve> shadows = Compute(mesh, vec, pMesh);

            DA.SetData(0, shadows);
        }

        public List<Curve> Compute(Mesh msh, Vector3d vec, List<Mesh> pMesh)
        {
            Plane pln = new Plane(new Point3d(0, 0, 0), vec);
            List<Curve> crvList = new List<Curve>();

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;


            List<PolyCurve> ppc = new List<PolyCurve>();
            List<Curve> pc = new List<Curve>();

            Polyline[] plArr = msh.GetOutlines(pln);
            
            List<PolylineCurve> _ppcs = new List<PolylineCurve>();
            Parallel.For(0, plArr.Length,
            i => {
                _ppcs.Add(plArr[i].ToPolylineCurve());
            });
            Curve[] _ppc = Curve.JoinCurves(_ppcs);
            foreach (PolyCurve c in _ppc)
            {
                ppc.Add(c);
            }

            // for each mesh in projection Mesh , Get The outline as polyline. convert to curve and join 

            

            Parallel.ForEach(pMesh,
                pM => {
                    Polyline[] _pPl = pM.GetOutlines(pln);
                    List<PolylineCurve> _pcs = new List<PolylineCurve>();
                    Parallel.For(0, _pPl.Length,
                    i => {
                        _pcs.Add(_pPl[i].ToPolylineCurve());
                    });
                    Curve[] _pc = Curve.JoinCurves(_pcs);
                    foreach (Curve c in _pc)
                    {
                        pc.Add(c);
                    }
              });

            Parallel.For(0, plArr.Length,
               i =>
               {
                   Curve[] _crv = Curve.ProjectToMesh(ppc[i], pMesh, vec, tol);

                   Parallel.ForEach(_crv,
                       c =>
                       {
                           if (c.IsClosed)
                           {
                               crvList.Add(c);
                           }
                           else
                           {
                               foreach (Curve curve in _ppcs)
                               {
                                   CurveIntersections inter = Intersection.CurveCurve(c, curve, tol, tol);
                               }
 
                           }
                       });
               });

            return crvList;

        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("C53F6D97-7F54-4CD7-969C-C91DD6C16CF8");
    }
}