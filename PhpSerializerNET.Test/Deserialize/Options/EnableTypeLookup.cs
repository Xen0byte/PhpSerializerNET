/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using Xunit;
using PhpSerializerNET.Test.DataTypes;
using System.Diagnostics;

namespace PhpSerializerNET.Test.Deserialize.Options;

public class EnableTypeLookupTest {
	[Fact]
	public void Enabled_Finds_Class() {
		var result = PhpSerialization.Deserialize(
			"O:11:\"MappedClass\":2:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";}",
			new PhpDeserializationOptions() { EnableTypeLookup = true }
		);

		Assert.IsType<MappedClass>(result);

		// Check that everything was deserialized onto the properties:
		var mappedClass = result as MappedClass;
		Assert.Equal("Hello World!", mappedClass.English);
		Assert.Equal("Hallo Welt!", mappedClass.German);
	}

	[Fact]
	public void CachingWorks() {
		PhpSerialization.ClearTypeCache();
		PhpSerialization.ClearPropertyInfoCache();
		var options = new PhpDeserializationOptions() {
			EnableTypeLookup = true,
			TypeCache = TypeCacheFlag.ClassNames | TypeCacheFlag.PropertyInfo
		};
		var stopWatch = new Stopwatch();
		stopWatch.Start();
		var result = PhpSerialization.Deserialize(
			"O:11:\"MappedClass\":2:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";}",
			options
		);
		stopWatch.Stop();
		long uncachedTime = stopWatch.ElapsedTicks;

		stopWatch.Reset();
		stopWatch.Start();
		result = PhpSerialization.Deserialize(
			"O:11:\"MappedClass\":2:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";}",
			options
		);
		stopWatch.Stop();
		long cachedTime = stopWatch.ElapsedTicks;

		Assert.True(uncachedTime / cachedTime  > 100);

		PhpSerialization.ClearTypeCache();
		PhpSerialization.ClearPropertyInfoCache();
		stopWatch.Reset();
		stopWatch.Start();
		result = PhpSerialization.Deserialize(
			"O:11:\"MappedClass\":2:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";}",
			options
		);
		stopWatch.Stop();
		long secondUncachedTime = stopWatch.ElapsedTicks;
		Assert.True(secondUncachedTime / cachedTime  > 100);
	}

	[Fact]
	public void Disabled_UseStdClass() {
		var result = PhpSerialization.Deserialize(
			"O:11:\"MappedClass\":2:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";}",
			new PhpDeserializationOptions() {
				EnableTypeLookup = false,
				StdClass = StdClassOption.Dictionary,
			}
		);

		Assert.IsType<PhpObjectDictionary>(result);

		// Check that everything was deserialized onto the properties:
		var dictionary = result as PhpObjectDictionary;
		Assert.Equal("Hello World!", dictionary["en"]);
		Assert.Equal("Hallo Welt!", dictionary["de"]);
	}

}