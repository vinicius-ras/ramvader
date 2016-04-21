using System;

namespace RAMvader.CodeInjection
{
	/** Base class for all memory alterations that can be performed through the #Injector class. */
	public abstract class MemoryAlterationBase : NotifyPropertyChangedAdapter
	{
		#region PRIVATE FIELDS
		/** Backs the #OriginalInstructionBytes property. */
		private byte [] m_targetOriginalBytes;
		/** Backs the #TargetAddress property. */
		private IntPtr m_targetAddress;
		#endregion





		#region PUBLIC PROPERTIES
		/** Keeps the bytes that represent the original instruction or value that was in memory before the memory
		 * alteration got activated. */
		public byte[] TargetOriginalBytes
		{
			get { return m_targetOriginalBytes; }
			protected set { m_targetOriginalBytes = value; SendPropertyChangedNotification(); }
		}
		/** The address (in the target process' memory space) where the memory alteration will occur. */
		public IntPtr TargetAddress
		{
			get { return m_targetAddress; }
			protected set { m_targetAddress = value; SendPropertyChangedNotification(); }
		}
		#endregion





		#region PUBLIC ABASTRACT METHODS
		/** Called to activate or deactivate a memory alteration into the target process' memory space.
		 * @param injectorRef A reference to an #Injector object, with which you can perform I/O operations on the
		 *    target process' memory space and access related data, like values and addresses of variables and code caves.
		 * @param bEnabled A flag specifying if the memory alteration should be enabled or disabled.
		 * @return Returns a flag specifying if the operation was successful or not. */
		public abstract bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable );
		#endregion





		#region PROTECTED METHODS
		/** Constructor.
		 * The constructor will read the bytes of the target process' memory space and keep a "snapshot" of it at the moment the
		 * constructor is called. When a memory alteration gets disabled, this "snapshot" will be used to restore the original
		 * bytes of the instruction or value that was in the target process' memory space before it was activated.
		 * @param targetIORef A reference to the #RAMvaderTarget object that will be used to read the target process' memory space.
		 * @param targetAddress The address (in the target process' memory space) where the memory alteration will take place.
		 * @param targetSize The size - given in bytes - of the instruction or value that the memory alteration will affect. */
		protected MemoryAlterationBase( RAMvaderTarget targetIORef, IntPtr targetAddress, int targetSize )
		{
			// The RAMvaderTarget MUST be attached for creating a #MemoryAlterationBase instance
			if ( targetIORef.Attached == false )
				throw new RAMvaderException( string.Format(
					"[{0}] Failed to create a Memory Alteration instance: the given {1} instance MUST be attached to a process before creating a Memory Alteration instance!",
					this.GetType().Name, targetIORef.GetType().Name ) );

			// Read the original bytes from the target process' memory space
			byte [] originalBytes = new byte[targetSize];
			if ( targetIORef.ReadFromTarget( targetAddress, originalBytes ) == false )
				throw new RAMvaderException( string.Format(
					"[{0}] Failed to create a Memory Alteration instance: failed to read original bytes in the target process' memory space!",
					this.GetType().Name ) );

			// Update internal data
			this.TargetAddress = targetAddress;
			this.TargetOriginalBytes = originalBytes;
		}
		#endregion
	}
}
