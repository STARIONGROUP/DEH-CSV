//  -------------------------------------------------------------------------------------------------
//  <copyright file="ICsvWriter.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;

    using  STARIONGROUP.DEHCSV.Mapping;

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
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be written. Please not that all <see cref="Thing"/>s
        /// that are available in the <see cref="Iteration.Cache"/> can be written to the CSV. The
        /// <see cref="Iteration"/> provides the entry point.
        /// </param>
        /// <param name="includeNestedElements">
        /// A value that indicates whether a volatile nested element tree needs to be generated 
        /// for each <see cref="Option"/> in the provided <see cref="Iteration"/> and added to
        /// the <see cref="Thing"/>s for which CSVs are to be written
        /// </param>
        /// <param name="maps">
        /// An <see cref="IEnumerable{TypeMap}"/> that specifies how the data is to be written
        /// </param>
        /// <param name="target">
        /// The target <see cref="DirectoryInfo"/> to which the CSV file is to be written
        /// </param>
        /// <param name="options">
        /// an object that may contain any kind of configuration that is required
        /// for the evaluation of the custom property. This may be a value property
        /// such as a string or int or a complex object. It is the responsibility
        /// of the interface implementation to verify that the options argument is
        /// of the correct type
        /// </param>
        public void Write(Iteration iteration, bool includeNestedElements, IEnumerable<TypeMap> maps, DirectoryInfo target, object options);
    }
}
