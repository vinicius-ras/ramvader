using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    An <see cref="IValueConverter"/> which compares two <see cref="IComparable"/> objects
	///    to verify if one is greater than the other.
	/// </summary>
	[ValueConversion( typeof( IComparable ), typeof( Boolean ), ParameterType = typeof( IComparable ) )]
	public class IntGreaterThanConverter : IValueConverter
	{
		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			// The types must be equal
			if ( value.GetType() != parameter.GetType() )
				return DependencyProperty.UnsetValue;

			// Compare the values
			IComparable comparableValue = (IComparable) value;
			IComparable comparableParam = (IComparable) parameter;

			int comparisonResult = comparableValue.CompareTo( comparableParam );
			return ( comparisonResult > 0 );
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
