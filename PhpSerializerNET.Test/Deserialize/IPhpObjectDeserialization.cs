/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/
using Xunit;
using PhpSerializerNET.Test.DataTypes;

namespace PhpSerializerNET.Test;

public class IPhpObjectDeserializationTest {

	[Fact]
	public void DeerializesIPhpObject() { // #Issue 25
		var result = PhpSerialization.Deserialize<MyPhpObject>("O:11:\"MyPhpObject\":1:{s:3:\"Foo\";s:0:\"\";}");
		Assert.Equal( // strings:
			"MyPhpObject",
			result.GetClassName()
		);
	}

	[Fact]
	public void DeerializesPhpObjectDictionary() {
		var result = PhpSerialization.Deserialize<PhpObjectDictionary>("O:11:\"MyPhpObject\":1:{s:3:\"Foo\";s:0:\"\";}");
		Assert.Equal(
			"MyPhpObject",
			result.GetClassName()
		);
	}

	[Fact]
	public void DeerializesIPhpObjectStruct() {
		var result = PhpSerialization.Deserialize<IPhpObjectStruct>("O:11:\"MyPhpObject\":1:{s:3:\"Foo\";s:0:\"\";}");
		Assert.Equal(
			"MyPhpObject",
			result.GetClassName()
		);
	}
	[Fact]
	public void DeserializesIPhpObjectStruct_Implicit() {
		var result = PhpSerialization.Deserialize("O:11:\"MyPhpObject\":1:{s:3:\"Foo\";s:0:\"\";}") as IPhpObject;
		Assert.Equal(
			"MyPhpObject",
			result.GetClassName()
		);
	}
}