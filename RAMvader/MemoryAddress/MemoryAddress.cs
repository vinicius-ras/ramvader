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
	///    <para>
	///       Represents a memory address for the RAMvader library.
	///       Memory addresses should be READ-ONLY: once they're created, the real memory address they
	///       represent internally must not be changed.
	///    </para>
	///    <para>
	///       Although they're read-only, the real addresses they represent are dynamically calculated
	///       when a call to <see cref="GetRealAddress"/>, and are recalculated everytime this method gets called.
	///       The only exception to this rececalculation rule is for the <see cref="AbsoluteMemoryAddress"/> class,
	///       which represent an absolute address that never changes (and thus is cached inside the instance of this class).
	///    </para>
	/// </summary>
	public abstract class MemoryAddress
	{
		#region PUBLIC PROPERTIES
		/// <summary>
		///    Retrieves the real/calculated address represented by the <see cref="MemoryAddress"/> object.
		/// </summary>
		public IntPtr Address
		{
			get { return this.GetRealAddress(); }
		}
		#endregion





		#region PROTECTED METHODS
		/// <summary>
		///    Specialized by subclasses to calculate the real address associated with
		///    the <see cref="MemoryAddress"/> object.
		/// </summary>
		/// <returns>Returns an <see cref="IntPtr"/> representing the real/calculated address associated to the <see cref="MemoryAddress"/> instance.</returns>
		protected abstract IntPtr GetRealAddress();
		#endregion
	}
}
