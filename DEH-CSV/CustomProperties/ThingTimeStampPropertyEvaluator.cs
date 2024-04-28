//  -------------------------------------------------------------------------------------------------
//  <copyright file="ThingTimeStampPropertyEvaluator.cs" company="Starion Group S.A.">
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
    using System;

    using CDP4Common.CommonData;

    using  STARIONGROUP.DEHCSV.Mapping;

    /// <summary>
    /// A sample implementation of the <see cref="ICustomPropertyEvaluator"/>
    /// </summary>
    public class ThingTimeStampPropertyEvaluator : ICustomPropertyEvaluator
    {
        /// <summary>
        /// Evaluates the value of the property of the provided <see cref="Thing"/>
        /// which is a custom value that cannot be automatically derived from the
        /// ECSS-E-TM-10-25 model
        /// </summary>
        /// <param name="thing">
        /// The subject <see cref="Thing"/> for which the property value needs to
        /// be evaluated
        /// </param>
        /// <param name="propertyMap">
        /// The <see cref="PropertyMap"/> that can be used to customize how the requested value
        /// should be returned
        /// </param>
        /// <param name="options">
        /// an object that may contain any kind of configuration that is required
        /// for the evaluation of the custom property. This may be a value property
        /// such as a string or int or a complex object. It is the responsibility
        /// of the interface implementation to verify that the options argument is
        /// of the correct type
        /// </param>
        /// <returns>
        /// a string that represents the value of the custom property
        /// </returns>
        public string QueryValue(Thing thing, PropertyMap propertyMap, object options)
        {
            // here we simple return the classKind and the current timestamp.
            // to use this add a PropertyMap in which the Source property is either
            // ThingTimeStamp or ThingTimeStampPropertyEvaluator and override the
            // CsvWriter.QueryValue method

            return $"{thing.ClassKind}:{DateTime.UtcNow}";
        }
    }
}
