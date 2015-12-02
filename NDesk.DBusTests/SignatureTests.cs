using System;
using NUnit.Framework;
using NDesk.DBus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NDesk.DBusTests
{
	[TestFixture]
	public class SignatureTests
	{
		[SetUp]
		public void SetUp()
		{
			TypeDefiner.dynamicTypeCount = 0;
			TypeDefiner.asmBdef = null;
			TypeDefiner.modBdef = null;
		}

		[TestCase("", ExpectedResult = new Type[0])]

		// Single Types.
		[TestCase("y", ExpectedResult = new[] { typeof(byte) })]
		[TestCase("b", ExpectedResult = new[] { typeof(bool) })]
		[TestCase("n", ExpectedResult = new[] { typeof(short) })]
		[TestCase("q", ExpectedResult = new[] { typeof(ushort) })]
		[TestCase("i", ExpectedResult = new[] { typeof(int) })]
		[TestCase("u", ExpectedResult = new[] { typeof(uint) })]
		[TestCase("x", ExpectedResult = new[] { typeof(long) })]
		[TestCase("t", ExpectedResult = new[] { typeof(ulong) })]
		[TestCase("d", ExpectedResult = new[] { typeof(double) })]
		[TestCase("s", ExpectedResult = new[] { typeof(string) })]
		[TestCase("o", ExpectedResult = new[] { typeof(ObjectPath) })]
		[TestCase("v", ExpectedResult = new[] { typeof(object) })]

		// Simple Array types.
		[TestCase("ai", ExpectedResult = new[] { typeof(int[]) })]
		[TestCase("ao", ExpectedResult = new[] { typeof(ObjectPath[]) })]
		[TestCase("as", ExpectedResult = new[] { typeof(string[]) })]

		// Dictionaries
		[TestCase("a{sv}", ExpectedResult = new[] { typeof(IDictionary<string, object>) })]
		[TestCase("a{sa{ss}}", ExpectedResult = new[] { typeof(IDictionary<string, IDictionary<string, string>>) })]

		// Multiple types
		[TestCase("iii", ExpectedResult = new[] { typeof(int), typeof(int), typeof(int) })]
		public Type[] ToTypes(string sig)
		{
			var s = new Signature(sig);
			return s.ToTypes();
		}

		[TestCase("a(ss)", ExpectedResult = new[] { "DynamicType0[]" })]
		[TestCase("(ios(ii))", ExpectedResult = new[] { "DynamicType0" })]
		public string[] ToTypes_DynamicTypes(string sig)
		{
			var s = new Signature(sig);
			return s.ToTypes().Select(t => t.Name).ToArray();
		}

		[TestCase(DType.Invalid, ExpectedResult = typeof(void))]
		[TestCase(DType.Byte, ExpectedResult = typeof(byte))]
		[TestCase(DType.Boolean, ExpectedResult = typeof(bool))]
		[TestCase(DType.Int16, ExpectedResult = typeof(short))]
		[TestCase(DType.UInt16, ExpectedResult = typeof(ushort))]
		[TestCase(DType.Int32, ExpectedResult = typeof(int))]
		[TestCase(DType.UInt32, ExpectedResult = typeof(uint))]
		[TestCase(DType.Int64, ExpectedResult = typeof(long))]
		[TestCase(DType.UInt64, ExpectedResult = typeof(ulong))]
		[TestCase(DType.Single, ExpectedResult = typeof(float))]
		[TestCase(DType.Double, ExpectedResult = typeof(double))]
		[TestCase(DType.String, ExpectedResult = typeof(string))]
		[TestCase(DType.ObjectPath, ExpectedResult = typeof(ObjectPath))]
		[TestCase(DType.Signature, ExpectedResult = typeof(Signature))]
		[TestCase(DType.Struct, ExpectedResult = typeof(ValueType))]
		[TestCase(DType.DictEntry, ExpectedResult = typeof(KeyValuePair<,>))]
		[TestCase(DType.Variant, ExpectedResult = typeof(object))]
		public Type ToType(DType dType)
		{
			var sut = new Signature(dType);
			return sut.ToType();
		}

		[Test]
		public void ToType_Invalid()
		{
			var sut = new Signature("(sv");
			Assert.That(() => sut.ToType(), Throws.Exception);
		}

		[Test]
		public void ToType_NullData()
		{
			var sut = new Signature();
			Assert.That(sut.ToType(), Is.EqualTo(typeof(void)));
		}

		[TestCase("(sv)", ExpectedResult = new[] { "String", "Object" })]
		[TestCase("(sa{sv}saa(s)s)",
			ExpectedResult = new[] { "String", "IDictionary`2", "String", "DynamicType1[][]", "String" })]

		// Testcases from spec (http://dbus.freedesktop.org/doc/dbus-specification.html#container-types)
		[TestCase("(i(ii))", ExpectedResult = new[] { "Int32", "DynamicType1"})]
		[TestCase("((ii)i)", ExpectedResult = new[] { "DynamicType1", "Int32"})]
		[TestCase("(iii)", ExpectedResult = new[] { "Int32", "Int32", "Int32"})]
		public string[] ToType_Struct(string signature)
		{
			var sut = new Signature(signature);
			var type = sut.ToType();

			Assert.That(type.Name, Is.EqualTo("DynamicType0"));
			Assert.That(type.IsSubclassOf(typeof(DValue)), Is.True);
			return type.GetFields().Select(f => f.FieldType.Name).ToArray();
		}

		[TestCase("as", ExpectedResult = typeof(string[]))]
		[TestCase("aas", ExpectedResult = typeof(string[][]))]
		[TestCase("a{sv}", ExpectedResult = typeof(IDictionary<string, object>))]
		public Type ToType_Array(string signature)
		{
			var sut = new Signature(signature);
			return sut.ToType();
		}

		[Test]
		public void ToType_ArrayOfStructs()
		{
			var sut = new Signature("a(sv)");
			var type = sut.ToType();

			Assert.That(type.IsArray, Is.True);
			Assert.That(type.GetElementType().Name, Is.EqualTo("DynamicType0"));
			Assert.That(type.GetElementType().IsSubclassOf(typeof(DValue)), Is.True);
		}

		[Test]
		public void GetFieldSignatures()
		{
			var sig = new Signature("(ios(ii))");
			var ret = sig.GetFieldSignatures();

			Assert.That(ret, Is.EqualTo(new[] { new Signature(DType.Int32),
				new Signature(DType.ObjectPath), new Signature(DType.String),
				new Signature("(ii)")}));
		}

		[Test]
		public void GetFieldSignatures_Invalid()
		{
			var sig = new Signature("s");
			Assert.That(() => sig.GetFieldSignatures().ToArray(),
				Throws.Exception.With.Message.EqualTo("Not a struct"));
		}

		[TestCase(typeof(void), ExpectedResult = DType.Invalid)]
		[TestCase(typeof(byte), ExpectedResult = DType.Byte)]
		[TestCase(typeof(bool), ExpectedResult = DType.Boolean)]
		[TestCase(typeof(Int16), ExpectedResult = DType.Int16)]
		[TestCase(typeof(UInt16), ExpectedResult = DType.UInt16)]
		[TestCase(typeof(Int32), ExpectedResult = DType.Int32)]
		[TestCase(typeof(UInt32), ExpectedResult = DType.UInt32)]
		[TestCase(typeof(Int64), ExpectedResult = DType.Int64)]
		[TestCase(typeof(UInt64), ExpectedResult = DType.UInt64)]
		[TestCase(typeof(Single), ExpectedResult = DType.Single)]
		[TestCase(typeof(Double), ExpectedResult = DType.Double)]
		[TestCase(typeof(string), ExpectedResult = DType.String)]
		[TestCase(typeof(ObjectPath), ExpectedResult = DType.ObjectPath)]
		[TestCase(typeof(Signature), ExpectedResult = DType.Signature)]
		[TestCase(typeof(object[]), ExpectedResult = DType.Array)]
		[TestCase(typeof(string[]), ExpectedResult = DType.Array)]
		[TestCase(typeof(object), ExpectedResult = DType.Variant)]
		[TestCase(typeof(MarshalByRefObject), ExpectedResult = DType.Struct)]
		[TestCase(typeof(Dictionary<,>), ExpectedResult = DType.DictEntry)]
		public DType TypeToDType(Type t)
		{
			return Signature.TypeToDType(t);
		}
	}
}
