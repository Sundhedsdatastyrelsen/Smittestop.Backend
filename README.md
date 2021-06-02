# Smittestop.Backend
Backend part of Smitte|stop solution for Covid-19 spread tracking

WebAPI project, which facilitates receiving, storing and distributing Temporary Exposure Keys<sup>1</sup>.

Mobile devices can use these keys to detect exposures and send notifications to the users.
To do that, the devices utilize Exposure Notification API<sup>2</sup> provided by Apple and Google.


## Installation

Installation process goes in the same way as with any other WebAPI project.
For example, you could use `Web Deploy` feature of IIS but other methods work fine as well.


Keep in mind that routes to all endpoints already have `api` prefix in them.
_Remove the suggested `api` from `Application Path` when importing in order to not have this prefix duplicated_.

### Requirements
To run the project you need the `ASP.NET Core Runtime`.
Any version starting with `3.1` should work fine i.e., `3.1.10`.
Choose `Hosting Bundle` when choosing a runtime version.

The project unfortunately has a couple of Windows dependencies and because of that, **you need to run it on Windows**.

Windows dependencies
- It uses Data Protection API<sup>3</sup> for encryption.
- It runs Hangfire<sup>4</sup> as a Windows service.
- It writes logs to Windows EventLog<sup>5</sup>, if configured to do so.

### Registering EventLog
To initiate EventLog for settings mentioned above you can run this PowerShell script with administrator privileges:

```
$file = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\EventLogMessages.dll"
New-EventLog -ComputerName . -Source "SS-API-DIGNDB.App.SmitteStop" -LogName Application -MessageResourceFile $file -CategoryResourceFile $file
New-EventLog -ComputerName . -Source "SS-Jobs-DIGNDB.App.SmitteStop" -LogName Application -MessageResourceFile $file -CategoryResourceFile $file
```
**Without this initialization no logs will appear in the Event Viewer.**

### Hangfire as a service
To create a service, which will run Hangfire execute the command below (in Command Prompt).

```
sc create "SmittestopHangfire" binpath="C:\SmittestopHangFire\DIGNDB.APP.SmitteStop.Jobs.exe"
```

### Creating cleaning SQL job
To ensure that keys older than 14 days won't clutter the database,
run the SQL command from this file [Tools/Scripts/create-cleaning-sql-job.sql](Tools/Scripts/create-cleaning-sql-job.sql).

## Usage

To use this `WebAPI` project you need an HTTP Client.
Both mobile devices and frontend frameworks can use some HTTP Client to consume the API.

### Swagger
To describe API endpoints, the project makes use of Swagger.

Request on root endpoint `/` will result in getting `Swagger UI` page.


Swagger along with the documentation also provides a functionality called _playground_.
Click `Try it out` and then `Execute` to send an HTTP request without a need for any HTTP client.


_For security reasons, this functionality works only in `development` environments_.

### Hangfire

#### Jobs

- `ValidateDatabaseJob` - This job is used to remove all invalid records from database.
- `UpdateZipFilesJob` - This job is used to generate new zip files with exposure keys.
- `RemoveOldZipFilesJob` - This job is used to remove zip files older than 14 days. It is performed to avoid overfilling drive with key data.
- `ProcessSSIDataInFolder` - This job is used to generate covid statistics using zip files uploaded by trifork.

### Configuration

The project uses the standard `ASP.NET Core` mechanism for splitting configuration for different environments.
To get yourself familiar with the concept take a look at this article [Use multiple environments in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-3.1).

#### Mobile token

Some endpoints require a _mobile token_ because of `MobileAuthorizationAttribute` attribute.
This token provides static authentication mechanism between the backend and the mobile app.

This kind of token had infinite expiration time and its value is the same on each device. Due to infinite time of validity this token must be provided for functionalities PullExposureKeys and Logging Mobile. Endpoints using this functionality have the [AuthorizationMobile] attribute which validates token value from header Authorization_Mobile. Before processing a request this attribute ensures that the token is valid and otherwise returns 401 unauthorized.

You need to provide this token to `appsettings.json` configuration for all environments apart from `development`.

appsettings.json

```
{
  "AppSettings": {
    "AuthorizationMobile": "To be replaced in Pipelines"
  }
}
```

To generate this token just pick a random string (treat it like a password, the longer and higher the entropy the better), share it with the mobile team, encrypt it using `ConfigEncryptionHelper.ProtectString` and put it as `AuthorizationMobile` value.
This method `ProtectString` uses `Data Protection API` under the hood, so you need to **repeat this encryption for each server**.

**If the API application runs as `apppool` user then this user needs to perform the encryption (call to `ProtectString`).**
**If user A performs the encryption and user B will try to decrypt the token then the decryption will fail.**

#### JWT validation
Endpoint `POST /diagnostickeys` uses JWT token to authenticate the client.

You can configure some JWT validation rules using the configuration below.

appsettings.json
```
{
  "JwtAuthorization" : {
    "JwtValidationRules": {
      "ClientId": "To be replaced in Pipelines",
      "SupportedAlgorithm": "RS256",
      "Issuer": "To be replaced in Pipelines"
    },
    "JwkUrl": "To be replaced in Pipelines"
  }
}
```
* `ClientId` - Client id from the token, which we consider valid.
* `SupportedAlgorithm` - Supported signature algorithm, which we consider valid.
* `Issuer` - Issuer from the token, which we consider valid.
* `JwkUrl` - Url from which the validator service will retrieve _the public key_.

#### API version deprecation
`AppSettings` section of `appsettings.json` configuration enables setting a specific version of API as deprecated.

To deprecate a specific API version use standard dotnet json configuration.
Use example below for reference.

.appsettings.json
```
{
  "AppSettings": {
    "DeprecatedVersions": [
      "//1",
      "//Remove `//` from line above to make version `1` deprecated."
    ]
  }
}

```
To make version 1 of the API deprecated just change //1 to 1. To add another deprecated version add another string to this DeprecatedVersions array.
Calling an endpoint in deprecated version will result in getting a response with the code `410` and content `API is deprecated`.

#### Logging configuration
The project uses different logging solutions when it comes to backend logs and mobile logs.

##### Backend logs
Backend uses solution provided by the framework, described in
[Logging in .NET Core and ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0).
`Startup` class calls `AddFile` extension method to also save logs to a file.

##### Mobile logs
Application running on devices pushes its logs using `/logging/logMessages` endpoint.
`LoggingController` receives those logs and saves them using `log4net` package.
This package uses `log4net.config` configuration file.

## Contributing

### Unused code

Don't feel surprised to find some portions of unused code.
As an example, you won't find any logical usages of `Translation` table or whole `FederationGatewayApi` project.
Development team removed the code using it because the project should not integrate with
[EU Federation Gateway Service](https://github.com/eu-federation-gateway-service/efgs-federation-gateway) for now.

### Patterns used in the project

#### Generic repository

To access the database please use `GenericRepository<T>` class.
Feel free to create a custom repository class based on the generic one if needed.

#### Dependency registration

Each module should have its dependencies registered in a separate extension method.

This pattern provides a number of benefits.

- It keeps all the registration calls in one place per module.
- It enables marking some implementation classes as internal (encapsulation).

### Database connection
To develop the project you need a working `SQL Server` instance.
You can either use a local instance or a `Docker` container.

#### Entity Framework Code First
The project utilizes `Code First` with Migrations approach when using `Entity Framework` package.

Please pay attention when running `dotnet ef` commands.
The database context lays in different project than the `API` so you need to specify the context project each time.

## License
Copyright (c) 2021 Agency for Digitisation (Denmark), 2021 the Danish Health and Medicines Authority, 2021 Netcompany Group AS

Smitte|stop is Open Source software released under the link:LICENSE[MIT license]

----
- <sup>1</sup>[Temporary Exposure Key (TEK) Publishing Guide](https://google.github.io/exposure-notifications-server/getting-started/publishing-temporary-exposure-keys.html)
- <sup>2</sup>[Exposure Notifications API documentation](https://developers.google.com/android/exposure-notifications/exposure-notifications-api)
- <sup>3</sup>[Data Protection API](https://en.wikipedia.org/wiki/Data_Protection_API)
- <sup>4</sup>[Hangfire website](https://www.hangfire.io/)
- <sup>5</sup>[Windows EventLog](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-5.0#welog)
