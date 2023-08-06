//  -------------------------------------------------------------------------------------------------
//  <copyright file="CsvWriter.cs" company="RHEA System S.A.">
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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using CDP4Common.CommonData;
    using CDP4Common.EngineeringModelData;
    using CDP4Common.PropertyAccesor;
    using CDP4Common.SiteDirectoryData;

    using Microsoft.Extensions.Logging;

    using RHEAGROUP.DEHCSV.Mapping;

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
        /// <param name="logger">
        /// The (injected) <see cref="ILogger{CsvWriter}"/>
        /// </param>
        public CsvWriter(ILogger<CsvWriter> logger)
        {
            this.logger = logger;
        }

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
        public void Write(SiteDirectory siteDirectory, Iteration iteration, IEnumerable<TypeMap> maps, DirectoryInfo target)
        {
            var values = siteDirectory.Cache.Select(item => item.Value.Value).ToList();

            this.WriteSiteDirectoryThings(siteDirectory, values, maps, target);

            this.WriteEngineeringModelThings(iteration, values, maps, target);
        }

        /// <summary>
        /// Writes all <see cref="Thing"/>  that are contained by the <see cref="SiteDirectory"/>
        /// </summary>
        /// <param name="siteDirectory"></param>
        /// <param name="things"></param>
        /// <param name="maps"></param>
        /// <param name="target"></param>
        private void WriteSiteDirectoryThings(SiteDirectory siteDirectory, IEnumerable<Thing> things, IEnumerable<TypeMap> maps, DirectoryInfo target)
        {
            var siteDirectoryThings = things.Where(x => x.TopContainer == siteDirectory).ToList();

            foreach (var typeMap in maps)
            {
                var targetThings = siteDirectoryThings.Where(x => x.ClassKind == typeMap.ClassKind).ToList();

                this.Write(targetThings, typeMap, target);
            }
        }

        /// <summary>
        /// Writes all <see cref="Thing"/>s that are contained by the <see cref="EngineeringModel"/> which is the container of the
        /// provided <see cref="Iteration"/>
        /// </summary>
        /// <param name="iteration">
        /// The <see cref="Iteration"/> that is to be written to the CSV file
        /// ></param>
        /// <param name="things"></param>
        /// <param name="maps"></param>
        /// <param name="target"></param>
        private void WriteEngineeringModelThings(Iteration iteration, IEnumerable<Thing> things, IEnumerable<TypeMap> maps, DirectoryInfo target)
        {
            var engineeringModelThings = things.Where(x => x.TopContainer == iteration.TopContainer).ToList();

            foreach (var typeMap in maps)
            {
                var targetThings = engineeringModelThings.Where(x => x.ClassKind == typeMap.ClassKind).ToList();

                this.Write(targetThings, typeMap, target);
            }
        }

        /// <summary>
        /// Writes the provide things to the target <see cref="DirectoryInfo"/>
        /// </summary>
        /// <param name="things"></param>
        /// <param name="typeMap">
        /// The <see cref="TypeMap"/> that is used for mapping
        /// </param>
        /// <param name="target">
        /// The target <see cref="DirectoryInfo"/> to which the CSV file is to be written
        /// </param>
        public virtual void Write(IEnumerable<Thing> things, TypeMap typeMap, DirectoryInfo target)
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

            var filename = this.QueryFileName(typeMap);

            var path = Path.Combine(target.FullName, $"{filename}-export.csv");

            using var writer = new StreamWriter(path);

            using var csvWriter = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

            this.WriteHeader(csvWriter, typeMap);

            foreach (var thing in things)
            {
                foreach (var propertyMap in typeMap.Properties)
                {
                    var queryResult = thing.QueryValue(propertyMap.Source);

                    if (queryResult is IEnumerable<object> queriedValues)
                    {
                        var csvValue = string.Join(propertyMap.Separator, queriedValues);

                        csvWriter.WriteField(csvValue, true);
                    }
                    else
                    {
                        if (queryResult != null)
                        {
                            csvWriter.WriteField(queryResult.ToString(), true);
                        }
                        else
                        {
                            csvWriter.WriteField("-", true);
                        }
                    }
                }

                csvWriter.NextRecord();
            }
        }

        /// <summary>
        /// Writes the Header (first row) to the CSV file
        /// </summary>
        /// <param name="csvWriter">
        /// The target <see cref="csvWriter"/>
        /// </param>
        /// <param name="typeMap">
        /// The <see cref="TypeMap"/> used to write the header data
        /// </param>
        protected void WriteHeader(CsvHelper.CsvWriter csvWriter, TypeMap typeMap)
        {
            this.logger.LogDebug("Writing Header for {typeMap}", typeMap.ClassKind.ToString());

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