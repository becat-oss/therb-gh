using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Utils;
//using Grasshopper.Kernel;
//using Rhino.Geometry;
//using Rhino.Geometry.Collections;
//using Rhino.Geometry.Intersect;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using THERBgh;
using Model;
using System.Net;

namespace THREB_GH_Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void UtilsTest()
        {
            Assert.AreEqual(" 3", Converter.FillEmpty(3, 2));

        }
        [TestMethod]
        public void ExBoxTest()
        {
            var test = new Newtonsoft.Json.Converters.BinaryConverter();
            //var rhino = new Rhino.Geometry.Box();
            //ThrebUtilsTest.ExBoxTest();
            Assert.AreEqual(0, 0);
        }
        [TestMethod]
        public void GetDataFromWebTest()
        {
            var wc = new WebClient();
            string text = wc.DownloadString("https://stingray-app-vgak2.ondigitalocean.app/constructions");

            #region test
            /*
            string text = @"{
  ""data"": [
    {
      ""category"": ""exteriorWall"", 
      ""description"": ""from therb"", 
      ""id"": ""1"", 
      ""materials"": [
        {
          ""classification"": 1, 
          ""conductivity"": 1.2, 
          ""density"": 2200.0, 
          ""description"": ""from Therb"", 
          ""id"": ""2"", 
          ""name"": ""concrete"", 
          ""specificHeat"": 840.0
        }
      ], 
      ""name"": ""RC exterior wall"", 
      ""tags"": [
        {
          ""id"": ""1"", 
          ""name"": ""RC\u9020""
        }
      ], 
      ""thickness"": [
        120.0
      ]
    }, 
    {
      ""category"": ""interiorFloor"", 
      ""description"": ""from therb"", 
      ""id"": ""3"", 
      ""materials"": [
        {
          ""classification"": 1, 
          ""conductivity"": 0.111, 
          ""density"": 550.0, 
          ""description"": ""from Therb"", 
          ""id"": ""3"", 
          ""name"": ""plywood"", 
          ""specificHeat"": 1880.0
        }, 
        {
          ""classification"": 1, 
          ""conductivity"": 0.037, 
          ""density"": 32.0, 
          ""description"": ""from Therb"", 
          ""id"": ""4"", 
          ""name"": ""polyurethane"", 
          ""specificHeat"": 840.0
        }
      ], 
      ""name"": ""interior floor"", 
      ""tags"": [], 
      ""thickness"": [
        15.0, 
        45.0
      ]
    }
  ], 
  ""message"": ""could retrieve constructions"", 
  ""status"": ""success""
}";
            */

            #endregion

            var source = JsonConvert.DeserializeObject<ResConstruction>(text);
        }

    }
}
