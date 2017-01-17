using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts <see cref="IntPtr"/> objects to string representations.
	/// </summary>
	[ValueConversion( typeof( IntPtr ), typeof( string ) )]
	public class IntPtrToStringConverter : IValueConverter
	{
		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null || value.GetType() != typeof(IntPtr) )
				return Binding.DoNothing;

			IntPtr intPtrVal = (IntPtr) value;
			return string.Format( "0x{0}", intPtrVal.ToString("X8") );
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
