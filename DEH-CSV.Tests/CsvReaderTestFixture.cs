// -------------------------------------------------------------------------------------------------
//  <copyright file="CsvReaderTestFixture.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using CDP4Dal;
    using CDP4Dal.DAL;

    using CDP4ServicesDal;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using  STARIONGROUP.DEHCSV.Mapping;

    using File = System.IO.File;

    [TestFixture]
    public class CsvReaderTestFixture
    {
        private readonly Uri uri = new("https://cdp4services-public.cdp4.org");
        private Credentials credentials;
        private CdpServicesDal dal;
        private CDPMessageBus messageBus;
        private Session session;
        private CsvReader csvReader;
        private JsonSerializerOptions options;

        [SetUp]
        public void Setup()
        {
            this.credentials = new Credentials("admin", "pass", this.uri);
            this.dal = new CdpServicesDal();
            this.messageBus = new CDPMessageBus();
            this.session = new Session(this.dal, this.credentials, this.messageBus);
            var loggerFactory = LoggerFactory.Create(x => x.AddConsole());

            this.csvReader = new CsvReader(loggerFactory.CreateLogger<CsvReader>());

            this.options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter<ClassKind>() }
            };
        }

        [TearDown]
        public void Teardown()
        {
            this.messageBus.Dispose();
        }

        [Test]
        public async Task VerifyCsvReaderImplementation()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            var csvPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "import-data.csv");
            var mappingFunctionPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Data", "import-mapping.json");

            var typeMaps = JsonSerializer.Deserialize<IEnumerable<TypeMap>>(await File.ReadAllTextAsync(mappingFunctionPath), this.options);

            var csvStream = File.OpenRead(csvPath);
            await this.session.Open();
            var loftModel = this.session.RetrieveSiteDirectory().Model.Find(x => x.Name == "LOFT")!;
            var iterationSetup = loftModel.IterationSetup.Single(x => x.FrozenOn == null);

            var iteration = new Iteration()
            {
                Iid = iterationSetup.IterationIid,
                IterationSetup = iterationSetup
            };

            var engineeringModel = new EngineeringModel()
            {
                Iid = loftModel.EngineeringModelIid,
                EngineeringModelSetup = loftModel
            };

            engineeringModel.Iteration.Add(iteration);

            var domain = loftModel.Participant.Single(x => x.Person == this.session.ActivePerson).SelectedDomain;
            await this.session.Read(iteration, domain);
            var mappedThings = (await this.csvReader.Read(csvStream, typeMaps.ToList(), this.session)).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(mappedThings, Is.Not.Empty);
                Assert.That(mappedThings, Has.Count.EqualTo(315));
                Assert.That(mappedThings.OfType<ElementDefinition>().ToImmutableList(), Has.Count.EqualTo(35));
                Assert.That(mappedThings.OfType<Parameter>().ToImmutableList(), Has.Count.EqualTo(140));
                Assert.That(mappedThings.OfType<ParameterValueSet>().ToImmutableList(), Has.Count.EqualTo(140));
            });

            var count = 0;

            foreach (var elementDefinition in mappedThings.OfType<ElementDefinition>())
            {
                Assert.Multiple(() =>
                {
                    Assert.That(elementDefinition.ShortName, Is.EqualTo($"shortName{count:0000}"));
                    Assert.That(elementDefinition.Name, Is.EqualTo($"Name {count:0000}"));
                    Assert.That(elementDefinition.Category[0].Name, Is.EqualTo("Subsystem"));
                    Assert.That(elementDefinition.Owner.Name, Is.EqualTo("System Engineering"));
                    Assert.That(elementDefinition.Parameter, Has.Count.EqualTo(4));
                    Assert.That(elementDefinition.Parameter[0].ParameterType.Name, Is.EqualTo("area"));
                    Assert.That(elementDefinition.Parameter[1].ParameterType.Name, Is.EqualTo("mass"));
                    Assert.That(elementDefinition.Parameter[2].ParameterType.Name, Is.EqualTo("dry mass"));
                    Assert.That(elementDefinition.Parameter[3].ParameterType.Name, Is.EqualTo("radius"));
                    Assert.That(elementDefinition.Parameter.Select(x => x.Owner).Distinct(), Is.EquivalentTo(new List<DomainOfExpertise> { elementDefinition.Owner }));
                });

                foreach (var parameter in elementDefinition.Parameter)
                {
                    Assert.That(parameter.ValueSet, Has.Count.EqualTo(1));
                }

                Assert.That(int.Parse(elementDefinition.Parameter[0].ValueSet[0].Manual[0]), Is.EqualTo(count));
                Assert.That(int.Parse(elementDefinition.Parameter[1].ValueSet[0].Manual[0]), Is.EqualTo(-count));
                Assert.That(int.Parse(elementDefinition.Parameter[2].ValueSet[0].Manual[0]), Is.EqualTo(count + 10));
                Assert.That(int.Parse(elementDefinition.Parameter[3].ValueSet[0].Manual[0]), Is.EqualTo(count + 100));

                count++;
            }
        }
    }
}
