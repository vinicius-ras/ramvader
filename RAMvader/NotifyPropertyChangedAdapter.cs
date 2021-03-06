﻿/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 *
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace RAMvader
{
    /// <summary>
    ///    An adapter class to make the implementation of the INotifyPropertyChanged interface easier
    ///    for any class willing to provide that implementation.
    /// </summary>
    public abstract class NotifyPropertyChangedAdapter : INotifyPropertyChanged
	{
		#region INTERFACE IMPLEMENTATION: INotifyPropertyChanged Members
		/// <summary>Used for implementing the <see cref="INotifyPropertyChanged"/> interface.</summary>
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion





		#region PROTECTED STATIC METHODS
		/// <summary>
		///    This method should be called inside PROPERTY SETTER METHODS to notify
		///    listeners of the "property changed" event that the property has been updated.
		/// </summary>
		/// <param name="propertyName">
		///    This parameter is automatically filled with the name of the
		///    updated property by the compiler, as long as it is called with no parameters
		///    inside a property-setter method.
		/// </param>
		protected void SendPropertyChangedNotification( [CallerMemberName] string propertyName = "" )
		{
			if ( PropertyChanged != null )
				PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
		}
		#endregion
	}
}
