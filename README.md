# DEH-CSV

The Digital Engineering Hub CSV **DEH-CSV** library is used to convert an ECSS-E-TM-10-25 data set into a CSV files.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=coverage)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=RHEAGROUP_DEH-CSV&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=RHEAGROUP_DEH-CSV)

## Installation

The packages are available on Nuget at https://www.nuget.org/packages/DEH-CSV/

[![NuGet Badge](https://buildstats.info/nuget/DEH-CSV)](https://buildstats.info/nuget/DEH-CSV)

## Quickstart

  1. Install the nuget package into your project or solution. 
  1. Add a mapping file that states how the properties of an ECSS-E-TM-10-25 [Thing](https://comet-dev-docs.mbsehub.org/) needs to be mapped to fields in a CSV file.
  1. Use the [ICsvWriter](https://github.com/RHEAGROUP/DEH-CSV/blob/master/DEH-CSV/ICsvWriter.cs) interface and/or the [CsvWriter](https://github.com/RHEAGROUP/DEH-CSV/blob/master/DEH-CSV/CsvWriter.cs) class.

The `ICsvWriter` interface exposes only one method: **write**.

```
 public void Write(Iteration iteration, bool includeNestedElements, IEnumerable<TypeMap> maps, DirectoryInfo target, object options);
```

  - iteration: Proive an `Iteration` instance that provides access to all the `Thing`s that need to be exported, this includes the container `EngineeringMoedel` and the `SiteDirectory`. 
  - includeNestedElements: A value that indicates whether a volatile nested element tree needs to be generated for each `Option` in the provided `Iteration` and added to the `Thing` instances for which CSVs are to be written.
  - maps:  a collection of `TypeMap`s that contain the configuration of how a certain kind of `Thing` is to be mapped to fields in a CSV file.
  - target: the target directory where the CSV files are to be generated.
  - options: any kind of `object` that can contain configuration information (this is igonred by the standard implementation but can be used when a derived CsvWriter is created where the `Write` method is overriden).

## Build Status

GitHub actions are used to build and test the libraries

Branch | Build Status
------- | :------------
Master | ![Build Status](https://github.com/RHEAGROUP/DEH-CSV/actions/workflows/CodeQuality.yml/badge.svg?branch=master)
Development | ![Build Status](https://github.com/RHEAGROUP/DEH-CSV/actions/workflows/CodeQuality.yml/badge.svg?branch=development)

# License

The DEH-CSV libraries are provided to the community under the Apache License 2.0.