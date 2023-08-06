//  -------------------------------------------------------------------------------------------------
//  <copyright file="IterationReaderTestFixture.cs" company="RHEA System S.A.">
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
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using RHEAGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="IterationReader"/> class
    /// </summary>
    [TestFixture]
    public class IterationReaderTestFixture
    {
        private ILoggerFactory loggerFactory;

        private IterationReader iterationReader;

        [SetUp]
        public void SetUp()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            var logger = this.loggerFactory.CreateLogger<IterationReader>();

            this.iterationReader = new IterationReader(logger);
        }

        [Test]
        public async Task Verify_that_iteration_can_be_read_from_data_source_demo_space()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            var uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials);

            await session.Open(false);

            var iteration = await this.iterationReader.Read(session, "DM_SPC", 1, "SYS");

            Assert.That(iteration, Is.Not.Null);
        }
    }
}
