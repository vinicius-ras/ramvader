using System;

namespace RAMvader.CodeInjection
{
	/// <summary>Base class for all memory alterations that can be performed through the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.</summary>
	public abstract class MemoryAlterationBase : NotifyPropertyChangedAdapter
	{
		#region PRIVATE FIELDS
		/// <summary>Backs the <see cref="TargetOriginalBytes"/> property.</summary>
		private byte [] m_targetOriginalBytes;
		/// <summary>Backs the <see cref="TargetAddress"/> property.</summary>
		private MemoryAddress m_targetAddress;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>
		///    Keeps the bytes that represent the original instruction or value that was in
		///    memory before the memory alteration got activated.
		/// </summary>
		public byte[] TargetOriginalBytes
		{
			get { return m_targetOriginalBytes; }
			protected set { m_targetOriginalBytes = value; SendPropertyChangedNotification(); }
		}
		/// <summary>The address (in the target process' memory space) where the memory alteration will occur.</summary>
		public MemoryAddress TargetAddress
		{
			get { return m_targetAddress; }
			protected set { m_targetAddress = value; SendPropertyChangedNotification(); }
		}
		#endregion





		#region PUBLIC ABASTRACT METHODS
		/// <summary>Called to activate or deactivate a memory alteration into the target process' memory space.</summary>
		/// <typeparam name="TMemoryAlterationID">The enumeration of Memory Alteration Sets used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TCodeCave">The enumeration of Code Caves used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TVariable">The enumeration of Injection Variables used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <param name="injectorRef">A reference to an <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object, with which you can perform I/O operations on the target process' memory space and access related data, like values and addresses of variables and code caves.</param>
		/// <param name="bEnable">A flag specifying if the memory alteration should be enabled or disabled.</param>
		/// <returns>Returns a flag specifying if the operation was successful or not.</returns>
		public abstract bool SetEnabled<TMemoryAlterationID, TCodeCave, TVariable>(
			Injector<TMemoryAlterationID, TCodeCave, TVariable> injectorRef, bool bEnable );
		#endregion





		#region PROTECTED METHODS
		/// <summary>
		///    Constructor.
		///    The constructor will read the bytes of the target process' memory space and keep a "snapshot" of it at the moment the
		///    constructor is called. When a memory alteration gets disabled, this "snapshot" will be used to restore the original
		///    bytes of the instruction or value that was in the target process' memory space before it was activated.
		/// </summary>
		/// <param name="targetIORef">A reference to the <see cref="RAMvaderTarget"/> object that will be used to read the target process' memory space.</param>
		/// <param name="targetAddress">The address (in the target process' memory space) where the memory alteration will take place.</param>
		/// <param name="targetSize">The size - given in bytes - of the instruction or value that the memory alteration will affect.</param>
		/// <exception cref="InstanceNotAttachedException">Thrown when the method is called while the given <see cref="RAMvaderTarget"/> is not attached to a process.</exception>
		/// <exception cref="RequiredReadException">
		///    Thrown when the method cannot successfully read the target process' memory space to retrieve the original bytes
		///    that the Memory Alteration will be replacing, once it is activated.
		/// </exception>
		protected MemoryAlterationBase( RAMvaderTarget targetIORef, MemoryAddress targetAddress, int targetSize )
		{
			// The RAMvaderTarget MUST be attached for creating a MemoryAlterationBase instance
			if ( targetIORef.Attached == false )
				throw new InstanceNotAttachedException();

			// Read the original bytes from the target process' memory space
			byte [] originalBytes = new byte[targetSize];
			if ( targetIORef.ReadFromTarget( targetAddress, originalBytes ) == false )
				throw new RequiredReadException( string.Format(
					"[{0}] Failed to create a Memory Alteration instance: failed to read original bytes in the target process' memory space!",
					this.GetType().Name ) );

			// Update internal data
			this.TargetAddress = targetAddress;
			this.TargetOriginalBytes = originalBytes;
		}
		#endregion
	}
}
