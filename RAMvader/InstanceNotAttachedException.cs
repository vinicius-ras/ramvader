namespace RAMvader
{
    /** Exception thrown when the user code tries to detach a #RAMvader instance
     * from its target process, but the #RAMvader instance has not been attached to
     * any process. */
    public class InstanceNotAttachedException : RAMvaderException
    {
        /** Constructor. */
        public InstanceNotAttachedException()
            : base( "This instance is not attached to any process." )
        {
        }
    }
}
