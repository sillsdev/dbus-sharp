using System;
using NUnit.Framework;
using NDesk.DBus;
using System.Collections.Generic;

namespace NDesk.DBusTests
{
	[TestFixture()]
	public class SignatureTests
	{
		[Test()]
		public void VariousSignatureTypes()
		{
			// Single Types.
			TestTypes("y", new Type[] { typeof(byte) });
			TestTypes("b", new Type[] { typeof(bool) });
			TestTypes("n", new Type[] { typeof(short) });
			TestTypes("q", new Type[] { typeof(ushort) });
			TestTypes("i", new Type[] { typeof(int) });
			TestTypes("u", new Type[] { typeof(uint) });
			TestTypes("x", new Type[] { typeof(long) });
			TestTypes("t", new Type[] { typeof(ulong) });
			TestTypes("d", new Type[] { typeof(double) });
			TestTypes("s", new Type[] { typeof(string) });
			TestTypes("o", new Type[] { typeof(ObjectPath) });
			TestTypes("v", new Type[] { typeof(object) });

			// Simple Array types.
			TestTypes("ai", new Type[] { typeof(int[]) });
			TestTypes("ao", new Type[] { typeof(ObjectPath[]) });
			TestTypes("as", new Type[] { typeof(string[]) });

			//Simple Stucts
			TestTypes("(ios(ii))", new Type[] { typeof(object) });
			TestTypes("a(ss)", new Type[] { typeof(object[]) });

			// Dictionaries
			TestTypes("a{sv}", new Type[] { typeof(IDictionary<string, object>) });
			TestTypes("a{sa{ss}}", new Type[] { typeof(IDictionary<string, IDictionary<string, string>>) });
		}

		public void TestTypes(string sig, Type[] expectedTypes)
		{
			var s = new Signature(sig);
			var types = s.ToTypes();

			if (types.Length > expectedTypes.Length)
				Assert.Fail("Got more types than expected {0}", sig);

			if (expectedTypes.Length > types.Length)
				Assert.Fail("Didn't all expected types. {0}", sig);

			for(int i = 0 ; i < types.Length; i++)
			{
				Assert.AreEqual(expectedTypes[i], types[i], String.Format("{0} => {1}", sig, i));
			}


			Assert.AreEqual(expectedTypes, types);
		}
	}
}
