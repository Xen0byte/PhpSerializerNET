/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Text;
using Xunit;

namespace PhpSerializerNET.Test.Deserialize.Options {
	public class NumberStringToBoolTest {
		[Fact]
		public void Enabled_Deserializes_Implicit() {
			var options = new PhpDeserializationOptions() { NumberStringToBool = true };

			Assert.Equal(true, PhpSerialization.Deserialize("s:1:\"1\";", options));
			Assert.Equal(false, PhpSerialization.Deserialize("s:1:\"0\";", options));
			Assert.Equal("2", PhpSerialization.Deserialize("s:1:\"2\";", options));
		}

		[Fact]
		public void Enabled_Deserializes_Explicit() {
			var options = new PhpDeserializationOptions() { NumberStringToBool = true };

			Assert.True(PhpSerialization.Deserialize<bool>("s:1:\"1\";", options));
			Assert.False(PhpSerialization.Deserialize<bool>("s:1:\"0\";", options));
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

		[Fact]
		public void Disabled_Throws() {
			var exception = Assert.Throws<DeserializationException>(
				() => PhpSerialization.Deserialize<bool>(
					"s:1:\"0\";",
					new PhpDeserializationOptions() { NumberStringToBool = false }
				)
			);

			Assert.Equal(
				"Exception encountered while trying to assign '0' to type Boolean. See inner exception for details.",
				exception.Message
			);
		}
	}
}