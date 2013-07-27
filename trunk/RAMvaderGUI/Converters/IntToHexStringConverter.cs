using System;
using System.Windows.Data;
using System.Globalization;


namespace RAMvaderGUI.Converters
{
    [ValueConversion( typeof( IntPtr ), typeof( String ) )]
    /** A converter, used for WPF bindings, which transforms IntPtr address values into
     * their representing Hex strings. */
    class IntToHexStringConverter : IValueConverter
    {
        #region PUBLIC STATIC METHODS
        /** Converts the given IntPtr to a String object.
         * @param value The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
        public static String convertIntPtrToString( IntPtr value )
        {
            return value.ToString( string.Format( "X{0}", IntPtr.Size * 2 ) );
        }


        /** Converts the given String to an IntPtr object.
         * @param value The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
        public static IntPtr convertStringToIntPtr( String value )
        {
            if ( IntPtr.Size == 8 )
                return new IntPtr( Int64.Parse( value, NumberStyles.HexNumber ) );
            else if ( IntPtr.Size == 4 )
                return new IntPtr( Int32.Parse( value, NumberStyles.HexNumber ) );
#if DEBUG
            throw new NotImplementedException( string.Format(
                "The application only supports 4 and 8 byte addresses. The {0} structure reported that the current platform address size is {1} bytes!",
                typeof( IntPtr ).Name, IntPtr.Size ) );
#else
            return IntPtr.Zero;
#endif
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
