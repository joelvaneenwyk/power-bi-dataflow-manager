# DataFlow Emergency Brake (aka Circuit Breaker)

Durable Function that utilizes Power BI REST API's to Monitor and Manage DataFlows

1) Periodically check status of any or all Dataflows in a given workspace (Polling interval is configurable)
2) Cancels all DataFlows in the event of a DataFlow error or non-responsive DataFlow (non-responsive as defined as the config value "FailureTimeOutInMinutes")
3) Retry (if configured) restart the cancelled Dataflows

##### More Info on blog at [ThePrestonVerse](https://theprestonverse.com/2022/07/01/power-bi-dataflow-monitor/)

## Configuration Keys:

#### Function Specific Configurations
FailureTimeOutInMinutes = (int) The amount of time a Dataflow can run before it is considered a hanging process or a silent failure

PollingIntervalInMinutes - (int) The amount of time for the process to wait in between requests

MonitorTImeInMinutes = (int) The entire amount of time for this process to run

RestartCancelledDataFlow = (bool) If true Restarts a Dataflow tha the process cancels

#### Integration Configurations
GroupId = Power BI Workspace that the user has access to [Documentation](https://docs.microsoft.com/en-us/rest/api/power-bi/groups)

SvcUser = Azure User Account with requisite permission (DataFlow.ReadWriteAll)

Password = Password for Azure User Account

ClientId = ClientId value of [Azure App Registration](https://docs.microsoft.com/en-us/power-bi/developer/embedded/register-app?tabs=customers%2CAzure)

ClientSecret = Azure App Registration Secret


## Process

Monitor Dataflows via Power BI REST APIs. 

[Durable Function Monitor](https://docs.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview?tabs=csharp#monitoring)

[Power BI Dataflow APIs](https://docs.microsoft.com/en-us/rest/api/power-bi/dataflows)




![CircuitBreakerMonitor](https://user-images.githubusercontent.com/84995595/176934730-41a11b33-08bf-4d9c-8388-9e82a8bccc41.png)


