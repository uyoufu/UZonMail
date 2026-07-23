using Microsoft.VisualStudio.TestTools.UnitTesting;
using UzonMail.Utils.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UzonMailUtilsTests.Json
{
    [TestClass()]
    public class JsonExtensionTests
    {
        [TestMethod()]
        public void PrimaryValue_For_NewtonJsonSerialize()
        {
            var value = true;
            var json = value.ToJson();
            Assert.IsTrue(json == "true");
        }
    }
}