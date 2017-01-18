/*
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

using System;

namespace RAMvader
{
	/// <summary>
	///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
	///    absolute/static/constant addresses.
	/// </summary>
	public class AbsoluteMemoryAddress : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>Represents the actual absolute/static/constant address represented by this instance.</summary>
		private IntPtr m_realAddress;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( IntPtr address )
		{
			m_realAddress = address;
		}


		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( int address )
		{
			m_realAddress = new IntPtr( address );
		}


		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( long address )
		{
			m_realAddress = new IntPtr( address );
		}
		#endregion





		#region OVERRIDEN ABSTRACT METHODS: MemoryAddress
		/// <summary>
		///    Specialized by subclasses to calculate the real address associated with
		///    the <see cref="MemoryAddress"/> object.
		/// </summary>
		/// <returns>Returns an <see cref="IntPtr"/> representing the real/calculated address associated to the <see cref="MemoryAddress"/> instance.</returns>
		protected override IntPtr GetRealAddress()
		{
			return m_realAddress;
		}
		#endregion
	}
}
