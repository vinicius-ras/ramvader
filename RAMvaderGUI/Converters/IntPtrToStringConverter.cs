using System;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts <see cref="IntPtr"/> objects to string representations.
	/// </summary>
	[ValueConversion( typeof( IntPtr ), typeof( string ) )]
	public class IntPtrToStringConverter : IValueConverter
	{
		#region PUBLIC STATIC METHODS
		/// <summary>Performs the conversion from <see cref="IntPtr"/> to <see cref="string"/>.</summary>
		/// <param name="val">The value to be converted to string.</param>
		/// <returns>Returns a string representation of the value.</returns>
		public static string ConvertIntPtrToString( IntPtr val )
		{
			return string.Format( "0x{0}", val.ToString( "X8" ) );
		}


		/// <summary>Performs the conversion from <see cref="string"/> to <see cref="IntPtr"/>.</summary>
		/// <param name="val">The value to be converted to <see cref="IntPtr"/>.</param>
		/// <returns>Returns an <see cref="IntPtr"/> representation of the value.</returns>
		public static IntPtr ConvertStringToIntPtr( string val )
		{
			val = val.ToLowerInvariant().Trim();
			if ( val.StartsWith( "0x" ) )
				val = val.Substring( 2 );

			Int64 valAsLong = System.Convert.ToInt64( val, 16 );
			return new IntPtr(valAsLong);
		}
		#endregion





		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null || value.GetType() != typeof(IntPtr) )
				return Binding.DoNothing;

			IntPtr intPtrVal = (IntPtr) value;
			return ConvertIntPtrToString( intPtrVal );
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			string strVal = (string) value;
			if ( strVal.StartsWith( "0x" ) )
				strVal = strVal.Substring( 2 );

			try {
				int intVal = System.Convert.ToInt32( strVal, 16 );
				return new IntPtr( intVal );
			}
			catch ( FormatException )
			{
				return Binding.DoNothing;
			}
			catch (Exception)
			{
				throw;
			}
		}
		#endregion
	}
}
