/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using PhpSerializerNET.Test.DataTypes;

namespace PhpSerializerNET.Test.Deserialize.Options {
	public class EmptyStringToDefaultTest {
		private const string EmptyPhpStringInput = "s:0:\"\";";

		#region Enabled

		[Fact]
		public void Enabled_EmptyStringToInt() {
			var result = PhpSerialization.Deserialize<int>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_EmptyStringToLong() {
			var result = PhpSerialization.Deserialize<long>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToDouble() {
			var result = PhpSerialization.Deserialize<double>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToFloat() {
			var result = PhpSerialization.Deserialize<float>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToDecimal() {
			var result = PhpSerialization.Deserialize<decimal>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToBool() {
			var result = PhpSerialization.Deserialize<bool>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToChar() {
			var result = PhpSerialization.Deserialize<char>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToEnum() {
			var result = PhpSerialization.Deserialize<IntEnum>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToGuid() {
			var result = PhpSerialization.Deserialize<Guid>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToString() {
			var result = PhpSerialization.Deserialize<string>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToObject() {
			var result = PhpSerialization.Deserialize<object>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToCustomObject() {
			var deserializedObject = PhpSerialization.Deserialize<SimpleClass>(
				"a:5:{s:7:\"AString\";s:0:\"\";s:9:\"AnInteger\";i:10;s:7:\"ADouble\";d:1.2345;s:4:\"True\";b:1;s:5:\"False\";b:0;}"
			);

			Assert.Equal(default, deserializedObject.AString);
		}

		[Fact]
		public void Enabled_StringArrayToCharCollection() {
			var result = PhpSerialization.Deserialize<List<char>>("a:2:{i:0;s:0:\"\";i:1;s:0:\"\";}");
			Assert.Equal(new List<char> { default, default }, result);
		}

		#region Nullables

		[Fact]
		public void Enabled_EmptyStringToIntNullable() {
			var result = PhpSerialization.Deserialize<int?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_EmptyStringToLongNullable() {
			var result = PhpSerialization.Deserialize<long?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToDoubleNullable() {
			var result = PhpSerialization.Deserialize<double?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToFloatNullable() {
			var result = PhpSerialization.Deserialize<float?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToDecimalNullable() {
			var result = PhpSerialization.Deserialize<decimal?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToBoolNullable() {
			var result = PhpSerialization.Deserialize<bool?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToCharNullable() {
			var result = PhpSerialization.Deserialize<char?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToEnumNullable() {
			var result = PhpSerialization.Deserialize<IntEnum?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		[Fact]
		public void Enabled_StringToGuidNullable() {
			var result = PhpSerialization.Deserialize<Guid?>(EmptyPhpStringInput);
			Assert.Equal(default, result);
		}

		#endregion

		[Fact]
		public void Enabled_StringArrayToIntList() {
			var result = PhpSerialization.Deserialize<List<int>>("a:1:{i:0;s:0:\"\";}");
			Assert.Equal(new List<int> { default }, result);
		}

		[Fact]
		public void Enabled_StringArrayToNullableIntList() {
			var result = PhpSerialization.Deserialize<List<int?>>("a:1:{i:1;s:0:\"\";}");
			Assert.Equal(new List<int?> { default }, result);
		}

		[Fact]
		public void Enabled_ClassArrayToClassDictionary() {
			var result = PhpSerialization.Deserialize<Dictionary<string, SimpleClass>>(
				"a:1:{s:4:\"AKey\";a:5:{s:7:\"AString\";s:0:\"\";s:9:\"AnInteger\";i:0;s:7:\"ADouble\";d:0;s:4:\"True\";b:0;s:5:\"False\";b:0;}}"
			);

			var expected = new Dictionary<string, SimpleClass>
			{
				{
					"AKey",
					new SimpleClass
					{
						ADouble = default,
						AString = default,
						AnInteger = default,
						False = default,
						True = default
					}

				}
			};

			// No easy way to assert dicts in MsTest :/

			Assert.Equal(expected.Count, result.Count);

			foreach (var ((expectedKey, expectedValue), (actualKey, actualValue)) in expected.Zip(result)) {
				Assert.Equal(expectedKey, actualKey);
				Assert.Equal(expectedValue.ADouble, actualValue.ADouble);
				Assert.Equal(expectedValue.AString, actualValue.AString);
				Assert.Equal(expectedValue.AnInteger, actualValue.AnInteger);
				Assert.Equal(expectedValue.False, actualValue.False);
				Assert.Equal(expectedValue.True, actualValue.True);
			}
		}

		#endregion

		#region Disabled

		[Fact]
		public void Disabled_EmptyStringToInt() {
			var exception = Assert.Throws<DeserializationException>(
				() => PhpSerialization.Deserialize<int>(EmptyPhpStringInput, new PhpDeserializationOptions { EmptyStringToDefault = false })
			);

			Assert.Equal(
				"Exception encountered while trying to assign '' to type Int32. See inner exception for details.",
				exception.Message
			);
		}

		[Fact]
		public void Disabled_StringToBool() {
			var exception = Assert.Throws<DeserializationException>(
				() => PhpSerialization.Deserialize<bool>(EmptyPhpStringInput, new PhpDeserializationOptions { EmptyStringToDefault = false })
			);

			Assert.Equal(
				"Exception encountered while trying to assign '' to type Boolean. See inner exception for details.",
				exception.Message
			);
		}

		[Fact]
		public void Disabled_StringToDouble() {
			var exception = Assert.Throws<DeserializationException>(
				() => PhpSerialization.Deserialize<double>(EmptyPhpStringInput, new PhpDeserializationOptions { EmptyStringToDefault = false })
			);

			Assert.Equal(
				"Exception encountered while trying to assign '' to type Double. See inner exception for details.",
				exception.Message
			);
		}

		#endregion
	}
}