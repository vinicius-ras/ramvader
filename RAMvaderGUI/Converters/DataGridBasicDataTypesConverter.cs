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
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;


namespace RAMvaderGUI.Converters
{
	/// <summary>
	/// A converter, used for WPF bindings, which transforms <see cref="AddressEntry"/> objects into strings
	/// representing their values to be displayed in the DataGrid which displays these objects,
	/// in the <see cref="MainWindow"/>.
	/// </summary>
	public class DataGridBasicDataTypesConverter : IValueConverter
    {
		#region PRIVATE CONSTANTS
		/// <summary>The string to be displayed when the converter cannot convert a given object type.</summary>
		private const string ERROR_STRING = "<DISPLAY ERROR>";
		/// <summary>The format used to display Single and Double values.</summary>
		private const string FLOATING_POINT_FORMATS_SPECIFIER = "0.000";
		#endregion





		#region PUBLIC STATIC METHODS
		/// <summary>Converts the given Object which represents a basic type supported by the RAMvader library to a String object.</summary>
		/// <param name="addressEntryObj">The object holding the value to be converted.</param>
		/// <returns>
		///    Returns the converted value, in case of success.
		///    Returns null in case of failure.
		/// </returns>
		public static String convertBasicDataTypeToString( AddressEntry addressEntryObj )
        {
            Object objVal = addressEntryObj.Value;
            if ( objVal is IntPtr )
                return IntToHexStringConverter.convertIntPtrToString( (IntPtr) objVal );
            else if ( objVal is Single )
                return ( (Single) objVal ).ToString( FLOATING_POINT_FORMATS_SPECIFIER );
            else if ( objVal is Double )
                return ( (Double) objVal ).ToString( FLOATING_POINT_FORMATS_SPECIFIER );
            else if ( objVal is Byte || objVal is Int16 || objVal is Int32
                || objVal is Int64 || objVal is UInt16 || objVal is UInt32
                || objVal is UInt64 )
            {
                // Display the number as-is
                if ( addressEntryObj.DisplayAsHex == false )
                    return objVal.ToString();

                // Dynamically call the number's "ToString()" class, specifying that it
                // needs to be displayed as an hex value
                Type valType = objVal.GetType();
                object invokeResult = valType.InvokeMember( "ToString",
                    BindingFlags.InvokeMethod, null, objVal, new object[] { "X" } );
                return string.Format( "0x{0}", (string) invokeResult );
            }

            // Fall back to the default: display an error
            return ERROR_STRING;
        }
        #endregion








        #region INTERFACE IMPLEMENTATION: IValueConverter
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return convertBasicDataTypeToString( (AddressEntry) value );
        }


        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            // There's no conversion back in this case...
            return DependencyProperty.UnsetValue;
        }
        #endregion
    }
}
