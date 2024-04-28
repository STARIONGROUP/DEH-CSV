//  -------------------------------------------------------------------------------------------------
//  <copyright file="MappingProviderTestFixture.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.Tests.Mapping
{
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;

    using Microsoft.Extensions.Logging;

    using NUnit.Framework;

    using  STARIONGROUP.DEHCSV.Mapping;
    using  STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="DataSourceSelector"/> class
    /// </summary>
    [TestFixture]
    public class MappingProviderTestFixture 
    {
        private ILoggerFactory loggerFactory;

        [SetUp]
        public void SetUp()
        {
            this.loggerFactory = LoggerFactory.Create(builder =>
                builder.AddConsole().SetMinimumLevel(LogLevel.Trace));
        }

        [Test]
        public void Verify_that_appsettings_file_gets_loaded_and_mappings_are_returned()
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory,"Data", "mapping.json");
            
            var  mappingProvider = new MappingProvider(this.loggerFactory);

            var typeMapping = mappingProvider.QueryMappings(path).First();

            Assert.That(typeMapping.ClassKind, Is.EqualTo(ClassKind.EngineeringModelSetup));
        }
    }
}
