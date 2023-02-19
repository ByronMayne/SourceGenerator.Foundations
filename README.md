![Nuget](https://img.shields.io/nuget/v/SourceGenerator.Foundations) ![GitHub Workflow Status](https://img.shields.io/github/workflow/status/ByronMayne/SourceGenerator.Foundations/.NET)

# SourceGenerator.Foundations

Source Generators are awesome but working with them can be a bit painful. This library is designed to try to smooth out some bumps. To get started using the library just add a nuget reference. Once that is done you will have access to new types contained within your assembly.

## Quick Start

To get started all you need to do is add the NuGet package reference. You may or may not have to restart Visual Studio for the new types to show up. Then implement the base class `IncrementalGenerator`. 

```cs
namespace Example
{
    // IncrementalGenerator, is a generated type from `SourceGenerator.Foundations'
    public class ExampleSourceGenerator : IncrementalGenerator 
    {
       protected override void OnInitialize(IncrementalGeneratorInitializationContext context)
       {
            // Attaches Visaul Studio debugger without prompt 
            AttachDebugger();

            // Writes output to Visual Studios output window
            WriteLine("Hello World");
       }
    }
}
```

# What is inside?

There is a series of features this library hopes to provide.

## Nuget Nightmares

Source generators run in a unique place and their references don't act the same. If you have a source generator that attempts to reference a NuGet package it will fail to be found at runtime. The reason is the source generators need to have their dll's alongside the running process but that also means that the project the generator is running aginst will to. If you look online the best option is to do this:

```xml
<ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.1" GeneratePathProperty="true" PrivateAssets="all" />
</ItemGroup>
<ItemGroup>
    <EmbeddedResource Include="$(PKGNewtonsoft_Json)\lib\netstandard2.0\Newtonsoft.Json.dll" Visible="false" />
</ItemGroup>
```
This MSBuild logic adds a reference to `Newtonsoft.Json` version `13.0.1` and sets a few attributes. Using `GeneratePathProperty="true"` will emit the variable `PKGNewtonsoft_Json` which points at the root of the NuGet package on disk. Using `EmbeddedResource` we then give the full path to the dll. When the project is compiled the dlls will be embeded as a resource.

This works well but requires a lot of boilerplate. Even worse is this is just for one package. You would need to repeat this for every dependency and any transitive ones as well. This gets out of hand very fast.


*Source.Generator foundations automates this all for you. Just add your NuGet references and nothing else*

## Logging Framework
Source generator run in the background and it can be very hard to debug. If you want to make a log you have to write the files to disk and open to read them. 

With this library we leverage the existing Output window and create an entry for each source generator. This is all done by using some Visaul Studio api. Internally this uses a custom Skin for [Serilog.net](https://serilog.net/). Every generator comes with a `Logger` property allowing you to output any information

![output](./img/OutputWindow.png)



### **Captured Exceptions**

When a source generator throws an exception it normally just stops. In the base library we process all exceptions and forward their output to the `Source Generator` output tab. 

```log
[13:35:39 ERR] Error! An unhandle exception was thrown while running the source generator.
System.IO.FileNotFoundException: Could not load file or assembly 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The system cannot find the file specified.
File name: 'System.Collections.Immutable, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
   at Jawon.SourceGenerator.JawonSourceGenerator.OnInitialize(IncrementalGeneratorInitializationContext context)
   at SGF.IncrementalGenerator.Microsoft.CodeAnalysis.IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext context)

WRN: Assembly binding logging is turned OFF.
To enable assembly bind failure logging, set the registry value [HKLM\Software\Microsoft\Fusion!EnableLog] (DWORD) to 1.
Note: There is some performance penalty associated with assembly bind failure logging.
To turn this feature off, remove the registry value [HKLM\Software\Microsoft\Fusion!EnableLog].
```
*Example of a missing Assembly exception*


### Debugging 

To be able to debug a source generators one of the suggested methods is to add the following.

```cs
static CustomSourceGenerator()
{
    Debugger.Launch();
}
```
This will popup the following window and you have to select your visual studio instance. Instaed with foundations you can just call a single method and break points work from that point on.

![AutoAttach](./img/DebuggerAttach.gif)

# How it works

SourceGenerator.Foundations is a source generator for source generators. It embedds additional addtional classes into your project to provide all the features above. 
