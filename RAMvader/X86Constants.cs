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
    static class X86Constants
	{
		#region PUBLIC CONSTANTS
		/// <summary>The opcode for the NOP instruction.</summary>
		public const byte OPCODE_NOP = 0x90;
		/// <summary>The byte value for the INT3 instruction.</summary>
		public const byte OPCODE_INT3 = 0xCC;
        /// <summary>Used to indicate to some methods that the client code doesn't care about the generated instruction size.</summary>
        public const int INSTRUCTION_SIZE_ANY = -1;
        /// <summary>The size of any JCC (collective nomenclature used for any "conditional jump") instructions which take a "rel8" (1-byte sized) displacement.</summary>
        public const int INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8 = 2;
        /// <summary>The size of any JCC (collective nomenclature used for any "conditional jump") instructions which take a "rel32" (4-byte sized) displacement.</summary>
        public const int INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32 = 6;
        /// <summary>The size of the instruction identified by <see cref="EX86BranchInstructionType.evJmpShortRelative8"/>.</summary>
        public const int INSTRUCTION_SIZE_JMP_SHORT_RELATIVE8 = 2;
        /// <summary>The size of the instruction identified by <see cref="EX86BranchInstructionType.evJmpNearRelative32"/>.</summary>
        public const int INSTRUCTION_SIZE_JMP_NEAR_RELATIVE32 = 5;
        /// <summary>The size of the instruction identified by <see cref="EX86BranchInstructionType.evCallNearRelative32"/>.</summary>
        public const int INSTRUCTION_SIZE_CALL_NEAR_RELATIVE32 = 5;
        #endregion
    }
}
