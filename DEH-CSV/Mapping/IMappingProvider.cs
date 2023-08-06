//  -------------------------------------------------------------------------------------------------
//  <copyright file="IMappingProvider.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV.Mapping
{
    using System.Collections.Generic;

    /// <summary>
    /// The <see cref="IMappingProvider"/> is used to provide the ECSS-E-TM-10-25 types to CSV mappings
    /// </summary>
    public interface IMappingProvider
    {
        /// <summary>
        /// Queries the available <see cref="TypeMap"/>s
        /// </summary>
        /// <param name="path">
        /// The path to the mapping file
        /// </param>
        /// <returns>
        /// An <see cref="IEnumerable{lassMap}"/>
        /// </returns>
        IEnumerable<TypeMap> QueryMappings(string path);
    }
}
