
/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using Xunit;
using System.Text;

namespace PhpSerializerNET.Test.Deserialize;

public class DeserializeUtf8Test {
	[Fact]
	public void DeserializesCorrectly() {
		Assert.Equal("👻", PhpSerialization.DeserializeUtf8("s:4:\"👻\";"u8));
		Assert.Equal("👻", PhpSerialization.DeserializeUtf8<string>("s:4:\"👻\";"u8));
		Assert.Equal("👻", PhpSerialization.DeserializeUtf8("s:4:\"👻\";"u8, typeof(string)));
	}

	[Fact]
	public void InvalidEncodingThrows() {
		var invalidOptions = new PhpDeserializationOptions() { InputEncoding = Encoding.Default };
		var exception = Assert.Throws<ArgumentException>(
			() => PhpSerialization.DeserializeUtf8<string>( "s:1:\"0\";"u8, invalidOptions)
		);
		Assert.Equal("Can not use input encoding other than UTF8 (Parameter 'options')", exception.Message);

		exception = Assert.Throws<ArgumentException>(
			() => PhpSerialization.DeserializeUtf8( "s:1:\"0\";"u8, typeof(string), invalidOptions)
		);
		Assert.Equal("Can not use input encoding other than UTF8 (Parameter 'options')", exception.Message);

		exception = Assert.Throws<ArgumentException>(
			() => PhpSerialization.DeserializeUtf8( "s:1:\"0\";"u8, invalidOptions)
		);
		Assert.Equal("Can not use input encoding other than UTF8 (Parameter 'options')", exception.Message);
	}
}