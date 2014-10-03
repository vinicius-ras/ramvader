using System;


namespace RAMvader
{
    /** The base class for all exceptions from the RAMvader library. */
    public abstract class RAMvaderException : Exception
    {
        /** Constructor.
         * @param msg The message associated to the exception. */
        public RAMvaderException( string msg )
            : base( msg )
        {
        }
    }
}
