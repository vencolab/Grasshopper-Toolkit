using Grasshopper.Kernel;
using Grasshopper;
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

namespace Hayball
{
    public class Feaso : GH_Component
    {
        public Feaso() : base("Curve Offset (custom)", "CO", "Offset", "Toolkit", "Curve")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Base Curves Single Curves", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Heights", "H", "Heights, 1 branch per curve(side)", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Offsets", "O", "Offsets, 1 branch per curve(side)", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("Outlines", "Out", "Porjected Outlines", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DataTree<Curve> crvTree = new DataTree<Curve>();
            DataTree<double> hTree = new DataTree<double>();
            DataTree<double> oTree = new DataTree<double>();

            if (!DA.GetData(0, ref crvTree)) { return; }
            if (!DA.GetData(1, ref hTree)) { return; }
            if (!DA.GetData(2, ref oTree)) { return; }

            Brep loft = Compute(crvTree, hTree, oTree);

            DA.SetData(0, loft);
        }

        public Brep Compute(DataTree<Curve> curves, DataTree<double> hTree, DataTree<double> oTree)
        {
            Brep b = new Brep();

            double tol = Rhino.RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;);


            for (int i = 0; i < curves.BranchCount; i++)
            {
                List<double>
                for (int j = 0; j < oTree.Branch(i).Count; j++)
                {
                    Curve _oCrv = curves[i].Offset(Plane.WorldXY, oTree.Branch(i)[j], tol, CurveOffsetCornerStyle.None);

                }
            }






            return b;

        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("");
    }
}