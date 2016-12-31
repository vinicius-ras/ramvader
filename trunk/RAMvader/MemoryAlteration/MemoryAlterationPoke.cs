using System;

namespace RAMvader.CodeInjection
{
	/// <summary>Represents a memory alteration that overwrites instructions of the target process' memory space with custom bytes.</summary>
	public class MemoryAlterationPoke : MemoryAlterationBase
	{
		#region PRIVATE FIELDS
		/// <summary>Keeps the custom bytes that will replace the target instruction(s).</summary>
		private byte [] m_customBytes;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="targetIORef">
		///    A reference to the <see cref="RAMvaderTarget"/> object that will be used to read the target process' memory space.
		///    This <see cref="RAMvaderTarget"/> MUST be attached to a process, as it will be used in this constructor method to read the process'
		///    memory and keep a snapshot of the original bytes at the given 'targetAddress' for restoring their values,
		///    whenever <see cref="MemoryAlterationPoke.SetEnabled{TMemoryAlterationID, TCodeCave, TVariable}(Injector{TMemoryAlterationID, TCodeCave, TVariable}, bool)"/> is called to deactivate a memory alteration.
		/// </param>
		/// <param name="targetAddress">The address of the instruction(s) that will be replaced with custom bytes.</param>
		/// <param name="customBytes">
		///    The bytes which will replace the instruction(s) at the given address.
		///    The length of this array is used to determine the size of the instructions to be replaced at that address.
		/// </param>
		public MemoryAlterationPoke( RAMvaderTarget targetIORef, MemoryAddress targetAddress, byte [] customBytes )
			: base( targetIORef, targetAddress, customBytes.Length )
		{
			m_customBytes = customBytes;
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
