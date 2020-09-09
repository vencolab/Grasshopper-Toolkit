using GH_IO.Serialization;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Collections.Generic;

namespace Toolkit.Libraries
{
    public static class DataConverter 
    {
        public static Dictionary<string, dynamic> GHStructToDict(string k, GH_Structure<IGH_Goo> values)
        {
            Dictionary<string, dynamic> jsonDictWrite = new Dictionary<string, dynamic>();
            if (!values.IsEmpty)
            {
                // if multiple items..
                if (values.FlattenData().Count > 1)
                {
                    //if list..
                    if (values.Branches.Count == 1)
                    {
                        dynamic _data = null;
                        List<dynamic> _dataList = new List<dynamic>();

                        values.FlattenData().ForEach(x => x.CastTo(out _data));
                        _dataList.Add(_data);

                        if (jsonDictWrite.ContainsKey(k))
                            jsonDictWrite[k] = _dataList;
                        else
                            jsonDictWrite.Add(k, _dataList);
                    }

                    // if datatree, convert to dictionary , then to List. Assign this new list as a value to jsondict using the input key
                    else
                    {
                        Dictionary<string, dynamic> _dataDict = new Dictionary<string, dynamic>();
                        for (int i = 0; i < values.Branches.Count; i++)
                        {
                            dynamic _data = null;
                            List<dynamic> _dataList = new List<dynamic>();

                            values[i].ForEach(x => x.CastTo(out _data));
                            _dataList.Add(_data);

                            if (_dataDict.ContainsKey(i.ToString()))
                                _dataDict[i.ToString()] = _dataList;
                            else
                                _dataDict.Add(i.ToString(), _dataList);
                            if (jsonDictWrite.ContainsKey(k))
                                jsonDictWrite[k] = _dataDict;
                            else
                                jsonDictWrite.Add(k, _dataDict);
                        }
                    }
                }

                // if single item..
                else
                    if (jsonDictWrite.ContainsKey(k))
                        jsonDictWrite[k] = values.get_FirstItem(true);
                    else
                        jsonDictWrite.Add(k, values.get_FirstItem(true));
            }
            return jsonDictWrite;
        }

        public static GH_Structure<IGH_Goo> DictToGHStruct(string k, Dictionary<string, dynamic> dict)
        {
            GH_Structure<IGH_Goo> ghStructure = new GH_Structure<IGH_Goo>();
            if (!(dict.Count == 0))
            {
                if (dict.TryGetValue(k, out dynamic _dataDict))
                {
                    //if value is a dictionary ..
                    if (_dataDict is Dictionary<string, dynamic>)
                    {
                        foreach (dynamic _key in _dataDict.Keys)
                        {
                            foreach (dynamic _value in _dataDict[_key])
                            {
                                if (int.TryParse(_key, out int _result))
                                {
                                    // if dictionary value is a list
                                    GH_Path p = new GH_Path(_result);
                                    if (_value is List<dynamic>)
                                    {
                                        for (int i = 0; i < _value.Count; i++)
                                        {
                                            IGH_Goo goo = null;
                                            if (goo.CastFrom(_value[i]))
                                                ghStructure.Insert(goo, p, i);
                                        }
                                    }
                                    // if dictionary value is a single item
                                    else
                                    {
                                        IGH_Goo goo = null;
                                        if (goo.CastFrom(_value))
                                            ghStructure.Insert(goo, p, 0);
                                    }
                                }
                            }
                        }
                    }

                    /// if its a list
                    else if (_dataDict is List<dynamic>)
                    {
                        GH_Path p = new GH_Path(int.Parse(k));
                        for (int i = 0; i < _dataDict.Count; i++)
                        {


                            //if (GH_Goo.CastFrom(_dataDict[i]))
                            //    ghStructure.Insert(goo, p, i);
                        }
                    }
                    else
                    {
                        GH_Path p = new GH_Path(int.Parse(k));
                        IGH_Goo goo = null;
                        if (goo.CastFrom(_dataDict))
                            ghStructure.Insert(goo, p, 0);
                    }
                }
            }
            return ghStructure;
        }
    }
}