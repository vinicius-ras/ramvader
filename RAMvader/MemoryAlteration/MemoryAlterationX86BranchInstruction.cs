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
    /// <summary>Represents a memory alteration that overwrites instructions of the target process' memory space with an x86 branch instruction.</summary>
    public class MemoryAlterationX86BranchInstruction : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/// <summary>The identifier of the type of branching instruction to be generated.</summary>
		private EX86BranchInstructionType m_instructionType;
        /// <summary>The target address where the branching instruction should make the code flow to.</summary>
        private MemoryAddress m_branchTarget;
        #endregion





        #region PUBLIC METHODS
        /// <summary>Constructor.</summary>
        /// <param name="targetIORef">
        ///    A reference to the <see cref="Target"/> object that will be used to read the target process' memory space.
        ///    This <see cref="Target"/> MUST be attached to a process, as it will be used in this constructor method to read the process'
        ///    memory and keep a snapshot of the original bytes at the given 'branchPoint' for restoring their values,
        ///    whenever <see cref="MemoryAlterationX86BranchInstruction.SetEnabled{TMemoryAlterationID, TCodeCave, TVariable}(Injector{TMemoryAlterationID, TCodeCave, TVariable}, bool)"/> is called
        ///    to deactivate a memory alteration.
        /// </param>
        /// <param name="branchPoint">The address of the instruction(s) that will be replaced by the branching instruction.</param>
        /// <param name="branchTarget">The address where the branching instruction will make the target application's code flow to.</param>
        /// <param name="instructionType">The type of branching instruction to be generated.</param>
        /// <param name="instructionSize">The size of the instruction(s) that will be replaced with the generated instruction (and some NOP instructions, whenever paddings are necessary).</param>
        public MemoryAlterationX86BranchInstruction( Target targetIORef, MemoryAddress branchPoint, MemoryAddress branchTarget,
			EX86BranchInstructionType instructionType, int instructionSize )
			: base( targetIORef, branchPoint, instructionSize )
		{
            m_instructionType = instructionType;
            m_branchTarget = branchTarget;
		}
		#endregion





		#region PUBLIC ABSTRACT METHODS IMPLEMENTATION: MemoryAlterationBase
		/// <summary>Called to activate or deactivate a memory alteration into the target process' memory space.</summary>
		/// <typeparam name="TMemoryAlterationID">The enumeration of Memory Alteration Sets used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <typeparam name="TCodeCave">The enumeration of Code Caves used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <typeparam name="TVariable">The enumeration of Injection Variables used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}" />.</typeparam>
		/// <param name="injectorRef">A reference to an <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object, with which you can perform I/O operations on the target process' memory space and access related data, like values and addresses of variables and code caves.</param>
		/// <param name="bEnable">A flag specifying if the memory alteration should be enabled or disabled.</param>
		/// <returns>Returns a flag specifying if the operation was successful or not.</returns>
		public override bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable )
		{
			// When enabling: replace the original instruction with a branching instruction.
			// When disabling: replace the instruction with its original bytes.
			if ( bEnable )
				return injectorRef.WriteX86BranchInstruction( m_instructionType, this.TargetAddress, this.m_branchTarget, this.TargetOriginalBytes.Length );
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, this.TargetOriginalBytes );
		}
		#endregion
	}
}
