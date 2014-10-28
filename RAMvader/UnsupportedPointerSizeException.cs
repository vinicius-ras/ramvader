using System;


namespace RAMvader
{
    /** An exception which is thrown when the user tries to attach a 32-bits process to a 64-bits target process. */
    public class UnsupportedPointerSizeException : RAMvaderException
    {
        #region PUBLIC METHODS
        /** Constructor. */
        public UnsupportedPointerSizeException()
            : base( string.Format( "RAMvader library currently does not support a 32-bits host process trying to target a 64-bits process." ) )
        {
        }
        #endregion

    }
}
