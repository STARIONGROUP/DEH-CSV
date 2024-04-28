//  -------------------------------------------------------------------------------------------------
//  <copyright file="PropertyMap.cs" company="Starion Group S.A.">
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

    using CDP4Common.CommonData;

    /// <summary>
    /// The purpose of the <see cref="TypeMap"/> is to handle ECSS-E-TM-10-25 to CSV mapping
    /// </summary>
    public class TypeMap
    {
        /// <summary>
        /// Gets or sets an optinal name for the CSV filename
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind"/> of the ECSS-E-TM-10-25 class
        /// </summary>
        public ClassKind ClassKind { get; set; } = ClassKind.NotThing;

        /// <summary>
        /// Gets or sets the <see cref="PropertyMap"/>s
        /// </summary>
        public List<PropertyMap> Properties { get; set; } = new List<PropertyMap>();
    }
}
