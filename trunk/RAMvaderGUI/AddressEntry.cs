using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAMvaderGUI
{
    /** Represents an address entry on the application's memory data grid. */
    class AddressEntry<ValueType> where ValueType : struct
    {
        #region PUBLIC PROPERTIES
        /** A user-defined identifier for the entry. */
        public string Identifier = string.Empty;
        /** The address, on the target process, associated to this entry. */
        public IntPtr Address = IntPtr.Zero;
        /** The value associated to this entry. If this value is not-null, the application
         * should use this value to freeze the entry. Else, the entry should not be frozen. */
        public ValueType? Value = null;
        #endregion
    }
}
