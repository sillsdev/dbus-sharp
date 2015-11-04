using System;
using System.Collections.Generic;
using org.freedesktop.DBus;

namespace NDesk.DBus
{
	public delegate void InterfacesAddedDelegate(ObjectPath object_Path, IDictionary<string, IDictionary<string, object>> interfaces_and_properties);
	public delegate void InterfacesRemovedDelegate(ObjectPath objectPath,string[] interfaces);

	[Interface("org.freedesktop.DBus.ObjectManager")]
	public interface ObjectManager : Introspectable
	{
		[return: ArgumentAttribute("object_paths_interfaces_and_properties")]
		IDictionary<ObjectPath, IDictionary<string, IDictionary<string, object>>> GetManagedObjects();

		event InterfacesAddedDelegate InterfacesAdded;
		event InterfacesRemovedDelegate IntefacesRemoved;
	}
}
