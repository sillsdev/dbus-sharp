using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NDesk.DBus;
using NUnit.Framework;
using org.freedesktop.DBus;

namespace NDesk.DBusTests.IntergrationTests
{
	[TestFixture]
	public class PrivateIBusTests
	{
		[Interface("org.freedesktop.IBus.Panel")]
		interface TestIBusPanel
		{
			void SetCursorLocation(int x, int ty, int w, int h);
		}


		[Test]
		public void ConnectToPrivateBusAndMakeSomeMethodCalls()
		{
			string configFileName = IBusConfigFilename();
			if (!File.Exists(configFileName))
				Assert.Fail("ibus connection not found");

			var bus = Bus.Open(GetSocket(configFileName));

			var obj = bus.GetObject<Introspectable>("org.freedesktop.IBus.Panel", new ObjectPath("/org/freedesktop/IBus/Panel"));
			Console.WriteLine(obj.Introspect());

			var panel = bus.GetObject<TestIBusPanel>("org.freedesktop.IBus.Panel", new ObjectPath("/org/freedesktop/IBus/Panel"));
			panel.SetCursorLocation(1, 2, 3, 4);

			bus.Close();
		}

		#region test helper methods

		const string ENV_IBUS_ADDRESS = "IBUS_ADDRESS";
		const string IBUS_ADDRESS = "IBUS_ADDRESS";

		/// <summary>
		/// Gets the display number and the hostname (if any) from the DISPLAY environment
		/// variable.
		/// </summary>
		static int GetDisplayNumber(out string hostname)
		{
			// default to 0 if we can't find from DISPLAY ENV var
			int displayNumber = 0;
			hostname = null;
			string display = System.Environment.GetEnvironmentVariable("DISPLAY");
			if (!string.IsNullOrEmpty(display))
			{
				// DISPLAY is hostname:displaynumber.screennumber
				// or more nomally ':0.0"
				// so look for first number after :
				int start = display.IndexOf(':');
				int end = display.IndexOf('.', start >= 0 ? start : 0);
				if (end < 0)
					end = display.Length;
				if (start >= 0)
				{
					int.TryParse(display.Substring(start + 1, end - start - 1), out displayNumber);
					hostname = display.Substring(0, start);
				}
			}
			if (string.IsNullOrEmpty(hostname))
				hostname = "unix";

			return displayNumber;
		}

		/// <summary>
		/// Attempts to return the file name of the ibus server config file that contains the socket name.
		/// </summary>
		static string IBusConfigFilename()
		{
			// Implementation Plan:
			// Read file in $XDG_CONFIG_HOME/ibus/bus/* if ($XDG_CONFIG_HOME) not set then $HOME/.config/ibus/bus/*
			// Actual file is called 'localmachineid'-'hostname'-'displaynumber'
			// eg: 5a2f89ae5421972c24f8a4414b0495d7-unix-0
			// could check $DISPLAY to see if we are running not on display 0 or not.
			// localmachineid comes from /var/lib/dbus/machine-id

			string directory = System.Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
			if (String.IsNullOrEmpty(directory))
			{
				directory = System.Environment.GetEnvironmentVariable("HOME");

				if (String.IsNullOrEmpty(directory))
					throw new ApplicationException("$XDG_CONFIG_HOME or $HOME Environment not set");

				directory = Path.Combine(directory, ".config");
			}
			directory = Path.Combine(directory, "ibus");
			directory = Path.Combine(directory, "bus");

			string hostname;
			int displayNumber = GetDisplayNumber(out hostname);

			string fileName = Path.Combine(directory,
										   string.Format("{0}-{1}-{2}", LocalMachineId, hostname, displayNumber));

			if (!File.Exists(fileName))
			{
				Debug.Print("Unable to locate IBus Config file {0}", fileName);
				return null;
			}

			return fileName;
		}

		/// <summary>
		/// Read config file and return the socket name from it.
		/// </summary>
		static string GetSocket(string filename)
		{
			// Look for line
			// Set Enviroment 'DBUS_SESSION_BUS_ADDRESS' so DBus Library actually connects to IBus' DBus.
			// IBUS_ADDRESS=unix:abstract=/tmp/dbus-DVpIKyfU9k,guid=f44265fa3b2781284d54c56a4b0d83f3

			using (StreamReader s = new StreamReader(filename))
			{
				string line = String.Empty;
				while (line != null)
				{
					line = s.ReadLine();

					if (line.Contains(IBUS_ADDRESS))
					{
						string[] toks = line.Split("=".ToCharArray(), 2);
						if (toks.Length != 2 || toks[1] == String.Empty)
							throw new ApplicationException(String.Format("IBUS config file : {0} not as expected for line {1}. Expected IBUS_ADDRESS='some socket'", filename, line));

						return toks[1];
					}
				}
			}
			throw new ApplicationException(String.Format("IBUS config file : {0} doesn't contain {1} token", filename, IBUS_ADDRESS));
		}

		/// <summary>
		/// Gets the local machine identifier.
		/// </summary>
		/// <remarks>The path to the machine-id file is hardcoded in ibus sources as
		/// /var/lib/dbus/machine-id (src/ibusshare.c).</remarks>
		private static string LocalMachineId
		{
			get
			{
				using (var machineIdFile = new StreamReader("/var/lib/dbus/machine-id"))
				{
					return machineIdFile.ReadToEnd().TrimEnd('\n');
				}
			}
		}

		#endregion
	}
}
