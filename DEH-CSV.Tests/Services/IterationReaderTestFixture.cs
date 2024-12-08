﻿//  -------------------------------------------------------------------------------------------------
//  <copyright file="IterationReaderTestFixture.cs" company="Starion Group S.A.">
// 
//    Copyright 2023-2024 Starion Group S.A.
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

namespace STARIONGROUP.DEHCSV.Tests.Services
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using  STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="IterationReader"/> class
    /// </summary>
    [TestFixture]
    public class IterationReaderTestFixture
    {
        private ILoggerFactory loggerFactory;

        private IterationReader iterationReader;
        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
            
            this.iterationReader = new IterationReader(this.loggerFactory);

            this.messageBus = new CDPMessageBus();
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.Dispose();
        }

        [Test]
        public void Verify_that_when_ReadAsync_is_called_with_null_iteration_exception_is_thrown()
        {
            Assert.That(() => this.iterationReader.ReadAsync(null, "DM_SPC", 1, "SYS"),
                Throws.ArgumentNullException);
        }

        [Test]
        public async Task Verify_that_iteration_can_be_read_from_data_source_demo_space()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            var uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials, this.messageBus);

            await session.Open(false);

            var iteration = await this.iterationReader.ReadAsync(session, "DM_SPC", 1, "SYS");

            Assert.That(iteration, Is.Not.Null);
        }

        [Test]
        public async Task Verify_that_iteration_read_throws_expected_exceptions()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            var uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials, this.messageBus);

            await session.Open(false);

            Assert.That(async () => await this.iterationReader.ReadAsync(session, "XXX", 1, "SYS"),
                Throws.Exception.With.Message.EqualTo("The EngineeringModelSetup with shortName XXX could not be found"));

            Assert.That(async () => await this.iterationReader.ReadAsync(session, "DM_SPC", 12, "SYS"),
                Throws.Exception.With.Message.EqualTo("The IterationSetup with number 12 could not be found"));

            Assert.That(async () => await this.iterationReader.ReadAsync(session, "DM_SPC", 1, "SYE"),
                Throws.Exception.With.Message.EqualTo("The DomainOfExpertise with shortName SYE could not be found"));
        }
    }
}
