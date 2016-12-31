using System;
using RAMvader.CodeInjection;
using System.Diagnostics;

namespace RAMvader
{
	/// <summary>
	///    A specialization for the <see cref="MemoryAddress"/> class, used to represent
	///    addresses of code caves that get injected by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
	///    into a target process' memory space.
	///    Due to restrictions in C# generics, this class must be instantiated by calling the static
	///    method <see cref="Instantiate{TMemoryAlterationSetID, TCodeCave, TVariable}(Injector{TMemoryAlterationSetID, TCodeCave, TVariable}, TCodeCave)"/>.
	/// </summary>
	public class InjectedCodeCaveMemoryAddress : MemoryAddress
	{
		#region PRIVATE FIELDS
		/// <summary>Keeps the real address associated to this instance.</summary>
		private IntPtr m_realAddress;
		#endregion





		#region PUBLIC STATIC METHODS
		/// <summary>
		///    Creates a new instance of the <see cref="InjectedCodeCaveMemoryAddress"/> class.
		///    This method is necessary (instead of using the class' constructor) due to restrictions in C# generics.
		/// </summary>
		/// <typeparam name="TMemoryAlterationSetID">
		///    An enumerated type which specifies the identifiers for Memory Alteration Sets
		///    that can be enabled or disabled into the target process' memory space.
		/// </typeparam>
		/// <typeparam name="TCodeCave">
		///    An enumerated type which specifies the identifiers for code caves. Each enumerator belonging to this enumeration
		///    should have the <see cref="CodeCaveDefinitionAttribute"/> attribute.
		/// </typeparam>
		/// <typeparam name="TVariable">
		///    An enumerated type which specifies the identifiers for variables to be injected at the target process.
		///    Each enumerator belonging to this enumeration should have the <see cref="VariableDefinitionAttribute"/> attribute.
		/// </typeparam>
		/// <param name="injector">
		///    A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which was used
		///    to inject the code cave into the <see cref="Process"/>' address space.
		/// </param>
		/// <param name="codeCaveId">The identifier of the code cave whose injection address is to be retrieved.</param>
		/// <returns>Returns the newly created instance of the <see cref="InjectedCodeCaveMemoryAddress"/> class.</returns>
		/// <exception cref="InstanceNotInjectedException">
		///    Thrown when the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance
		///    is not in the "injected" state.
		///    The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> is put in "injected" state when a call
		///    to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject()"/>
		///    or <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject(MemoryAddress)"/> is made.
		/// </exception>
		public static InjectedCodeCaveMemoryAddress Instantiate<TMemoryAlterationSetID, TCodeCave, TVariable>( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector, TCodeCave codeCaveId )
		{
			if ( injector.IsInjected == false )
				throw new InstanceNotInjectedException();

			// Retrieve the address of the injected variable
			IntPtr injectedCodeCaveAddress = injector.GetInjectedCodeCaveAddress( codeCaveId );

			// Create and return the new instance
			InjectedCodeCaveMemoryAddress newInstance = new InjectedCodeCaveMemoryAddress( injectedCodeCaveAddress );
			return newInstance;
		}
		#endregion





		#region PRIVATE METHODS
		/// <summary>
		///    Constructor.
		///    Due to restrictions in C#'s generics,
		///    the <see cref="Instantiate{TMemoryAlterationSetID, TCodeCave, TVariable}(Injector{TMemoryAlterationSetID, TCodeCave, TVariable}, TCodeCave)"/>
		///    method should be used to create new instances of the <see cref="InjectedCodeCaveMemoryAddress"/> class.
		/// </summary>
		/// <param name="realAddress">The real address of the injected code cave in the target process' memory space.</param>
		private InjectedCodeCaveMemoryAddress( IntPtr realAddress )
		{
			m_realAddress = realAddress;
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
