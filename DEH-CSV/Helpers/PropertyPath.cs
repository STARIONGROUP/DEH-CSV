// -------------------------------------------------------------------------------------------------
//  <copyright file="PropertyPath.cs" company="RHEA System S.A.">
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

namespace RHEAGROUP.DEHCSV.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    using CDP4Common.PropertyAccesor;

    using RHEAGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The <see cref="PropertyPath" /> class is used to help building the path to follow based on a collection of association of
    /// <see cref="PropertyDescriptor" /> and <see cref="PropertyMap" />
    /// </summary>
    public class PropertyPath
    {
        /// <summary>
        /// Initializes a new instance of <see cref="PropertyPath" />
        /// </summary>
        /// <param name="properties">
        /// The collection of <see cref="PropertyMap" /> that will build the containment of
        /// <see cref="PropertyPath" />
        /// </param>
        public PropertyPath(IReadOnlyCollection<PropertyMap> properties)
        {
            var allIdentifiers = properties.Where(x => x.IsIdentifierProperty && !string.IsNullOrEmpty(x.Path))
                .Select<PropertyMap, (PropertyDescriptor, PropertyMap)>(x => (PropertyDescriptor.QueryPropertyDescriptor(x.Path), x));

            var root = properties.Single(x => x.IsIdentifierProperty && string.IsNullOrEmpty(x.Path));
            this.InitializeProperties(null, root, allIdentifiers.ToImmutableList());
        }

        /// <summary>
        /// Initializes a new <see cref="PropertyPath"/>
        /// </summary>
        /// <param name="descriptor">The associated <see cref="PropertyDescriptor"/></param>
        /// <param name="map">The associated <see cref="PropertyMap"/></param>
        /// <param name="otherProperties">
        /// The collection of <see cref="Tuple{T1,T2}" /> composed with <see cref="PropertyDescriptor" />
        /// and <see cref="PropertyMap" />
        /// </param>
        protected PropertyPath(PropertyDescriptor descriptor, PropertyMap map, IReadOnlyCollection<(PropertyDescriptor Descriptor, PropertyMap Map)> otherProperties)
        {
            this.InitializeProperties(descriptor, map, otherProperties);
        }

        /// <summary>
        /// Gets the collection of contained <see cref="PropertyPath" />
        /// </summary>
        public IReadOnlyCollection<PropertyPath> Children { get; private set; }

        /// <summary>
        /// Gets the associated <see cref="PropertyMap" />
        /// </summary>
        public PropertyMap PropertyMap { get; private set; }

        /// <summary>
        /// Gets the associated <see cref="PropertyDescriptor" />
        /// </summary>
        public PropertyDescriptor PropertyDescriptor { get; private set; }

        /// <summary>
        /// Initializes this <see cref="PropertyPath" /> properties
        /// </summary>
        /// <param name="descriptor">The associated <see cref="PropertyDescriptor" /></param>
        /// <param name="map">The associated <see cref="PropertyMap" /></param>
        /// <param name="allProperties">
        /// The collection of <see cref="Tuple{T1,T2}" /> composed with <see cref="PropertyDescriptor" />
        /// and <see cref="PropertyMap" />
        /// </param>
        private void InitializeProperties(PropertyDescriptor descriptor, PropertyMap map, IReadOnlyCollection<(PropertyDescriptor, PropertyMap)> allProperties)
        {
            this.PropertyMap = map;
            this.PropertyDescriptor = descriptor;
            this.ComputeChildren(allProperties);
        }

        /// <summary>
        /// Compute the <see cref="Children" /> property an initialize contained <see cref="PropertyPath" />
        /// </summary>
        /// <param name="allProperties">
        /// The collection of <see cref="Tuple{T1,T2}" /> composed with <see cref="PropertyDescriptor" />
        /// and <see cref="PropertyMap" />
        /// </param>
        private void ComputeChildren(IReadOnlyCollection<(PropertyDescriptor Descriptor, PropertyMap Map)> allProperties)
        {
            var currentDepth = this.PropertyDescriptor?.Depth ?? 0;
            var nextProperties = allProperties.Where(x => x.Descriptor.Depth == currentDepth + 1).ToList();

            if (nextProperties.Count == 1)
            {
                var (descriptor, map) = nextProperties[0];
                this.Children = new List<PropertyPath> { new (descriptor, map, allProperties) };
            }
            else
            {
                var propertiesWithSameSource = nextProperties.Where(x => x.Map.Source == this.PropertyMap.Source).ToList();
                var collectionToUseForChildren = propertiesWithSameSource.Count == 0 ? nextProperties : propertiesWithSameSource; 
                this.Children = new List<PropertyPath>(collectionToUseForChildren.Select(x => new PropertyPath(x.Descriptor, x.Map, allProperties)));
            }
        }
    }
}
