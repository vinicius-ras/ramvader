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
	///    Exception thrown when the compiler reports an unexpected size for a basic type handled by the RAMvader library.
	///    This exception exists for safety purposes only, and should never be thrown on standard development environments.
	/// </summary>
	public class UnexpectedDataTypeSizeException : RAMvaderException
	{
		/// <summary>Constructor.</summary>
		/// <param name="msg">The message to be associated with the exception.</param>
		public UnexpectedDataTypeSizeException( string msg )
			: base( msg )
		{
		}
	}
}
