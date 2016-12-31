using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAMvader
{
	/// <summary>
	///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
	///    absolute/static/constant addresses.
	/// </summary>
	public class AbsoluteMemoryAddress : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>Represents the actual absolute/static/constant address represented by this instance.</summary>
		private IntPtr m_realAddress;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( IntPtr address )
		{
			m_realAddress = address;
		}


		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( int address )
		{
			m_realAddress = new IntPtr( address );
		}


		/// <summary>Constructor.</summary>
		/// <param name="address">The absolute/static/constant address associated to this instance.</param>
		public AbsoluteMemoryAddress( long address )
		{
			m_realAddress = new IntPtr( address );
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
