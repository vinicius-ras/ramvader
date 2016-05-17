/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 * 
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

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
        /** A flag specifying if this entry should be displayed as an hex value to the
         * user. This flag is ignored for Single, Double and IntPtr data types. */
        private bool m_bDisplayAsHex = false;
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
        /** A flag indicating if the value should be displayed as an hex value. */
        public bool DisplayAsHex
        {
            get { return m_bDisplayAsHex; }
            set { m_bDisplayAsHex = value; onPropertyChanged(); }
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
