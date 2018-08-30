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

using RAMvader.Attributes;
using RAMvader.CodeInjection;
using System;

namespace RAMvader
{
    /// <summary>
    ///    Enumeration providing the x86 branch instruction types, used for changing the flow of execution of a target application's
    ///    code. Branching can be used, for example, to make the target application execute code that you have
    ///    injected in the target process' memory space. Corresponding instruction names and opcodes are listed for
    ///    each enumerator, following the same conventions as seen in the Instruction Set Reference section of
    ///    the document: “Intel® 64 and IA-32 Architectures Software Developer’s Manual V2” (available online free
    ///    of charges code is being published).
    /// </summary>
    public enum EX86BranchInstructionType
    {
        /// <summary>Identifies the branch instruction "JMP rel8" (opcodes: EB cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JMP_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0xEB })]
        evJmpShortRelative8,
        /// <summary>Identifies the branch instruction "JA rel8" (opcodes: 77 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x77 })]
        evJaShortRelative8,
        /// <summary>Identifies the branch instruction "JAE rel8" (opcodes: 73 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x73 })]
        evJaeShortRelative8,
        /// <summary>Identifies the branch instruction "JB rel8" (opcodes: 72 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x72 })]
        evJbShortRelative8,
        /// <summary>Identifies the branch instruction "JBE rel8" (opcodes: 76 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x76 })]
        evJbeShortRelative8,
        /// <summary>Identifies the branch instruction "JG rel8" (opcodes: 7F cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x7F })]
        evJgShortRelative8,
        /// <summary>Identifies the branch instruction "JGE rel8" (opcodes: 7D cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x7D })]
        evJgeShortRelative8,
        /// <summary>Identifies the branch instruction "JL rel8" (opcodes: 7C cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x7C })]
        evJlShortRelative8,
        /// <summary>Identifies the branch instruction "JLE rel8" (opcodes: 7E cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x7E })]
        evJleShortRelative8,
        /// <summary>Identifies the branch instruction "JE rel8" (opcodes: 74 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x74 })]
        evJeShortRelative8,
        /// <summary>Identifies the branch instruction "JNE rel8" (opcodes: 75 cb).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(SByte), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_SHORT_RELATIVE8, MainOpcodeBytes = new byte[] { 0x75 })]
        evJneShortRelative8,
        /// <summary>Identifies the branch instruction "JA rel32" (opcodes: 0F 87 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x87 })]
        evJaNearRelative32,
        /// <summary>Identifies the branch instruction "JAE rel32" (opcodes: 0F 83 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x83 })]
        evJaeNearRelative32,
        /// <summary>Identifies the branch instruction "JB rel32" (opcodes: 0F 82 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x82 })]
        evJbNearRelative32,
        /// <summary>Identifies the branch instruction "JBE rel32" (opcodes: 0F 86 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x86 })]
        evJbeNearRelative32,
        /// <summary>Identifies the branch instruction "JG rel32" (opcodes: 0F 8F cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x8F })]
        evJgNearRelative32,
        /// <summary>Identifies the branch instruction "JGE rel32" (opcodes: 0F 8D cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x8D })]
        evJgeNearRelative32,
        /// <summary>Identifies the branch instruction "JL rel32" (opcodes: 0F 8C cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x8C })]
        evJlNearRelative32,
        /// <summary>Identifies the branch instruction "JLE rel32" (opcodes: 0F 8E cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x8E })]
        evJleNearRelative32,
        /// <summary>Identifies the branch instruction "JE rel32" (opcodes: 0F 84 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x84 })]
        evJeNearRelative32,
        /// <summary>Identifies the branch instruction "JNE rel32" (opcodes: 0F 85 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JCC_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0x0F, 0x85 })]
        evJneNearRelative32,
        /// <summary>Identifies the branch instruction "JMP rel32" (opcodes: E9 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_JMP_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0xE9 })]
        evJmpNearRelative32,
        /// <summary>Identifies the branch instruction "CALL rel32" (opcodes: E8 cd).</summary>
        [X86BranchInstructionMetadata(OffsetType = typeof(Int32), TotalInstructionSize = X86Constants.INSTRUCTION_SIZE_CALL_NEAR_RELATIVE32, MainOpcodeBytes = new byte[] { 0xE8 })]
        evCallNearRelative32,
    }
}
