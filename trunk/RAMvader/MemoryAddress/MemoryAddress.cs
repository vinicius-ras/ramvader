using System;

namespace RAMvader
{
	/// <summary>
	///    Represents a memory address for the RAMvader library.
	///    Memory addresses should be READ-ONLY: once they're created, the real memory address they
	///    represent internally must not be changed.
	/// </summary>
	public abstract class MemoryAddress
	{
		#region PUBLIC PROPERTIES
		/// <summary>
		///    Retrieves the real/calculated address represented by the <see cref="MemoryAddress"/> object.
		/// </summary>
		public IntPtr Address
		{
			get { return this.GetRealAddress(); }
		}
		#endregion





		#region PROTECTED METHODS
		/// <summary>
		///    Specialized by subclasses to calculate the real address associated with
		///    the <see cref="MemoryAddress"/> object.
		/// </summary>
		/// <returns>Returns an <see cref="IntPtr"/> representing the real/calculated address associated to the <see cref="MemoryAddress"/> instance.</returns>
		protected abstract IntPtr GetRealAddress();
		#endregion
	}
}
