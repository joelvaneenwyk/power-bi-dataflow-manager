# DataFlow Emergency Brake (aka Circuit Breaker)
Durable Function that utilizes Power BI REST API's to Monitor and Manage DataFlows

1) Periodically check status of any or all Dataflows in a given workspace (Polling interval is configurable)
2) Cancels all DataFlows in the event of a DataFlow error or non-responsive DataFlow (non-responsive as defined as the config value "FailureTimeOutInMinutes")
3) Retry (if configured) restart the cancelled Dataflows


## Configuration Keys:

GroupId = Power BI Workspace that the user has access to [Documentation](https://docs.microsoft.com/en-us/rest/api/power-bi/groups)

SvcUser = Azure User Account with requisite permission (DataFlow.ReadWriteAll)

Password = Password for Azure User Account

ClientId = ClientId value of [Azure App Registration](https://docs.microsoft.com/en-us/power-bi/developer/embedded/register-app?tabs=customers%2CAzure)

ClientSecret = Azure App Registration Secret

## How it works

Monitor Dataflows via Power BI REST APIs

<div style="width: 50%; height: 50%">
  
![CircuitBreakerMonitor](https://user-images.githubusercontent.com/84995595/176929712-0d4d446b-c079-4c18-a8c3-fcf972f263f5.png)

</div>

## How it works

![CircuitBreakerMonitor](https://user-images.githubusercontent.com/84995595/176929712-0d4d446b-c079-4c18-a8c3-fcf972f263f5.png)
