using System;
using System.Collections.Generic;

namespace RAMvader.CodeInjection
{
	/** Represents a memory alteration that overwrites instructions of the target process' memory space with x86 CALL instruction. */
	public class MemoryAlterationX86Call : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/** The identifier of the code cave to which the CALL instruction will make the target process' code flow. */
		private Enum m_codeCaveID;
		#endregion





		#region PUBLIC METHODS
		/** Constructor.
		 * @param targetIORef A reference to the #RAMvaderTarget object that will be used to read the target process' memory space.
		 *    This #RAMvaderTarget MUST be attached to a process, as it will be used in this constructor method to read the process'
		 *    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		 *    whenever #MemoryAlterationBase.setEnabled() is called to deactivate a memory alteration.
		 * @param targetAddress The address of the instruction(s) that will be replaced x86 CALL instruction.
		 * @param targetCodeCaveID The target code cave, to which the code should be diverted.
		 * @param instructionSize The size of the instruction(s) that will be replaced with NOP instructions. */
		public MemoryAlterationX86Call( RAMvaderTarget targetIORef, IntPtr targetAddress, Enum targetCodeCaveID, int instructionSize )
			: base( targetIORef, targetAddress, instructionSize )
		{
			m_codeCaveID = targetCodeCaveID;
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

			// When enabling: replace the original instruction with a CALL instruction.
			// When disabling: replace the instruction with its original bytes.
			if ( bEnable )
				return injectorRef.WriteX86CallToCodeCaveInstruction( this.TargetAddress, (TCodeCave) (Object) m_codeCaveID, TargetOriginalBytes.Length );
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, this.TargetOriginalBytes );
		}
		#endregion
	}
}
