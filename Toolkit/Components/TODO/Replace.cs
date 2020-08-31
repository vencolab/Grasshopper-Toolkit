using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;
using Heteroptera;
using Heteroptera.Properties;
using Rhino;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

public class ToolsReplacer : GH_Component
{
	private readonly RhinoDoc _doc = RhinoDoc.get_ActiveDoc();

	private List<Guid> _history = new List<Guid>();

	public override GH_Exposure Exposure => (GH_Exposure)64;

	protected override Bitmap Icon => Resources.replace;

	public override Guid ComponentGuid => new Guid("{078ad4ec-d53f-458f-9ba7-cc39a81b576f}");

	public ToolsReplacer()
		: this("Replacer", "RPLC", "Click on Replace Button to Replace a Rhino-object with another geometry", Resources.HeteroTitle, Resources.ToolSub)
	{
	}

	public override void CreateAttributes()
	{
		base.m_attributes = (IGH_Attributes)(object)new AttButt((GH_Component)(object)this, "Replace");
	}

	protected override void RegisterInputParams(GH_InputParamManager pManager)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		pManager.AddParameter((IGH_Param)(object)new Param_Guid(), "Rhino Geometry", "R", "Reference Rhino-Object to replace", (GH_ParamAccess)1);
		pManager.AddGeometryParameter(Resources.GeoSub, "G", "Basic geometry to replace with (Brep,Surface,Curve,Point)", (GH_ParamAccess)1);
		pManager.AddGenericParameter("Attributes", "A", "", GH_ParamAccessList)
	}

	protected override void RegisterOutputParams(GH_OutputParamManager pManager)
	{
	}

	protected override void SolveInstance(IGH_DataAccess da)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		if ((base.m_attributes as AttButt).Reset)
		{
			_history.ForEach(delegate (Guid i)
			{
				_doc.get_Objects().Select(i);
			});
			return;
		}
		List<Guid> list = new List<Guid>();
		List<IGH_GeometricGoo> list2 = new List<IGH_GeometricGoo>();
		da.GetDataList<Guid>(0, list);
		da.GetDataList<IGH_GeometricGoo>(1, list2);
		if (!ButtonActivate())
		{
			return;
		}
		_history = list.ToList();
		ObjectTable objects = RhinoDoc.get_ActiveDoc().get_Objects();
		int count = list2.Count;
		Point3d val11 = default(Point3d);
		Circle val10 = default(Circle);
		Arc val9 = default(Arc);
		Rectangle3d val8 = default(Rectangle3d);
		Line val7 = default(Line);
		Curve val6 = default(Curve);
		Mesh val5 = default(Mesh);
		Surface val4 = default(Surface);
		Brep val3 = default(Brep);
		for (int j = 0; j < list.Count; j++)
		{
			IGH_GeometricGoo val = list2[j % count];
			Guid guid = list[j];
			ObjRef val2 = (ObjRef)(object)new ObjRef(guid);
			switch (((IGH_Goo)val).get_TypeName())
			{
				case "Plane":
				case "Point":
					((IGH_Goo)val).CastTo<Point3d>(ref val11);
					objects.Replace(guid, val11);
					break;
				case "Circle":
					((IGH_Goo)val).CastTo<Circle>(ref val10);
					objects.Replace(guid, val10);
					break;
				case "Arc":
					((IGH_Goo)val).CastTo<Arc>(ref val9);
					objects.Replace(guid, val9);
					break;
				case "Line":
					((IGH_Goo)val).CastTo<Line>(ref val7);
					objects.Replace(guid, val7);
					break;
				case "Curve":
					((IGH_Goo)val).CastTo<Curve>(ref val6);
					objects.Replace(guid, val6);
					break;
				case "Mesh":
					((IGH_Goo)val).CastTo<Mesh>(ref val5);
					objects.Replace(guid, val5);
					break;
				case "Surface":
					((IGH_Goo)val).CastTo<Surface>(ref val4);
					objects.Replace(guid, val4);
					break;
				case "Twisted Box":
				case "Box":
				case "Brep":
					((IGH_Goo)val).CastTo<Brep>(ref val3);
					objects.Replace(guid, val3);
					break;
			}
			ObjectAttributes attributes = val2.Object().get_Attributes();
			attributes.set_ObjectId(guid);
			objects.ModifyAttributes(_doc.get_Objects().MostRecentObject().get_Id(), attributes, true);
		}
	}

	protected bool ButtonActivate()
	{
		return (base.m_attributes as AttButt)?.Activate ?? false;
	}
}