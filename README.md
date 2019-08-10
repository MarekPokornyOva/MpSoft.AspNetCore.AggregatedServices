![Logo](./icon.png "Logo")
# MpSoft.AspNetCore.AggregatedServices

[![Package Version](https://img.shields.io/nuget/v/MpSoft.AspNetCore.AggregatedServices.svg)](https://www.nuget.org/packages/MpSoft.AspNetCore.AggregatedServices)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MpSoft.AspNetCore.AggregatedServices.svg)](https://www.nuget.org/packages/MpSoft.AspNetCore.AggregatedServices)
[![License](https://img.shields.io/github/license/MarekPokornyOva/MpSoft.AspNetCore.AggregatedServices.svg)](https://github.com/MarekPokornyOva/MpSoft.AspNetCore.AggregatedServices/blob/master/LICENSE)

### Description
AggregatedServices is simply generator which generates .NET class based on provided interface. This way simplifies services aggregation and mass services usage.
AggregatedServices is reimplementation of AutoFac's AggregatedServices to no need all it's infrastructure.

### Features
* Generated type/assembly can be saved to storage (disk, memory, or basically whatever)
* Generated type can be reused after application restart
* Various resolve modes - OnCreate,OnDemand,OnDemandCached

### ASP.NET Core Mvc
There's also package which simplifies usage in ASP.NET Core Mvc.

### Documentation
[See](./Documentation.md)

### Release notes
[See](./ReleaseNotes.md)
