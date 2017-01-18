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

namespace RAMvader
{
	/// <summary>
	///    Exception thrown when the identifier given for a code cave does not match the expected enumeration type of identifiers
	///    for code caves of an <see cref="CodeInjection.Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.
	/// </summary>
	public class UnmatchedCodeCaveIdentifierException : RAMvaderException
	{
		/// <summary>Constructor.</summary>
		/// <param name="msg">The message to be associated with the exception.</param>
		public UnmatchedCodeCaveIdentifierException( string msg )
			: base( msg )
		{
		}
	}
}
