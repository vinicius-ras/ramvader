using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
