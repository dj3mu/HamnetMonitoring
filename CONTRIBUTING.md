# `Hamnet` SNMP Monitoring Tools Contribution guide
The project has been started by DJ3MU.  
It's written in C# as Dotnet Core application. It's using quite a couple of useful Nuget package for which I want to take the change to say "Thank you" to the package developers.

The code does not in any way claim to be perfect. I've tried to obey software design concepts as much as I could. But I'm really open to all kinds of improvement requests. Feel free to contact me or submit pull requests and I'll do my best to improve.

**Everybody is encouraged to actively use the tool and report back bugs and/or new requirements.**

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

Continuous integration is still to be set up.

There are a couple of Unit Tests available. But they leave a lot of space for improvements.

Even a release concept is still to be implemented. There's currently no packaging of the self-contained publish result for Linux (RPM / Debian packages) or Windows (msi or exe).

Pull requests are appreciated.
