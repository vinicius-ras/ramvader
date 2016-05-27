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
using System.Reflection;

namespace RAMvader.CodeInjection
{
	/// <summary>Keeps the metadata related to an injection variable.</summary>
	[AttributeUsage( AttributeTargets.Field, AllowMultiple=false )]
    public class VariableDefinitionAttribute : Attribute
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





		#region PUBLIC STATIC METHODS
		/// <summary>Utility method which retrieves the <see cref="VariableDefinitionAttribute"/> from the given enumerator value.</summary>
		/// <param name="elm">The enumerator from which the <see cref="VariableDefinitionAttribute"/> should be retrieved.</param>
		/// <returns>
		///    Returns the <see cref="VariableDefinitionAttribute"/> associated with the given enumerator, if any.
		///    Returns null if no <see cref="VariableDefinitionAttribute"/> is associated with the given enumerator.
		/// </returns>
		public static VariableDefinitionAttribute GetVariableDefinitionAttributeFromEnum( Enum elm )
        {
            Type enumType = elm.GetType();
            FieldInfo fieldInfo = enumType.GetField( elm.ToString() );
            return fieldInfo.GetCustomAttribute<VariableDefinitionAttribute>();
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
		public VariableDefinitionAttribute( Object initialValue )
        {
            m_initialValue = initialValue;
        }
        #endregion
    }
}
