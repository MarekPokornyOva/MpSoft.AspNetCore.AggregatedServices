# Documentation

### CreateOptions
* AssemblyRepository - (optional) repository to store and load generated assembly. The repository might be both temporary and permanent.
* AssemblyNameProvider - (optional) AssemblyName generator.

### GenerateOptions
* ResolveMode - defines when will the property values resolved.
	- OnCreate - on type's constructor.
	- OnDemand - on property getter.
    - OnDemandCached - on first property getter call.
* ServicesRequired - defines if to use GetService or GetRequiredService method to resolve service.
