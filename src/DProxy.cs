// Copyright 2006 Alp Toker <alp@atoker.com>
// This software is made available under the MIT License
// See COPYING for details

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Messaging;

namespace NDesk.DBus
{
	public class DProxy : RealProxy
	{
		Connection conn;

		string bus_name;
		ObjectPath object_path;

		//Dictionary<string,string> methods = new Dictionary<string,string> ();

		public DProxy (Connection conn, string bus_name, ObjectPath object_path, Type type) : base(type)
		{
			this.conn = conn;
			this.bus_name = bus_name;
			this.object_path = object_path;

			//messy and only relevant to imported objects, but works
			//note that the foreach is useless since there can be only key
			//probably does not deal with class inheritance etc.

			foreach (InterfaceAttribute ia in type.GetCustomAttributes (typeof (InterfaceAttribute), false))
				conn.RegisteredTypes[type] = ia.Name;

			foreach (Type t in type.GetInterfaces ())
				foreach (InterfaceAttribute ia in t.GetCustomAttributes (typeof (InterfaceAttribute), false))
					conn.RegisteredTypes[t] = ia.Name;

			/*
			methods["Hello"] = "";
			methods["ListNames"] = "";
			methods["NameHasOwner"] = "s";
			*/
		}

		public override IMessage Invoke (IMessage msg)
		{
			IMethodCallMessage mcm = (IMethodCallMessage) msg;

			MethodReturnMessageWrapper newRet = new MethodReturnMessageWrapper ((IMethodReturnMessage) msg);

			//Console.WriteLine (mcm.MethodName);

			//if (!methods.ContainsKey (mcm.MethodName))
			//	return null;

			//foreach (object myObj in mcm.InArgs)
			//	Console.WriteLine("arg value: " + myObj.ToString());

			MethodInfo imi = mcm.MethodBase as MethodInfo;
			string methodName = mcm.MethodName;

			if (imi != null && imi.IsSpecialName) {
				if (methodName.StartsWith ("add_")) {
					string[] parts = mcm.MethodName.Split ('_');
					string ename = parts[1];
					Delegate dlg = (Delegate)mcm.InArgs[0];

					conn.Handlers[ename] = dlg;

					//TODO: make the match rule more specific, and cache the DBus object somewhere sensible
					if (bus_name != "org.freedesktop.DBus") {
						org.freedesktop.DBus.Bus bus = conn.GetInstance<org.freedesktop.DBus.Bus> ("org.freedesktop.DBus", new ObjectPath ("/org/freedesktop/DBus"));
						bus.AddMatch (MessageFilter.CreateMatchRule (MessageType.Signal, bus_name, ename));
					}

					return (IMethodReturnMessage) newRet;
				}

				if (mcm.MethodName.StartsWith ("remove_")) {
					string[] parts = mcm.MethodName.Split ('_');

					string ename = parts[1];
					//Delegate dlg = (Delegate)mcm.InArgs[0];

					//TODO: make the match rule more specific, and cache the DBus object somewhere sensible
					if (bus_name != "org.freedesktop.DBus") {
						org.freedesktop.DBus.Bus bus = conn.GetInstance<org.freedesktop.DBus.Bus> ("org.freedesktop.DBus", new ObjectPath ("/org/freedesktop/DBus"));
						bus.RemoveMatch (MessageFilter.CreateMatchRule (MessageType.Signal, bus_name, ename));
					}

					conn.Handlers.Remove (ename);

					return (IMethodReturnMessage) newRet;
				}
			}

			Message callMsg = new Message ();

			//build the outbound method call message
			{

				Signature inSig = new Signature ("");

				if (mcm.InArgs != null && mcm.InArgs.Length != 0) {
					callMsg.Body = new System.IO.MemoryStream ();

					foreach (object arg in mcm.InArgs)
						Message.Write (callMsg.Body, arg.GetType (), arg);

					inSig = GetSig (mcm.InArgs);
				}

				string iface = null;

				//if the type is registered, use that, otherwise use legacy iface string
				if (imi != null && conn.RegisteredTypes.ContainsKey (imi.DeclaringType))
					iface = conn.RegisteredTypes[imi.DeclaringType];

				//map property accessors
				//TODO: this needs to be done properly, not with simple String.Replace
				//note that IsSpecialName is also for event accessors, but we already handled those and returned
				if (imi != null && imi.IsSpecialName) {
					methodName = methodName.Replace ("get_", "Get");
					methodName = methodName.Replace ("set_", "Set");
				}

				if (inSig.Data.Length == 0)
					callMsg.WriteHeader (new HeaderField (FieldCode.Path, object_path), new HeaderField (FieldCode.Interface, iface), new HeaderField (FieldCode.Member, methodName), new HeaderField (FieldCode.Destination, bus_name));
				else
					callMsg.WriteHeader (new HeaderField (FieldCode.Path, object_path), new HeaderField (FieldCode.Interface, iface), new HeaderField (FieldCode.Member, methodName), new HeaderField (FieldCode.Destination, bus_name), new HeaderField (FieldCode.Signature, inSig));
			}

			bool needsReply = true;

			MethodInfo mi = newRet.MethodBase as MethodInfo;
			if (mi.ReturnType == typeof (void))
				needsReply = false;

			if (!needsReply) {
				conn.Send (callMsg);
				return (IMethodReturnMessage) newRet;
			}

			Message retMsg = conn.SendWithReplyAndBlock (callMsg);

			//handle the reply message
			{
				Type[] retTypeArr = new Type[1];
				retTypeArr[0] = mi.ReturnType;
				newRet.ReturnValue = conn.GetDynamicValues (retMsg, retTypeArr)[0];
			}

			return (IMethodReturnMessage) newRet;
		}

		/*
		public override ObjRef CreateObjRef (Type ServerType)
		{
			throw new System.NotSupportedException ();
		}
		*/

		public static Signature GetSig (object[] objs)
		{
			return GetSig (Type.GetTypeArray (objs));
		}

		public static Signature GetSig (Type[] types)
		{
			MemoryStream ms = new MemoryStream ();

			foreach (Type type in types) {
				{
					byte[] data = GetSig (type).Data;
					ms.Write (data, 0, data.Length);
				}
			}

			Signature sig;
			sig.Data = ms.ToArray ();
			return sig;
		}

		public static Signature GetSig (Type type)
		{
			MemoryStream ms = new MemoryStream ();

			if (type.IsArray) {
				ms.WriteByte ((byte)DType.Array);

				Type elem_type = type.GetElementType ();
				//TODO: recurse
				//DType elem_dtype = Signature.TypeToDType (elem_type);
				//ms.WriteByte ((byte)elem_dtype);
				{
					byte[] data = GetSig (elem_type).Data;
					ms.Write (data, 0, data.Length);
				}
			} else if (type.IsGenericType && (type.GetGenericTypeDefinition () == typeof (IDictionary<,>) || type.GetGenericTypeDefinition () == typeof (Dictionary<,>))) {
				Type[] genArgs = type.GetGenericArguments ();

				ms.WriteByte ((byte)'a');
				ms.WriteByte ((byte)'{');

				{
					byte[] data = GetSig (genArgs[0]).Data;
					ms.Write (data, 0, data.Length);
				}

				{
					byte[] data = GetSig (genArgs[1]).Data;
					ms.Write (data, 0, data.Length);
				}

				ms.WriteByte ((byte)'}');
			} else if (!type.IsPrimitive && type.IsValueType && !type.IsEnum) {
				//if (type.IsGenericParameter && type.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
				if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
					ms.WriteByte ((byte)'{');
				else
					ms.WriteByte ((byte)'(');

				ConstructorInfo[] cis = type.GetConstructors ();
				if (cis.Length != 0) {
					System.Reflection.ConstructorInfo ci = cis[0];
					System.Reflection.ParameterInfo[]  parms = ci.GetParameters ();

					foreach (ParameterInfo parm in parms) {
						{
							byte[] data = GetSig (parm.ParameterType).Data;
							ms.Write (data, 0, data.Length);
						}
					}

				} else {
					foreach (FieldInfo fi in type.GetFields ()) {
						{
							byte[] data = GetSig (fi.FieldType).Data;
							ms.Write (data, 0, data.Length);
						}
					}
				}
				if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
					ms.WriteByte ((byte)'}');
				else
					ms.WriteByte ((byte)')');
			} else {
				DType dtype = Signature.TypeToDType (type);
				ms.WriteByte ((byte)dtype);
			}

			Signature sig;
			sig.Data = ms.ToArray ();
			return sig;
		}
	}

	[AttributeUsage (AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
	public class InterfaceAttribute : Attribute
	{
		public string Name;

		public InterfaceAttribute (string name)
		{
			this.Name = name;
		}
	}
}
