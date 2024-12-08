//  -------------------------------------------------------------------------------------------------
//  <copyright file="MappingProvider.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.Mapping
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// The <see cref="MappingProvider"/> is used to provide the ECSS-E-TM-10-25 types to CSV mappings
    /// </summary>
    public class MappingProvider : IMappingProvider
    {
        /// <summary>
        /// The (injected) <see cref="ILogger{MappingProvider}"/>
        /// </summary>
        private readonly ILogger<MappingProvider> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingProvider"/>
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to setup logging
        /// </param>
        public MappingProvider(ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory == null ? NullLogger<MappingProvider>.Instance : loggerFactory.CreateLogger<MappingProvider>();
        }

        /// <summary>
        /// Queries the available <see cref="TypeMap"/>s
        /// </summary>
        /// <param name="path">
        /// The path to the mapping file
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{lassMap}"/>
        /// </returns>
        public IEnumerable<TypeMap> QueryMappings(string path)
        {
            this.logger.LogDebug("Reading the mapping from {Path}", path);

            var json = File.ReadAllText(path);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = {
                    new JsonStringEnumConverter()
                }
            };

            var mappings = JsonSerializer.Deserialize<List<TypeMap>>(json, options);

            this.logger.LogDebug("A total of {Count} mappings have been found", mappings.Count);

            return mappings;
        }
    }
}
