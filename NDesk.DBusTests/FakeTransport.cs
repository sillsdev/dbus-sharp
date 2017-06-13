using System;
using NDesk.DBus.Transports;
using NDesk.DBus;

namespace NDesk.DBusTests
{
	/// <summary>
	/// Transport for unittests that just reads from a buffer.
	/// In addition if messages are supplied via construcor, then these are what
	/// ReadMessage returns.
	/// </summary>
	internal class FakeTransport : Transport
	{
		#region implemented abstract members of Transport
		byte[] _buffer;
		int _index;

		int _messageIndex;
		Message[] _messages;

		public FakeTransport(byte[] buffer)
		{
			_buffer = buffer;
		}

		public FakeTransport(byte[] buffer, Message[] messages) : this(buffer)
		{
			_messages = messages;
		}

		public override void Open(NDesk.DBus.AddressEntry entry)
		{

		}

		public override string AuthString()
		{
			return String.Empty;
		}

		public override void WriteCred()
		{

		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			var ret = Math.Min(_buffer.Length - _index, offset + count);
			Array.Copy(_buffer, _index + offset, buffer, 0, ret - offset);
			_index += ret;
			return ret - offset;
		}

		public override void WriteMessage(Message msg)
		{
			Console.WriteLine("Wrote message {0}", msg);
		}

		public override Message ReadMessage()
		{
			if (_messages == null)
				return base.ReadMessage();

			return _messages[_messageIndex++];
		}

		#endregion
	}
}
