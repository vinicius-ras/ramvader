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
