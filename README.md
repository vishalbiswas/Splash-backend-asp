# Splash
This repository hosts the backend ASP.NET webservices required to run a
private forum server. This backend is meant to be used in conjunction
with two other projects: [Splash Android Frontend](https://www.github.com/vishalbiswas/splash)
and [Splash backend database](https://www.github.com/vishalbiswas/splash-backend-database)

This repository should be the last of the three steps to set it up.

## Requirements
* .NET Core 1.1

## Prequisities
You will need to have a configuration file in the folder [Splash-backend](Splash-backend).
The file should be named `appsettings.json`. There should a x509 certificate file named
`certificate.pfx` in the same folder. The `appsettings.json` file should contain:

1) The connection string of the database to connect to.
2) Password of the SSL certificate file certificate.pfx

Example `appsettings.json`:
```json
{
  "connectionStrings": {
    "splashConString": "Data Source=VISHAL-PC\\MSSQL;Initial Catalog=splash;Integrated Security=True;"
  },
  "certificatePassword": "SomePassword"
}
```

## Setup
1) Clone this repo. `git clone https://www.github.com/vishalbiswas/splash-backend-asp`
2) Open a Command Prompt or Terminal and run `dotnet restore`
3) Then `dotnet run`

Using the `dotnet` command requires that you have it in your path variable. 
