//  -------------------------------------------------------------------------------------------------
//  <copyright file="DataSourceSelector.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV.Services
{
    using System;

    using CDP4Dal.DAL;

    /// <summary>
    /// The purpose of the <see cref="IDataSourceSelector"/> is to select the appropriate <see cref="IDal"/> implementation
    /// based on the provided URI
    /// </summary>
    public class DataSourceSelector : IDataSourceSelector
    {
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
            switch (uri.Scheme.ToLower())
            {
                case "http":
                case "https":

                    var cdpServicesDal = new CDP4ServicesDal.CdpServicesDal();
                    return cdpServicesDal;

                case "file":

                    var jsonFileDal = new CDP4JsonFileDal.JsonFileDal();
                    return jsonFileDal;

                default:
                    throw new ArgumentException($"The URI scheme is not supported: {uri.Scheme}");
            }
        }
    }
}
