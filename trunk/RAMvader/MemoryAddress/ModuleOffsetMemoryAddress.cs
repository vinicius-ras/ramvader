using System;
using System.Diagnostics;

namespace RAMvader
{
	/// <summary>
	///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
	///    addresses that are calculated as offsets from a given module of the process to which
	///    the <see cref="RAMvaderTarget"/> is attached.
	/// </summary>
	public class ModuleOffsetMemoryAddress : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>Keeps the real address associated to this instance.</summary>
		private IntPtr m_realAddress;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="target">A reference to the object used to access the target <see cref="Process"/>' address space.</param>
		/// <param name="moduleName">The name of the module whose base address will be used to calculate the final (real) address.</param>
		/// <param name="offset">The offset to apply to the base of the given module in order to find the final (real) address.</param>
		/// <exception cref="ModuleNotFoundException">Thrown when a module with the given name has not been found in the target process' modules list.</exception>
		public ModuleOffsetMemoryAddress( RAMvaderTarget target, String moduleName, int offset )
		{
			// Instantiating this object requires an attachment to a target process
			if ( target.IsAttached() == false )
				throw new InstanceNotAttachedException();

			// Request the target process' module address, and calculate the real address represented by this instance
			IntPtr moduleBaseAddress = target.GetTargetProcessModuleBaseAddress(moduleName);
			if ( moduleBaseAddress == IntPtr.Zero )
				throw new ModuleNotFoundException( moduleName );
			m_realAddress = IntPtr.Add( moduleBaseAddress, offset );
		}
		#endregion








		#region OVERRIDEN ABSTRACT METHODS: MemoryAddress
		/// <summary>
		///    Specialized by subclasses to calculate the real address associated with
		///    the <see cref="MemoryAddress"/> object.
		/// </summary>
		/// <returns>Returns an <see cref="IntPtr"/> representing the real/calculated address associated to the <see cref="MemoryAddress"/> instance.</returns>
		protected override IntPtr GetRealAddress()
		{
			return m_realAddress;
		}
		#endregion
	}
}
