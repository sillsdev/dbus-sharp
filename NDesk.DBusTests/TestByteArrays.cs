using System;

namespace NDesk.DBusTests
{
	public static class TestByteArrays
	{
		public static readonly byte[] Header = new byte[] {
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		};

		#region Test data

		public static readonly byte[] INT_257 = new byte[] { 1, 1, 0, 0 };
		public static readonly byte[] INT64_257 = new byte[] { 1, 1, 0, 0, 0, 0, 0, 0 };
		public static readonly byte[] STRING_C = new byte[] { 1, 0, 0, 0, 67, 0};
		public static readonly byte[] STRING_D = new byte[] { 1, 0, 0, 0, 68, 0};
		public static readonly byte[] SIG_s = new byte[] { 1, 115, 0};
		public static readonly byte[] STRING_ReturnFromDbusHello = new byte[] { 6, 0, 0, 0, 58, 49, 46, 49, 49, 56, 0, };
		public static readonly byte[] SIG_DICT_os = new byte[] { 5, 97, 123, 111, 115, 125, 0};
		public static readonly byte[] VAR_STRING_C = new byte[] { 1, 115, 0, 0, 1, 0, 0, 0, 67, 0};
		public static readonly byte[] VAR_DICT_os = new byte[] { 5, 97, 123, 111, 115, 125, 0, 0, 14, 0, 0, 0 , 0, 0, 0, 0, 1, 0, 0, 0, 67, 0,0, 0, 1, 0, 0, 0, 68, 0, 0, 0};
		public static readonly byte[] DICT_1_ss = new byte[] { 14, 0, 0, 0 , 0, 0, 0, 0, 1, 0, 0, 0, 67, 0,0, 0, 1, 0, 0, 0, 68, 0, 0, 0};
		public static readonly byte[] DICT_2_ss =  new byte[] { 30, 0, 0, 0 , 0, 0, 0, 0,
																	1, 0, 0, 0, 67, 0,0, 0, 1, 0, 0, 0, 68, 0, 0, 0,
																	1, 0, 0, 0, 68, 0,0, 0, 1, 0, 0, 0, 69, 0, 0, 0};
		public static readonly byte[] VAR_ARRAY_BYTE = new byte[] { 2, 97, 121, 0, 5, 0, 0, 0, 47, 109, 101, 100, 105, 0 };
		public static readonly byte[] VAR_ARRAY_BYTE__IncludingNullTermInArray = new byte[] { 2, 97, 121, 0, 6, 0, 0, 0, 47, 109, 101, 100, 105, 0 };

		// Dictionary<s, Dictionary<string, Dictionary<string, string>>>
		public static readonly byte[] DICT_1_sDICT_1_sDICT_1_ss =
		new byte[] { 1, 0, 0, 0 , 0, 0, 0, 0,
			// 1 item in Dictionary<s,Dictionary<string, Dictionary<string, string>>
			// C
			1, 0, 0, 0, 67, 0, 0, 0,
				1, 0, 0, 0 , 0, 0, 0, 0,
				// 1 item in Dictionary<string, Dictionary<string, string>>
				// D
				1, 0, 0, 0, 68, 0, 0, 0,
					1, 0, 0, 0 , 0, 0, 0, 0,
					// 1 item in Dictionary<string, string>>
					// E
					1, 0, 0, 0, 69, 0, 0, 0,
					// F
					1, 0, 0, 0, 70, 0, 0, 0,
		};

		// Dictionary<s, Dictionary<string, Dictionary<string, object>>>
		public static readonly byte[] DICT_1_sDICT_1_sDICT_1_so =
			new byte[] { 32, 0, 0, 0 , 0, 0, 0, 0,
			// 1 item in Dictionary<s,Dictionary<string, Dictionary<string, object>>
			// C
			1, 0, 0, 0, 67, 0, 0, 0,
				8 + 16, 0, 0, 0 , 0, 0, 0, 0,
				// 1 item in Dictionary<string, Dictionary<string, object>>
				// D
				1, 0, 0, 0, 68, 0, 0, 0,
					16 , 0, 0, 0 , 0, 0, 0, 0,
					// 1 item in Dictionary<string, object>>
					// E
					1, 0, 0, 0, 69, 0,
					// VAR_STRING_C (the object value)
					1, 115, 0, 0, 0, 0, 1, 0, 0, 0, 67, 0
		};

		#endregion

		#region Real Data

		/// <summary>
		/// Signature "o"
		/// </summary>
		public static readonly byte[] RealData_ObjectPath = new byte[] { 38,0,0,0,47,111,114,103,47,102,114,101,101,100,101,115,107,116,111,112,47,73,66,117,115,47,73,110,112,117,116,67,111,110,116,101,120,116,95,53,50,51,0, };

		#endregion

	}
}
