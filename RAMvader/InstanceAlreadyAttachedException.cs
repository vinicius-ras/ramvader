using System;
using System.Diagnostics;


namespace RAMvader
{
    /** Exception thrown when the user tries to attach a #RAMvader instance to a
     * process, but the instance is already attached to another process. Before
     * attaching to a process, the #RAMvader instance must be detached from any
     * other process. */
    public class InstanceAlreadyAttachedException : RAMvaderException
    {
        /** Constructor.
         * @param oldProcess The process to which the #RAMvader instance is
         *    currently attached. */
        public InstanceAlreadyAttachedException( Process oldProcess )
            : base( string.Format(
                "{0} instance already attached to process with PID {1}.",
                typeof( RAMvaderTarget ).Name,
                oldProcess.Id ) )
        {
        }
    }
}
