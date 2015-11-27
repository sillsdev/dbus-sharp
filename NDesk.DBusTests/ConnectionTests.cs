using System;
using NUnit.Framework;
using NDesk.DBus;
using org.freedesktop.DBus;

namespace NDesk.DBusTests
{
	[TestFixture]
	public class ConnectionTests
	{
		[Test]
		public void Close_ConnectedBus_BusIsDisconnected()
		{
			Connection c = Bus.Session;
			Assert.IsTrue(c.IsConnected);

			c.Close();

			Assert.IsFalse(c.IsConnected);
		}

		[Test]
		public void Close_OnClosedConnection_NullOp()
		{
			Connection c = Bus.Session;
			Assert.IsTrue(c.IsConnected);

			c.Close();
			c.Close();

			Assert.IsFalse(c.IsConnected);
		}
	}
}
