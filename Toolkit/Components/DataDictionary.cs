using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

//TO-DO:
//- Icon
namespace Toolkit
{
    public class DataDictionary : GH_Component
    {
        public DataDictionary() : base("Custom Series", "CusSeries", "Generate a list of numbers between 2 extremes based on increment value", "Hayball", "Math")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle", "T", "Minium Value", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "R", "Maximum Value", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Key", "k", "Key to set or retrieve", GH_ParamAccess.item);
            pManager.AddNumberParameter("Value", "v", "Values to set to key", GH_ParamAccess.tree);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "Values at the supplied key ", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            boolean toggle = false;
            boolean reset = false;
            var key = null;
            DataTree<object> values = new DataTree<object>();


            if (!DA.GetData(0, ref toggle)) { return; }
            if (!DA.GetData(1, ref reset)) { return; }
            if (!DA.GetData(2, ref key)) { return; }
            if (!DA.GetData(2, ref values)) { return; }

            if (toggle && key!= null)
            {
            if(PersistentData.ContainsKey(key))
                PersistentData[key] = data;

            else
                PersistentData.Add(key, data);
            }

            if (reset)
            {
            PersistentData.Clear();
            PersistentData.Add(0, null);
            }

            if(PersistentData.ContainsKey(key))

             //   AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The increment value is too high");
  
            //List<double> combinations = Compute(min, max, incr);
            DA.SetData(0, PersistentData[key]);
        }

          Dictionary<int, object> PersistentData = new Dictionary<int, object>();


        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("439ca649-e3b1-4571-b397-70ebf28f748f");
    }
}