//  -------------------------------------------------------------------------------------------------
//  <copyright file="PropertyMap.cs" company="RHEA System S.A.">
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
    using CDP4Common.CommonData;

    /// <summary>
    /// The purpose of the <see cref="PropertyMap"/> is to handle ECSS-E-TM-10-25 to CSV mapping
    /// </summary>
    public class PropertyMap
    {
        /// <summary>
        /// Gets or sets the source property name on the ECSS-E-TM-10-25 class in case of the mapping from ECSS-E-TM10-25 to CSV.
        /// Gets or sets the column name of the CSV to use to retrieve a value
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets a string that is used to prefix the value that will be written to the
        /// value of the <see cref="Target"/> property
        /// </summary>
        public string ValuePrefix { get; set; }

        /// <summary>
        /// Gets or sets the target property name
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// Gets or sets the character that is to be used as separator for values
        /// that are enumerable
        /// </summary>
        /// <remarks>
        /// The default value is the pipe character |.
        /// </remarks>
        public string Separator { get; set; } = "|";

        /// <summary>
        /// Gets or sets a value indicating whether the current property targets a value identifying an existing thing or
        /// for a thing to be created
        /// </summary>
        public bool IsIdentifierProperty { get; set; }
        
        /// <summary>
        /// Gets or sets the path to find Thing(s)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name of property to apply the search filter
        /// </summary>
        public string Search { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" /> that should be retrieved 
        /// </summary>
        public ClassKind? ClassKind { get; set; }

        /// <summary>
        /// Gets or sets the asserts that the search value is the name of the CSV header
        /// </summary>
        public bool SearchBasedOnHeader { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ClassKind" /> that the search value have to have
        /// </summary>
        public ClassKind? SearchClassKind { get; set; }

        /// <summary>
        /// Gets or sets the asserts that the object to get is the first or the default based on a reference.
        /// </summary>
        public bool FirstOrDefault { get; set; }
    }
}
