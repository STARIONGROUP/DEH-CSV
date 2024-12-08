//  -------------------------------------------------------------------------------------------------
//  <copyright file="DataSourceSelector.cs" company="Starion Group S.A.">
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
    using System;
    using System.Globalization;

    using CDP4Dal.DAL;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// The purpose of the <see cref="IDataSourceSelector"/> is to select the appropriate <see cref="IDal"/> implementation
    /// based on the provided URI
    /// </summary>
    public class DataSourceSelector : IDataSourceSelector
    {
        /// <summary>
        /// The (injected) <see cref="ILogger{DataSourceSelector}"/>
        /// </summary>
        private readonly ILogger<DataSourceSelector> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceSelector"/>
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to setup logging
        /// </param>
        public DataSourceSelector(ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory == null ? NullLogger<DataSourceSelector>.Instance : loggerFactory.CreateLogger<DataSourceSelector>();
        }

        /// <summary>
        /// Selects the <see cref="IDal"/> implementation based on the scheme of the URI. Only HTTP, HTTPS and FILE 
        /// are supported
        /// </summary>
        /// <param name="uri">
        /// The <see cref="Uri"/> of the 10-25 data source
        /// </param>
        /// <returns>
        /// A IDal implementation
        /// </returns>
        public IDal Select(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            switch (uri.Scheme.ToLower(CultureInfo.InvariantCulture))
            {
                case "http":
                case "https":

                    this.logger.LogInformation("a CdpServicesDal will be used to read ECSS-E-TM-10-25 data");

                    var cdpServicesDal = new CDP4ServicesDal.CdpServicesDal();
                    return cdpServicesDal;

                case "file":

                    this.logger.LogInformation("a JsonFileDal will be used to read ECSS-E-TM-10-25 data");

                    var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();
                    return jsonFileDal;

                default:
                    throw new ArgumentException($"The URI scheme is not supported: {uri.Scheme}");
            }
        }
    }
}
