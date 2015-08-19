using System;


namespace RAMvader.CodeInjection
{
    /** A generic expection that might be thrown by the #Injector class. */
    public class InjectorException : Exception
    {
        #region PUBLIC METHODS
        /** Constructor.
         * @param msg The message used to initialize the Exception. */
        public InjectorException( string msg )
            : base( msg )
        {
        }
        #endregion
    }
}
