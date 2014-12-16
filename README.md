## App configuration for .NET. Strong-typed, JSON file-based, host-independent, and manageable.

[![Build Status](https://ci.appveyor.com/api/projects/status/github/jonsequitur/Its.Configuration)](https://ci.appveyor.com/project/jonsequitur/its-configuration)

Most bugs in deployed services arise from configuration errors. Configuration in .NET usually consists of code calling ConfigurationManager.AppSettings or RoleEnvironment.GetConfigurationSettingValue to get a keyed value from an XML file, and casting it to an expected type. This tends to fail at a number of points: missing values, unconvertable values, non-obvious default behaviors. Managing configurations for diverse environments tends to amplify these problems. And of course Azure has a different configuration mechanism from vanilla ASP.NET applications.

### Goals

Its.Configuration tries to address a number of common configuration pain points to simplify configuration and make it more robust.

- Management
  - Reuse configurations across environments or operational modes
  - Keep groups of configuration values together that belong together

- Robustness and strong-typing
  - Create settings classes that can guarantee their own internal consistency 
  - Settings classes are plain old C# objects that can define meaningful defaults, expose behavior rather than data, and have unit test coverage.

- Security
  - Encrypt settings that need to be protected
  - Separate secrets from keys
  - Remove the need for your application to know how to decrypt protected settings

- Consistency between Azure and IIS
  - A single set of configuration files that works regardless of whether you're hosted in a cloud service using .cscfg-based configuration or in vanilla ASP.NET using web.config.

- Testability
  - Pass settings objects, not primitive values
  - Establish known-good combinations of configurations during integration tests so that if you need to reconfigure a deployed environment, you can change it to a configuration you've already tested

### Overview

#### A settings class

The starting point is to take an object-oriented approach to configuration by defining classes for the settings you need:

```csharp
    public class AzureStorageSettings
    {
        public AzureStorageSettings()
        {
            NumberOfConnectionRetries = 3;
        }

        public string BuildConnectionString()
        {
            return string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", AccountName, AccountKey);
        }

        public string AccountName { get; set; }

        public string AccountKey { get; set; }

        public int NumberOfConnectionRetries { get; set; }
    }
```

Since `AccountName` and `AccountKey` go together, this approach is bit more organized than having (for example) two separate keys and values in `web.config`. It also allows us to add behavior such as the `BuildConnectionString` method.   

#### The settings values

The standard setup for your configurations is in a set of files in your project under a folder called `.config`. 

```
<project root>
|
└───\.config
    |
    └───\internal
        |
        └───AzureStorageSettings.json
    |
    └───\local
    |
    └───\production
        |
        └───AzureStorageSettings.json
    |
    └───\test    
```

These files should be copied to the project output so that they will be included in your deployment. 

The names of these folders are up to you. You can use them to define categories across which some of your configurations might differ, for example environments, data centers, operating modes, testing stages, etc. 

The `.json` files contain JSON that will be deserialized into an instance of your settings class. 

```json
    {
        "AccountName":"myaccount",
        "AccountKey":"ikDE8Xi5CupwkjQyeQud3kltGv8AHVfU6/Nlqe30t=="
    }
```

In this example, you can see that `NumberOfConnectionRetries` is not set, allowing the class default to be used.

To access these settings in your code, simply call:

```csharp
    AzureStorageSettings settings = Settings.Get<AzureStorageSettings>();
```

This will look for a file called `AzureStoreSettings.json` (or `AzureStoreSettings.json.secure`) and deserialize its contents into an instance of `AzureStoreSettings`. 

#### Precedence

You'll notice in the screen shot above that there are several folders containing files having the same name. For example, `DiagnosticSettings.json` is found in both the local and production folders. Likewise, there are both `local\AuthenticationSettings.json` and `production\AuthenticationSettings.json.secure`, which both deserialize to the `AuthenticationSettings` class.

The decision of which to use is made based on a configurable settings precedence. In the above example, the precedence used by developers on their local machine would be `local|internal`. This indicates that settings should be looked up first in the `local` folder, then, if not found there, in the `internal` folder. If more than one matching file exists in this lookup path, the first one takes precedence and others are ignored. 

The settings precedence can be set several different ways: 

Programmatically:

```csharp
    Settings.Precedence = new[] { "local", "internal" };
```

In web.config or app.config:

```xml
    <appSettings>
        <add key="Its.Configuration.Settings.Precedence" value="local|internal" />
```

In Azure configuration (`.cscfg`):

```xml
    <ConfigurationSettings>
      <Setting name="Its.Configuration.Settings.Precedence" value="local|internal" />
```

Using an environment variable:

```
    c:\>set Its.Configuration.Settings.Precedence=local|internal
```

So if more than one of these approaches is used, what's _their_ precedence? It's as follows:

1. Programmatic
2. Environment variable
3. `.cscfg`
4. `web.config` / `app.config`

In practice, for an Azure-deployed web application, this means we set the precedence for local development in web.config, for deployment as an Azure cloud service in the ```.cscfg```, and for deployment to Azure Web Sites using an environment variable set via the Azure Management Portal.

By changing the precedence setting, you can switch to an entirely different configuration with a single change. Want to debug your production topology locally? Switch the precedence to "production" on your local machine.

#### Azure

As you may have inferred, Its.Configuration is able to read Azure configuration settings. The precedence setting is read using the following method:

```csharp
    var setting = Settings.AppSetting("some-key");
```

You can use this method to directly look up a value from the following settings sources. It will return the first one that matches, in this order:

1. Environment variable
2. `.cscfg`
3. `web.config` / `app.config`

#### Security

If a JSON file's contents are encrypted and `.secure` is appended to the filename, then `Settings.Get<T>()` will transparently decrypt the contents in order to deserialize your settings class. It uses the `System.Security.Cryptography.Pkcs.EnvelopedCms` class. Any certificate found in the local machine/personal certificate store is a candidate. You do not need to specify a certificate, as `EnvelopedCms` handles that.  

The Its.Configuration command line tool can be used to encrypt and decrypt files. This is mainly a convenience. You can also use `EnvelopedCms` via PowerShell, or use the `Encrypt` and `Decrypt` methods found in `Its.Configuration.CryptographyExtensions`.

