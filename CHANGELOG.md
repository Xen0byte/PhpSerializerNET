# 2.0.2 (Future)
- Fixed a bug where integers with a plus sign where deserialized incorrectly
  `i:+1;` was deserialized as `49`.

# 2.0.1 (2024-11-18)

## Bugfixes
- Fixed validation error when deserializing a list of objects. The deserializer would check the wrong token for it's 
  datatype and throw and exception like this:
  `Can not deserialize array at position [x] to list: It has a non-integer key 'name' at element [y]` 
  [GH #40](https://github.com/StringEpsilon/PhpSerializerNET/issues/40)
- Related to the above: Some nested arrays or arrays with object values would never implicetly deserialize into a        
  `List<object>` because the check if the array keys are consecutive integers was faulty.
- Do not throw when a property or field is decorated with it's own name such as `[PhpProperty["A"]] public int A;`.

# 2.0.0 (2024-11-13)

## Breaking
- Now targets .NET 8.0 and .NET 9.0
- `PhpTokenizer` class is now internal.
- Removed support for `net6.0` and `net7.0`.
- The default implicit type for numeric values is now `int` instead of `long`
  1.x: `PhpSerialization.Deserialize("i:42;") == 42L` 
  2.x: `PhpSerialization.Deserialize("i:42;") == 42` 
- Changed the signature of `[PhpPropery(long)]` to `[PhpPropery(long)]` to align with the above change.

## Features
- Added `PhpSerialization.DeserializeUtf8(ReadOnlySpan<byte>)` overloads for cases in which consumers directly work with
  UTF8 inputs and can skip the re-encoding.
- Added `PhpSerializationFilter` attribute base class, allowing customization of class and struct member serialization.
  See the `PhpSerializationFilterTest` for an example. See also [Issue #33](https://github.com/StringEpsilon/PhpSerializerNET/issues/33).

## Regular changes
- Integers and doubles without a value now give a better error message (`i:;` and `d:;`).

## Performance
- Reduced time to decode / re-encode the input string.
- Reduced memory allocations both in the input re-encoding and the deserialization.
- Delay the materialization of strings when deserializing. This can avoid string allocations entirely for integers,
  doubles and booleans. 
- Improved performance for implicit deserialization of integers as well as minor improvements for implicit 
  deserialization of arrays.
- Improved serialization performance for strings, integers, `IList<T>`, `ExpandoObject`, Dictionaries and `PhpDynamicObject`

## Internal
Split the deserialization into 3 phases:
  1. Validation of the input and counting of the data tokens.
  2. Parsing of the input into tokens
  3. Deserializations of the tokens into the target C# objects/structs.

In version 1.4 and prior, this was a 2 step process. This is slightly slower on some inputs, but overall a little 
neater because we're cleanly separating the tasks. 

# 1.4.0
- Now targets .NET 6.0, 7.0 and 8.0
- Improved tokenization performance by allowing and forcing more aggresive inlining.
  - In my benchmark, this is about 8 to 9% faster

# 1.3.0
- Removed net5 support and added net7 support

# 1.2.0
- Added overload of `[PhpProperty()]` that accepts integer / long keys. See [#32](https://github.com/StringEpsilon/PhpSerializerNET/issues/32)
- Allow deserialization of Objects with integer keys

# 1.1.0
- Made type information caches thread safe.
- Added support for PhpProperty on enums, allowing consumers to specify different field names
- Performance: Cache enum field information with `TypeCacheFlag.PropertyInfo`.

# 1.0.0

This is just 0.11.0 packaged as a new version to mark it as stable.

# 0.11.0

**Deserialization:**
- Added `string Serialize(object? input, PhpSerializiationOptions? options = null)` to `PhpSerialization` so the target type can be specified at run time.
- `PhpSerialization` (entry point of the library) is now null reference aware, aiding library consumers in caching `NullReferenceException`.
- `PhpSerialization` throws `ArgumentOutOfRangeException` instead of the more generalised `ArgumentException`
- Bugfix: "INF" and "-INF" would not be handled correctly when using explicit typing (`Deserialize<T>`) for some target types.
- Bugfix: Properly set classname when deserializing with explicit types that implement IPhpObject.
- Bugfix: With the AllowExcessKeys, the deserialization of the given struct or object would abort when an excess key was encountered, leaving the properties after the excess key unassigned. See issue [#27](https://github.com/StringEpsilon/PhpSerializerNET/issues/27).
- Performance tweaks:
	- Minor improvements on memory use during deserialization.
	- Improved performance for deserializing Double and Integer values with explicit types.

**General:**
* Bugfix: `PhpSerialization.ClearTypeCache()` was not exposed.
* Bugfix: `PhpSerialization.ClearPropertyInfoCache()` was not exposed.

# 0.10.0:

## Breaking:
- Trying to set the classname on PhpDateTime will throw an exception now instead of doing nothing.
- Behavior of the option `EmptyStringToDefault` changed:
	- And empty string will now result in `default(string)` (which is null) instead of an empty string.
	- For some target types, the return value might have changed due to better checks for the proper default value.

**Beware**: EmptyStringToDefault is enabled by default.

## Regular changes:

**Deserialization:**
- Added support for `Nullable<>`
- Added `PhpSerializerNET.ClearTypeCache()`
- Added `TypeCache` deserialization option
	- Allows to *disable* the classname type cache. (enabled by default)
	- Allows to *enable* a property information cache. (disabled by default)
- Added `PhpSerializerNET.ClearPropertyInfoCache()`
- When converting to an enum member that is not known a better exception is thrown instead of a nullref (because the fieldinfo cannot be found)
- Added support for arrays

**Serialization:**
- Added support for serializing `PhpDynamicObject` and `ExpandoObject`.
- Always serialize implementations of `IPhpObject` using object notation.
	**This is technically a breaking change**, but it was always intended to work that way.

# 0.9.0:

## Breaking:
- Targeting net6.0

## Semi breaking:
- Type lookup: Now negative lookup results are also cached.
	- This may also lead to undeseriable results when adding classes and structs at runtime.
	- May increase the memory footprint over time faster than before.
	- On the upside: It is significantly faster when dealing with objects where automapping doesn't work without having to disable the feature entirely.
- Different exception (System.ArgumentException) on empty input for `PhpSerialization.Deserialize<T>()`

## Regular changes

- Rewrote the parsing and validation logic, which results in different exception messages in many cases.
- Parsing: A very slight performance gain for some deserialization workloads.
- Object / struct creation: Improved performance.
- General: Reduced amount of memory allocated while deserializing.
- Fixed exception message for non-integer keys in lists.
- Fixed exception message for failed field / property assignments / binding.

# 0.8.0:
- Improved performance of the validation step of deserialization.
- Sped up deserializing into explicit types (particularly structs and classes) significantly.
- Sped up serialization, especially when using attribute annotated classes and structs.
- Improved exception messages on malformed inputs when deserializing.
- Cleaner exception when trying to deserialize into incompatible types (i.e. "a" to int32.).

# 0.7.4:
- Improved deserialization performance.
- Fixed invalid output when using PhpSerializiationOptions.NumericEnums = false

# 0.7.3:
- Fixed an issue with empty string deserialization, caused by the `EmptyStringToDefault` code in 0.7.2.

# 0.7.2:
- Added `EmptyStringToDefault` deserialization option, defaults to true.
	- When true, empty strings will be deserialized into the default value of the target IConvertible.
	  For example `s:0:"";` deserialized to an integer yields `0`.
	See issue #13 for details.
- Fixed a regression introduced in 0.7.1 where some data would no longer parse correctly (#12) due to improper handling of array brackets.

# 0.7.1:
- Fixed issue with nested array / object validation (issue #11)
- Added support for System.Guid (issue #10)

# 0.7.0:
- Support de/serialization of enums
- Added serialization option `NumericEnums`:
	Whether or not to serialize enums as integer values
	Defaults to true. If set to false, the enum.ToString() representation will be used.

# 0.6.0:

- Allow more (valid) characters in object class names.
- Added public interface IPhpObject
- Added public class PhpObjectDictionary (implementing IPhpObject).
	- This replaces `IDictionary<string, object>` as the default deserialization target of objects.
- Added public class PhpDynamicObject (implementing IPhpObject)
- Added PhpDateTime to avoid conflicts with System.DateTime.

With IPhpObjects, you can get the class name specified in the serialized data via `GetClassName()`.

**Known issues:**
- Can not deserialize dynamic objects.

# 0.5.1

- Fixed misleading exception message on malformed objects.
- Fixed valid classnames being rejected as malformed.
- Fixed type-lookup logic trying to deserialize with `null` Type information.

Known issues:
- ~~Objects with classname `DateTime` will fail to deserialize, unless the option `EnableTypeLookup` is set to `false`.~~ (fixed since)

# 0.5.0

**BREAKING**
- Renamed the static class `PhpSerializer` to `PhpSerialization`

Other changes:
- Added support for object de/serialization (`O:4:"name":n:{...}`).
- Added `[PhpClass()]` attribute.
- Added `StdClass` and `EnableTypeLookup` to deserialization options
- Added options for `PhpSerialization.Serialize()`.
	- `ThrowOnCircularReferences` - whether or not to throw on circular references, defaults to false (this might change in the future.)
- Updated and adjusted some of the XML documentation.

# 0.4.0

- Support for structs.
- Fixed performance drop due to over-checking the input string.
- Refactored deserializer to work in less steps and with cleaner code.
- Slight tweaks of some error messages.

# 0.3.0

- Added InputEncoding option.
- Added ability to deserialize into `List<MyClass>` specifically
	- Currently only works when also setting UseList = Never
- Fixed a big issue with nested arrays stepping over keys.
- Added tests.

# 0.2.0

- Added option to convert strings "1" and "0" into bools when deserializing an object.
- Changed how validation is handled and moved validation out of the tokenization step.
- Added `[PhpIgnore]` and `[PhpProperty("name")]`

# 0.1.0

- Initial release.