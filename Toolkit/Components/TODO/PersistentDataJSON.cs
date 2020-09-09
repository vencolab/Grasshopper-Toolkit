using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
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
            pManager.AddBooleanParameter("WriteJson", "W", "Write to Json", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("ReadJson", "R", "Read Json file", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Filepath", "F", "Json File path", GH_ParamAccess.item);
            pManager[2].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Retained or passed data", GH_ParamAccess.tree);
            pManager.AddTextParameter("jsonWrite", "j", "", GH_ParamAccess.item);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{20269257-D06D-4C0B-92FB-4704329A1112}"); }
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
            bool write = false;
            bool read = false;
            string filePath = "";
            var input = Params.Input[2] as Param_GenericObject;

            access.GetData(0, ref toggle);
            access.GetData(1, ref key);
            access.GetData(3, ref clear);
            access.GetData(4, ref write);
            access.GetData(5, ref read);
            access.GetData(6, ref filePath);


            
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
            }

            //json write
            if (write)
            {
                this.JsonStringWrite = JsonSerializer.Serialize<Dictionary<string, GH_Structure<IGH_Goo>>>(storage, this.jso);
                this.WriteJson(filePath);
            }
            //json read
            IsRead = read;
            if (read && File.Exists(filePath))
                this.ReadJson(filePath);

            if (storage.ContainsKey(key))
            {
                data = storage[key];
                access.SetDataTree(0, data);
            }
            else
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "The given key is not present.");

            access.SetData(1, this.JsonStringWrite);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

#pragma warning disable CS0628 // New protected member declared in sealed class
        protected Dictionary<string, GH_Structure<IGH_Goo>> storage = new Dictionary<string, GH_Structure<IGH_Goo>>();
#pragma warning restore CS0628 // New protected member declared in sealed class

        private string JsonStringWrite { get; set; }
        private string JsonStringRead { get; set; }

        //private GH_IWriter GHWriter { get; set; }
        private bool IsRead { get; set; }
        //private bool IsWrite { get; set; }
        //private bool WriteSuccess { get; set; }

        private readonly JsonSerializerOptions jso = new JsonSerializerOptions()
        {
            WriteIndented = true
        };

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

                //json
                //this.JsonStringWrite = JsonSerializer.Serialize(IsRead, this.jso);
                
                pair.Value.Write(treeChunk);
            }
            //WriteSuccess = base.Write(writer);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            storage.Clear();

            // Try and read as many "StoredData" chunks as possible.
            // This approach demands that all these chunks are stored under increasing indices.
            for (int i = 0; i < int.MaxValue; i++)
            {
                var chunk = reader.FindChunk("StoredData", i);
                chunk.
                if (IsRead)
                {
                    try
                    {
                        chunk = JsonSerializer.Deserialize<GH_IReader>(JsonStringRead, this.jso);
                    }
                    catch
                    {
                        chunk = reader.FindChunk("StoredData", i);
                    } 
                }
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

        public void WriteJson(string FilePath)
        {
            string fileExtensionString = FilePath.Substring(Math.Max(0, FilePath.Length - 5)).ToUpper();
            if (fileExtensionString.Equals(".JSON"))
                File.WriteAllText(FilePath, this.JsonStringWrite);
            else
                File.WriteAllText(FilePath + ".JSON", this.JsonStringWrite);
        }

        public void ReadJson(string FilePath)
        {
            string fileExtensionString = FilePath.Substring(Math.Max(0, FilePath.Length - 4)).ToUpper();
            if (fileExtensionString.Equals("JSON") && File.Exists(FilePath))
                this.JsonStringRead = File.ReadAllText(FilePath);
            else
                this.JsonStringRead = null;
        }
    }
}