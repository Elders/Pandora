Pandora aims to externalize the application configuration. Usually in .NET projects the configuration is done in app/web.config with transformations.

The problem arises when production configuration is needed which should not be part of the application repository because it is an OSS project for example.
This is where Pandora comes. You can configure the application using the following structure and store these files in a separate repository:

`ReferenceConfiguration.json`
```
{
    "name": "ReferenceConfiguration",
    "defaults": {
        "refSetting0": "refSetting0",
        "refSetting1": "refSetting1",
        "refSetting2": "refSetting2"
    },
    "clusters": {
        "local": {
            "refSetting0": "int_refSetting0",
            "refSetting1": "int_refSetting1",
            "refSetting2": "int_refSetting2"
        },
        "test": {
            "refSetting0": "test_refSetting0",
            "refSetting1": "test_refSetting1",
            "refSetting2": "test_refSetting2"
        }
    },
    "machines": {
        "COMPUTERNAME": {
            "refSetting0": "machine_refSetting0"
        }
    }
}
```

`SampleConfiguration.json`
```
{
    "name": "SampleConfiguration",
    "references": [
        { "jar": "ReferenceConfiguration.json" }
    ],
    "defaults": {
        "setting0": "setting0",
        "setting1": "setting1",
        "setting2": "setting2"
    },
    "clusters": {
        "local": {
            "setting0": "int_setting0",
            "setting1": "int_setting1",
            "setting2": "int_setting2"
        },
        "test": {
            "setting0": "test_setting0",
            "setting1": "test_setting1",
            "setting2": "test_setting2"
        }
    }
    "machines": {
        "COMPUTERNAME": {
            "cluster": "local"
            "setting0": "int_setting0",
            "setting1": "int_setting1",
            "setting2": "int_setting2"
        }
    }
}
```

- jar file: this is the file containing configuration. In this case `SampleConfiguration.json`
- name: The application name. The recommended approach is to name the jar file with the same name
- defaults: defines configuration keys for the current application and sets default values
- clusters: defines different environment configurations where the application will be deployed. Here `local` is the dev environment and `test` is the place for QA
- machines: defines configurations for specific machine inside a cluster. The COMPUTERNAME is the default ENV VAR in windows.
- references: merges two jar files. Same rules apply here. You are not allowed to have the duplicate keys in ref and main jars

In order to install the configurations you will have to use Pandora.Cli. By default the configurations are applied and stored in environment variables.

