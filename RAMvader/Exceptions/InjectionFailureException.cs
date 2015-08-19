namespace RAMvader.CodeInjection
{
    /** An exception thrown by the #Injector to indicate the injection method has
     * failed. */
    public class InjectionFailureException : InjectorException
    {
        #region PUBLIC ENUMERATIONS
        /** Indicates the type of failure which caused the exception. */
        public enum EFailureType
        {
            /** Indicates that the #Injector has not been initialized with a
             * #RAMvaderTarget object. This can be done by calling #Injector.SetTargetProcess(). */
            evFailureRAMvaderTargetNull,
            /** Indicates that the #RAMvaderTarget object associated to the #Injector is currently not
             * attached to any process. */
            evFailureNotAttached,
            /** Indicates that the #Injector has tried to allocate virtual memory in the target process'
             * memory space, but the allocation failed. Memory allocation happens when you can the
             * #Injector.Inject() method (the parameterless version of it). */
            evFailureMemoryAllocation,
            /** Indicates that the call to #RAMvaderTarget.WriteToTarget() method has failed. */
            evFailureWriteToTarget,
        }
        #endregion





        #region PRIVATE FIELDS
        /** Keeps the type of failure that caused the exception to be thrown. */
        private EFailureType m_failureType;
        #endregion





        #region PUBLIC PROPERTIES
        /** The type of failure that caused the exception to be thrown. */
        public EFailureType FailureType
        {
            get { return m_failureType; }
        }
        #endregion





        #region PUBLIC METHODS
        /** Constructor.
         * @param failureType The type of failure which caused the exception to be
         *    thrown. */
        public InjectionFailureException( EFailureType failureType )
            : base( string.Format( "Injection process has failed with code: {0}.", failureType.ToString() ) )
        {
            m_failureType = failureType;
        }
        #endregion
    }
}
