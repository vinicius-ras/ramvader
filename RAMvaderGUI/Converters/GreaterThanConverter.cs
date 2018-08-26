/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 *
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

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
