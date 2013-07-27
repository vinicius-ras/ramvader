using System;

namespace RAMvaderGUI
{
    /** Represents an address entry on the application's memory data grid. */
    public class AddressEntry
    {
        #region PUBLIC PROPERTIES
        /** A user-defined description for the entry. */
        public string Description { get; set; }
        /** The address, on the target process, associated to this entry. */
        public IntPtr Address { get; set; }
        /** The type represented by this entry. */
        public Type ValueType { get; set; }
        /** A flag indicating if the value should be frozen or not. */
        public bool Freeze { get; set; }
        /** The value associated to this entry. */
        public Object Value { get; set; }
        #endregion








        #region PUBLIC METHODS
        /** Constructor. */
        public AddressEntry()
        {
            Description = string.Empty;
            Address = IntPtr.Zero;
            ValueType = typeof( Int32 );
            Freeze = false;
            Value = new Int32();
        }
        #endregion
    }
}
