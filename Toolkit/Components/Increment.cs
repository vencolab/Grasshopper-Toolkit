using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

//TO-DO:
//- Icon
namespace Toolkit
{
    public class Increment : GH_Component
    {
        public Increment() : base("Custom Series", "CSeries", "Generate a list of numbers between 2 extremes based on increment value", "Toolkit", "Math")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Min", "Min", "Minium Value", GH_ParamAccess.item, 2);
            pManager.AddNumberParameter("Max", "Max", "Maximum Value", GH_ParamAccess.item, 8);
            pManager.AddNumberParameter("Increment", "I", "Incremeent value", GH_ParamAccess.item, 0.2);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Numbers", "N", "Combinations", GH_ParamAccess.list);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double min = 0.0, max = 0.0, incr = 0.0;

            if (!DA.GetData(0, ref min)) { return; }
            if (!DA.GetData(1, ref max)) { return; }
            if (!DA.GetData(2, ref incr)) { return; }
            if (max < min)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Maximum Value should to be larger than Minium Value");
            }
            if (max - min <= incr)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The increment value is too high");
            }

            List<double> combinations = Compute(min, max, incr);

            DA.SetData(0, combinations);
        }

        public List<double> Compute(double min, double max, double incr)
        {
            List<double> numbers = new List<double>
            {
                min
            };

            while (max - min > incr && incr > 0)
            {
                min += incr;
                numbers.Add(min);
            }
            numbers.Add(max);

            return numbers;
        }

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("E9DC2366-078E-4E93-9495-902F92B5A9E8");
    }
}