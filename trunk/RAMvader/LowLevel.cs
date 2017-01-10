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
	/// <summary>This class is used to keep low-level definitions, such as opcodes that can be used to generate x86 code.</summary>
	public static class LowLevel
	{
		#region PUBLIC CONSTANTS
		/// <summary>The byte value for the x86 NOP instruction.</summary>
		public static readonly byte OPCODE_x86_NOP = 0x90;
		/// <summary>The byte value for the x86 INT3 instruction.</summary>
		public static readonly byte OPCODE_x86_INT3 = 0xCC;
		/// <summary>Represents the byte value for the x86 CALL instruction.</summary>
		public static readonly byte OPCODE_x86_CALL = 0xE8;
		/// <summary>Represents the byte value for the x86 NEAR JMP instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JMP = 0xEB;
		/// <summary>Represents the byte value for the x86 NEAR JA instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JA  = 0x77;
		/// <summary>Represents the byte value for the x86 NEAR JAE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JAE = 0x73;
		/// <summary>Represents the byte value for the x86 NEAR JB instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JB  = 0x72;
		/// <summary>Represents the byte value for the x86 NEAR JBE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JBE = 0x76;
		/// <summary>Represents the byte value for the x86 NEAR JG instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JG  = 0x7F;
		/// <summary>Represents the byte value for the x86 NEAR JGE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JGE = 0x7D;
		/// <summary>Represents the byte value for the x86 NEAR JL instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JL  = 0x7C;
		/// <summary>Represents the byte value for the x86 NEAR JLE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JLE = 0x7E;
		/// <summary>Represents the byte value for the x86 NEAR JE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JE  = 0x74;
		/// <summary>Represents the byte value for the x86 NEAR JNE instruction.</summary>
		public static readonly byte OPCODE_x86_NEAR_JNE = 0x75;
		/// <summary>Represents the byte value for the x86 FAR JMP instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JMP = { 0xE9 };
		/// <summary>Represents the byte value for the x86 FAR JA instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JA  = { 0x0F, 0x87 };
		/// <summary>Represents the byte value for the x86 FAR JAE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JAE = { 0x0F, 0x83 };
		/// <summary>Represents the byte value for the x86 FAR JB instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JB  = { 0x0F, 0x82 };
		/// <summary>Represents the byte value for the x86 FAR JBE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JBE = { 0x0F, 0x86 };
		/// <summary>Represents the byte value for the x86 FAR JG instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JG  = { 0x0F, 0x8F };
		/// <summary>Represents the byte value for the x86 FAR JGE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JGE = { 0x0F, 0x8D };
		/// <summary>Represents the byte value for the x86 FAR JL instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JL  = { 0x0F, 0x8C };
		/// <summary>Represents the byte value for the x86 FAR JLE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JLE = { 0x0F, 0x8E };
		/// <summary>Represents the byte value for the x86 FAR JE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JE  = { 0x0F, 0x84 };
		/// <summary>Represents the byte value for the x86 FAR JNE instruction.</summary>
		public static readonly byte [] OPCODE_x86_FAR_JNE = { 0x0F, 0x85 };
		/// <summary>The size of a x86 CALL instruction, given in bytes.</summary>
		public const int INSTRUCTION_SIZE_x86_CALL = 5;
		/// <summary>
		///    The size of a x86 NEAR JUMP instruction, given in bytes.
		///    Near jumps allow jumps to instructions up to a distance of 0xFF bytes.
		/// </summary>
		public const int INSTRUCTION_SIZE_x86_NEAR_JUMP = 2;
		/// <summary>The size of a x86 FAR JUMP instruction, given in bytes.</summary>
		public const int INSTRUCTION_SIZE_x86_FAR_JUMP = 6;
		#endregion
	}
}
