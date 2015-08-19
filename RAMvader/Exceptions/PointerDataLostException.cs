namespace RAMvader
{
    /** An exception which is thrown when trying to perform an I/O operation with pointers
     * between two processes with different pointer sizes. */
    public class PointerDataLostException : RAMvaderException
    {
        #region PUBLIC METHODS
        /** Constructor.
         * @param bIsReadOperation A flag specifying if the exception has been thrown during a read
         *    operation (true) or a write operation (false). */
        public PointerDataLostException( bool bIsReadOperation )
            : base( string.Format(
                "{0} operation failed: the size of pointers on the target process is different from the size of pointers on the process which runs RAMvader!",
                bIsReadOperation ? "READ" : "WRITE" ) )
        {
        }
        #endregion
    }
}
