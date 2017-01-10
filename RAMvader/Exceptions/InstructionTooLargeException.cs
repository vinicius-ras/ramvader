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
	///    Exception thrown when the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> tries
	///    to generate an instruction whose size (in bytes) is larger than the space given for the generation of that instruction.
	/// </summary>
	public class InstructionTooLargeException : InjectorException
	{
		/// <summary>Constructor.</summary>
		/// <param name="givenSize">The size given for the instruction to be generated.</param>
		/// <param name="requiredSize">The size that is actually required to generate the instruction.</param>
		public InstructionTooLargeException( int givenSize, int requiredSize )
			: base( string.Format( "Instruction was given {1} bytes of space to be generated, while it requires {0} bytes." ) )
		{
		}
	}
}
