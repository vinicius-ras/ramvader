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
using System.Windows.Data;
using System.Globalization;


namespace RAMvaderGUI.Converters
{
	/// <summary> A converter, used for WPF bindings, which transforms IntPtr
	/// address values into their representing Hex strings. </summary>
	[ValueConversion( typeof( IntPtr ), typeof( String ) )]
    public class IntToHexStringConverter : IValueConverter
    {
		#region PUBLIC STATIC METHODS
		/** 
 * @param textToParse 
 * @return 
 *     */

		/// <summary>Converts the given IntPtr to a String object.</summary>
		/// <param name="textToParse">The value to be converted.</param>
		/// <returns>
		///    Returns the converted value, in case of success.
		///    Returns null in case of failure.
		/// </returns>
		public static String convertIntPtrToString( IntPtr textToParse )
        {
            string hexText = textToParse.ToString( string.Format( "X{0}", IntPtr.Size * 2 ) );
            return string.Format( "0x{0}", hexText );
        }


		/// <summary>Converts the given String to an IntPtr object.</summary>
		/// <param name="textToParse">The value to be converted.</param>
		/// <returns>
		///    Returns the converted value, in case of success.
		///    Returns null in case of failure.
		/// </returns>
		public static IntPtr convertStringToIntPtr( String textToParse )
        {
            // Verify if the number starts with the hexadecimal specifier ("0x")
            textToParse = textToParse.Trim();
            NumberStyles parsingStyle = NumberStyles.Integer;
            if ( textToParse.StartsWith( "0x", true, CultureInfo.InvariantCulture ) )
            {
                textToParse = textToParse.Substring( 2 );
                parsingStyle = NumberStyles.HexNumber;
            }

            // Parse the text
            if ( IntPtr.Size == 8 )
                return new IntPtr( Int64.Parse( textToParse, parsingStyle ) );
            else if ( IntPtr.Size == 4 )
                return new IntPtr( Int32.Parse( textToParse, parsingStyle ) );
            
            throw new NotImplementedException( string.Format(
                "The application only supports 4 and 8 byte addresses. The {0} structure reported that the current platform address size is {1} bytes!",
                typeof( IntPtr ).Name, IntPtr.Size ) );
        }
        #endregion








        #region INTERFACE IMPLEMENTATION: IValueConverter
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return convertIntPtrToString( (IntPtr) value );
        }


        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return convertStringToIntPtr( (String) value );
        }
        #endregion



    }
}
