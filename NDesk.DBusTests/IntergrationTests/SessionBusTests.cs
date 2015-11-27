using System;
using NUnit.Framework;
using NDesk.DBus;
using org.freedesktop.DBus;
using System.Threading;
using System.Collections.Generic;

namespace NDesk.DBusTests.IntergrationTests
{
	[TestFixture]
	public class SessionBusTests
	{
		[Test]
		public void NameAcquired_CallingRequestName_SignalIsFired()
		{
			const string TestName = "org.asd.sdf";
			string nameAcquiredResult = null;
			var test = Bus.Session.GetObject<IBus>("org.freedesktop.DBus", new ObjectPath("/org/freedesktop/DBus"));
			test.NameAcquired += (name) => nameAcquiredResult = name;
			test.RequestName(TestName, NameFlag.DoNotQueue);

			Assert.AreEqual(nameAcquiredResult, TestName);
		}
	}
}
