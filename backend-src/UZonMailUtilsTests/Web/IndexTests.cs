using Microsoft.VisualStudio.TestTools.UnitTesting;
using UZonMail.Utils.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMail.Utils.Web.Service;
using UZonMailUtilsTests.Web.TestSource;

namespace UZonMail.Utils.Web.Tests
{
    [TestClass()]
    public class IndexTests
    {

        [DataRow(typeof(TransientServiceTest))]
        [TestMethod()]
        public void GetServiceTypeTest(Type implementationType)
        {
            var serviceType = Index.GetServiceTypes(implementationType);
            Assert.AreEqual(implementationType, serviceType.FirstOrDefault());
        }
    }
}