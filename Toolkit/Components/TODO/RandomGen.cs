using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hayball
{
    public class RandomGenerator : GH_Component
    {
        public RandomGenerator() : base("Random Generator", "ExDom", "Extend Domain Range by Value", "Hayball", "Math")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Numbers", "N", "Numbers for generating combinations", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Count", "C", "Number of combinations", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Min", "Min", "Minium number of items in a combination", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Max", "Max", "Maximum number of items in a combination", GH_ParamAccess.item, 5);
            pManager.AddIntegerParameter("Seed", "S", "seed value", GH_ParamAccess.item, 2);

            // If you want to change properties of certain parameters,
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("Numbers", "N", "Combinations", GH_ParamAccess.list);

            // Sometimes you want to hide a specific parameter from the Rhino preview.
            // You can use the HideParameter() method as a quick way:
            //pManager.HideParameter(0);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // First, we need to retrieve all data from the input parameters.
            // We'll start by declaring variables and assigning them starting values.
            List<int> combList = new List<int>();
            int min = 0, max = 0, count = 0, seed = 2;

            // Then we need to access the input parameters individually.
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetDataList(0, combList)) { return; }
            if (!DA.GetData(1, ref min)) { return; }
            if (!DA.GetData(2, ref max)) { return; }
            if (!DA.GetData(3, ref count)) { return; }
            if (!DA.GetData(4, ref count)) { return; }
            List<int> combinations = Compute(combList, min, max, count, seed);

            // Finally assign to the output parameter.
            DA.SetData(0, combinations);
        }

        public List<int> Compute(List<int> combList, int min, int max, int count, int seed)
        {
            List<int> combos = new List<int>();

            Random r = new Random(seed);
            int i = 0;
            while (i <= count)
            {
                int size = r.Next(min, max);

                var rand = combList.OrderBy(a => r.Next());

                combos = rand.ToList();

                break;
            }

            return combos;
        }

        /// <summary>
        /// The Exposure property controls where in the panel a component icon
        /// will appear. There are seven possible locations (primary to septenary),
        /// each of which can be combined with the GH_Exposure.obscure flag, which
        /// ensures the component will only be visible on panel dropdowns.
        /// </summary>
        public override GH_Exposure Exposure => GH_Exposure.primary;

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;


        // You can add image files to your project resources and access them like this:
        //return Resources.IconForThisComponent;

        /// <summary>
        /// Each component must have a unique Guid to identify it.
        /// It is vital this Guid doesn't change otherwise old ghx files
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("896D0F63-0311-42E2-B770-376E1C7FE8AB");
    }
}