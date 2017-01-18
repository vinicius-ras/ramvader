using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts a <see cref="Type"/> object into a friendly name to be read by the user.
	/// </summary>
	[ValueConversion( typeof( Type ), typeof( string ) )]
	public class TypeToFriendlyNameConverter : IValueConverter
	{
		#region PRIVATE STATIC FIELDS
		/// <summary>A dictionary containing all the friendly names for <see cref="Type"/> objects this converter knows.</summary>
		private Dictionary<Type,string> sm_typeNames = new Dictionary<Type, string>()
		{
			{ typeof( Byte ), "Byte" },
			{ typeof( Int16 ), "Int16 (WORD)" },
			{ typeof( Int32 ), "Int32 (DWORD)" },
			{ typeof( Int64 ), "Int64 (QWORD)" },
			{ typeof( UInt16 ), "UInt16 (Unsigned WORD)" },
			{ typeof( UInt32 ), "UInt32 (Unsigned DWORD)" },
			{ typeof( UInt64 ), "UInt64 (Unsigned QWORD)" },
			{ typeof( Single ), "Single (Float)" },
			{ typeof( Double ), "Double" },
			{ typeof( IntPtr ), "IntPtr" },
		};
		#endregion





		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Type typeObj = (Type) value;
			string result;
			if ( sm_typeNames.TryGetValue( typeObj, out result ) == false )
				result = typeObj.FullName;
			return result;
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
