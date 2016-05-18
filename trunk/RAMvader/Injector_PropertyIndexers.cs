/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 * 
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

/* This file provides implementations of nested classes that are used inside the Injector class to
 * implement properties that can be used as indexers and that access methods of the Injector. This
 * enables the client code to do Data Binding in WPF. */

using System;
namespace RAMvader.CodeInjection
{
	/// <inheritdoc />
    public partial class Injector<TMemoryAlterationSetID, TCodeCave, TVariable>
    {
		#region PRIVATE CONSTANTS
		/// <summary>
		///    The default name of an indexer property, which is used to raise the "property changed"
		///    event (provided by standard WPF INotifyPropertyChanged implementation) when a indexer property
		///    has its value updated.
		/// </summary>
		private const string DEFAULT_INDEXER_PROPERTY_NAME = "Item[]";
		#endregion





		#region NESTED CLASSES
		/// <summary>Provides an indexer used to access code cave offsets, through the property <see cref="CodeCaveOffset"/>.</summary>
		public class NestedPropertyIndexerCodeCaveOffset : NotifyPropertyChangedAdapter
        {
			#region PRIVATE FIELDS
			/// <summary>Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which owns this object.</summary>
			private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
			#endregion





			#region PUBLIC METHODS
			/// <summary>
			///    Constructor.
			///    The internal scope-modifier ensures this class will not be instanced outside of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
			/// </summary>
			/// <param name="injector">A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve data for the indexer property.</param>
			internal NestedPropertyIndexerCodeCaveOffset( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


			/// <summary>Indexer used to retrieve the offset of a code cave, through a call to <see cref="GetCodeCaveOffset(TCodeCave)"/>.</summary>
			/// <param name="codeCaveID">The identifier of the code cave whose offset is to be retrieved.</param>
			/// <returns>Returns the offset of the given code cave.</returns>
			public int this[TCodeCave codeCaveID]
            {
                get { return m_injector.GetCodeCaveOffset( codeCaveID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }




		/// <summary>
		///    Provides an indexer used to access the address where a code cave has been injected,
		///    through <see cref="InjectedCodeCaveAddress"/>.
		/// </summary>
		public class NestedPropertyIndexerInjectedCodeCaveAddress : NotifyPropertyChangedAdapter
        {
			#region PRIVATE FIELDS
			/// <summary>Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which owns this object.</summary>
			private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
			#endregion





			#region PUBLIC METHODS
			/// <summary>
			///    Constructor.
			///    The internal scope-modifier ensures this class will not be instanced outside of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
			/// </summary>
			/// <param name="injector">A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve data for the indexer property.</param>
			internal NestedPropertyIndexerInjectedCodeCaveAddress( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


			/// <summary>
			///    Indexer used to retrieve the address where a code cave has been injected, through a call
			///    to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.GetInjectedCodeCaveAddress().
			/// </summary>
			/// <param name="codeCaveID">The identifier of the code cave whose injected address is to be retrieved.</param>
			/// <returns>Returns the address where the code cave has been injected.</returns>
			public IntPtr this[TCodeCave codeCaveID]
            {
                get { return m_injector.GetInjectedCodeCaveAddress( codeCaveID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





		/// <summary>Provides an indexer used to access variable offsets, through the property <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.VariableOffset.</summary>
		public class NestedPropertyIndexerVariableOffset : NotifyPropertyChangedAdapter
        {
			#region PRIVATE FIELDS
			/// <summary>Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which owns this object.</summary>
			private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
			#endregion





			#region PUBLIC METHODS
			/// <summary>
			///    Constructor.
			///    The internal scope-modifier ensures this class will not be instanced outside of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
			/// </summary>
			/// <param name="injector">A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve data for the indexer property.</param>
			internal NestedPropertyIndexerVariableOffset( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


			/// <summary>Indexer used to retrieve the offset of a variable, through a call to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.GetVariableOffset().</summary>
			/// <param name="variableID">The identifier of the variable whose offset is to be retrieved.</param>
			/// <returns>Returns the offset of the given variable.</returns>
			public int this[TVariable variableID]
            {
                get { return m_injector.GetVariableOffset( variableID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





		/// <summary>
		///    Provides an indexer used to access the address where a variable has been injected,
		///    through the property <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.InjectedVariableAddress.
		/// </summary>
		public class NestedPropertyIndexerInjectedVariableAddress : NotifyPropertyChangedAdapter
        {
			#region PRIVATE FIELDS
			/// <summary>Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which owns this object.</summary>
			private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
			#endregion





			#region PUBLIC METHODS
			/// <summary>
			///    Constructor.
			///    The internal scope-modifier ensures this class will not be instanced outside of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
			/// </summary>
			/// <param name="injector">A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve data for the indexer property.</param>
			internal NestedPropertyIndexerInjectedVariableAddress( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


			/// <summary>
			///    Indexer used to retrieve the address where a variable has been injected, through a call
			///    to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.GetInjectedVariableAddress().
			/// </summary>
			/// <param name="variableID">The identifier of the variable whose injected address is to be retrieved.</param>
			/// <returns>Returns the address where the given variable has been injected.</returns>
			public IntPtr this[TVariable variableID]
            {
                get { return m_injector.GetInjectedVariableAddress( variableID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





		/// <summary>
		///    Provides an indexer used to retrieve the size of a variable,
		///    through the property <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.VariableSize.
		/// </summary>
		public class NestedPropertyIndexerVariableSize : NotifyPropertyChangedAdapter
        {
			#region PRIVATE FIELDS
			/// <summary>Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which owns this object.</summary>
			private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
			#endregion





			#region PUBLIC METHODS
			/// <summary>
			///    Constructor.
			///    The internal scope-modifier ensures this class will not be instanced outside of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
			/// </summary>
			/// <param name="injector">A reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve data for the indexer property.</param>
			internal NestedPropertyIndexerVariableSize( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


			/// <summary>Indexer used to retrieve the size of a variable, through a call to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.GetVariableSize().</summary>
			/// <param name="variableID">The identifier of the variable whose size is to be retrieved.</param>
			/// <returns>Returns the size of the given variable.</returns>
			public int this[TVariable variableID]
            {
                get { return m_injector.GetVariableSize( variableID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }
        #endregion
    }
}