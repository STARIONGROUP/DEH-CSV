// -------------------------------------------------------------------------------------------------
//  <copyright file="ThingTimeStampPropertyEvaluatorTestFixture.cs" company="Starion Group S.A.">
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
    using System.Collections.Concurrent;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using CDP4Common.CommonData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;
    using CDP4Dal;
    using CDP4Dal.DAL;
    using Newtonsoft.Json.Bson;
    using NUnit.Framework;

    using STARIONGROUP.DEHCSV.CustomProperties;
    using STARIONGROUP.DEHCSV.Mapping;
    using STARIONGROUP.DEHCSV.Services;

    /// <summary>
    /// Suite of tests for the <see cref="ThingTimeStampPropertyEvaluator"/> class
    /// </summary>
    [TestFixture]
    public class ThingTimeStampPropertyEvaluatorTestFixture
    {
        private ThingTimeStampPropertyEvaluator thingTimeStampPropertyEvaluator;

        [SetUp]
        public void Setup()
        {
            this.thingTimeStampPropertyEvaluator = new ThingTimeStampPropertyEvaluator();
        }

        [Test]
        public void Verify_that_when_Thing_is_null_exception_is_thrown()
        {
            Assert.That(() => this.thingTimeStampPropertyEvaluator.QueryValue(null, new PropertyMap(), null),
                Throws.ArgumentNullException); ;
        }

        [Test]
        public void Verify_that_when_value_is_queried_expected_result_is_returned()
        {
            var domainOfExpertise = new DomainOfExpertise(Guid.NewGuid(), new ConcurrentDictionary<CacheKey, Lazy<Thing>>(),
                new Uri("http://stariongroup.eu"));

            var result = this.thingTimeStampPropertyEvaluator.QueryValue(domainOfExpertise, new PropertyMap(), null);

            Assert.That(result, Does.StartWith("DomainOfExpertise:"));
        }
    }
}