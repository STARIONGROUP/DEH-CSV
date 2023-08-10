//  -------------------------------------------------------------------------------------------------
//  <copyright file="CsvWriterTestFixture.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Dal;
    using CDP4Dal.DAL;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using RHEAGROUP.DEHCSV.Mapping;
    using RHEAGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="CsvWriter"/> class
    /// </summary>
    [TestFixture]
    public class CsvWriterTestFixture
    {
        private CsvWriter csvWriter;

        private ILoggerFactory loggerFactory;

        private IterationReader iterationReader;

        private IMappingProvider mappingProvider;

        private Uri uri;

        private string mappingPath;

        [SetUp]
        public void SetUp()
        {
            this.mappingPath = Path.Combine("Data", "mapping.json");

            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));

            this.mappingProvider = new MappingProvider(this.loggerFactory);

            this.iterationReader = new IterationReader(this.loggerFactory);

            this.csvWriter = new CsvWriter(this.loggerFactory);
        }

        [Test]
        public async Task Verify_that_demosat_model_can_be_written_to_CSV_file()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            this.uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials);

            await session.Open(false);

            var iteration = await this.iterationReader.Read(session, "DM_SPC", 1, "SYS");

            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "CSV");
            var target = new DirectoryInfo(outputPath);

            var mapping = this.mappingProvider.QueryMappings(this.mappingPath);

            Assert.That(() => this.csvWriter.Write(session.RetrieveSiteDirectory(), iteration, true,mapping, target, null), Throws.Nothing);
        }

        [Test]
        public async Task Verify_that_when_ValuePrefix_is_set_CSV_File_is_Written_with_prefix()
        {
            var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();

            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "demo-space.zip");

            this.uri = new Uri(path);

            var credentials = new Credentials("admin", "pass", uri);

            var session = new Session(jsonFileDal, credentials);

            await session.Open(false);

            var iteration = await this.iterationReader.Read(session, "DM_SPC", 1, "SYS");
            
            var typeMaps = new List<TypeMap>
            {
                new TypeMap()
                {
                    ClassKind = ClassKind.SiteDirectory,
                    FileName = "SiteDirectory-Test-With-Prefix",
                    Properties = new List<PropertyMap>()
                    {
                        new PropertyMap()
                        {
                            Source = "shortname",
                            Target = "shortname"
                        },
                        new PropertyMap()
                        {
                            Source = "name",
                            Target = "name",
                            ValuePrefix = "prefix-"
                        },
                    }
                }
            };

            var outputPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "CSV");
            var target = new DirectoryInfo(outputPath);

            Assert.That(() => this.csvWriter.Write(session.RetrieveSiteDirectory(), iteration,true, typeMaps, target, null), Throws.Nothing);
        }

        [Test]
        public void Verify_that_QueryFileName_returns_expected_result()
        {
            var mappings = this.mappingProvider.QueryMappings(this.mappingPath);

            var typeMap = mappings.First();

            var result = this.csvWriter.QueryFileName(typeMap);

            Assert.That(result, Is.EqualTo("EngineeringModelSetup-Name-Definition"));
        }

        [Test]
        public void Verify_that_when_FileName_is_provided_QueryFileName_returns_expected_result()
        {
            var typeMap = new TypeMap
            {
                ClassKind = ClassKind.ActionItem,
                FileName = "TEST"
            };
            var result = this.csvWriter.QueryFileName(typeMap);
            Assert.That(result, Is.EqualTo("TEST"));
        }

        [Test]
        public void Verify_that_argument_exceptions_are_thrown_on_write()
        {
            Assert.That(() => this.csvWriter.Write(null, null, null, null), 
                Throws.TypeOf<ArgumentNullException>());

            var things = new List<Thing>();

            Assert.That(() => this.csvWriter.Write(things, null, null, null),
                Throws.TypeOf<ArgumentNullException>());

            var typeMap = new TypeMap();

            Assert.That(() => this.csvWriter.Write(things, typeMap, null, null),
                Throws.TypeOf<ArgumentNullException>());
        }
    }
}
