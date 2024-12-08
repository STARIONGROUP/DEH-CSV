// -------------------------------------------------------------------------------------------------
//  <copyright file="ICsvReader.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;

    using CDP4Dal;

    using  STARIONGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The purpose of the <see cref="ICsvReader" /> is to read CSV files and transform the content to
    /// ECSS-E-TM-10-25 data set based on <see cref="TypeMap" />s
    /// </summary>
    public interface ICsvReader
    {
        /// <summary>
        /// Reads the CSV content of the <see cref="Stream" /> and maps it to <see cref="Thing" />s based on the provided collection of
        /// <see cref="TypeMap" />s
        /// </summary>
        /// <param name="stream">The <see cref="Stream" /> that contains CSV content</param>
        /// <param name="typeMaps">The collection of <see cref="TypeMap" />s</param>
        /// <param name="session">The <see cref="ISession" /> that helps to retrieve <see cref="Thing" /></param>
        /// <returns>A <see cref="Task{T}" /> that returns a collection of mapped <see cref="Thing" />s</returns>
        Task<IEnumerable<Thing>> ReadAsync(Stream stream, IReadOnlyCollection<TypeMap> typeMaps, ISession session);
    }
}
