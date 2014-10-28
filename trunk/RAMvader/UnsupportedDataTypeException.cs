using System;


namespace RAMvader
{
    public class UnsupportedDataTypeException : RAMvaderException
    {
        /** Constructor.
         * @param dataType The data type for which RAMvader does not offer support
         *    to. */
        public UnsupportedDataTypeException( Type dataType )
            : base( string.Format(
                "RAMvader library does not support reading/writing operations on the data type \"{0}\"!",
                dataType.Name ) )
        {
        }
    }
}
