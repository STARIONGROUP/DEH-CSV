﻿//  -------------------------------------------------------------------------------------------------
//  <copyright file="ThingTimeStampedCSVWriter.cs" company="Starion Group S.A.">
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

namespace STARIONGROUP.DEHCSV.CustomProperties
{
    using CDP4Common.CommonData;

    using  STARIONGROUP.DEHCSV.Mapping;
    using System;

    /// <summary>
    /// Custom CSV Writer that used the <see cref="ThingTimeStampPropertyEvaluator"/>
    /// to write data to a csv column
    /// </summary>
    public class ThingTimeStampedCSVWriter : CsvWriter
    {
        /// <summary>
        /// Queries the value from the <see cref="Thing"/> based on the information
        /// encoded in the <see cref="PropertyMap"/>
        /// </summary>
        /// <param name="thing">
        /// The subject <see cref="Thing"/>
        /// </param>
        /// <param name="propertyMap">
        /// The subject <see cref="PropertyMap"/>
        /// </param>
        /// <param name="options">
        /// an object that may contain any kind of configuration that is required
        /// for the evaluation of the custom property. This may be a value property
        /// such as a string or int or a complex object. It is the responsibility
        /// of the interface implementation to verify that the options argument is
        /// of the correct type
        /// </param>
        /// <returns>
        /// the value associated to the <see cref="Thing"/> and <see cref="PropertyMap"/>
        /// </returns>
        /// <remarks>
        /// Override this method in case <see cref="ICustomPropertyEvaluator"/> need to
        /// be used to query the property value of the provided <see cref="Thing"/>
        /// </remarks>
        protected override object QueryValue(Thing thing, PropertyMap propertyMap, object options)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }

            if (propertyMap == null)
            {
                throw new ArgumentNullException(nameof(propertyMap));
            }

            switch (propertyMap.Source)
            {
                case "ThingTimeStamp":
                case "ThingTimeStampPropertyEvaluator":
                    var thingTimeStampPropertyEvaluator = new ThingTimeStampPropertyEvaluator();
                    return thingTimeStampPropertyEvaluator.QueryValue(thing, propertyMap, options);

                default:
                    return base.QueryValue(thing, propertyMap, options);
            }
        }
    }
}
