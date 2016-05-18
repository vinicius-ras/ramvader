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
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI.Converters
{
	/// <summary>Converts Type objects from fully-qualified class names to human-readable, more user-friendly names.</summary>
	[ValueConversion( typeof( Type ), typeof( String ) )]
    public class HumanReadableTypeConverter : IValueConverter
    {
		#region STATIC PROPERTIES
		/// <summary>Maps Type values to their corresponding (human-readable/user-friendly) String values.</summary>
		private static Dictionary<Type, String> sm_typeNames = new Dictionary<Type, string>()
        {
            { typeof( Byte ), "BYTE" },
            { typeof( Int16 ), "WORD" },
            { typeof( Int32 ), "DWORD" },
            { typeof( Int64 ), "QWORD" },
            { typeof( UInt16 ), "Unsigned WORD" },
            { typeof( UInt32 ), "Unsigned DWORD" },
            { typeof( UInt64 ), "Unsigned QWORD" },
            { typeof( Single ), "FLOAT" },
            { typeof( Double ), "DOUBLE" },
            { typeof( IntPtr ), string.Format( "{0}-BITS POINTER", IntPtr.Size * 8 ) },
        };
		/// <summary>Maps String values back to their corresponding Type values.</summary>
		private static Dictionary<String, Type> sm_stringsToTypes = new Dictionary<string, Type>();
		#endregion








		#region STATIC INITIALIZER
		/// <summary>Class static initializer.</summary>
        static HumanReadableTypeConverter() {
            foreach ( KeyValuePair<Type, String> keyValPair in sm_typeNames )
                sm_stringsToTypes.Add( keyValPair.Value.ToUpper( CultureInfo.InvariantCulture ), keyValPair.Key );
        }
		#endregion








		#region PUBLIC STATIC FUNCTIONS
		/// <summary>Converts the given Type to a user-friendly String.</summary>
		/// <param name="valueToConvert"> value to be converted.</param>
		/// <returns>
		///    Returns the converted value, in case of success.
		///    Returns null in case of failure.
		/// </returns>
		public static String convertTypeToString( Type valueToConvert )
        {
            if ( valueToConvert != null && sm_typeNames.ContainsKey( valueToConvert ) )
                return sm_typeNames[valueToConvert];
            return null;
        }


		/// <summary>Converts the given user-friendly String to a Type.</summary>
		/// <param name="valueToConvert">The value to be converted.</param>
		/// <returns>
		///    Returns the converted value, in case of success.
		///    Returns null in case of failure.
		/// </returns>
		public static Type convertStringToType( String valueToConvert )
        {
            String targetSearchKey = valueToConvert.ToUpper( CultureInfo.InvariantCulture );
            if ( targetSearchKey != null && sm_stringsToTypes.ContainsKey( targetSearchKey ) )
                return sm_stringsToTypes[targetSearchKey];
            return null;
        }
        #endregion








        #region INTERFACE IMPLEMENTATION: IValueConverter
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return convertTypeToString( (Type) value );
        }


        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return convertStringToType( value.ToString() );
        }
        #endregion
    }
}
