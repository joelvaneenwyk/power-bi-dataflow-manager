# DataFlow Emergency Brake
Durable Function that utilizes Power BI REST API's to query, monitor and manage DataFlows

Cancels all DataFlows in the event of a DataFlow error or non-responsive DataFlow (non-responsive as defined as the config value "FailureTimeOutInMinutes")


Configuration Keys:

GroupId = Power BI Workspace that the user has access to 
(https://docs.microsoft.com/en-us/rest/api/power-bi/groups)

SvcUser = Azure User Account with requisite permission (DataFlow.ReadWriteAll)

Password = Password for Azure User Account

ClientId = Azure App Registration ClientId  
(https://docs.microsoft.com/en-us/power-bi/developer/embedded/register-app?tabs=customers%2CAzure)

ClientSecret = Azure App Registration Secret
