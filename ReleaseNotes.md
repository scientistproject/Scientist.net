### New in 2.1.0

**Features**

- Added targeting for .NET Standard 2.0, .NET 6 and .NET 8

**Fixes**

-  `Scientist.Enabled()` now affects all instances by default, not just the shared static instance. - [#129](https://github.com/scientistproject/Scientist.net/pull/129)
-  Resolve deadlock issue on async experiments when caller uses sync over async - [#131](https://github.com/scientistproject/Scientist.net/pull/131)

### New in 2.0.0 (Released 2018/06/05)

**Features**

 - Make IoC/DI friendlier - [#108](https://github.com/scientistproject/Scientist.net/pull/108) via @martincostello
 - Add `FireAndForgetResultPublisher`. Wrap an existing `IResultPublisher` to delegate publishing to another thread and avoid publishing delays when running experiments - [#83](https://github.com/scientistproject/Scientist.net/pull/83) via @thematthopkins and @joncloud

### New in 1.0.1 (Released 2016/09/29)

Initial stable release of Scientist.NET, a port of the Ruby Scientist library for carefully refactoring critical paths.

### New in 1.0.0-alpha6 (Released 2016/06/10)

**Features**

 - Move to .NET Core RC2 - [#61](https://github.com/scientistproject/Scientist.net/pull/61) via @joncloud
 - Add `ThrowOnMismatches` - [#53](https://github.com/scientistproject/Scientist.net/pull/53) via @joncloud
 - Add `Thrown` - [#56](https://github.com/scientistproject/Scientist.net/pull/56) via @joncloud
 - Cleaned up internals - [#58](https://github.com/scientistproject/Scientist.net/pull/58) via @joncloud

### New in 1.0.0-alpha5 (Released 2016/04/14)

**Features**

 - Move to .NET Core and target .NET 4.5+ - [#37](https://github.com/scientistproject/Scientist.net/pull/37) via @davezych
 - Add `AddContext` - [#48](https://github.com/scientistproject/Scientist.net/pull/48) via @davezych
 - Add `Ignore` - [#47](https://github.com/scientistproject/Scientist.net/pull/47) via @davezych
 - Add ability to configure multiple `Try` methods - [#45](https://github.com/scientistproject/Scientist.net/pull/45) via @davezych
 - Add support for RunIf - [#33](https://github.com/scientistproject/Scientist.net/pull/33) via @joncloud

### New in 1.0.0-alpha4 (Released 2016/02/26)
* Initial release
