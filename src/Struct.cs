// Copyright (c) 2015 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;

namespace NDesk.DBus
{
	/// <summary>
	/// Base class for DBus structs
	/// </summary>
	/// <remarks>This class serves as a marker so that we can distinguish classes that represent
	/// DBus structs from other objects</remarks>
	public class Struct: MarshalByRefObject
	{
	}
}
