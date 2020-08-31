using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;


namespace Yeeboii
{
    public class TextureExtraction : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public TextureExtraction()
          : base("Texture Extract", "TS",
              "Extract Textures",
              "yeeeboiii", "Render")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Trigger", "T", "Enabled or Disabled", GH_ParamAccess.item);
            pManager.AddGenericParameter("geo", "g", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Name", "N", "Texture Name", GH_ParamAccess.item);
            pManager.AddVectorParameter("OffsetU", "ofU", "U value of Offset", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool b = false;
            Rhino.DocObjects.RhinoObject myGeo = null;

            ///Rhino.DocObjects.RhinoObject obj = x as Rhino.DocObjects.RhinoObject;
            ///A = x;


            if (!DA.GetData(0, ref b)) return;
            if (!DA.GetData(1, ref myGeo)) return;
            

            string texName = TextureName(b, myGeo);
            Vector3d OffU = CreateUOffset(b, myGeo);

            DA.SetData(0, texName);
            DA.SetData(1, OffU);
        }

        private string TextureName(bool b, Rhino.DocObjects.RhinoObject myGeo)
        {
            //string tex = "No Texture";

            if ((b) && Rhino.RhinoDoc.ActiveDoc.RenderTextures.Count > 0)
            {
                Rhino.Render.RenderMaterial myGeoRendMat = myGeo.GetRenderMaterial(true);
                Rhino.Render.TextureMapping texMapping = myGeo.GetTextureMapping(1);
                Rhino.Render.Fields.Field fields = myGeoRendMat.Fields.GetField(myGeoRendMat.Name);
                string tex = fields.Name;



                //Rhino.Render.RenderTexture renderTexture = myGeoRendMat.TextureChildSlotName

                return tex;

            }

            else
            {
                return "No Texture";
            }
        }

        //public void FindTextures(bool find, Rhino.RhinoDoc actDoc)
        //{

        //    if (find)
        //    {

        //        //for(int i=0; i < actDoc.RenderTextures.Count; i++)
        //        //{
        //        //    GetInfo(actDoc.RenderTextures[i]);
        //        //}
        //    }

        //}

        //public List<Vector3d> GetInfo(Rhino.Render.RenderTexture renderTex)
        //{

        //}

        

       

        private Vector3d CreateUOffset(bool b, Rhino.DocObjects.RhinoObject myGeo)
        {
            if (b && Rhino.RhinoDoc.ActiveDoc.RenderTextures.Count > 0)
            {
                Vector3d tex = new Vector3d(Rhino.RhinoDoc.ActiveDoc.RenderTextures[0].GetOffset());
               

                return tex;

            }

            else
            {
                //Vector3d vec2 = new Vector3d();
               
                return Vector3d.Unset;
            }
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

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("a1846432-5129-40e7-8c00-7f30e3a857d6"); }
        }
    }
}