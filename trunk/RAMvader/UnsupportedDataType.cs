using System;


namespace RAMvader
{
    public class UnsupportedDataType : RAMvaderException
    {
        /** Constructor.
         * @param dataType The data type for which RAMvader does not offer support
         *    to. */
        public UnsupportedDataType( Type dataType )
            : base( string.Format(
                "RAMvader library does not support reading/writing operations on the data type \"{0}\"!",
                dataType.Name ) )
        {
        }
    }
}
