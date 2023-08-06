//  -------------------------------------------------------------------------------------------------
//  <copyright file="DataSourceSelectorTestFixture.cs" company="RHEA System S.A.">
// 
//    Copyright 2023 RHEA System S.A.
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// 
//  </copyright>
//  -------------------------------------------------------------------------------------------------

namespace RHEAGROUP.DEHCSV.Tests.Services
{
    using System;
    using System.IO;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using RHEAGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceSelector"/> class
    /// </summary>
    [TestFixture]
    public class DataSourceSelectorTestFixture
    {
        private DataSourceSelector dataSourceSelector;

        private ILoggerFactory loggerFactory;

        [SetUp]
        public void SetUp()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            this.dataSourceSelector = new DataSourceSelector();
        }

        [Test]
        public void Verify_that_when_a_file_uri_is_provided_a_file_dal_is_returned()
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory,"Data", "Xipe.zip");

            var uri = new Uri(path);

            var dal = this.dataSourceSelector.Select(uri);

            Assert.That(dal, Is.InstanceOf(typeof(CDP4JsonFileDal.JsonFileDal)));
        }

        [Test]
        public void Verify_that_when_a_http_uri_is_provided_a_file_dal_is_returned()
        {
            var httpUri = new Uri("http://cdp4services-public.cdp4.org");
            var httpsUri = new Uri("https://cdp4services-public.cdp4.org");

            var dal = this.dataSourceSelector.Select(httpUri);

            Assert.That(dal, Is.InstanceOf(typeof(CDP4ServicesDal.CdpServicesDal)));

            dal = this.dataSourceSelector.Select(httpsUri);

            Assert.That(dal, Is.InstanceOf(typeof(CDP4ServicesDal.CdpServicesDal)));
        }

        [Test]
        public void verify_that_when_an_unsupported_uri_is_provided_an_excetion_is_thrown()
        {
            var uri = new Uri("atom://www.rheagroup.com");

            Assert.That(() => this.dataSourceSelector.Select(uri), Throws.TypeOf<ArgumentException>()
                .With.Message.EqualTo("The URI scheme is not supported: atom"));
        }
    }
}
