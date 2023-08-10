//  -------------------------------------------------------------------------------------------------
//  <copyright file="ICustomPropertyEvaluator.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV.CustomPropertyWriter
{
    using CDP4Common.CommonData;
    
    using RHEAGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The purpose of the <see cref="ICustomPropertyEvaluator"/> interface is to support
    /// any custom property that is defined in the <see cref="PropertyMap"/> to be evaluated.
    /// </summary>
    public interface ICustomPropertyEvaluator
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
        public string QueryValue(Thing thing, object options);
    }
}
