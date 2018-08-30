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

namespace RAMvader.CodeInjection
{
    /// <summary>Keeps the metadata related to an injection variable.</summary>
    public class VariableDefinition
	{
		#region PRIVATE FIELDS
		/// <summary>
		///    Stores the initial value for the variable. Used to initialize the
		///    variable's value, when it is first injected into the target process'
		///    memory.
		/// </summary>
		private Object m_initialValue;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>Backed by the <see cref="m_initialValue"/> field.</summary>
		public Object InitialValue
		{
			get { return m_initialValue; }
		}
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="initialValue">
		///    The initial value of the variable.
		///    This should be specified with structures from the basic values which are supported by
		///    the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> (Byte, Int32, UInt64, Single, Double, etc.). By providing these structures, you are both telling
		///    the injector about the SIZE of the injected variable and its initial value.
		/// </param>
		/// <exception cref="NullReferenceException">Thrown when the given initial value of the variable is <code>null</code>.</exception>
		public VariableDefinition( Object initialValue )
		{
			if ( initialValue == null )
				throw new NullReferenceException( "The initial value of an injection variable cannot be null!" );
			m_initialValue = initialValue;
		}


		/// <summary>
		///    Retrieves the initial value of the variable.
		///    When the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> injects the variable in
		///    the target process' memory space, it initializes the variable to this value.
		/// </summary>
		/// <returns>Returns the initial value of the variable.</returns>
		public Object GetInitialValue()
		{
			return m_initialValue;
		}


		/// <summary>
		///    <para>Retrieves the <see cref="Type"/> of the injection variable.</para>
		///    <para>
		///       The <see cref="Type"/> of the injection variable is defined to be the same type as the
		///       initial value of the variable, which is passed in its definition's constructor (<see cref="VariableDefinition(object)"/>).
		///       Thus, another way to get the injection variable's type is by calling <see cref="GetInitialValue"/> to
		///       retrieve the initial value of the variable, and then calling <see cref="object.GetType"/> on the
		///       returned value.
		///    </para>
		/// </summary>
		/// <returns>Returns the <see cref="Type"/> of the injection variable.</returns>
		public Type GetInjectionVariableType()
		{
			return m_initialValue.GetType();
		}
		#endregion
	}
}
