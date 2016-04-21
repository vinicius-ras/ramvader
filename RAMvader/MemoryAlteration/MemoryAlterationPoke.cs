using System;

namespace RAMvader.CodeInjection
{
	/** Represents a memory alteration that overwrites instructions of the target process' memory space with custom bytes. */
	public class MemoryAlterationPoke : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/** Keeps the custom bytes that will replace the target instruction(s). */
		private byte [] m_customBytes;
		#endregion





		#region PUBLIC METHODS
		/** Constructor.
		 * @param targetIORef A reference to the #RAMvaderTarget object that will be used to read the target process' memory space.
		 *    This #RAMvaderTarget MUST be attached to a process, as it will be used in this constructor method to read the process'
		 *    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		 *    whenever #MemoryAlterationBase.setEnabled() is called to deactivate a memory alteration.
		 * @param targetAddress The address of the instruction(s) that will be replaced with custom bytes.
		 * @param customBytes The bytes which will replace the instruction(s) at the given address.
		 *    The length of this array is used to determine the size of the instructions to be replaced at that address. */
		public MemoryAlterationPoke( RAMvaderTarget targetIORef, IntPtr targetAddress, byte [] customBytes )
			: base( targetIORef, targetAddress, customBytes.Length )
		{
			m_customBytes = customBytes;
		}
		#endregion





		#region PUBLIC ABSTRACT METHODS IMPLEMENTATION: MemoryAlterationBase
		public override bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable )
		{
			// When enabling: replace the original instruction with the defined custom bytes.
			// When disabling: replace the instruction with its original bytes.
			byte [] bytesToWrite;
			if ( bEnable )
				bytesToWrite = m_customBytes;
			else
				bytesToWrite = this.TargetOriginalBytes;

			// Write data into the target process' memory space
			return injectorRef.TargetProcess.WriteToTarget( this.TargetAddress, bytesToWrite );
		}
		#endregion
	}
}
