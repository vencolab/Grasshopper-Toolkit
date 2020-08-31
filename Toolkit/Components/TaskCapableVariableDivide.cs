using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Toolkit
{
    public class TaskCapableVariableDivide : GH_TaskCapableComponent<VariableDivide>, IGH_VariableParameterComponent
    {
        // initialise new instance  of variableDivide Class
        public TaskCapableVariableDivide() : base("Divide Variable Distance", "VarDist", "Divide curves using a list of distances between points", "Toolkit", "Curve")
        {
        }

        //register all the input parameters
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Crve", "C", "Curves to Divide", GH_ParamAccess.item);
            pManager.AddNumberParameter("Distance", "D", "List of Distances", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Pattern", "P", "Pattern index if using optional distances. ", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Repeat Type", "R", "0 = Wrap, 1 = Repeat Last, 2 = No Repeat. Default is 0", GH_ParamAccess.item, 0);
            pManager.HideParameter(0);
        }

        //register all output parameters
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Curve", "C", "Curves with updated domains", GH_ParamAccess.list);
            pManager.AddPointParameter("Points", "P", "Division Points", GH_ParamAccess.list);
            pManager.AddVectorParameter("Tangents", "T", "Tangents at divisions", GH_ParamAccess.list);
            pManager.AddNumberParameter("Parameters", "t", "Division Parameters", GH_ParamAccess.list);
            pManager.HideParameter(0);
        }

        // method that does work
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (InPreSolve)
            {
                //inputs
                Curve crv = null;
                List<double> distances = new List<double>();
                int rep = 0;
                Dictionary<int, List<double>> dictDivisions = new Dictionary<int, List<double>>();

                Task<VariableDivide> tsk = null;

                for (int i = 3; i < Params.Input.Count; i++)
                {
                    List<double> _divisions = new List<double>();
                    if (!DA.GetDataList(i, _divisions)) { return; }
                    dictDivisions.Add(i, _divisions);
                }

                if (DA.GetData(0, ref crv) && DA.GetDataList(1, distances) && DA.GetData(2, ref rep))
                {
                    tsk = Task.Run(() => VariableDivide.Divide(crv, distances, rep, dictDivisions), CancelToken);
                }
                TaskList.Add(tsk);
                return;
            }
            if (!GetSolveResults(DA, out VariableDivide results))
            {
                // 1. Collect
                Curve crv = null;
                List<double> divisions = new List<double>();
                int rep = 0;
                Dictionary<int, List<double>> dictDivisions = new Dictionary<int, List<double>>();

                if (!DA.GetData(0, ref crv)) { return; }
                if (!DA.GetDataList(1, divisions)) { return; }
                if (!DA.GetData(2, ref rep)) { return; }
                for (int i = 3; i < Params.Input.Count; i++)
                {
                    List<double> _divisions = new List<double>();
                    if (!DA.GetDataList(i, _divisions)) { return; }
                    dictDivisions.Add(i, _divisions);
                }


                // 2. Compute
                results = VariableDivide.Divide(crv, divisions, rep, dictDivisions);
            }
            // 3. Set
            if (results != null)
            {
                if (results.WarningMessageTol)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " Distances less than or equal to your rhino model tolerance will be ignored");
                }
                if (results.WarningMessageEndPts)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, " Distance between start and end points of curve(s) may be too small." +
                    " If it yields undesirable results, consider increasing distance between them to the max value in the supplied distances.");
                }

                if (results.Parameters != null)
                {
                    DA.SetDataList(0, results.Curves);
                    DA.SetDataList(1, results.Points);
                    DA.SetDataList(2, results.Tangents);
                    DA.SetDataList(3, results.Parameters);
                }
            }
        }

        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index)
        {
            if (side == GH_ParameterSide.Input && Params.Input.Count > 3)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            Param_Number param = new Param_Number();

            param.Name = param.NickName = GH_ComponentParamServer.InventUniqueNickname("list " + index.ToString(), Params.Input);
            param.Description = "Additional Parameters No." + (Params.Input.Count + 1).ToString();
            param.SetPersistentData(0.0);

            return param;
        }

        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            return true;
        }

        public void VariableParameterMaintenance()
        {
            for (int i = 0; i < Params.Input.Count; i++)
            {
                if (string.IsNullOrEmpty(Params.Input[i].NickName))
                {
                    Params.Input[i].NickName = GH_ComponentParamServer.InventUniqueNickname("params", Params.Input);
                }
            }
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => Properties.Resources.DivideDistance;

        public override Guid ComponentGuid => new Guid("0C85412A-A5CF-4767-941B-A84C19F57AF1");
    }
}