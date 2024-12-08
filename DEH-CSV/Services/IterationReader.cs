//  -------------------------------------------------------------------------------------------------
//  <copyright file="IterationReader.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.Services
{
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Dal;
    using CDP4Dal.Exceptions;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;
    using CDP4Common.Types;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using System;

    /// <summary>
    /// The purpose of the <see cref="IIterationReader"/> is to read an <see cref="Iteration"/>
    /// using the <see cref="ISession"/> object
    /// </summary>
    public class IterationReader : IIterationReader
    {
        /// <summary>
        /// The (injected) <see cref="ILogger{IterationReader}"/>
        /// </summary>
        private readonly ILogger<IterationReader> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="IterationReader"/>
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to setup logging
        /// </param>
        public IterationReader(ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory == null ? NullLogger<IterationReader>.Instance : loggerFactory.CreateLogger<IterationReader>();
        }

        /// <summary>
        /// ReadAsync the iteration from the 
        /// </summary>
        /// <param name="session">
        /// The <see cref="ISession"/> object used to read the <see cref="Iteration"/> data
        /// </param>
        /// <param name="modelShortName">
        /// The shortName of the <see cref="EngineeringModel"/> that is to be opened
        /// </param>
        /// <param name="iterationNumber">
        /// The number of hte <see cref="Iteration"/> that is to be read
        /// </param>
        /// <param name="domainOfExpertiseShortName">
        /// The shortName of the <see cref="DomainOfExpertise"/> used to open the <see cref="EngineeringModel"/> with
        /// </param>
        /// <returns>
        /// An instance of <see cref="Iteration"/>
        /// </returns>
        public async Task<Iteration> ReadAsync(ISession session, string modelShortName, int iterationNumber, string domainOfExpertiseShortName)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            this.logger.LogDebug("Setting up the read request for {ModelShortName}:{IterationNumber}", modelShortName, iterationNumber);

            var siteDirectory = session.RetrieveSiteDirectory();
            var engineeringModelSetup = siteDirectory.Model.SingleOrDefault(x => x.ShortName == modelShortName);

            if (engineeringModelSetup == null)
            {
                throw new InstanceNotFoundException($"The EngineeringModelSetup with shortName {modelShortName} could not be found");
            }

            var iterationSetup = engineeringModelSetup.IterationSetup.SingleOrDefault(x => x.IterationNumber == iterationNumber);

            if (iterationSetup == null)
            {
                throw new InstanceNotFoundException($"The IterationSetup with number {iterationNumber} could not be found");
            }

            var domainOfExpertise = siteDirectory.Domain.SingleOrDefault(x => x.ShortName == domainOfExpertiseShortName);

            if (domainOfExpertise == null)
            {
                throw new InstanceNotFoundException($"The DomainOfExpertise with shortName {domainOfExpertiseShortName} could not be found");
            }
            
            var engineeringModel = new EngineeringModel(engineeringModelSetup.EngineeringModelIid, session.Assembler.Cache, session.Credentials.Uri);
            engineeringModel.EngineeringModelSetup = engineeringModelSetup;

            var iteration = new Iteration(iterationSetup.IterationIid, session.Assembler.Cache, session.Credentials.Uri);
            iteration.IterationSetup = iterationSetup;

            engineeringModel.Iteration.Add(iteration);

            this.logger.LogDebug("Reading iteration data from the data-source {DataSource}", session.DataSourceUri);
            await session.Read(iteration, domainOfExpertise);

            this.logger.LogDebug("Reading iteration from cache");
            session.Assembler.Cache.TryGetValue(new CacheKey(iteration.Iid, null), out var lazyIteration);
            return (Iteration)lazyIteration.Value;
        }
    }
}
