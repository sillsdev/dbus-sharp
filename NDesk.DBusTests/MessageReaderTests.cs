using NUnit.Framework;
using System;
using NDesk.DBus;
using System.Collections.Generic;

namespace NDesk.DBusTests
{
	[TestFixture()]
	public class MessageReaderTests
	{
		[Test()]
		public void DummyHeaderWithNoBody_DataIsEmpry()
		{
			var transport = new FakeTransport(TestByteArrays.Header);
			var message = transport.ReadMessage();

			var mr = new MessageReader(message);
			Assert.AreEqual(0, mr.data.Length);
		}

		[Test()]
		public void ReadInt16()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT_257;
			Assert.AreEqual(257, mr.ReadInt16());
		}

		[Test()]
		public void ReadInt32()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT_257;
			Assert.AreEqual(257, mr.ReadInt32());
		}

		[Test()]
		public void ReadInt64()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT64_257;
			Assert.AreEqual(257, mr.ReadInt64());
		}

		[Test()]
		public void ReadString()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.STRING_C;
			Assert.AreEqual("C", mr.ReadString());
		}

		[Test()]
		public void ReadObjectPath()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.STRING_C;
			Assert.AreEqual("C", mr.ReadObjectPath().Value);
		}

		[Test()]
		public void ReadSignature_Simple()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.SIG_s;
			Assert.AreEqual(new Signature("s"), mr.ReadSignature());
		}

		[Test()]
		public void ReadSignature_Complex()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.SIG_DICT_os;
			Assert.AreEqual(new Signature("a{os}"), mr.ReadSignature());
		}

		[Test()]
		public void ReadVariant()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.VAR_STRING_C;
			Assert.AreEqual("C", mr.ReadVariant());
		}

		[Test()]
		public void ReadVariant_VariantIsAComplexObject_ObjectOfCorrectTypeReturned()
		{
			var mr = new MessageReader(new Message());
			// a{os}
			mr.data = TestByteArrays.VAR_DICT_os;
			var obj = mr.ReadVariant();
			var dictionary = (IDictionary<ObjectPath, string>)obj;
			Assert.AreEqual("D", dictionary[new ObjectPath("C")]);
		}

		[Test()]
		public void ReadValue_object_VariantTypeIsReadAndReturnedAsObject()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.VAR_STRING_C;
			var obj = mr.ReadValue(typeof(object));
			Assert.AreEqual("C", obj);
			Assert.AreEqual(typeof(string), obj.GetType());
		}

		[Test()]
		public void ReadValue_DictionaryKObjectPathVString_DictionaryOfExpectedTypeIsReturnedAndContainsExpectedValues()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.DICT_1_ss;
			var obj = mr.ReadValue(typeof(IDictionary<ObjectPath, string>));
			Assert.AreEqual(typeof(Dictionary<ObjectPath, string>), obj.GetType());
			var dictionary = (IDictionary<ObjectPath, string>)obj;
			Assert.AreEqual("D", dictionary[new ObjectPath("C")]);
		}

		[Test()]
		public void ReadValue_DictionaryKStringVString_DictionaryOfExpectedTypeIsReturnedAndContainsExpectedValues()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.DICT_1_ss;
			var obj = mr.ReadValue(typeof(IDictionary<string, string>));
			Assert.AreEqual(typeof(Dictionary<string, string>), obj.GetType());
			var dictionary = (IDictionary<string, string>)obj;
			Assert.AreEqual("D", dictionary["C"]);
		}

		[Test()]
		public void ReadValue_DictionaryKObjectPathVStringMultipleEntries_DictionaryOfExpectedTypeIsReturnedAndContainsExpectedValues()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.DICT_2_ss;
			var obj = mr.ReadValue(typeof(IDictionary<ObjectPath, string>));
			Assert.AreEqual(typeof(Dictionary<ObjectPath, string>), obj.GetType());
			var dictionary = (IDictionary<ObjectPath, string>)obj;
			Assert.AreEqual(2, dictionary.Count);
			Assert.AreEqual("D", dictionary[new ObjectPath("C")]);
		}
	}
}
