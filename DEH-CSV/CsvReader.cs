// -------------------------------------------------------------------------------------------------
//  <copyright file="CsvReader.cs" company="RHEA System S.A.">
// 
//    Copyright 2023-2024 RHEA System S.A.
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
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using CDP4Common.CommonData;
    using CDP4Common.Helpers;
    using CDP4Common.PropertyAccesor;

    using CDP4Dal;

    using CsvHelper;
    using CsvHelper.Configuration;

    using Microsoft.Extensions.Logging;

    using RHEAGROUP.DEHCSV.Helpers;
    using RHEAGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The purpose of the <see cref="CsvReader" /> is to read CSV files and transform the content to
    /// ECSS-E-TM-10-25 data set based on <see cref="TypeMap" />s
    /// </summary>
    public class CsvReader : ICsvReader
    {
        /// <summary>
        /// Gets the injected <see cref="ILogger{TCategoryName}" />
        /// </summary>
        private readonly ILogger<CsvReader> logger;

        /// <summary>
        /// Initializes a new instance of <see cref="CsvReader" />
        /// </summary>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}" /></param>
        public CsvReader(ILogger<CsvReader> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Reads the CSV content of the <see cref="Stream" /> and maps it to <see cref="Thing" />s based on the provided collection of
        /// <see cref="TypeMap" />s
        /// </summary>
        /// <param name="stream">The <see cref="Stream" /> that contains CSV content</param>
        /// <param name="typeMaps">The collection of <see cref="TypeMap" />s</param>
        /// <param name="session">The <see cref="ISession" /> that helps to retrieve <see cref="Thing" /></param>
        /// <returns>A <see cref="Task{T}" /> that returns a collection of mapped <see cref="Thing" />s</returns>
        public async Task<IEnumerable<Thing>> Read(Stream stream, IReadOnlyCollection<TypeMap> typeMaps, ISession session)
        {
            ValidateReadParameters(stream, typeMaps, session);

            var things = new List<Thing>();

            var accessibleThings = session.Assembler.Cache
                .Where(x => x.Value.IsValueCreated)
                .Select(x => x.Value.Value)
                .ToImmutableList();

            stream.Position = 0;
            using var streamReader = new StreamReader(stream);

            using var reader = new CsvHelper.CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                DetectDelimiter = true
            });

            await this.ReadHeader(reader, typeMaps);

            while (await reader.ReadAsync())
            {
                foreach (var typeMap in typeMaps)
                {
                    this.MapCsvRow(reader, typeMap, accessibleThings, things);
                }
            }

            return things;
        }

        /// <summary>
        /// Map a row of the CSV file to <see cref="Thing" />
        /// </summary>
        /// <param name="reader">The <see cref="IReader" /> used to read CSV content</param>
        /// <param name="typeMap">A <see cref="TypeMap" /> to process</param>
        /// <param name="accessibleThings">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" />s that comes from the cache</param>
        /// <param name="allMappedThings">The collection of all <see cref="Thing" />s that have been mapped for a CSV file</param>
        private void MapCsvRow(IReaderRow reader, TypeMap typeMap, IReadOnlyCollection<Thing> accessibleThings, List<Thing> allMappedThings)
        {
            var mappedThings = new List<Thing>();

            var path = new PropertyPath(typeMap.Properties);

            var currentPropertyMap = typeMap.Properties.Single(x => x.IsIdentifierProperty && string.IsNullOrEmpty(x.Path));

            this.ValidateEntryPoint(typeMap, currentPropertyMap);

            var currentClasskind = currentPropertyMap.ClassKind!.Value;
            var alreadyReadThings = new List<Thing>(allMappedThings);
            var entryPointValue = QueryValueToUse(reader, currentPropertyMap);
            var lastThingsBeforeTargetClassKind = new Dictionary<(Thing Thing, string PropertyName), List<Thing>>();

            this.logger.LogDebug("Processing Entry point with ClassKind {0}", currentClasskind);

            var previousThings = QueryMatchingThings(currentClasskind, entryPointValue, currentPropertyMap, alreadyReadThings, accessibleThings);

            if (currentPropertyMap.ClassKind == typeMap.ClassKind)
            {
                if (previousThings.Count != 0)
                {
                    foreach (var previousThing in previousThings)
                    {
                        UpdateThingValues(reader, previousThing, typeMap, currentPropertyMap, alreadyReadThings, accessibleThings);
                    }

                    mappedThings.AddRange(previousThings);
                }
                else
                {
                    var newThing = TypeInitializer.Initialize(currentClasskind);
                    UpdateThingValues(reader, newThing, typeMap, currentPropertyMap, alreadyReadThings, accessibleThings);
                    mappedThings.Add(newThing);
                }
            }
            else
            {
                if (previousThings.Count == 0)
                {
                    this.logger.LogError("The provided CSV references Thing(s) that are not part of the Database: Source : {0}", path.PropertyMap.Source);
                    throw new InvalidDataException($"The provided CSV references Thing(s) that are not part of the Database: Source : {path.PropertyMap.Source}");
                }

                foreach (var child in path.Children)
                {
                    this.ProcessPath(child, previousThings, reader, alreadyReadThings, accessibleThings, lastThingsBeforeTargetClassKind, typeMap, mappedThings);
                }
            }

            foreach (var kvp in lastThingsBeforeTargetClassKind)
            {
                // Handle case if the value to set is a not a collection
                if (kvp.Value.Count == 1)
                {
                    kvp.Key.Thing.SetValue(kvp.Key.PropertyName, kvp.Value[0]);
                }
                else
                {
                    kvp.Key.Thing.SetValue(kvp.Key.PropertyName, kvp.Value);
                }
            }

            allMappedThings.AddRange(mappedThings.Distinct().Where(x => !allMappedThings.Contains(x)));
        }

        /// <summary>
        /// Process a <see cref="PropertyPath" /> to map CSV content to <see cref="Thing" />s
        /// </summary>
        /// <param name="path">The <see cref="PropertyPath" /> to process</param>
        /// <param name="previousThings">
        /// A collection of <see cref="Thing" /> that are part of the previous
        /// <see cref="PropertyPath" />
        /// </param>
        /// <param name="reader">The <see cref="IReaderRow" /> to be able to read CSV content</param>
        /// <param name="alreadyReadThings">A collection of already read <see cref="Thing" />s</param>
        /// <param name="accessibleThings">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" />s that comes from the cache</param>
        /// <param name="lastThingsBeforeTargetClassKind">
        /// A <see cref="Dictionary{TKey,TValue}" /> that tracks before last
        /// <see cref="Thing" /> with the propertyName where the mapped <see cref="Thing" /> for the <see cref="TypeMap" /> have to be set
        /// </param>
        /// <param name="typeMap">The <see cref="TypeMap" /></param>
        /// <param name="mappedThings">A collection of <see cref="Thing" /> that have been mapped during the process of one CSV row</param>
        /// <exception cref="InvalidDataException">If one of the requested <see cref="Thing" /> during the path build does not exist</exception>
        private void ProcessPath(PropertyPath path, IReadOnlyList<Thing> previousThings, IReaderRow reader, List<Thing> alreadyReadThings, IReadOnlyCollection<Thing> accessibleThings, Dictionary<(Thing Thing, string PropertyName), List<Thing>> lastThingsBeforeTargetClassKind, TypeMap typeMap, List<Thing> mappedThings)
        {
            var value = QueryValueToUse(reader, path.PropertyMap);
            var currentClassKind = path.PropertyMap.ClassKind!.Value;

            var relatedThings = QueryMatchingThings(currentClassKind, value, path.PropertyMap, alreadyReadThings, accessibleThings);
            var referencedThings = new List<Thing>();

            foreach (var previousThing in previousThings)
            {
                var referencedValue = previousThing.QueryValue(path.PropertyDescriptor[path.PropertyDescriptor.Depth - 1].Input);

                switch (referencedValue)
                {
                    case Thing tthing:
                        referencedThings.Add(tthing);
                        break;
                    case IEnumerable<Thing> tthings:
                        if (path.PropertyMap.FirstOrDefault)
                        {
                            if (tthings.FirstOrDefault() is { } firstThing)
                            {
                                referencedThings.Add(firstThing);
                            }
                        }
                        else
                        {
                            referencedThings.AddRange(tthings);
                        }

                        break;
                }
            }

            var foundThings = referencedThings.Intersect(relatedThings).ToList();
            alreadyReadThings.AddRange(foundThings);

            if (foundThings.Count == 0)
            {
                if (currentClassKind != typeMap.ClassKind)
                {
                    this.logger.LogError("The provided CSV references Thing(s) that are not part of the Database: Source : {0}, Value: {1}", path.PropertyMap.Source, value);
                    throw new InvalidDataException($"The provided CSV references Thing(s) that are not part of the Database: Source : {path.PropertyMap.Source}, Value: {value}");
                }

                var newThing = TypeInitializer.Initialize(currentClassKind);
                UpdateThingValues(reader, newThing, typeMap, path.PropertyMap, alreadyReadThings, accessibleThings);
                mappedThings.Add(newThing);

                foreach (var previousThing in previousThings)
                {
                    var key = (previousThing, path.PropertyDescriptor[path.PropertyDescriptor.Depth - 1].Input);

                    if (lastThingsBeforeTargetClassKind.TryGetValue(key, out var thingsToSet))
                    {
                        thingsToSet.Add(newThing);
                    }
                    else
                    {
                        lastThingsBeforeTargetClassKind[key] = [newThing];
                    }
                }
            }
            else
            {
                if (currentClassKind == typeMap.ClassKind)
                {
                    foreach (var foundThing in foundThings)
                    {
                        UpdateThingValues(reader, foundThing, typeMap, path.PropertyMap, alreadyReadThings, accessibleThings);
                        mappedThings.Add(foundThing);
                    }

                    foreach (var previousThing in previousThings)
                    {
                        var key = (previousThing, path.PropertyDescriptor[path.PropertyDescriptor.Depth - 1].Input);

                        if (lastThingsBeforeTargetClassKind.TryGetValue(key, out var thingsToSet))
                        {
                            thingsToSet.AddRange(foundThings);
                        }
                        else
                        {
                            lastThingsBeforeTargetClassKind[key] = [..foundThings];
                        }
                    }
                }
            }

            foreach (var propertyPath in path.Children)
            {
                this.ProcessPath(propertyPath, foundThings, reader, alreadyReadThings, accessibleThings, lastThingsBeforeTargetClassKind, typeMap, mappedThings);
            }
        }

        /// <summary>
        /// Queries a collection of <see cref="Thing" /> for that matches the provided <see cref="ClassKind" /> and that have a property value that contains the provided
        /// <paramref name="value" />
        /// </summary>
        /// <param name="classKind">The <see cref="ClassKind" /> to match</param>
        /// <param name="value">The expected value</param>
        /// <param name="propertyMap">The <see cref="PropertyMap" /> that contains information for property name to search on</param>
        /// <param name="alreadyReadThings">A collection of already read <see cref="Thing" />s</param>
        /// <param name="accessibleThings">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" />s that comes from the cache</param>
        /// <returns>The collection of retrieve <see cref="Thing" /> that matches the request</returns>
        /// <remarks>
        /// If the <see cref="PropertyMap.FirstOrDefault" /> is set, every <see cref="Thing" /> that matches the
        /// <see cref="ClassKind" /> will be retrieved
        /// </remarks>
        private static List<Thing> QueryMatchingThings(ClassKind classKind, string value, PropertyMap propertyMap, IEnumerable<Thing> alreadyReadThings, IEnumerable<Thing> accessibleThings)
        {
            var allThings = new List<Thing>(accessibleThings);
            allThings.AddRange(alreadyReadThings);
            allThings = allThings.Distinct().ToList();

            return allThings.Where(Predicate).ToList();

            bool Predicate(Thing thing)
            {
                return thing.ClassKind == classKind
                       && (propertyMap.FirstOrDefault || DoesContainsValue(thing.QueryValue(propertyMap.Search), value, propertyMap.Separator));
            }
        }

        /// <summary>
        /// Update all values that have been defined inside the <see cref="TypeMap" />  as value setter (when the
        /// <see cref="PropertyMap.Target" /> value is defined)
        /// </summary>
        /// <param name="reader">The <see cref="IReaderRow" /> that provide CSV content read</param>
        /// <param name="thingToUpdate">The <see cref="Thing" /> that where values have to be set</param>
        /// <param name="typeMap">The defined <see cref="TypeMap" /></param>
        /// <param name="identifierPropertyMap">
        /// The <see cref="PropertyMap" /> to provide identification to retrieve or create the
        /// <paramref name="thingToUpdate" />
        /// </param>
        /// <param name="alreadyReadThings">A collection of already read <see cref="Thing" />s</param>
        /// <param name="accessibleThings">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" />s that comes from the cache</param>
        private static void UpdateThingValues(IReaderRow reader, Thing thingToUpdate, TypeMap typeMap, PropertyMap identifierPropertyMap, IReadOnlyCollection<Thing> alreadyReadThings, IReadOnlyCollection<Thing> accessibleThings)
        {
            if (identifierPropertyMap.Target != null)
            {
                thingToUpdate.SetValue(identifierPropertyMap.Target, QueryObjectValueToSet(reader, identifierPropertyMap, alreadyReadThings, accessibleThings));
            }

            foreach (var targetPropertyMap in typeMap.Properties.Where(x => !x.IsIdentifierProperty && !string.IsNullOrWhiteSpace(x.Target)))
            {
                thingToUpdate.SetValue(targetPropertyMap.Target, QueryObjectValueToSet(reader, targetPropertyMap, alreadyReadThings, accessibleThings));
            }
        }

        /// <summary>
        /// Queries the object value that have to be set based on the provided <see cref="PropertyMap" /> and the content of the CSV
        /// </summary>
        /// <param name="reader">The <see cref="IReaderRow" /> that provide CSV content read</param>
        /// <param name="propertyMap">The <see cref="PropertyMap" /> that defines logic</param>
        /// <param name="alreadyReadThings">A collection of already read <see cref="Thing" />s</param>
        /// <param name="accessibleThings">A <see cref="IReadOnlyCollection{T}" /> of <see cref="Thing" />s that comes from the cache</param>
        /// <returns>The object value that have to be set</returns>
        private static object QueryObjectValueToSet(IReaderRow reader, PropertyMap propertyMap, IReadOnlyCollection<Thing> alreadyReadThings, IReadOnlyCollection<Thing> accessibleThings)
        {
            var value = QueryValueToUse(reader, propertyMap);

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var splittedValue = value.Split(new[] { propertyMap.Separator }, StringSplitOptions.RemoveEmptyEntries);

            var objects = new List<object>();

            foreach (var split in splittedValue)
            {
                if (propertyMap.SearchClassKind.HasValue)
                {
                    bool Predicate(Thing thing)
                    {
                        var descriptor = PropertyDescriptor.QueryPropertyDescriptor(propertyMap.Search);
                        var searchValue = descriptor[descriptor.Depth - 1].Input;

                        return thing.ClassKind == propertyMap.SearchClassKind!.Value
                               && DoesContainsValue(thing.QueryValue(searchValue), split, propertyMap.Separator);
                    }

                    var referencedThings = alreadyReadThings.Where(Predicate).ToList();
                    referencedThings.AddRange(accessibleThings.Where(Predicate));

                    objects.AddRange(referencedThings.Distinct());
                }
                else
                {
                    objects.Add(split);
                }
            }

            return objects.Count == 1 ? objects[0] : objects;
        }

        /// <summary>
        /// Validate that the entryPoint <see cref="PropertyMap" /> is a valid one
        /// </summary>
        /// <param name="typeMap">The <see cref="TypeMap" /></param>
        /// <param name="entryPoint">The entry point <see cref="PropertyMap" /></param>
        /// <exception cref="InvalidDataException">
        /// If the <paramref name="entryPoint" /> is invalid. The <paramref name="entryPoint" /> is invalid in following cases:
        /// - If none entry point is defined
        /// - If the <see cref="PropertyMap.ClassKind" /> is not set
        /// - If the <see cref="PropertyMap.ClassKind" /> is equals to <see cref="TypeMap.ClassKind" /> but some identifier
        /// <see cref="PropertyMap" /> are defined
        /// </exception>
        private void ValidateEntryPoint(TypeMap typeMap, PropertyMap entryPoint)
        {
            var identifierProperties = typeMap.Properties.Where(x => x.IsIdentifierProperty && x != entryPoint);

            if (entryPoint == default)
            {
                this.logger.LogError("The provided json mapping function does not provide the entry point of the csv file");
                throw new InvalidDataException("The provided json mapping function does not provide the entry point of the csv file");
            }

            if (entryPoint.ClassKind == null)
            {
                this.logger.LogError("The defined EntryPoint does not specify the ClassKind");
                throw new InvalidDataException("The defined EntryPoint does not specify the ClassKind");
            }

            if (entryPoint.ClassKind == typeMap.ClassKind && identifierProperties.Any())
            {
                this.logger.LogError("The provided json mapping is invalid since the entry point is of the same type as the hub type but expect to build a path via other identifiers objects");
                throw new InvalidDataException("The provided json mapping is invalid since the entry point is of the same type as the hub type but expect to build a path via other identifiers objects");
            }
        }

        /// <summary>
        /// Reads the header row from the provided <see cref="CsvReader" />
        /// </summary>
        /// <param name="reader">The <see cref="IReader" /> used to read CSV content</param>
        /// <param name="typeMaps">The collection of <see cref="TypeMap" />s</param>
        /// <returns>A <see cref="Task" /></returns>
        private async Task ReadHeader(IReader reader, IEnumerable<TypeMap> typeMaps)
        {
            await reader.ReadAsync();

            if (!reader.ReadHeader())
            {
                this.logger.LogError("The provided Csv does not provide any header, the mapping cannot continue");
                throw new InvalidOperationException("The provided Csv does not provide any header, the mapping cannot continue");
            }

            var headers = reader.HeaderRecord;

            if (typeMaps.SelectMany(x => x.Properties).FirstOrDefault(p => Array.TrueForAll(headers, x => !x.Equals(p.Source))) is { } invalidPropertyMap)
            {
                this.logger.LogError("The provided CSV does not contains any header for the source {0}", invalidPropertyMap.Source);
                throw new InvalidDataException($"The provided CSV does not contains any header for the source {invalidPropertyMap.Source}");
            }
        }

        /// <summary>
        /// Validates all parameters provided for the <see cref="Read" /> method
        /// </summary>
        /// <param name="stream">The provided <see cref="Stream" /></param>
        /// <param name="typeMaps">The provided collection of <see cref="TypeMap" />s</param>
        /// <param name="session">The provided <see cref="ISession" /></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        private static void ValidateReadParameters(Stream stream, IReadOnlyCollection<TypeMap> typeMaps, ISession session)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (typeMaps == null)
            {
                throw new ArgumentNullException(nameof(typeMaps));
            }

            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }

            if (typeMaps.Count == 0)
            {
                throw new ArgumentException("The provided collection of TypeMap is empty", nameof(typeMaps));
            }

            if (typeMaps.SelectMany(x => x.Properties).Any(x => x.IsIdentifierProperty && !x.ClassKind.HasValue))
            {
                throw new InvalidDataException("One of the IdentifierProperty do not specify the ClassKind to query");
            }
        }

        /// <summary>
        /// Queries the value to use for a cell inside a CSV row based on a <see cref="PropertyMap" />
        /// </summary>
        /// <param name="reader">The <see cref="IReaderRow" /> that can retrieve the value of a CSV cell</param>
        /// <param name="propertyMap">The current <see cref="PropertyMap" /></param>
        /// <returns>The string value to use</returns>
        /// <remarks>
        /// If the <see cref="PropertyMap.SearchBasedOnHeader" /> is set, will return <see cref="PropertyMap.Source" />,
        /// the content of the CSV cell is return in the other case
        /// </remarks>
        private static string QueryValueToUse(IReaderRow reader, PropertyMap propertyMap)
        {
            return propertyMap.SearchBasedOnHeader ? propertyMap.Source : reader.GetField<string>(propertyMap.Source);
        }

        /// <summary>
        /// Verifies that an <see cref="object" /> contains the <paramref name="expectedValue" />
        /// </summary>
        /// <param name="toSearchInto">The <see cref="object" /> where we want to search into</param>
        /// <param name="expectedValue">An expected string to retrieve</param>
        /// <param name="separator">A separator</param>
        /// <returns>The result of the search</returns>
        private static bool DoesContainsValue(object toSearchInto, string expectedValue, string separator)
        {
            if (toSearchInto == null)
            {
                return expectedValue == null;
            }

            if (expectedValue.Contains(separator))
            {
                var splittedExpectedValue = expectedValue.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                return Array.Exists(splittedExpectedValue, x => DoesContainsValue(toSearchInto, x, separator));
            }

            if (toSearchInto is IEnumerable enumerable and not string)
            {
                return enumerable.Cast<object>().Any(value => value.ToString() == expectedValue);
            }

            return toSearchInto.ToString() == expectedValue;
        }
    }
}
