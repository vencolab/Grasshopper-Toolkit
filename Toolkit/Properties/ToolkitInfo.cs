using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Toolkit
{
    public class ToolkitInfo : GH_AssemblyInfo
    {
        public override string Name => "Toolkit";

        public override Bitmap Icon =>
                //Return a 24x24 pixel bitmap to represent this GHA library.
                null;

        public override string Description =>
                //Return a short string describing the purpose of this GHA library.
                "A set of custom components";

        public override Guid Id => new Guid("6f53c23f-259f-4c10-bc58-f1998488436b");

        public override string AuthorName =>
                //Return a string identifying you or your company.
                "Venkatesh Natarjan";

        public override string AuthorContact =>
                //Return a string representing your preferred contact details.
                "venkatesh.natarajan@hotmail.com";
    }
}