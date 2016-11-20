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
