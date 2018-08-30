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
using System.Diagnostics;

namespace RAMvader
{
    /// <summary>
    ///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
    ///    addresses that are calculated as offsets from a given module of the process to which
    ///    the <see cref="Target"/> is attached.
    /// </summary>
    public class ModuleOffsetMemoryAddress : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>A reference to the object used to access the target <see cref="Process"/>' address space.</summary>
		private Target m_target;
		/// <summary>The name of the module whose base address will be used to calculate the final (real) address.</summary>
		private String m_moduleName;
		/// <summary>The offset to apply to the base of the given module in order to find the final (real) address.</summary>
		private int m_offset;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="target">A reference to the object used to access the target <see cref="Process"/>' address space.</param>
		/// <param name="moduleName">The name of the module whose base address will be used to calculate the final (real) address.</param>
		/// <param name="offset">The offset to apply to the base of the given module in order to find the final (real) address.</param>
		public ModuleOffsetMemoryAddress( Target target, String moduleName, int offset )
		{
			m_target = target;
			m_moduleName = moduleName;
			m_offset = offset;
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
			// Request the target process' module address, and calculate the real address represented by this instance
			IntPtr moduleBaseAddress = m_target.GetTargetProcessModuleBaseAddress(m_moduleName);
			if ( moduleBaseAddress == IntPtr.Zero )
				throw new ModuleNotFoundException( m_moduleName );

			IntPtr result = IntPtr.Add( moduleBaseAddress, m_offset );
			return result;
		}
		#endregion
	}
}
