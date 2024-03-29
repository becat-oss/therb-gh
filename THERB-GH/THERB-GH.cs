﻿using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace THERBgh
{
    public class THERBghInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "THERB";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("48f5d989-240d-4c03-bc76-a14877caf16a");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
