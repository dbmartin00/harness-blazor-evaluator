* Use Feature Flags with Blazor

Blazor isn't a web assembly, so SDK alternatives must be employed.

The Harness FME Evaluator is a RESTful approach to flagging.
Feature equivalent to an SDK, you can test flags with simple
HTTP GETs.

In this sample app, a FeatureFlagService encapsulates the 
Evaluator use, presenting helper methods to the Blazor pages,
giving maximal flexibility at the page while hiding the 
HTTP usage necessary.

FeatureFlagService getFlagAsync method takes a user key,
flag name, and (optionally) a set of attributes, so all 
feature flag evaluations are possible, fast, scalable,
and secure.

To install, 

wwwroot/appsettings.json

```
{
  "FeatureFlagConfig": {
    "BaseUrl": "https://<your evaluator host>:<port>",
    "EvaluatorApiKey": "<your evaluator auth key>"  
  }
}
```

David.Martin@harness.io

