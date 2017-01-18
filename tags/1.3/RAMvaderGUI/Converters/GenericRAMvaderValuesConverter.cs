using System;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts objects supported by the RAMvaderLibrary to their string representations for
	///    displaying data to the user.
	/// </summary>
	[ValueConversion( typeof( object ), typeof( string ) )]
	public class GenericRAMvaderValuesConverter : IValueConverter
	{
		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null )
				return Properties.Resources.strConversionError;

			if ( value.GetType() == typeof( IntPtr ) )
				return IntPtrToStringConverter.ConvertIntPtrToString( ( IntPtr ) value );
			return value.ToString();
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value == null )
				return Binding.DoNothing;

			if ( value.GetType() == typeof(IntPtr) )
				return IntPtrToStringConverter.ConvertStringToIntPtr( (string) value );
			return System.Convert.ChangeType( value, targetType );
		}
		#endregion
	}
}
