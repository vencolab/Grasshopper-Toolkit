using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Toolkit.Libraries;

//TO-DO:
//- Icon
namespace Toolkit
{
    public class DataDictionary : GH_Component
    {
        public DataDictionary() : base("DataDictionary", "DataDict", "Store items as key value pairs and optionally write to JSON", "Toolkit", "Data")
        {
        }

        //return Resources.IconForThisComponent;
        public override Guid ComponentGuid => new Guid("439ca649-e3b1-4571-b397-70ebf28f748f");

        public override GH_Exposure Exposure => GH_Exposure.primary;

        protected override System.Drawing.Bitmap Icon => null;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Toggle", "T", "update dict with new KV pairs", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "R", "Clear GH Dictionary", GH_ParamAccess.item, false);
            pManager[1].Optional = true;
            pManager.AddTextParameter("Key", "K", "Key to set or retrieve as an integer", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "v", "Values to set to key", GH_ParamAccess.tree);
            pManager[3].Optional = true;
            pManager.AddBooleanParameter("Write", "w", "Write all the values stored as JSON", GH_ParamAccess.item, false);
            pManager[4].Optional = true;
            pManager.AddBooleanParameter("Read", "R", "Read JSON file for this session. This will overwrite any existing data in this component", GH_ParamAccess.item, false);
            pManager[5].Optional = true;
            pManager.AddTextParameter("Path", "F", "complete filepath and name for JSON file", GH_ParamAccess.item, "C:/");
            pManager[6].Optional = true;
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Write Status", "s", "True if write was successful", GH_ParamAccess.item);
            pManager.AddGenericParameter("Value", "V", "Values at the supplied key ", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Keys", "K", "List of Keys present", GH_ParamAccess.list);
            pManager.AddGenericParameter("Values", "V", "List of values present", GH_ParamAccess.list);
            pManager.AddTextParameter("JSW", "JSW", "", GH_ParamAccess.item);
            pManager.AddTextParameter("JSR", "JSR", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool toggle = false;
            bool reset = false;
            string k = "";
            bool write = false;
            bool read = false;
            string filepath = null;

            if (!DA.GetData(0, ref toggle)) { return; }
            if (!DA.GetData(1, ref reset)) { return; }
            if (!DA.GetData(2, ref k)) { return; }
            if (!DA.GetDataTree(3, out GH_Structure<IGH_Goo> values)) { return; }
            if (!DA.GetData(4, ref write)) { return; }
            if (!DA.GetData(5, ref read)) { return; }
            if (!DA.GetData(6, ref filepath)) { return; }

            DataConverter.GHStructToDict(k, values).TryGetValue(k, out dynamic val);

            bool writeStatus = false;
            string jsonWritten = null;
            string jsonRead = null;

            if (toggle)
            {
                if (!jsonDictWrite.ContainsKey(k))
                    jsonDictWrite.Add(k, val);
                else
                    jsonDictWrite[k] = val;
            }

            if (reset)
            {
                jsonDictWrite.Clear();
            }
            
            if (write)
            {
                var jso = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                jsonWritten = JsonSerializer.Serialize(jsonDictWrite, jso);
                writeStatus = WriteJson(filepath, jsonWritten);
            }

            if (!read)
            {
                if (jsonDictWrite.ContainsKey(k))
                {
                    GH_Structure<IGH_Goo> writeData = DataConverter.DictToGHStruct(k, jsonDictWrite);
                    DA.SetDataTree(1, writeData);
                    DA.SetDataList(2, jsonDictWrite.Keys.ToList());
                    DA.SetDataList(3, jsonDictWrite.Values.ToList());
                }
                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Given key not found");
            }
            else
            {
                Dictionary<string, dynamic> readJsonDict = ReadJson(filepath, out jsonRead);
                GH_Structure<IGH_Goo> readData = DataConverter.DictToGHStruct(k, readJsonDict);

                DA.SetDataTree(1, readData);
                DA.SetDataList(2, jsonDictRead.Keys.ToList());
                DA.SetDataList(3, jsonDictRead.Values.ToList());
            }
            DA.SetData(0, writeStatus);
            DA.SetData(4, jsonWritten);
            DA.SetData(5, jsonRead);
        }

        private Dictionary<string, dynamic> jsonDictWrite = new Dictionary<string, dynamic>();

        private Dictionary<string, dynamic> jsonDictRead = new Dictionary<string, dynamic>();

        public bool WriteJson(string FilePath, string jsonString)
        {
            string fileExtensionString = FilePath.Substring(Math.Max(0, FilePath.Length - 5)).ToUpper();
            if (fileExtensionString.Equals(".JSON"))
                File.WriteAllText(FilePath, jsonString);
            else
                File.WriteAllText(FilePath + ".JSON", jsonString);

            return true;
        }

        public Dictionary<string, dynamic> ReadJson(string FilePath, out string jsonString)
        {
            string fileExtensionString = FilePath.Substring(Math.Max(0, FilePath.Length - 4)).ToUpper();
            if (fileExtensionString.Equals("JSON") && File.Exists(FilePath))
            {
                jsonString = File.ReadAllText(FilePath);
                var jso = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                return (Dictionary<string, dynamic>)JsonSerializer.Deserialize(jsonString, jsonDictWrite.GetType(), jso);
            }
            else
            {
                jsonString = null;
                return null;
            }
        }
    }
}