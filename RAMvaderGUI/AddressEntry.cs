using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RAMvaderGUI
{
    /** Represents an address entry on the application's memory data grid. */
    public class AddressEntry : INotifyPropertyChanged
    {
        #region PRIVATE FIELDS
        /** A user-defined description for the entry. */
        private string m_description = string.Empty;
        /** The address, on the target process, associated to this entry. */
        private IntPtr m_address = IntPtr.Zero;
        /** The type represented by this entry. */
        private Type m_valueType = typeof( Int32 );
        /** A flag indicating if the value should be frozen or not. */
        private bool m_freeze = false;
        /** The value associated to this entry. */
        private Object m_value = new Int32();
        #endregion








        #region PUBLIC PROPERTIES
        /** A user-defined description for the entry. */
        public string Description
        {
            get { return m_description; }
            set { m_description = value; onPropertyChanged(); }
        }
        /** The address, on the target process, associated to this entry. */
        public IntPtr Address
        {
            get { return m_address; }
            set { m_address = value; onPropertyChanged(); }
        }
        /** The type represented by this entry. */
        public Type ValueType
        {
            get { return m_valueType; }
            set { m_valueType = value; onPropertyChanged(); }
        }
        /** A flag indicating if the value should be frozen or not. */
        public bool Freeze
        {
            get { return m_freeze; }
            set { m_freeze = value; onPropertyChanged(); }
        }
        /** The value associated to this entry. */
        public Object Value
        {
            get { return m_value; }
            set { m_value = value; onPropertyChanged(); }
        }
        #endregion








        #region EVENTS
        /** Allows registering of PropertyChanged events. */
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion







        #region PRIVATE METHODS
        /** Called from the setter methods of the class' properties to notify when one
         * of these properties have changed.
         * @param propertyName The name of the property that has been changed. Automatically
         *    filled with the name, because of its CallerMemberName attribute. */
        private void onPropertyChanged( [CallerMemberName] string propertyName = "" )
        {
            if ( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion
    }
}
