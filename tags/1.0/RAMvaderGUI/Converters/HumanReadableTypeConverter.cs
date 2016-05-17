using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI.Converters
{
    [ValueConversion( typeof( Type ), typeof( String ) )]
    /** Converts Type objects from fully-qualified class names to human-readable,
     * more user-friendly names. */
    public class HumanReadableTypeConverter : IValueConverter
    {
        #region STATIC PROPERTIES
        /** Maps Type values to their corresponding (human-readable/user-friendly) String values. */
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
        /** Maps String values back to their corresponding Type values. */
        private static Dictionary<String, Type> sm_stringsToTypes = new Dictionary<string, Type>();
        #endregion








        #region STATIC INITIALIZER
        /** Class static initializer. */
        static HumanReadableTypeConverter() {
            foreach ( KeyValuePair<Type, String> keyValPair in sm_typeNames )
                sm_stringsToTypes.Add( keyValPair.Value.ToUpper( CultureInfo.InvariantCulture ), keyValPair.Key );
        }
        #endregion








        #region PUBLIC STATIC FUNCTIONS
        /** Converts the given Type to a user-friendly String.
         * @param valueToConvert The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
        public static String convertTypeToString( Type valueToConvert )
        {
            if ( valueToConvert != null && sm_typeNames.ContainsKey( valueToConvert ) )
                return sm_typeNames[valueToConvert];
            return null;
        }


        /** Converts the given user-friendly String to a Type.
         * @param valueToConvert The value to be converted.
         * @return Returns the converted value, in case of success.
         *    Returns null in case of failure. */
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
