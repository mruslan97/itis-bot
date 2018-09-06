using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Integration.Tests
{
    [TestFixture]
    public class TgBotShould : HostIntegrationFixture
    {

        [OneTimeSetUp]
        public void AdditonalSetUp()
        {
           
        }

        [Test]
        public void BeRunnable()
        {
            Assert.Pass();
        }

    }
}
