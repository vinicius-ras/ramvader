using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAMvader
{
    /** Exception thrown when the user code tries to detach a #RAMvader instance
     * from its target process, but the #RAMvader instance has not been attached to
     * any process. */
    class InstanceNotAttachedException : RAMvaderException
    {
        /** Constructor. */
        public InstanceNotAttachedException()
            : base( "This instance is not attached to any process." )
        {
        }
    }
}
