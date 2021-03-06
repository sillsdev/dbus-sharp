using System;
using NUnit.Framework;
using NDesk.DBus;
using org.freedesktop.DBus;
using System.Threading;
using System.Collections.Generic;

namespace NDesk.DBusTests.IntergrationTests
{
	[TestFixture]
	public class SystemBus
	{

		[Test]
		public void DBus_Introspect()
		{
			var expectedResult = @"<!DOCTYPE node PUBLIC ""-//freedesktop//DTD D-BUS Object Introspection 1.0//EN""
""http://www.freedesktop.org/standards/dbus/1.0/introspect.dtd"">
<node>
  <interface name=""org.freedesktop.DBus"">
	<method name=""Hello"">
	  <arg direction=""out"" type=""s""/>
	</method>
	<method name=""RequestName"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""in"" type=""u""/>
	  <arg direction=""out"" type=""u""/>
	</method>
	<method name=""ReleaseName"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""u""/>
	</method>
	<method name=""StartServiceByName"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""in"" type=""u""/>
	  <arg direction=""out"" type=""u""/>
	</method>
	<method name=""UpdateActivationEnvironment"">
	  <arg direction=""in"" type=""a{ss}""/>
	</method>
	<method name=""NameHasOwner"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""b""/>
	</method>
	<method name=""ListNames"">
	  <arg direction=""out"" type=""as""/>
	</method>
	<method name=""ListActivatableNames"">
	  <arg direction=""out"" type=""as""/>
	</method>
	<method name=""AddMatch"">
	  <arg direction=""in"" type=""s""/>
	</method>
	<method name=""RemoveMatch"">
	  <arg direction=""in"" type=""s""/>
	</method>
	<method name=""GetNameOwner"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""s""/>
	</method>
	<method name=""ListQueuedOwners"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""as""/>
	</method>
	<method name=""GetConnectionUnixUser"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""u""/>
	</method>
	<method name=""GetConnectionUnixProcessID"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""u""/>
	</method>
	<method name=""GetAdtAuditSessionData"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""ay""/>
	</method>
	<method name=""GetConnectionSELinuxSecurityContext"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""ay""/>
	</method>
	<method name=""GetConnectionAppArmorSecurityContext"">
	  <arg direction=""in"" type=""s""/>
	  <arg direction=""out"" type=""s""/>
	</method>
	<method name=""ReloadConfig"">
	</method>
	<method name=""GetId"">
	  <arg direction=""out"" type=""s""/>
	</method>
	<signal name=""NameOwnerChanged"">
	  <arg type=""s""/>
	  <arg type=""s""/>
	  <arg type=""s""/>
	</signal>
	<signal name=""NameLost"">
	  <arg type=""s""/>
	</signal>
	<signal name=""NameAcquired"">
	  <arg type=""s""/>
	</signal>
  </interface>
  <interface name=""org.freedesktop.DBus.Introspectable"">
	<method name=""Introspect"">
	  <arg direction=""out"" type=""s""/>
	</method>
  </interface>
</node>
";

			var dbus = Bus.System.GetObject<Introspectable>("org.freedesktop.DBus", new ObjectPath("/var/run/dbus/system_bus_socket"));
			Assert.AreEqual(expectedResult, dbus.Introspect());

			Bus.System.Close();
		}
	}
}
