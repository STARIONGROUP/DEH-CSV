//  -------------------------------------------------------------------------------------------------
//  <copyright file="CsvWriter.cs" company="Starion Group S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.Helpers;
    using CDP4Common.PropertyAccesor;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    using  STARIONGROUP.DEHCSV.CustomProperties;
    using  STARIONGROUP.DEHCSV.Mapping;

    /// <summary>
    /// The purpose of the <see cref="CsvWriter"/> is to write CSV files based on the provided
    /// ECSS-E-TM-10-25 data set and <see cref="TypeMap"/>
    /// <see cref="TypeMap"/>s
    /// </summary>
    public class CsvWriter : ICsvWriter
    {
        /// <summary>
        /// The (injected) <see cref="ILogger{CsvWriter}"/>
        /// </summary>
        private readonly ILogger<CsvWriter> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvWriter"/>
        /// </summary>
        /// <param name="loggerFactory">
        /// The (injected) <see cref="ILoggerFactory"/> used to setup logging
        /// </param>
        public CsvWriter(ILoggerFactory loggerFactory = null)
        {
            this.logger = loggerFactory == null ? NullLogger<CsvWriter>.Instance : loggerFactory.CreateLogger<CsvWriter>();
        }

        /// <summary>
        /// Writes an <see cref="Iteration"/> to a CSV file in the <paramref name="target"/>
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be written. Please not that all <see cref="Thing"/>s
        /// that are available in the <see cref="Iteration.Cache"/> can be written to the CSV. The
        /// <see cref="Iteration"/> provides the entry point.
        /// </param>
        /// <param name="maps">
        /// An <see cref="IEnumerable{TypeMap}"/> that specifies how the data is to be written
        /// </param>
        /// <param name="includeNestedElements">
        /// A value that indicates whether a volatile nested element tree needs to be generated 
        /// for each <see cref="Option"/> in the provided <see cref="Iteration"/> and added to
        /// the <see cref="Thing"/>s for which CSVs are to be written
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
        public void Write(Iteration iteration, bool includeNestedElements, IEnumerable<TypeMap> maps, DirectoryInfo target, object options)
        {
            if (iteration == null)
            {
                throw new ArgumentNullException(nameof(iteration));
            }

            if (maps == null)
            {
                throw new ArgumentNullException(nameof(maps));
            }

            var things = iteration.Cache.Select(item => item.Value.Value).ToList();

            if (includeNestedElements)
            {
                this.logger.LogDebug("Generating Nested Elements to be included in the data set");

                var nestedElementTreeGenerator = new NestedElementTreeGenerator();

                foreach (CDP4Common.EngineeringModelData.Option option in iteration.Option)
                {
                    nestedElementTreeGenerator.Generate(option, true);

                    things.AddRange(option.NestedElement);
                }
            }

            this.logger.LogDebug("A total of {Count} are available in the data set", things.Count);

            foreach (var typeMap in maps)
            {
                var targetThings = things.Where(x => x.ClassKind == typeMap.ClassKind).ToList();

                this.Write(targetThings, typeMap, target, options);

                this.logger.LogInformation("Writing CSV file for {ClassKind}", typeMap.ClassKind);
            }
        }

        /// <summary>
        /// Writes the provide things to the target <see cref="DirectoryInfo"/>
        /// </summary>
        /// <param name="things">
        /// The <see cref="List{Thing}"/> that is to be written to the CSV files
        /// </param>
        /// <param name="typeMap">
        /// The <see cref="TypeMap"/> that is used for mapping <see cref="Thing"/> properties
        /// to CSV columns
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
        public virtual void Write(IEnumerable<Thing> things, TypeMap typeMap, DirectoryInfo target, object options)
        {
            if (things == null)
            {
                throw new ArgumentNullException(nameof(things));
            }

            if (typeMap == null)
            {
                throw new ArgumentNullException(nameof(typeMap));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (!target.Exists)
            {
                target.Create();
            }

            var sw = Stopwatch.StartNew();

            var filename = this.QueryFileName(typeMap);

            var path = Path.Combine(target.FullName, $"{filename}.csv");

            using var writer = System.IO.File.CreateText(path);

            using var csvWriter = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

            this.WriteHeader(csvWriter, typeMap);

            foreach (var thing in things)
            {
                foreach (var propertyMap in typeMap.Properties)
                {
                    var queryResult = this.QueryValue(thing, propertyMap, options);

                    if (queryResult is IEnumerable<object> queriedValues)
                    {
                        var csvValue = string.Join(propertyMap.Separator, queriedValues);

                        csvValue = propertyMap.ValuePrefix + csvValue;

                        csvWriter.WriteField(csvValue, true);
                    }
                    else
                    {
                        if (queryResult != null)
                        {
                            var csvValue = propertyMap.ValuePrefix + queryResult;
                            
                            csvWriter.WriteField(csvValue, true);
                        }
                        else
                        {
                            csvWriter.WriteField("-", true);
                        }
                    }
                }

                csvWriter.NextRecord();
            }

            csvWriter.Flush();

            this.logger.LogDebug("A total of {Records} records was written to {Filepath} in {Time} [ms]",
                things.Count(), path, sw.ElapsedMilliseconds);
        }

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
        protected virtual object QueryValue(Thing thing, PropertyMap propertyMap, object options)
        {
            if (thing == null)
            {
                throw new ArgumentNullException(nameof(thing));
            }

            if (propertyMap == null)
            {
                throw new ArgumentNullException(nameof(propertyMap));
            }

            return thing.QueryValue(propertyMap.Source);
        }

        /// <summary>
        /// Writes the Header (first row) to the CSV file
        /// </summary>
        /// <param name="csvWriter">
        /// The target <see cref="CsvHelper.CsvWriter"/>
        /// </param>
        /// <param name="typeMap">
        /// The <see cref="TypeMap"/> used to write the header data
        /// </param>
        protected void WriteHeader(CsvHelper.CsvWriter csvWriter, TypeMap typeMap)
        {
            if (csvWriter == null)
            {
                throw new ArgumentNullException(nameof(csvWriter));
            }

            if (typeMap == null)
            {
                throw new ArgumentNullException(nameof(typeMap));
            }

            this.logger.LogDebug("Writing Header for {TypeMap}", typeMap.ClassKind.ToString());

            foreach (var propertyMap in typeMap.Properties)
            {
                csvWriter.WriteField(propertyMap.Target, true);
            }

            csvWriter.NextRecord();
        }

        /// <summary>
        /// Queries the name of the file to be used when exporting the CSV. The name is based
        /// on the <see cref="ClassKind"/> of the <see cref="TypeMap"/> and the first part of
        /// each property source.
        /// </summary>
        /// <param name="typeMap">
        /// The subject <see cref="TypeMap"/> from which the file name is queried. 
        /// </param>
        /// <returns>
        /// A string representing the filename
        /// </returns>
        protected internal string QueryFileName(TypeMap typeMap)
        {
            if (typeMap == null)
            {
                throw new ArgumentNullException(nameof(typeMap));
            }

            if (!string.IsNullOrEmpty(typeMap.FileName))
            {
                return typeMap.FileName;
            }

            var result = new List<string> { typeMap.ClassKind.ToString() };

            foreach (var propertyMap in typeMap.Properties)
            {
                var pd = PropertyDescriptor.QueryPropertyDescriptor(propertyMap.Source);
                result.Add(pd.Name);
            }

            return string.Join("-", result);
        }
    }
}
