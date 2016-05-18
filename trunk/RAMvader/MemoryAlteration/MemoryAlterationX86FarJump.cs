﻿using System;

namespace RAMvader.CodeInjection
{
	/// <summary>Represents a memory alteration that overwrites instructions of the target process' memory space with x86 FAR JUMP instruction.</summary>
	public class MemoryAlterationX86FarJump : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/// <summary>The identifier of the code cave to which the JUMP instruction will make the target process' code flow.</summary>
		private Enum m_codeCaveID;
		/// <summary>The identifier of the type of x86 FAR JUMP instruction to be generated.</summary>
		private EJumpInstructionType m_jumpType;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="targetIORef">
		///    A reference to the <see cref="RAMvaderTarget"/> object that will be used to read the target process' memory space.
		///    This <see cref="RAMvaderTarget"/> MUST be attached to a process, as it will be used in this constructor method to read the process'
		///    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		///    whenever <see cref="MemoryAlterationX86FarJump.SetEnabled{TMemoryAlterationID, TCodeCave, TVariable}(Injector{TMemoryAlterationID, TCodeCave, TVariable}, bool)"/> is called to deactivate a memory alteration.
		/// </param>
		/// <param name="targetAddress">The address of the instruction(s) that will be replaced x86 NEAR JUMP instruction.</param>
		/// <param name="targetCodeCaveID">The target code cave, to which the code should be diverted.</param>
		/// <param name="jumpInstructionType">The specific type of jump instruction that should be generated.</param>
		/// <param name="instructionSize">The size of the instruction(s) that will be replaced with NOP instructions.</param>
		public MemoryAlterationX86FarJump( RAMvaderTarget targetIORef, IntPtr targetAddress,
			Enum targetCodeCaveID, EJumpInstructionType jumpInstructionType, int instructionSize )
			: base( targetIORef, targetAddress, instructionSize )
		{
			m_codeCaveID = targetCodeCaveID;
			m_jumpType = jumpInstructionType;
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
			// This method fails if the specified code cave identifier doesn't identify one of the Injector's code caves.
			if ( m_codeCaveID is TCodeCave == false )
				throw new RAMvaderException( string.Format(
					"[{0}] Cannot enable/disable {0}: failed to divert target process' code flow to code cave identified by '{1}' - the given {2} can only handle code caves identified by the enumerated type '{3}'!",
					this.GetType().Name, m_codeCaveID.ToString(), injectorRef.GetType().Name, typeof( TCodeCave ).Name ) );

			// When enabling: replace the original instruction with a jump instruction.
			// When disabling: replace the instruction with its original bytes.
			if ( bEnable )
				return injectorRef.WriteX86FarJumpToCodeCaveInstruction( m_jumpType, this.TargetAddress, (TCodeCave) (Object) m_codeCaveID, this.TargetOriginalBytes.Length );
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, this.TargetOriginalBytes );
		}
		#endregion
	}
}
