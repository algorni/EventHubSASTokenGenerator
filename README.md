# Event Hub SASToken Generator sample app
Just a simpler .NET Core 3.1 console app to generate a SAS Token for Event Hub given a Shared Access Key information

## Command Line Arguments

It requires 3 mandatory command line parameters plus 1 optional:

1) Hostname of the Event Hub Namespace
![Event Hub Namespace HostName!](/doc/EventHubURI.jpg)
2) The Shared Access Policy Name
3) The Primary Key of the Access Policy 
![Shared Access Policy!](/doc/SharedAccessPolicy.jpg)
4) The Validity Time in minutes from now (as _string_)

## Remarks

       This tool is *NOT* intended for a production utilization.
       This is a sample and provided as it is.

# Bonus ---> Sample HTTP Client for Event Hub 
This repository includes also a sample .net core console app project running a background task
which send multiple HTTP request to the Event Hub HTTP Endpoint with an eavy payload to test out 
the scalability of that endpoint!

You need to add an appConfig.json file with the configuration

```json 
{
  "exclude": [
    "**/bin",
    "**/bower_components",
    "**/jspm_packages",
    "**/node_modules",
    "**/obj",
    "**/platforms"
  ],
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "EHDataProducerConfig": {
    "EventHubConnectionString": "Endpoint=sb://<your event hub namespace>.servicebus.windows.net/;SharedAccessKeyName=<yourKeyName>;SharedAccessKey=<theKey>;EntityPath=<yourEventHubName>",
    "BatchSize": 10,
    "ParallelSender": 30,
    "MaxTimeBetweenSendBatchOperationInMillieconds": 50
  }
}
```


