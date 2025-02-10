using Microsoft.VisualStudio.TestTools.UnitTesting;
using UZonMail.Core.Database.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMail.DB.SQL.Emails;

namespace UZonMail.Core.Database.Validators.Tests
{
    [TestClass()]
    public class InboxValidatorTests
    {
        [DataRow("linzirui@ra.sc.e.titech.ac.jp")]
        [TestMethod()]
        public void InboxValidatorTest(string email)
        {
            var inbox = new Inbox()
            {
                Email = email
            };
            var validator = new InboxValidator();
            var vdResult = validator.Validate(inbox);
            Assert.IsTrue(vdResult.IsValid);
        }
    }
}