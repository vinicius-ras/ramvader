using System;
using System.Windows.Data;
using System.Globalization;


namespace RAMvaderGUI.Converters
{
    [ValueConversion( typeof( IntPtr ), typeof( String ) )]
    /** A converter, used for WPF bindings, which transforms IntPtr address values into
     * their representing Hex strings. */
    public class IntToHexStringConverter : IValueConverter
    {
        #region PUBLIC STATIC METHODS
        /** Converts the given IntPtr to a String object.
         * @param textToParse The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
        public static String convertIntPtrToString( IntPtr textToParse )
        {
            string hexText = textToParse.ToString( string.Format( "X{0}", IntPtr.Size * 2 ) );
            return string.Format( "0x{0}", hexText );
        }


        /** Converts the given String to an IntPtr object.
         * @param textToParse The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
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
