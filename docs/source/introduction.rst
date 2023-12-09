Introduction
=====

.. _installation:

About
------------
A scenario where Hormonized is used could be to get a file from one place then transform the files content and then store/send the content some where.

Above scenario could be a worker manager with 3 workers

    worker 1 -> GetFileFromStorage
    worker 2 -> TransformFileFromTo
    worker 3 -> StoreFileOnFTP

The scenario could also be only one worker thar performs al the tasks: Gets the file transform it and stores it on the ftp.

The advantage of using smal single responsibility workers is the possibility to reuse the worker in many integrations.

A scenario could be as simple as a worker manager  with one worker that monitor a folder and do stuff to new files found in folder like rename or move to backup folder.

There can be one to many workers (integrations) and each worker is running as its own host within the service. 

GetStarted
----------------

Create a C# project.

Add nuget package HarmonizeService >> insert name of nuget package here.

In your startup.cs file add the Harmonize WorkerHandlerService to serviceCollection

.. code-block:: csharp

 public void ConfigureServices(IServiceCollection services)
        {
           services.AddSingleton<IWorkerHandlerService, WorkerHandlerService>();
        }


Above will add WorkerHandlerService to service collection and that will not instantiate the service thus the service will not get started.
Some ware in the application WorkerHandlersService needs to get instantiated to start.

To Auto start service when added to serviceCollection instead of above AddSingleton use the service collection extension AddHarmonizeService();.

.. code-block:: csharp
   
      services.AddHarmonizeService();


Edit this

insert link to harmonize configuration docs

Add a harmonize.config.json file to application root folder.
Add plugin dll files needed by worker configured in harmonize.config.json in folder root/workerActionsPlugin/MyPlugin/MyPlugin.dll

done!