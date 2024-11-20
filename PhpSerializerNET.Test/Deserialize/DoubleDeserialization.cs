

/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using PhpSerializerNET.Test.Deserialize.Options;
using Xunit;

namespace PhpSerializerNET.Test.Deserialize;

public class DoubleDeserializationTest {

	[Theory]
	[InlineData("d:+1;", 1.0)]
	[InlineData("d:-1;", -1.0)]
	[InlineData("d:+1.23456789;", 1.23456789)]
	[InlineData("d:-1.23456789;", -1.23456789)]
	[InlineData("d:-1.7976931348623157E+308;", double.MinValue)]
	[InlineData("d:-1.7976931348623157e+308;", double.MinValue)]
	[InlineData("d:1.7976931348623157E+308;", double.MaxValue)]
	[InlineData("d:1.7976931348623157e+308;", double.MaxValue)]
	[InlineData("d:NAN;", double.NaN)]
	[InlineData("d:INF;", double.PositiveInfinity)]
	[InlineData("d:-INF;", double.NegativeInfinity)]
	public void DeserializesDoubles(string input, double expected) {
		Assert.Equal(
			expected,
			PhpSerialization.Deserialize<double>(input)
		);
		Assert.Equal(
			expected,
			PhpSerialization.Deserialize(input)
		);
	}

	[Fact]
	public void ExplicetlyDeserializesToInteger() {
		double number = PhpSerialization.Deserialize<double>("d:10;");
		Assert.Equal(10, number);
	}

}