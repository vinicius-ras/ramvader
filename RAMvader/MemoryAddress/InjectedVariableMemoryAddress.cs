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

using RAMvader.CodeInjection;
using System;
using System.Diagnostics;

namespace RAMvader
{
    /// <summary>
    ///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
    ///    addresses of variables that get injected by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
    ///    into a target process' memory space.
    /// </summary>
    /// <typeparam name="TMemoryAlterationSetID">
    ///    An enumerated type which specifies the identifiers for Memory Alteration Sets
    ///    that can be enabled or disabled into the target process' memory space.
    /// </typeparam>
    /// <typeparam name="TCodeCave">An enumerated type which specifies the identifiers for code caves.</typeparam>
    /// <typeparam name="TVariable">
    ///    An enumerated type which specifies the identifiers for variables to be injected at the
    ///    target process.
    /// </typeparam>
    public class InjectedVariableMemoryAddress<TMemoryAlterationSetID, TCodeCave, TVariable> : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>
		///    A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which is used
		///    to inject the variable into the <see cref="Process"/>' address space.
		/// </summary>
		private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
		/// <summary>The identifier of the variable whose injection address is to be retrieved.</summary>
		private TVariable m_variableId;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="injector">
		///    A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which is used
		///    to inject the code cave into the <see cref="Process"/>' address space.
		/// </param>
		/// <param name="variableId">The identifier of the variable whose injection address is to be retrieved.</param>
		public InjectedVariableMemoryAddress( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector,
			TVariable variableId )
		{
			m_injector = injector;
			m_variableId = variableId;
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
			IntPtr injectedVariableAddress = m_injector.GetInjectedVariableAddress( m_variableId ).Address;
			return injectedVariableAddress;
		}
		#endregion
	}
}
