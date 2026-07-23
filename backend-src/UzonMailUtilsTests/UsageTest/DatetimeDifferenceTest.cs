using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UzonMail.Utils.Json;

namespace UzonMailUtilsTests.UsageTest
{
    [TestClass()]
    public class DatetimeDifferenceTest
    {
        [TestMethod()]
        public void VerifyDatetimeDifferenceTest()
        {
            var now = DateTime.UtcNow;
            var utc = now.ToUniversalTime();

            Assert.IsTrue(true);
        }
    }
}
