//  -------------------------------------------------------------------------------------------------
//  <copyright file="ICsvWriter.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV
{
    using System.Collections.Generic;
    using System.IO;

    using CDP4Common.EngineeringModelData;
    using CDP4Common.SiteDirectoryData;

    using RHEAGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The purpose of the <see cref="ICsvWriter"/> is to write CSV files based on the provided
    /// ECSS-E-TM-10-25 data set and <see cref="TypeMap"/>
    /// <see cref="TypeMap"/>s
    /// </summary>
    public interface ICsvWriter
    {
        /// <summary>
        /// Writes an <see cref="Iteration"/> to a CSV file in the <paramref name="target"/>
        /// </summary>
        /// <param name="siteDirectory">
        /// The <see cref="SiteDirectory"/> top container
        /// </param>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be written
        /// </param>
        /// <param name="maps">
        /// An <see cref="IEnumerable{TypeMap}"/> that specifies how the data is to be written
        /// </param>
        /// <param name="target">
        /// The target <see cref="DirectoryInfo"/> to which the CSV file is to be written
        /// </param>
        public void Write(SiteDirectory siteDirectory, Iteration iteration, IEnumerable<TypeMap> maps, DirectoryInfo target);
    }
}