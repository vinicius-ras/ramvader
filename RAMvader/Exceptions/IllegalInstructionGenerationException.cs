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

namespace RAMvader.CodeInjection
{
    /// <summary>
    ///    Exception thrown when an instruction cannot be generated, because the generated instruction would be illegal.
    ///    An example of that is trying to generate a "short" branching instruction that would try to perform a jump larger than
    ///    the maximum value of a byte, which is impossible.
    /// </summary>
    public class IllegalInstructionGenerationException : InjectorException
	{
		/// <summary>Constructor.</summary>
		/// <param name="msg">The message to be associated with the exception.</param>
		public IllegalInstructionGenerationException( string msg )
			: base( msg )
		{
		}
	}
}
