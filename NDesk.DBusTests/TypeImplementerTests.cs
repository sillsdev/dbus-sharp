using System;
using NUnit.Framework;
using NDesk.DBus;
using org.freedesktop.DBus;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NDesk.DBusTests
{
	[TestFixture]
	public class TypeImplementerTests
	{
		public interface TestObj : Introspectable
		{
			void Ping();
			int Count();
			IDictionary<string, string> GetSimpleDictionary();
			IDictionary<ObjectPath, IDictionary<string, IDictionary<string, string>>> GetComplexDictionary();
			IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>> GetComplexDictionaryWithVariant();
			string Hello();
			object ReadVariantByteArray();
			ObjectPath ReadData1();

		}

		[Test]
		public void CallMethod_NoReturn()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			var transport = new FakeTransport(TestByteArrays.Header,

			new Message[] { message });

			var instObj = CreateFakeBusObject<TestObj>(transport);

			instObj.Ping();
		}

		[Test]
		public void CallMethod_SimpleReturnReturn()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.INT_257;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			Assert.AreEqual(257, instObj.Count());
		}

		[Test]
		public void CallMethod_SimpleDictionaryReturn()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.DICT_2_ss;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });

			TestObj instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.GetSimpleDictionary();
			Assert.AreEqual(typeof(Dictionary<string, string>), obj.GetType());
			var dictionary = (Dictionary<string, string>)obj;
			Assert.AreEqual("D", dictionary["C"]);
			Assert.AreEqual("E", dictionary["D"]);
		}

		[Test]
		public void CallMethod_ComplexDictionaryReturn()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.DICT_1_sDICT_1_sDICT_1_ss;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.GetComplexDictionary();
			Assert.AreEqual(typeof(Dictionary<ObjectPath, IDictionary<string, IDictionary<string, string>>>), obj.GetType());
			var dictionary = (Dictionary<ObjectPath, IDictionary<string, IDictionary<string, string>>>)obj;
			Assert.AreEqual("F", dictionary[new ObjectPath("C")]["D"]["E"]);
		}

		[Test]
		public void CallMethod_ComplexDictionaryWithVariantReturn()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.DICT_1_sDICT_1_sDICT_1_so;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.GetComplexDictionaryWithVariant();
			Assert.AreEqual(typeof(Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>), obj.GetType());
			var dictionary = (Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>)obj;
			Assert.AreEqual("C", dictionary[new ObjectPath("C")]["D"]["E"]);
		}

		[Test]
		public void CallMethod_ComplexDictionaryWithVariantReturnUsingRealData()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			var data = File.ReadAllText("data/data.txt");
			var bytes = data.Split(',').Select(x => Byte.Parse(x)).ToArray();
			message.Body = bytes;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.GetComplexDictionaryWithVariant();
			Assert.AreEqual(typeof(Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>), obj.GetType());
			var dictionary = (Dictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>>)obj;
			Assert.AreEqual("2.1.6", dictionary[new ObjectPath("/org/freedesktop/UDisks2/Manager")]["org.freedesktop.UDisks2.Manager"]["Version"]);
		}

		[Test]
		public void CallMethod_ReturnsDBusHelloReply()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.STRING_ReturnFromDbusHello;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.Hello();
			Assert.AreEqual(":1.118", obj);
		}

		[Test]
		public void CallMethod_ReturnsRealDataObjectPath()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.RealData_ObjectPath;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.ReadData1();
			Assert.AreEqual("/org/freedesktop/IBus/InputContext_523", obj.Value);
		}

		[Test]
		public void CallMethod_ReturnsByteArray()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.VAR_ARRAY_BYTE;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.ReadVariantByteArray();
			Assert.AreEqual("/medi", Encoding.UTF8.GetString((byte[])obj));
		}

		// While this behaviour may seem unhelpful given the byte[] may not be strings we can't auto trim nulls"
		[Test()]
		public void CallMethod_ReturnsByteArrayWhichContainsNullTerm_NullTermIsNotTrimmed()
		{
			var message = new Message();
			message.Header[FieldCode.ReplySerial] = 1u;
			message.Header.MessageType = MessageType.MethodReturn;
			message.Body = TestByteArrays.VAR_ARRAY_BYTE__IncludingNullTermInArray;

			var transport = new FakeTransport(TestByteArrays.Header, new Message[] { message });
			var instObj = CreateFakeBusObject<TestObj>(transport);

			var obj = instObj.ReadVariantByteArray();
			Assert.AreEqual("/medi\0", Encoding.UTF8.GetString((byte[])obj));
		}

		#region helper methods

		static T CreateFakeBusObject<T>(FakeTransport transport)
		{
			var typeImplementer = new TypeImplementer("test", false);
			var t = typeImplementer.GetImplementation(typeof(TestObj));

			T instObj = (T)Activator.CreateInstance(t);

			BusObject inst = BusObject.GetBusObject(instObj);
			inst.conn = new Connection(transport);
			inst.bus_name = "something";
			inst.object_path = new ObjectPath("DontCare");

			return (T)instObj;
		}

		#endregion
	}
}
