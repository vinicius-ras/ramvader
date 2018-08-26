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
	/// <summary>
	///    Exception thrown when trying to perform an operation involving injection variables, and the data type used
	///    is different from the data type of the injection variable.
	///    Read/write operations on injection variables must ALWAYS match the exact type declared for the injection variable.
	/// </summary>
	public class UnmatchedDataTypeException : InjectorException
	{
		/// <summary>Constructor.</summary>
		/// <param name="givenType">The incorrect type used in the operation wich was tried against the injection variable.</param>
		/// <param name="requiredType">The actual type of the injection variable, which was required for the operation to succeed.</param>
		/// <param name="variableID">The identifier of the variable.</param>
		public UnmatchedDataTypeException( Type givenType, Type requiredType, Enum variableID )
			: base( string.Format(
				"Operation failed because type \"{0}\" does not match the type \"{1}\" required by the injection variable identified by \"{2}\".",
				givenType.Name, requiredType.Name, variableID.ToString() ) )
		{
		}
	}
}
