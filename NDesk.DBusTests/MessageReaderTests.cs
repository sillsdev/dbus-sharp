using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using NDesk.DBus;

namespace NDesk.DBusTests
{
	[TestFixture]
	public class MessageReaderTests
	{
		[SetUp]
		public void SetUp()
		{
			TypeDefiner.dynamicTypeCount = 0;
			TypeDefiner.asmBdef = null;
			TypeDefiner.modBdef = null;
		}

		[Test]
		public void DummyHeaderWithNoBody_DataIsEmpry()
		{
			var transport = new FakeTransport(TestByteArrays.Header);
			var message = transport.ReadMessage();

			var mr = new MessageReader(message);
			Assert.AreEqual(0, mr.data.Length);
		}

		[Test]
		public void ReadInt16()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT_257;
			Assert.AreEqual(257, mr.ReadInt16());
		}

		[Test]
		public void ReadInt32()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT_257;
			Assert.AreEqual(257, mr.ReadInt32());
		}

		[Test]
		public void ReadInt64()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.INT64_257;
			Assert.AreEqual(257, mr.ReadInt64());
		}

		[Test]
		public void ReadString()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.STRING_C;
			Assert.AreEqual("C", mr.ReadString());
		}

		[Test]
		public void ReadObjectPath()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.STRING_C;
			Assert.AreEqual("C", mr.ReadObjectPath().Value);
		}

		[Test]
		public void ReadSignature_Simple()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.SIG_s;
			Assert.AreEqual(new Signature("s"), mr.ReadSignature());
		}

		[Test]
		public void ReadSignature_Complex()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.SIG_DICT_os;
			Assert.AreEqual(new Signature("a{os}"), mr.ReadSignature());
		}

		[Test]
		public void ReadVariant()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.VAR_STRING_C;
			Assert.AreEqual("C", mr.ReadVariant());
		}

		[Test]
		public void ReadVariant_VariantIsAComplexObject_ObjectOfCorrectTypeReturned()
		{
			var mr = new MessageReader(new Message());
			// a{os}
			mr.data = TestByteArrays.VAR_DICT_os;
			var obj = mr.ReadVariant();
			var dictionary = (IDictionary<ObjectPath, string>)obj;
			Assert.AreEqual("D", dictionary[new ObjectPath("C")]);
		}

		[Test]
		public void ReadValue_object_VariantTypeIsReadAndReturnedAsObject()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.VAR_STRING_C;
			var obj = mr.ReadValue(typeof(object));
			Assert.AreEqual("C", obj);
			Assert.AreEqual(typeof(string), obj.GetType());
		}

		[Test]
		public void ReadValue_DictionaryKObjectPathVString_DictionaryOfExpectedTypeIsReturnedAndContainsExpectedValues()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.DICT_1_ss;
			var obj = mr.ReadValue(typeof(IDictionary<ObjectPath, string>));
			Assert.AreEqual(typeof(Dictionary<ObjectPath, string>), obj.GetType());
			var dictionary = (IDictionary<ObjectPath, string>)obj;
			Assert.AreEqual("D", dictionary[new ObjectPath("C")]);
		}

		[Test]
		public void ReadValue_DictionaryKStringVString_DictionaryOfExpectedTypeIsReturnedAndContainsExpectedValues()
		{
			var mr = new MessageReader(new Message());
			mr.data = TestByteArrays.DICT_1_ss;
			var obj = mr.ReadValue(typeof(IDictionary<string, string>));
			Assert.AreEqual(typeof(Dictionary<string, string>), obj.GetType());
			var dictionary = (IDictionary<string, string>)obj;
			Assert.AreEqual("D", dictionary["C"]);
		}

		[Test]
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

		[Test]
		public void ReadVariant_Null()
		{
			var mr = new MessageReader(new Message());
			mr.data = new byte[] { 0, 0, 0, 0 };
			Assert.That(() => mr.ReadVariant(), Throws.Nothing);
		}

		[Test]
		public void ReadValue_StructSimple()
		{
			// Setup
			var msg = new Message();
			msg.Body = new byte[] { 3, (byte)'(', (byte)'s', (byte)')', 0, 0, 0, 0,
				/*string:*/1, 0, 0, 0, (byte)'A', 0 };
			var sut = new MessageReader(msg);

			// Execute
			var obj = sut.ReadValue(typeof(Struct));

			// Verify
			var fields = obj.GetType().GetFields();
			Assert.That(fields.Length, Is.EqualTo(1));
			Assert.That(fields[0].FieldType, Is.EqualTo(typeof(string)));
			Assert.That(fields[0].GetValue(obj), Is.EqualTo("A"));
		}

		[Test]
		public void ReadValue_StructTwoTypes()
		{
			// Setup
			var msg = new Message();
			msg.Body = new byte[] { 4, (byte)'(', (byte)'s', (byte)'i', (byte)')', 0, 0, 0,
				/*string:*/1, 0, 0, 0, (byte)'A', 0, 0, 0,
				/*int:*/15, 0, 0, 0 };
			var sut = new MessageReader(msg);

			// Execute
			var obj = sut.ReadValue(typeof(Struct));

			// Verify
			var fields = obj.GetType().GetFields();
			Assert.That(fields.Length, Is.EqualTo(2));
			Assert.That(fields[0].FieldType, Is.EqualTo(typeof(string)));
			Assert.That(fields[0].GetValue(obj), Is.EqualTo("A"));
			Assert.That(fields[1].FieldType, Is.EqualTo(typeof(int)));
			Assert.That(fields[1].GetValue(obj), Is.EqualTo(15));
		}

		[Test]
		public void ReadValue_StructNested()
		{
			// Setup
			var msg = new Message();
			msg.Body = new byte[] { 7, (byte)'(', (byte)'s', (byte)'i', (byte)'(', (byte)'s', (byte)')', (byte)')',
				0, 0, 0, 0, 0, 0, 0, 0,
				/*string:*/1, 0, 0, 0, (byte)'A', 0, 0, 0,
				/*int:*/15, 0, 0, 0, 0, 0, 0, 0,
				/*string:*/2, 0, 0, 0, (byte)'B', (byte)'c', 0};
			var sut = new MessageReader(msg);

			// Execute
			var obj = sut.ReadValue(typeof(Struct));

			// Verify
			var fields = obj.GetType().GetFields();
			Assert.That(fields.Length, Is.EqualTo(3));
			Assert.That(fields[0].FieldType, Is.EqualTo(typeof(string)));
			Assert.That(fields[0].GetValue(obj), Is.EqualTo("A"));
			Assert.That(fields[1].FieldType, Is.EqualTo(typeof(int)));
			Assert.That(fields[1].GetValue(obj), Is.EqualTo(15));
			Assert.That(fields[2].FieldType.IsSubclassOf(typeof(Struct)), Is.True);
			var subObj = fields[2].GetValue(obj);
			var subFields = subObj.GetType().GetFields();
			Assert.That(subFields[0].FieldType, Is.EqualTo(typeof(string)));
			Assert.That(subFields[0].GetValue(subObj), Is.EqualTo("Bc"));
		}
	}
}
