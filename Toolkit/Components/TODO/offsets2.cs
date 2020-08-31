using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using System.Linq;


/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { __out.Add(text); }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { __out.Add(string.Format(format, args)); }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj)); }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { __out.Add(GH_ScriptComponentUtilities.ReflectType_CS(obj, method_name)); }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private IGH_Component Component; 
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments, 
  /// Output parameters as ref arguments. You don't have to assign output parameters, 
  /// they will have a default value.
  /// </summary>
  private void RunScript(DataTree<Curve> cTree, DataTree<double> oTree, double angleThreshold, ref object crvs, ref object crvs2, ref object pts, ref object perps)
  {
    
    DataTree<Curve> outputCrvs = new DataTree<Curve>();
    DataTree<Point3d> outputPts = new DataTree<Point3d>();
    DataTree<Curve> fillerCrvs = new DataTree<Curve>();


    List<Curve> outCrvs = new List<Curve>();
    for(int i = 0; i < cTree.BranchCount; i++)
    {
      List<Curve> cList = cTree.Branch(i);

      GH_Path p = cTree.Path(i);
      Curve crv = null;
      for(int j = 0; j < cList.Count; j++)
      {
        double offsetValue = oTree.Branch(i)[j];

        //offset & extend
        if (offsetValue != 0)
        {
          Curve[] cArr = cList[j].Offset(Plane.WorldXY, offsetValue, RhinoMath.DefaultDistanceToleranceMillimeters, CurveOffsetCornerStyle.None);
          foreach(Curve c in cArr)
          {
            crv = c;
          }
        }

        else
        {
          crv = cList[j];
        }

        // if for loop is not in the first iteration && tangent of current curve equals tangent of previous curve && curves are disjoint, Create a line perpendidular between the 2 curves)
        if((i > 0))
        {

          Curve prevCrv = outputCrvs.Branch(i - 1)[j];

          double vecAngle = RhinoMath.ToDegrees(Vector3d.VectorAngle(crv.TangentAtStart, prevCrv.TangentAtEnd));

          if(vecAngle < angleThreshold)
          {
            List <Point3d> _pts = new List<Point3d>()
              { prevCrv.PointAtEnd,
                crv.PointAtStart
                };
            outCrvs.Add(Curve.CreateControlPointCurve(_pts));
            fillerCrvs.Add(Curve.CreateControlPointCurve(_pts), p);
          }
          else
          {
            crv = crv.Extend(CurveEnd.Start, crv.GetLength(), CurveExtensionStyle.Line);
          }

        }

        outCrvs.Add(crv);
        outputCrvs.Add(crv, p);
        outputPts.Add(crv.PointAtNormalizedLength(0.5), p);

      }
    }
    outputCrvs.MergeTree(fillerCrvs);

    crvs = outputCrvs;
    crvs2 = outCrvs;
    pts = outputPts;
    perps = fillerCrvs;

  }

  // <Custom additional code> 
  
  // </Custom additional code> 

  private List<string> __err = new List<string>(); //Do not modify this list directly.
  private List<string> __out = new List<string>(); //Do not modify this list directly.
  private RhinoDoc doc = RhinoDoc.ActiveDoc;       //Legacy field.
  private IGH_ActiveObject owner;                  //Legacy field.
  private int runCount;                            //Legacy field.
  
  public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
  {
    //Prepare for a new run...
    //1. Reset lists
    this.__out.Clear();
    this.__err.Clear();

    this.Component = owner;
    this.Iteration = iteration;
    this.GrasshopperDocument = owner.OnPingDocument();
    this.RhinoDocument = rhinoDocument as Rhino.RhinoDoc;

    this.owner = this.Component;
    this.runCount = this.Iteration;
    this. doc = this.RhinoDocument;

    //2. Assign input parameters
        DataTree<Curve> cTree = null;
    if (inputs[0] != null)
    {
      cTree = GH_DirtyCaster.CastToTree<Curve>(inputs[0]);
    }

    DataTree<double> oTree = null;
    if (inputs[1] != null)
    {
      oTree = GH_DirtyCaster.CastToTree<double>(inputs[1]);
    }

    double angleThreshold = default(double);
    if (inputs[2] != null)
    {
      angleThreshold = (double)(inputs[2]);
    }



    //3. Declare output parameters
      object crvs = null;
  object crvs2 = null;
  object pts = null;
  object perps = null;


    //4. Invoke RunScript
    RunScript(cTree, oTree, angleThreshold, ref crvs, ref crvs2, ref pts, ref perps);
      
    try
    {
      //5. Assign output parameters to component...
            if (crvs != null)
      {
        if (GH_Format.TreatAsCollection(crvs))
        {
          IEnumerable __enum_crvs = (IEnumerable)(crvs);
          DA.SetDataList(1, __enum_crvs);
        }
        else
        {
          if (crvs is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(crvs));
          }
          else
          {
            //assign direct
            DA.SetData(1, crvs);
          }
        }
      }
      else
      {
        DA.SetData(1, null);
      }
      if (crvs2 != null)
      {
        if (GH_Format.TreatAsCollection(crvs2))
        {
          IEnumerable __enum_crvs2 = (IEnumerable)(crvs2);
          DA.SetDataList(2, __enum_crvs2);
        }
        else
        {
          if (crvs2 is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(2, (Grasshopper.Kernel.Data.IGH_DataTree)(crvs2));
          }
          else
          {
            //assign direct
            DA.SetData(2, crvs2);
          }
        }
      }
      else
      {
        DA.SetData(2, null);
      }
      if (pts != null)
      {
        if (GH_Format.TreatAsCollection(pts))
        {
          IEnumerable __enum_pts = (IEnumerable)(pts);
          DA.SetDataList(3, __enum_pts);
        }
        else
        {
          if (pts is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(3, (Grasshopper.Kernel.Data.IGH_DataTree)(pts));
          }
          else
          {
            //assign direct
            DA.SetData(3, pts);
          }
        }
      }
      else
      {
        DA.SetData(3, null);
      }
      if (perps != null)
      {
        if (GH_Format.TreatAsCollection(perps))
        {
          IEnumerable __enum_perps = (IEnumerable)(perps);
          DA.SetDataList(4, __enum_perps);
        }
        else
        {
          if (perps is Grasshopper.Kernel.Data.IGH_DataTree)
          {
            //merge tree
            DA.SetDataTree(4, (Grasshopper.Kernel.Data.IGH_DataTree)(perps));
          }
          else
          {
            //assign direct
            DA.SetData(4, perps);
          }
        }
      }
      else
      {
        DA.SetData(4, null);
      }

    }
    catch (Exception ex)
    {
      this.__err.Add(string.Format("Script exception: {0}", ex.Message));
    }
    finally
    {
      //Add errors and messages... 
      if (owner.Params.Output.Count > 0)
      {
        if (owner.Params.Output[0] is Grasshopper.Kernel.Parameters.Param_String)
        {
          List<string> __errors_plus_messages = new List<string>();
          if (this.__err != null) { __errors_plus_messages.AddRange(this.__err); }
          if (this.__out != null) { __errors_plus_messages.AddRange(this.__out); }
          if (__errors_plus_messages.Count > 0) 
            DA.SetDataList(0, __errors_plus_messages);
        }
      }
    }
  }
}