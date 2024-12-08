﻿//  -------------------------------------------------------------------------------------------------
//  <copyright file="ThingTimeStampedCSVWriterTestFixture.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.Tests.CustomProperties
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using NUnit.Framework;

    using  STARIONGROUP.DEHCSV.CustomProperties;
    using  STARIONGROUP.DEHCSV.Mapping;
    using  STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="ThingTimeStampedCSVWriter"/> class
    /// </summary>
    [TestFixture]
    public class ThingTimeStampedCSVWriterTestFixture
    {
        private ThingTimeStampedCSVWriter thingTimeStampedCsvWriter;
        
        private IterationReader iterationReader;

        private MappingProvider mappingProvider;

        private Uri uri;

        private string mappingPath;

        private CDPMessageBus messageBus;

        [SetUp]
        public void SetUp()
        {
            this.mappingPath = Path.Combine("Data", "thing-time-stamped-mapping.json");
            
            this.mappingProvider = new MappingProvider();

            this.iterationReader = new IterationReader();

            this.thingTimeStampedCsvWriter = new ThingTimeStampedCSVWriter();

            this.messageBus = new CDPMessageBus();
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.Dispose();
        }

        [Test]
        public async Task Verify_that_demosat_model_can_be_written_to_CSV_file()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            this.uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials, this.messageBus);

            await session.Open(false);

            var iteration = await this.iterationReader.ReadAsync(session, "DM_SPC", 1, "SYS");

            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "CSV-Thing-TimeStamped");
            var target = new DirectoryInfo(outputPath);

            var mapping = this.mappingProvider.QueryMappings(this.mappingPath);

            Assert.That(() => this.thingTimeStampedCsvWriter.Write(iteration, true, mapping, target, null), Throws.Nothing);
        }
    }
}