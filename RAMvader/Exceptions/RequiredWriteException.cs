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
	/// <summary>Exception that is thrown when an output/write operation that was required to succeed returns a fail state.</summary>
	public class RequiredWriteException : RequiredIOException
	{
		/// <summary>Constructor.</summary>
		/// <param name="msg">The message associated to the exception.</param>
		public RequiredWriteException( string msg )
			: base( msg )
		{
		}
	}
}
