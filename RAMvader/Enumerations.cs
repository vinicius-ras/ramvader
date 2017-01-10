﻿/*
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

/* This file implements the basic enumerations used on the library. */

namespace RAMvader
{
	/// <summary>Defines the possible endianness options which RAMvader can operate on.</summary>
	public enum EEndianness
	{
		/// <summary>A value indicating that RAMvader should operate in the same endianness as the process that RAMvader is running on.</summary>
		evEndiannessDefault,
		/// <summary>A value indicating that RAMvader should operate in Little-Endian byte order.</summary>
		evEndiannessLittle,
		/// <summary>A value indicating that RAMvader should operate in Big-Endian byte order.</summary>
		evEndiannessBig,
	}


	/// <summary>Defines the supported pointer sizes for the target process.</summary>
	public enum EPointerSize
	{
		/// <summary>
		///    The default pointer size configuration, where the target process' pointer size is assumed to be the same as the pointer
		///    size of the process which runs RAMvader. The pointer size can be retrieved through IntPtr.Size.
		/// </summary>
		evPointerSizeDefault,
		/// <summary>Explicitly identifies a 32-bit pointer.</summary>
		evPointerSize32,
		/// <summary>Explicitly identifies a 64-bit pointer.</summary>
		evPointerSize64,
	}


	/// <summary>Defines how errors with different pointer sizes are handled by the library.</summary>
	public enum EDifferentPointerSizeError
	{
		/// <summary>
		///    Throws an exception if the target process and the process which runs RAMvader have different pointer sizes.
		///    This is the default behaviour, for safety reasons.
		/// </summary>
		evThrowException,
		/// <summary>
		///    If the target process and the process which uses RAMvader have different pointer sizes, operations with pointers truncate
		///    the pointers to 32-bits when necessary. If any data is lost during the truncation process, a <see cref="PointerDataLostException"/>
		///    is thrown.
		/// </summary>
		evSafeTruncation,
		/// <summary>
		///    If the target process and the process which uses RAMvader have different pointer sizes, operations with pointers truncate the
		///    pointers to 32-bits when necessary. If any data is lost during the truncation process, nothing happens. Thus, this is the less
		///    recommended option and should be used with caution.
		/// </summary>
		evUnsafeTruncation,
	}


	/// <summary>Defines the types of JUMP instructions that can be generated by the <see cref="CodeInjection.Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.</summary>
	public enum EJumpInstructionType
	{
		/// <summary>Identifies the jump instruction: JMP ("unconditional jump).</summary>
		evJMP,
		/// <summary>Identifies the jump instruction: JA ("jump if above" - for unsigned values).</summary>
		evJA,
		/// <summary>Identifies the jump instruction: JAE ("jump if above or equal" - for unsigned values).</summary>
		evJAE,
		/// <summary>Identifies the jump instruction: JB ("jump if below" - for unsigned values).</summary>
		evJB,
		/// <summary>Identifies the jump instruction: JBE ("jump if below or equal" - for unsigned values).</summary>
		evJBE,
		/// <summary>Identifies the jump instruction: JG ("jump if greater than" - for signed values).</summary>
		evJG,
		/// <summary>Identifies the jump instruction: JGE ("jump if greater than or equal to" - for signed values).</summary>
		evJGE,
		/// <summary>Identifies the jump instruction: JL ("jump if less than" - for signed values).</summary>
		evJL,
		/// <summary>Identifies the jump instruction: JLE ("jump if less than or equal to" - for signed values).</summary>
		evJLE,
		/// <summary>Identifies the jump instruction: JE ("jump if equal").</summary>
		evJE,
		/// <summary>Identifies the jump instruction: JNE ("jump if not equal").</summary>
		evJNE,
	}
}
