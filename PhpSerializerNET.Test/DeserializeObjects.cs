/**
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
**/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhpSerializerNET.Test.DataTypes;

namespace PhpSerializerNET.Test {
	[TestClass]
	public partial class DeserializeObjects {
		public class CircularTest {
			public string Foo { get; set; }
			public CircularTest Bar { get; set; }
		}

		[TestMethod]
		public void DeserializesObjectWithMappingInfo() {
			var deserializedObject = PhpSerialization.Deserialize<MappedClass>(
				"a:3:{s:2:\"en\";s:12:\"Hello World!\";s:2:\"de\";s:11:\"Hallo Welt!\";s:2:\"it\";s:11:\"Ciao mondo!\";}"
			);

			// en and de mapped to differently named property:
			Assert.AreEqual("Hello World!", deserializedObject.English);
			Assert.AreEqual("Hallo Welt!", deserializedObject.German);
			// "it" correctly ignored:
			Assert.AreEqual(null, deserializedObject.it);
		}



		[TestMethod]

		public void Test_Issue11() {
			var deserializedObject = PhpSerialization.Deserialize(
				"a:1:{i:0;a:7:{s:1:\"A\";N;s:1:\"B\";N;s:1:\"C\";s:1:\"C\";s:5:\"odSdr\";i:1;s:1:\"D\";d:1;s:1:\"E\";N;s:1:\"F\";a:3:{s:1:\"X\";i:8;s:1:\"Y\";N;s:1:\"Z\";N;}}}"
			);
			Assert.IsNotNull(deserializedObject);
		}

		[TestMethod]
		public void AssignsGuids() {
			var result = PhpSerialization.Deserialize<MappedClass>(
				"a:1:{s:4:\"guid\";s:36:\"82e2ebf0-43e6-4c10-82cf-57d60383a6be\";}"
			);
			Assert.AreEqual(
				new Guid("82e2ebf0-43e6-4c10-82cf-57d60383a6be"),
				result.guid
			);
		}

		[TestMethod]
		public void DeserializeList() {
			var result = PhpSerialization.Deserialize<List<String>>("a:3:{i:0;s:5:\"Hello\";i:1;s:5:\"World\";i:2;i:12345;}");

			Assert.AreEqual(3, result.Count);
			Assert.AreEqual("Hello", result[0]);
			Assert.AreEqual("World", result[1]);
			Assert.AreEqual("12345", result[2]);
		}

		[TestMethod]
		public void DeserializeNestedObject() {
			var result = PhpSerialization.Deserialize<CircularTest>("a:2:{s:3:\"Foo\";s:5:\"First\";s:3:\"Bar\";a:2:{s:3:\"Foo\";s:6:\"Second\";s:3:\"Bar\";N;}}");

			Assert.AreEqual(
				"First",
				result.Foo
			);
			Assert.IsNotNull(
				result.Bar
			);
			Assert.AreEqual(
				"Second",
				result.Bar.Foo
			);
		}

		[TestMethod]
		public void Test_Issue12() {
			var result = PhpSerialization.Deserialize("a:1:{i:0;a:4:{s:1:\"A\";s:2:\"63\";s:1:\"B\";a:2:{i:558710;s:1:\"2\";i:558709;s:1:\"2\";}s:1:\"C\";s:2:\"71\";s:1:\"G\";a:3:{s:1:\"x\";s:6:\"446368\";s:1:\"y\";s:1:\"0\";s:1:\"z\";s:5:\"1.029\";}}}");
			Assert.IsNotNull(result);
		}
	}
}
