﻿using System;
using RAMvader;

namespace RAMvaderGUI
{
	/// <summary>
	///    Represents the data related to an address, which is displayed in the <see cref="System.Windows.Controls.DataGrid"/>
	///    for the user to monitor/edit.
	/// </summary>
	public class AddressData : NotifyPropertyChangedAdapter
	{
		#region PRIVATE FIELDS
		/// <summary>Backs the <see cref="Identifier"/> property.</summary>
		private string m_identifier;
		/// <summary>Backs the <see cref="v"/> property.</summary>
		private IntPtr m_address;
		/// <summary>Backs the <see cref="Value"/> property.</summary>
		private Object m_value;
		/// <summary>Backs the <see cref="Freeze"/> property.</summary>
		private Boolean m_freeze;
		/// <summary>
		///    This flag prevents recursion when updating the <see cref="Value"/> or <see cref="Type"/> properties.
		///    When one of these properties gets updated, the other is automatically updated, which can cause an infinite
		///    recursion between the two setter methods of these properties. This flag is used to prevent that recursion
		///    from happening.
		/// </summary>
		private Boolean m_lockValueTypeRecursion = false;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>The custom identifier given by the user to the address entry.</summary>
		public string Identifier { get { return m_identifier; } set { m_identifier = value; SendPropertyChangedNotification(); } }
		/// <summary>The actual memory address used to perform read/write operations.</summary>
		public IntPtr Address { get { return m_address; } set { m_address = value; SendPropertyChangedNotification(); } }
		/// <summary>
		///    The <see cref="Type"/> used to represent the object which should be stored in the target process' memory
		///    space. This is actually obtained by calling <see cref="object.GetType()"/> on the <see cref="Value"/> property.
		/// </summary>
		public Type Type
		{
			get
			{
				return Value.GetType();
			}
			set
			{
				// This property is not backed by any actual field: it depends on the Value property.
				// We must only simulate a "property changed" notification.
				SendPropertyChangedNotification();

				// Update the Value property accordingly, preventing it from recursivelly re-updating the Type property again.
				Type newType = value;
				if ( m_lockValueTypeRecursion == false && newType != Value.GetType() )
				{
					// Block the recursion from happening
					m_lockValueTypeRecursion = true;

					// Cast the Value property accordingly, setting it to zero whenever the cast fails
					object newValue = null;
					Exception caughtException = null;
					try
					{
						try
						{
							newValue = Convert.ChangeType( Value, newType );
						}
						catch ( InvalidCastException ex )
						{
							caughtException = ex;
						}
						catch ( OverflowException ex )
						{
							caughtException = ex;
						}

						// In case of casting errors, try to create a default instance of the new Type
						if ( caughtException != null )
							newValue = Activator.CreateInstance( newType );

						// Update the value
						this.Value = newValue;
					}
					finally
					{
						// Unblock the recursion
						m_lockValueTypeRecursion = false;
					}
				}
			}
		}
		/// <summary>
		///    <para>
		///       The value associated to the address.
		///       The <see cref="Type"/> property is obtained by calling the <see cref="object.GetType()"/> method on
		///       the <see cref="Value"/> property. Setting this value also simmulates an update to the <see cref="Type"/> property.
		///    </para>
		///    <para>
		///       If <see cref="Freeze"/> is set to <code>true</code>, this value represents the value that should be frozen
		///       on the target process' memory space.
		///       Else, this value represents the last value read from the address on the target process' memory space.
		///     </para>
		/// </summary>
		public Object Value
		{
			get
			{
				return m_value;
			}
			set
			{
				// Update the property
				if ( m_lockValueTypeRecursion == false )
				{
					// When the user updates the value through the UI, the "value" is received as a string here...
					// The following ChangeType() converts the "value" from string, keeping the Type of the value unchanged
					object oldValue = m_value;
					bool revertToOldValue = false;
					try
					{
						m_value = Convert.ChangeType( value, this.Type );
					}
					catch ( OverflowException )
					{
						revertToOldValue = true;
					}
					catch ( FormatException )
					{
						revertToOldValue = true;
					}

					// Whenever an error happens, the value is reverted back to the old value
					if ( revertToOldValue )
						this.Value = oldValue;
				}
				else 
				{
					// If the code falls here, it means the user has updted the "Type" of the AddressData object.
					// We must forcivelly update the value to the new value, which was already correctly set by
					// the this.Type setter method.
					m_value = value;
				}
				SendPropertyChangedNotification();

				// Update the Type property, preventing it from recursivelly re-updating the Value property again.
				if ( m_lockValueTypeRecursion == false && m_value.GetType() != this.Type )
				{
					m_lockValueTypeRecursion = true;
					this.Type = value.GetType();
					m_lockValueTypeRecursion = false;
				}
			}
		}
		/// <summary>
		///    A flag specifying if the value in the address should be "frozen" by the application.
		///    A "frozen" value means that the application periodically replaces that value with another previously
		///    specified value.
		/// </summary>
		public Boolean Freeze {
			get {
				return m_freeze;
			}
			set {
				m_freeze = value;
				SendPropertyChangedNotification();
			}
		}
		/// <summary>
		///    A flag indicating if the <see cref="Value"/> of this <see cref="AddressData"/> object is currently being edited by the user.
		///    When this flag is true, the application stops reading and overwriting the value associated to the <see cref="AddressData"/>,
		///    so that the user can edit it without the "automatic refreshment" of the application replacing the contents the user is editing.
		/// </summary>
		public Boolean IsValueBeingEdited { get; set; }
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="identifier">The identifier of the registered address.</param>
		/// <param name="address">The actual address that was registered (in the target process' memory space).</param>
		/// <param name="value">The contents of the registered address.</param>
		/// <param name="freeze">A flag specifying if the contents of the registered address should be frozen.</param>
		public AddressData( string identifier, IntPtr address, Object value, bool freeze )
		{
			m_identifier = identifier;
			m_address = address;
			m_value = value;
			m_freeze = freeze;
		}
		#endregion
	}
}
