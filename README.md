# Harmonize
Plugin based integration service

## Chanels

Creating a custom channel is as simple as creating a class that inherits from IChannel and implementing the methods.
The custom channel class needs to handle messages of any type given that cahnnel class will be like: 

```csharp
public class CustomChannel<T> : IChannel<T>
{
	//... implementation 
}
```

Channels created in the Harmonize project are separat nuget pakages.
Channels are loaded at runtime using reflection and the name of the channel class.
The name of the channel class is specified in the configuration file.

## Channel UnboundMemoryChannel

This channel is a simple in memory channel that stores jobs in memory.
The channel is fast and is the default channel used if no channel is specified in configuration.
The negative side of this channel type is that if jobs builds up faster then a plugin can consume and process jobs it can led to application using up all free memory on the machine.
In case of system failure or shut-down uncompleted jobs can and will get lost do to being store in memory.
