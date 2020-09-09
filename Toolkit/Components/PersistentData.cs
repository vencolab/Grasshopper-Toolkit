using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml;

namespace Toolkit
{
    public sealed class PersistentData : GH_Component
    {
        public PersistentData()
        : base("Persistent Data", "PData", "Retain the last known data for as long as possible", "Toolkit", "Data")
        { }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle", "T", "Toggle to register key-value pair", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Key", "K", "key to save", GH_ParamAccess.item);
            pManager.AddGenericParameter("Values", "V", "values to retain", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Clear", "C", "Clear All values held in this component", GH_ParamAccess.item, false);
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Retained or passed data", GH_ParamAccess.tree);
            pManager.AddTextParameter("Keys", "K", "Keys stored as a list", GH_ParamAccess.list);
            pManager.AddGenericParameter("Values", "V", "Values stored as a list", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess access)
        {
            // If the input parameter has persistent data, or is connected to sources,
            // then assign the RetainedData from the input.
            // Otherwise use the retained data.
            bool toggle = false;
            string key = "0";
            GH_Structure<IGH_Goo> data;
            bool clear = false;
            GH_Structure<IGH_Goo> data2 = new GH_Structure<IGH_Goo>();

            Param_GenericObject input = Params.Input[2] as Param_GenericObject;

            access.GetData(0, ref toggle);
            access.GetData(1, ref key);
            access.GetData(3, ref clear);

            if (input is null)
                throw new InvalidCastException("Input was supposed to be a Param_GenericObject.");

            if (clear)
                storage.Clear();

            if (input.SourceCount > 0 || !input.PersistentData.IsEmpty)
            {
                access.GetDataTree(2, out data);
                if (toggle)
                {
                    if (storage.ContainsKey(key))
                        storage[key] = data.ShallowDuplicate();
                    else
                        storage.Add(key, data.ShallowDuplicate());
                }
                for (var i = 0; i < storage.Values.Count; i++)
                {
                    data2.AppendRange(storage.Values.ElementAt(i).FlattenData(), new GH_Path(i));
                }
            }

            if (storage.ContainsKey(key))
            {
                data = storage[key];
                access.SetDataTree(0, data);
            }
            else
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The given key is not present.");
 
            access.SetDataList(1, storage.Keys.ToList());
            access.SetDataTree(2, data2);
        }

        private Dictionary<string, GH_Structure<IGH_Goo>> storage = new Dictionary<string, GH_Structure<IGH_Goo>>();

        public override Guid ComponentGuid => new Guid("{20269257-D06D-4C0B-92FB-4704329A1112}");

        protected override System.Drawing.Bitmap Icon => null;

        public override bool Write(GH_IWriter writer)
        {
            int n = 0;
            foreach (var pair in storage)
            {
                // Create a chunk with a specific name and an increasing index.
                var chunk = writer.CreateChunk("StoredData", n++);
                // Store the dictionary name as a string on the chunk.
                chunk.SetString("Key", pair.Key);

                // Store the dictionary value in a sub-chunk.
                var treeChunk = chunk.CreateChunk("Tree");

                pair.Value.Write(treeChunk);
            }

            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if(storage.Count > 0)
                storage.Clear();
            // Try and read as many "StoredData" chunks as possible.
            // This approach demands that all these chunks are stored under increasing indices.
            for (int i = 0; i < int.MaxValue; i++)
            {
                var chunk = reader.FindChunk("StoredData", i);
                // Stop looking once we've run out.
                if (chunk is null) break;

                var key = chunk.GetString("Key");
                var treeChunk = chunk.FindChunk("Tree"); 
                var tree = new GH_Structure<IGH_Goo>();
                tree.Read(treeChunk);

                storage.Add(key, tree);
            }
            return base.Read(reader);
        }

    }
}