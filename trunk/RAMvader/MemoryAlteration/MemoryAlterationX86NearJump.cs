using System;

namespace RAMvader.CodeInjection
{
	/** Represents a memory alteration that overwrites instructions of the target process' memory space with x86 NEAR JUMP instruction.
	 * ATTENTION: It is NOT recommended that you use NEAR JUMP instructions to jump to code caves that belong to automatically-allocated
	 * memory (which is the default behaviour of the #Injector class). If you need to jump to a code cave in automatically-allocated memory spaces,
	 * you should strongly consider using #MemoryAlterationX86FarJump instead of #MemoryAlterationX86NearJump. */
	public class MemoryAlterationX86NearJump : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/** The identifier of the code cave to which the JUMP instruction will make the target process' code flow. */
		private Enum m_codeCaveID;
		/** The identifier of the type of x86 FAR JUMP instruction to be generated. */
		private EJumpInstructionType m_jumpType;
		#endregion





		#region PUBLIC METHODS
		/** Constructor.
		 * @param targetIORef A reference to the #RAMvaderTarget object that will be used to read the target process' memory space.
		 *    This #RAMvaderTarget MUST be attached to a process, as it will be used in this constructor method to read the process'
		 *    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		 *    whenever #MemoryAlterationBase.setEnabled() is called to deactivate a memory alteration.
		 * @param targetAddress The address of the instruction(s) that will be replaced x86 NEAR JUMP instruction.
		 * @param targetCodeCaveID The target code cave, to which the code should be diverted.
		 * @param jumpInstructionType The specific type of jump instruction that should be generated. */
		public MemoryAlterationX86NearJump( RAMvaderTarget targetIORef, IntPtr targetAddress,
			Enum targetCodeCaveID, EJumpInstructionType jumpInstructionType, int instructionSize )
			: base( targetIORef, targetAddress, instructionSize )
		{
			m_codeCaveID = targetCodeCaveID;
			m_jumpType = jumpInstructionType;
		}
		#endregion





		#region PUBLIC ABSTRACT METHODS IMPLEMENTATION: MemoryAlterationBase
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
				return injectorRef.WriteX86NearJumpToCodeCaveInstruction( m_jumpType, this.TargetAddress, (TCodeCave) (Object) m_codeCaveID, this.TargetOriginalBytes.Length );
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, this.TargetOriginalBytes );
		}
		#endregion
	}
}
