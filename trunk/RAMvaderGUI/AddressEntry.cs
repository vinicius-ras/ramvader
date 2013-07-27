using System;

namespace RAMvaderGUI
{
    /** Represents an address entry on the application's memory data grid. */
    public class AddressEntry
    {
        #region PUBLIC PROPERTIES
        /** A user-defined description for the entry. */
        public string Description = string.Empty;
        /** The address, on the target process, associated to this entry. */
        public IntPtr Address = IntPtr.Zero;
        /** The value associated to this entry. If this value is not-null, the application
         * should use this value to freeze the entry. Else, the entry should not be frozen. */
        public Object Value = null;
        #endregion
    }
}
