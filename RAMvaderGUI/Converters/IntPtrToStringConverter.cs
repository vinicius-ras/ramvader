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
			return $"0x{val.ToString( "X" ).PadLeft(8, '0')}";
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
