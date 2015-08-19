/* This file provides implementations of nested classes that are used inside the #Injector class to
 * implement properties that can be used as indexers and that access methods of the #Injector. This
 * enables the client code to do Data Binding in WPF. */

using System;
namespace RAMvader.CodeInjection
{
    public partial class Injector<TCodeCave, TVariable>
    {
        #region PRIVATE CONSTANTS
        /** The default name of an indexer property, which is used to raise the "property changed"
         * event (provided by standard WPF INotifyPropertyChanged implementation) when a indexer property
         * has its value updated. */
        private const string DEFAULT_INDEXER_PROPERTY_NAME = "Item[]";
        #endregion





        #region NESTED CLASSES
        /** Provides an indexer used to access code cave offsets, through the
         * property #Injector.CodeCaveOffset. */
        public class NestedPropertyIndexerCodeCaveOffset : NotifyPropertyChangedAdapter
        {
            #region PRIVATE FIELDS
            /** Reference to the #Injector which owns this object. */
            private Injector<TCodeCave, TVariable> m_injector;
            #endregion





            #region PUBLIC METHODS
            /** Constructor. The internal scope-modifier ensures this class will not be
             * instanced outside of the #Injector class.
             * @param injector A reference to the #Injector object used to retrieve data
             *    for the indexer property. */
            internal NestedPropertyIndexerCodeCaveOffset( Injector<TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


            /** Indexer used to retrieve the offset of a code cave, through a call
             * to #Injector.GetCodeCaveOffset().
             * @param codeCaveID The identifier of the code cave whose offset is to be retrieved.
             * @return Returns the offset of the given code cave. */
            public int this[TCodeCave codeCaveID]
            {
                get { return m_injector.GetCodeCaveOffset( codeCaveID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }




        /** Provides an indexer used to access the address where a code cave has been injected,
         * through #Injector.InjectedCodeCaveAddress. */
        public class NestedPropertyIndexerInjectedCodeCaveAddress : NotifyPropertyChangedAdapter
        {
            #region PRIVATE FIELDS
            /** Reference to the #Injector which owns this object. */
            private Injector<TCodeCave, TVariable> m_injector;
            #endregion





            #region PUBLIC METHODS
            /** Constructor. The internal scope-modifier ensures this class will not be
             * instanced outside of the #Injector class.
             * @param injector A reference to the #Injector object used to retrieve data
             *    for the indexer property. */
            internal NestedPropertyIndexerInjectedCodeCaveAddress( Injector<TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


            /** Indexer used to retrieve the address where a code cave has been injected, through a call
             * to #Injector.GetInjectedCodeCaveAddress().
             * @param codeCaveID The identifier of the code cave whose injected address is to be retrieved.
             * @return Returns the address where the code cave has been injected. */
            public IntPtr this[TCodeCave codeCaveID]
            {
                get { return m_injector.GetInjectedCodeCaveAddress( codeCaveID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





        /** Provides an indexer used to access variable offsets, through the
         * property #Injector.VariableOffset. */
        public class NestedPropertyIndexerVariableOffset : NotifyPropertyChangedAdapter
        {
            #region PRIVATE FIELDS
            /** Reference to the #Injector which owns this object. */
            private Injector<TCodeCave, TVariable> m_injector;
            #endregion





            #region PUBLIC METHODS
            /** Constructor. The internal scope-modifier ensures this class will not be
             * instanced outside of the #Injector class.
             * @param injector A reference to the #Injector object used to retrieve data
             *    for the indexer property. */
            internal NestedPropertyIndexerVariableOffset( Injector<TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


            /** Indexer used to retrieve the offset of a variable, through a call
             * to #Injector.GetVariableOffset().
             * @param variableID The identifier of the variable whose offset is to be retrieved.
             * @return Returns the offset of the given variable. */
            public int this[TVariable variableID]
            {
                get { return m_injector.GetVariableOffset( variableID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





        /** Provides an indexer used to access the address where a variable has been injected,
         * through the property #Injector.InjectedVariableAddress. */
        public class NestedPropertyIndexerInjectedVariableAddress : NotifyPropertyChangedAdapter
        {
            #region PRIVATE FIELDS
            /** Reference to the #Injector which owns this object. */
            private Injector<TCodeCave, TVariable> m_injector;
            #endregion





            #region PUBLIC METHODS
            /** Constructor. The internal scope-modifier ensures this class will not be
             * instanced outside of the #Injector class.
             * @param injector A reference to the #Injector object used to retrieve data
             *    for the indexer property. */
            internal NestedPropertyIndexerInjectedVariableAddress( Injector<TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


            /** Indexer used to retrieve the address where a variable has been injected, through a call
             * to #Injector.GetInjectedVariableAddress().
             * @param variableID The identifier of the variable whose injected address is to be retrieved.
             * @return Returns the address where the given variable has been injected. */
            public IntPtr this[TVariable variableID]
            {
                get { return m_injector.GetInjectedVariableAddress( variableID ); }
                internal set { this.SendPropertyChangedNotification( DEFAULT_INDEXER_PROPERTY_NAME ); }
            }
            #endregion
        }





        /** Provides an indexer used to retrieve the size of a variable, through the
         * property #Injector.VariableSize. */
        public class NestedPropertyIndexerVariableSize : NotifyPropertyChangedAdapter
        {
            #region PRIVATE FIELDS
            /** Reference to the #Injector which owns this object. */
            private Injector<TCodeCave, TVariable> m_injector;
            #endregion





            #region PUBLIC METHODS
            /** Constructor. The internal scope-modifier ensures this class will not be
             * instanced outside of the #Injector class.
             * @param injector A reference to the #Injector object used to retrieve data
             *    for the indexer property. */
            internal NestedPropertyIndexerVariableSize( Injector<TCodeCave, TVariable> injector )
            {
                m_injector = injector;
            }


            /** Indexer used to retrieve the size of a variable, through a call
             * to #Injector.GetVariableSize().
             * @param variableID The identifier of the variable whose size is to be retrieved.
             * @return Returns the size of the given variable. */
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