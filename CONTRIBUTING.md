# `Hamnet` SNMP Monitoring Tools Contribution guide
The project has been started by Kurt, DJ3MU.  
It's written in C# as Dotnet Core application. It's using quite a couple of useful Nuget package for which I want to take the chance to say "Thank you" to all the package developers.

The code does not in any way claim to be perfect. I've tried to obey software design concepts and do clean coding as much as I could. But I'm really open to all kinds of improvement requests. Feel free to contact me or submit pull requests and I'll do my best to improve.

**Everybody is encouraged to actively use the tool and report back bugs and/or new requirements.**

**Also Pull Requests for fixes are functional extensions are highly appreciated.**

### Building and deploying
From the root folder of repository run
```shell
dotnet build
```
To publish to a [self-contained, frame-work-dependent executable](https://docs.microsoft.com/en-us/dotnet/core/deploying/#framework-dependent-executables-fde) (i.e. an executable with all required DLLs inside same folder) use
```shell
dotnet publish -c Release -r win10-x64
```
for or
```shell
dotnet publish -c Release -r linux-x64
```
For more platforms see the dotnet core [runtime identifier catalog](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).


## Configuration Management
Even though I've started on a single master branch, the tool is now using a [GitFlow](https://datasift.github.io/gitflow/IntroducingGitFlow.html) development model.

Continuous integration is currently being set up using [Github Actions](https://github.com/features/actions).

There are a couple of Unit Tests available. But they leave a lot of space for improvements. Unit testing also still needs to be added to the CI.

There is a packaging of the self-contained publish result for Linux-X64 as tar archive done by the CI.  
But that archive is only available for download on the project's [Github Actions page](https://github.com/dj3mu/HamnetMonitoring/actions).

No automatic deployment is yet set up. Mainly because Github cannot easily access servers in `Hamnet`.
