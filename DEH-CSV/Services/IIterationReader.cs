//  -------------------------------------------------------------------------------------------------
//  <copyright file="IIterationReader.cs" company="Starion Group S.A.">
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
    using System.Threading.Tasks;
    
    using CDP4Dal;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    /// <summary>
    /// The purpose of the <see cref="IIterationReader"/> is to read an <see cref="Iteration"/>
    /// using the <see cref="ISession"/> object
    /// </summary>
    public interface IIterationReader
    {
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
        Task<Iteration> ReadAsync(ISession session, string modelShortName, int iterationNumber, string domainOfExpertiseShortName);
    }
}
