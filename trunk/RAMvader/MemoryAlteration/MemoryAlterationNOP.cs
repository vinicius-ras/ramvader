using System;
using System.Linq;

namespace RAMvader.CodeInjection
{
	/** Represents a memory alteration that overwrites instructions of the target process' memory space with NOP instructions. */
	public class MemoryAlterationNOP : MemoryAlterationBase
	{
		#region PUBLIC METHODS
		/** Constructor.
		 * @param targetIORef A reference to the #RAMvaderTarget object that will be used to read the target process' memory space.
		 *    This #RAMvaderTarget MUST be attached to a process, as it will be used in this constructor method to read the process'
		 *    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		 *    whenever #MemoryAlterationBase.setEnabled() is called to deactivate a memory alteration.
		 * @param targetAddress The address of the instruction(s) that will be replaced with NOP instructions.
		 * @param instructionSize The size of the instruction(s) that will be replaced with NOP instructions. */
		public MemoryAlterationNOP( RAMvaderTarget targetIORef, IntPtr targetAddress, int instructionSize )
			: base( targetIORef, targetAddress, instructionSize )
		{
		}
		#endregion





		#region PUBLIC ABSTRACT METHODS IMPLEMENTATION: MemoryAlterationBase
		public override bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable )
		{
			// When enabling: replace the original instruction with several NOP instructions.
			// When disabling: replace the instruction with its original bytes.
			byte [] bytesToWrite;
			if ( bEnable )
				bytesToWrite = Enumerable.Repeat<byte>( LowLevel.OPCODE_x86_NOP, this.TargetOriginalBytes.Length ).ToArray();
			else
				bytesToWrite = this.TargetOriginalBytes;

			// Write data into the target process' memory space
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, bytesToWrite );
		}
		#endregion
	}
}
