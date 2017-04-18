# configuration-and-monitoring
Sample project for using Azure Service Fabric framework.

The project exposes a RESTful API to submit configuration and consume monitoring information.
The main purpose of the App is to bridge the need to update/change lgacy application for cloud best practices by having an Adapter hiding the bridging implementation.

The application has the following components:
1. Web Frontend Service - Stateless Service exposing WCF Service for REST API
2. Store Service - Stateful Service exposing RPC based communication, the service is responsible for interacting with the NoSQL Database.
3. Adapter Actor - Actor Service, the service get notified whenever a new configuration submitted and as a response to the notification parse the new onfiguration and prepares config files, env variables, call APIs and etc. in order to update the real Application.
