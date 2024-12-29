using Microsoft.VisualStudio.TestTools.UnitTesting;
using UZonMail.DB.SqLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZonMail.DB.SqLite.Tests
{
    [TestClass()]
    public class SqLiteContextFactoryTests
    {
        [TestMethod()]
        public void CreateDbContextTest()
        {
            var factory = new SqLiteContextFactory();
            var context = factory.CreateDbContext([]);
            Assert.AreNotEqual(null, context);
        }
    }
}