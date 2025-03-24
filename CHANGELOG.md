## [4.0.1](https://github.com/Elders/Pandora/compare/v4.0.0...v4.0.1) (2025-03-24)


### Bug Fixes

* Do not throw an error when prevent all configs from loading when parsing a broken json config ([6cdcae3](https://github.com/Elders/Pandora/commit/6cdcae36cadc543425b6318acc918f6f77e94cc9))

# [4.0.0](https://github.com/Elders/Pandora/compare/v3.3.0...v4.0.0) (2025-03-10)

# [3.3.0](https://github.com/Elders/Pandora/compare/v3.2.1...v3.3.0) (2022-12-16)


### Features

* parse nested json objects and arrays as settings ([26ed94a](https://github.com/Elders/Pandora/commit/26ed94a9e365a66bfba002e41826688869859691))

# [3.3.0-preview.1](https://github.com/Elders/Pandora/compare/v3.2.1...v3.3.0-preview.1) (2022-12-16)


### Features

* parse nested json objects and arrays as settings ([26ed94a](https://github.com/Elders/Pandora/commit/26ed94a9e365a66bfba002e41826688869859691))

## [3.2.1](https://github.com/Elders/Pandora/compare/v3.2.0...v3.2.1) (2022-08-16)


### Bug Fixes

* pipeline update ([fb01163](https://github.com/Elders/Pandora/commit/fb01163b41e3f1f1c5d4dce267347d515ad48458))

# [3.2.0](https://github.com/Elders/Pandora/compare/v3.1.1...v3.2.0) (2022-07-06)


### Features

* Support for JSON array setting values ([8e70c02](https://github.com/Elders/Pandora/commit/8e70c0294ef03e626dc8f34a755366ea3bfde496))

## [3.1.1](https://github.com/Elders/Pandora/compare/v3.1.0...v3.1.1) (2022-04-23)


### Bug Fixes

* Notifies the change to the ConfigurationProvider. This will trigger the reload of the ConfigurationProvider. ([7fde3d3](https://github.com/Elders/Pandora/commit/7fde3d3f60c349235d52c879a8ade4dc00113a2b))

# [3.1.0](https://github.com/Elders/Pandora/compare/v3.0.1...v3.1.0) (2022-03-23)


### Features

* Allows pandora configurations to control when a reload should occur using the IPandoraWatcher ([c8ee685](https://github.com/Elders/Pandora/commit/c8ee68576472fccaf3c5bd7b9a286a5a8ba0ad66))

## [3.0.1](https://github.com/Elders/Pandora/compare/v3.0.0...v3.0.1) (2021-12-09)


### Bug Fixes

* Trigger pipeline ([e5b151e](https://github.com/Elders/Pandora/commit/e5b151ebb8b81e59c63d0abf3afefe586b639f03))

#### 3.0.0 - 03.12.2021
* Updates to dotnet 6

#### 2.0.2 - 31.08.2020
* Rolls back to Newtonsoft.Json because System.Text.Json dictionary support is not on a professional level

#### 2.0.1 - 17.08.2020
* Fixes a deserialization error

#### 2.0.0 - 26.03.2020
* Upgrades to netcore3.1
* Removes LibLog
* Makes the configuration key to be case insensitive
* Removes Newtonsoft.Json
* The Pandora context is now required when getting the entire configuration from a configuration repository

#### 1.1.0 - 10.12.2018
* Updates to DNC 2.2

#### 1.0.2 - 30.11.2018
* Logs a FATAL instead of crashing badly when the configuration source which provides the settings crashes

#### 1.0.1 - 29.11.2018
* Adds logging

#### 1.0.0 - 14.11.2018
* Registers EnvVars for Application, Cluster and Machine name
* Introduces new environment variables: `pandora_cluster`, `pandora_application`, `pandora_machine`. The old environment variables `CLUSTER_NAME` and `COMPUTERNAME` are still valid
* PandoraConfigurationProvider now loads only the settings relevant to the provided context instead of all settings because you may have multiple application running side by side
* Adds support for dotnet core configuration provider/source
* Fixes to the Environment variables calls
* Adds ability to get cluster key from configuration
* Ability to instanciate Pandora via IPandoraFactory
* Starts targeting NetStandard2.0

#### 0.9.0 - 26.09.2017
* Added TryGet method

#### 0.8.2 - 20.06.2017
* Fix configuration merging
* Update Newtonsoft to 10.0.3

#### 0.8.1 - 05.06.2017
* Downgrades Newtonsoft to 9.0.1

#### 0.8.0 - 01.06.2017
* Pandora raw keys will always have a machine name. If it is a cluster key the machine name is '*'
* Adds a parser for a DeployedSetting

#### 0.7.2 - 23.02.2017
* Fix cluster comparison

#### 0.7.1 - 22.02.2017
* Remove forgotten hard coded path

#### 0.7.0 - 22.02.2017
* Add support for complex values

#### 0.7.0-beta0001 - 21.02.2017
* Add support for complex values

#### 0.6.9 - 21.11.2016
* Makes cluster and machine case insensitive while opening the jar

#### 0.6.8 - 21.11.2016
* New logo

#### 0.6.7 - 21.11.2016
* Versioned Pandora

#### 0.6.7 - 21.11.2016
* Versioned Pandora

#### 0.6.5 - 20.11.2016
* Introduces IPAndoraContext

#### 0.6.4 - 16.11.2016
* Fixes runtime error when building ApplicationContext

#### 0.6.3 - 16.11.2016
* Fixes backwards compatibility issues

#### 0.6.2 - 16.11.2016
* Fixed ApplicationContext.CreateContext: machine and cluster to transform to lower on context create

#### 0.6.1 - 16.11.2016
* Added optional params for ApplicationContext.CreateContext: machine and cluster

#### 0.6.0 - 16.11.2016
* Changed Get<T> and added GetAll() for getting all keys in the storage
* Removes `UserRawSetting` option
* Adds support for Uniquie Cluster Keys

#### 0.5.4 - 04.11.2016
* Adds the ability to set settings for other contexts using Pandora.Set(...)

#### 0.5.3 - 04.11.2016
* Adds the ability to set settings using Pandora.Set(...)

#### 0.5.2 - 03.11.2016
* Adds Exists(...) method for IConfigurationRepository

#### 0.5.1 - 02.11.2016
* Exposes current ApplicationContext for Pandora

#### 0.5.0 - 01.11.2016
* Pandora.Open(...) is moved as an extention method
* Adds IConfigurationRepository to abstract the configuration storage. By default WindowsEnvironmentVariables repository is used

#### 0.4.2 - 22.07.2016
* vNext

#### 0.4.1 - 22.07.2016
* Update Nyx build script

#### 0.4.0 - 03.02.2016
* Added PandoraOptions and exposed references in Pandora.Box

#### 0.4.0-beta0001 - 25.01.2016
* Added PandoraOptions and exposed references in Pandora.Box

#### 0.3.0 - 29.05.2015
* Remove the Cli.

#### 0.2.1 - 25.05.2015
* Properly merge two boxes

#### 0.2.0 - 10.05.2015
* New feature: You can now reference configurations

#### 0.1.18 - 09.05.2015
* Fix minor issues with the CLI

#### 0.1.17 - 27.04.2015
* Fix assembly attribute for copyright

#### 0.1.16 - 27.04.2015
* Completely rewrite the CLI

#### 0.1.15 - 07.04.2015
* Add overload for ApplicationConfiguration.Get<T>(key,context)

#### 0.1.14 - 07.04.2015
* Make setting keys case insensitive

#### 0.1.13 - 07.04.2015
* Added 'cluster' setting key to the reserved settings collection

#### 0.1.12 - 07.04.2015
* Added optional cluster configuration for machines

#### 0.1.11 - 18.02.2015
* Exit if the jar file is not found

#### 0.1.10 - 18.02.2015
* Allow to specify json config file

#### 0.1.9 - 08.02.2015
* Generic T Get<T>(string key)

#### 0.1.8 - 13.12.2014
* Get all machine settings represented as DeployedSetting

#### 0.1.7 - 17.11.2014
* Improve getting settings from env vars using a static class

#### 0.1.6 - 17.11.2014
* Fix how we get the computer name

#### 0.1.5 - 17.11.2014
* Fix how the box is converted from a jar

#### 0.1.4 - 14.11.2014
* Added ApplicationConfiguration for clients to get setting value by key

#### 0.1.3 - 14.11.2014
* Added NameBuilder for files and setting key names

#### 0.1.2 - 14.11.2014
* Fix misspelled name in the exe file

#### 0.1.1 - 14.11.2014
* Fix misspelled name

#### 0.1.0 - 13.11.2014
* Initial release for testing
