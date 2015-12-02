// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using System.Linq;
using NUnit.Framework;
using NDesk.DBus;

namespace NDesk.DBusTests
{
	[TestFixture]
	public class TypeDefinerTests
	{
		[SetUp]
		public void SetUp()
		{
			TypeDefiner.dynamicTypeCount = 0;
			TypeDefiner.asmBdef = null;
			TypeDefiner.modBdef = null;
		}

		[TestCase("(s)", new[] { "String" })]
		[TestCase("(si)", new[] { "String", "Int32" })]
		[TestCase("(si(s))", new[] { "String", "Int32", "DynamicType1" })]
		[TestCase("((si)s)", new[] { "DynamicType1", "String" })]
		public void CreateStructType(string signature, string[] fieldTypes)
		{
			// Setup
			var sig = new Signature(signature);

			// Execute
			var t = TypeDefiner.CreateStructType(sig);

			// Verify
			Assert.That(t.IsSubclassOf(typeof(DValue)), Is.True);
			var fields = t.GetFields();
			Assert.That(fields.Select(t1 => t1.FieldType.Name), Is.EqualTo(fieldTypes));
		}
	}
}
