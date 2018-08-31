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
using RAMvader.Utilities;
using System;

namespace RAMvader.CodeInjection
{
    /// <summary>
    ///    Specialization of the <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class used to add
    ///    an x86 branch instruction to a code cave. Such instructions include jumps, both conditional ("JMP") and
    ///    unconditional (instructions like "JA", "JNE", "JL", etc... collectively known as "JCC" instructions), and procedure
    ///    calling instructions ("CALL").
    /// </summary>
    public class CodeCaveArtifactX86BranchInstruction<TMemoryAlterationSetID, TCodeCave, TVariable> : CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable>
	{
        #region PRIVATE FIELDS
        /// <summary>The specific branch instruction type which needs to be written.</summary>
        private EX86BranchInstructionType m_branchInstructionType;
		/// <summary>The target address to where the branching will be made.</summary>
		private MemoryAddress m_targetAddress;
        #endregion





        #region PUBLIC METHODS
        /// <summary>Constructor.</summary>
        /// <param name="instructionType">The specific type of branch instruction to be generated.</param>
        /// <param name="targetAddress">The address to where the branch will divert the target process' execution flow.</param>
        public CodeCaveArtifactX86BranchInstruction(EX86BranchInstructionType instructionType, MemoryAddress targetAddress )
		{
            m_branchInstructionType = instructionType;
            m_targetAddress = targetAddress;
		}
		#endregion




		#region OVERRIDEN METHODS: CodeCaveArtifact
		/// <summary>
		///    Generates the bytes which correspond to the artifact instance.
		///    These bytes are the ones to be actually written to the target process' memory space by
		///    the <see cref="CodeInjection.Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    during the injection procedure.
		/// </summary>
		/// <returns>
		///    Returns an array of bytes corresponding to the artifact when it is injected in the target
		///    process' memory space.
		/// </returns>
		public override byte[] GenerateArtifactBytes()
		{
			var injectorRef = this.GetLockingInjector();
			IntPtr curInjectionAddress = injectorRef.GetCurrentInjectionAddress();
			MemoryAddress currentInjectionAddress = new AbsoluteMemoryAddress( curInjectionAddress );

			byte [] result = Injector<TMemoryAlterationSetID, TCodeCave, TVariable>.GetX86BranchInstructionBytes(
                m_branchInstructionType, currentInjectionAddress, m_targetAddress );
			return result;
		}


		/// <summary>Retrieves the total size of a given artifact, in bytes.</summary>
		/// <param name="target">
		///    The instance of <see cref="Target"/> that is setup to access the target process' memory space.
		///    This instance is used to know properties of the target process, such as its pointers size.
		/// </param>
		/// <returns>Returns the total size of the artifact, in bytes.</returns>
		public override int GetTotalSize( Target target )
		{
            var instructionMetadata = m_branchInstructionType.GetAttribute<X86BranchInstructionMetadata>();
            return instructionMetadata.TotalInstructionSize;
		}
		#endregion
	}
}
