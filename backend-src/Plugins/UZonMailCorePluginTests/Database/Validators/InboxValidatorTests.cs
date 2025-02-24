using Microsoft.VisualStudio.TestTools.UnitTesting;
using UZonMail.Core.Database.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UZonMail.DB.SQL.Emails;
using System.Data;

namespace UZonMail.Core.Database.Validators.Tests
{
    [TestClass()]
    public class InboxValidatorTests
    {
        [DataRow("eschuitsza@vet-med.fu-berin.de")]
        [DataRow("eschuitsza@vetmed.fu-berin.de")]
        [DataRow("vasudaia@cc.miyazaki-u.ac.jp")]
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