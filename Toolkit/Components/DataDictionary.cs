using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;

//TO-DO:
//- Icon
namespace Toolkit
{
    public class DataDictionary : GH_Component
    {
        public DataDictionary() : base("DataDictionary", "DataDict", "Store items as persistent data as key value pairs", "Toolkit", "Math")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle", "T", "Minium Value", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "R", "Maximum Value", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Index", "i", "Key to set or retrieve as an integer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "v", "Values to set to key", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Value", "V", "Values at the supplied key ", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool toggle = false;
            bool reset = false;
            int k = 0;
            object values = new object();


            if (!DA.GetData(0, ref toggle)) { return; }
            if (!DA.GetData(1, ref reset)) { return; }
            if (!DA.GetData(2, ref k)) { return; }
            if (!DA.GetData(3, ref values)) { return; }

            if (toggle)
            {
                if (PersistentData.ContainsKey(k))
                    PersistentData[k] = values;

                else
                    PersistentData.Add(k, values);
            }

            if (reset)
            {
                PersistentData.Clear();
                //PersistentData.Add(0, null);
            }

            if (PersistentData.ContainsKey(k))
                DA.SetData(0, PersistentData[k]);

            else
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Given key not found");
        }

        Dictionary<int, object> PersistentData = new Dictionary<int, object>();


        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("439ca649-e3b1-4571-b397-70ebf28f748f");
    }
}