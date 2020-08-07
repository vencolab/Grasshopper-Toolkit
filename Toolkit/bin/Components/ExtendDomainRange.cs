using Grasshopper.Kernel;
using Rhino.Geometry;
using System;


// This file is for use as a reference

//TO-DO:
//- Icon





// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace Hayball
{
    public class ExtendDomainRange : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public constructor without any arguments.
        /// Category represents the Tab in which the component will appear,
        /// Subcategory the panel. If you use non-existing tab or panel names,
        /// new tabs/panels will automatically be created.
        /// </summary>
        public ExtendDomainRange() : base("Extend Domain Range", "ExDom", "Extend Domain Range by Value", "Hayball", "Math")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntervalParameter("Domain", "D", "Domain to Extend", GH_ParamAccess.item, new Interval(0, 1));
            pManager.AddNumberParameter("Domain Start Extension", "Ex S", "Extension Value for Domain Start", GH_ParamAccess.item, -0.1);
            pManager.AddNumberParameter("Domain End Extension", "Ex E", "Extension Value for Domain End", GH_ParamAccess.item, 0.1);
            // If you want to change properties of certain parameters,
            // you can use the pManager instance to access them by index:
            //pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddIntervalParameter("Extended Domain", "D", "Extended Domain", GH_ParamAccess.item);

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
            Interval domain = new Interval(0, 1);
            double t0 = 0;
            double t1 = 0;

            // Then we need to access the input parameters individually.
            // When data cannot be extracted from a parameter, we should abort this method.
            if (!DA.GetData(0, ref domain)) { return; }
            if (!DA.GetData(1, ref t0)) { return; }
            if (!DA.GetData(2, ref t1)) { return; }

            Interval newDom = new Interval(domain.T0 + t0, domain.T1 + t1);

            // Finally assign to the output parameter.
            DA.SetData(0, newDom);
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
        public override Guid ComponentGuid => new Guid("DF3841D4-EAF4-4723-B71A-11B88CEF7470");
    }
}