using System;
using System.Collections.Generic;
using System.Reflection;


namespace RAMvader.CodeInjection
{
    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false )]
    public class CodeCaveDefinitionAttribute : Attribute
    {
        #region PRIVATE FIELDS
        /** Keeps the code cave definition which is initialized into this class'
         * constructor method. */
        private Object [] m_codeCaveDefinition;
        #endregion





        #region PUBLIC STATIC METHODS
        /** Utility method which retrieves the #CodeCaveDefinitionAttribute from the given enumerator value.
         * @param elm The enumerator from which the #CodeCaveDefinitionAttribute should be retrieved.
         * @return Returns the #CodeCaveDefinitionAttribute associated with the given enumerator, if any.
         *    Returns null if no #CodeCaveDefinitionAttribute is associated with the given enumerator. */
        public static CodeCaveDefinitionAttribute GetCodeCaveDefinitionAttributeFromEnum( Enum elm )
        {
            Type enumType = elm.GetType();
            FieldInfo fieldInfo = enumType.GetField( elm.ToString() );
            return fieldInfo.GetCustomAttribute<CodeCaveDefinitionAttribute>();
        }
        #endregion





        #region PUBLIC METHODS
        /** Constructor.
         * @param specs An array of objects representing the parts which constitute
         *    the code cave. Acceptable values are byte values (this method accepts
         *    int values from 0 to 255 and converts them to the byte type internally)
         *    and #TVariable values. When an #TVariable is found, it is
         *    replaced by the address of the corresponding injected variable. */
        public CodeCaveDefinitionAttribute( params Object [] codeCaveDefinition )
        {
            m_codeCaveDefinition = codeCaveDefinition;
        }


        /** Checks the definitions for the code caves, throwing an exception when there's anything wrong with the definition.
         * @throw InjectorException Thrown when any errors are found in the definition of a given code cave. The exception's message
         *    explain the errors that must be fixed.
         * @tparam TVariable An enumerated type which specifies the identifiers for variables to be injected at the target process.
         *    Each enumerator belonging to this enumeration should have the #VariableDefinitionAttribute attribute. */
        public void PerformSafetyChecks<TVariable>()
        {
            // SAFETY CHECK: values must either be byte-convertible (integers ranging
            // from 0 to 255, both inclusive) or #TVariable values
            foreach ( Object curValue in m_codeCaveDefinition )
            {
                if ( curValue is TVariable == false && curValue is int == false )
                {
                    throw new InjectorException( string.Format(
                        "[{0}] Invalid value type specified for a {0} attribute! Values must either be integers ranging from 0 to 255 or enumerators identifying variables to be injected in the target process' memory space.",
                        typeof( CodeCaveDefinitionAttribute ).Name ) );
                }
                else if ( curValue is int )
                {
                    int iCurValue = (int) curValue;
                    if ( iCurValue < 0 || iCurValue > 255 )
                    {
                        throw new InjectorException( string.Format(
                            "[{0}] Invalid value type specified for a {0} attribute! Values must either be integers ranging from 0 to 255 or enumerators identifying variables to be injected in the target process' memory space.",
                            typeof( CodeCaveDefinitionAttribute ).Name ) );
                    }
                }
            }
        }


        /** Retrives a sequence of bytes representing the code cave that this attribute
         * is associated to. This method preserves the byte values passed to the
         * #CodeCaveDefinitionAttribute constructor and replaces any
         * #TVariable value passed to this constructor by the injected variable's
         * address.
         * NOTICE: This method should only be called after the #Injector class has
         * calculated the addresses for injected variables.
         * @param injector The #Injector object used to retrie
         * @return Returns a byte sequence representing the code cave, ready to be
         *    injected into the game's memory. */
        public byte[] GetCodeCaveBytes<TCodeCave, TVariable>( Injector<TCodeCave, TVariable> injector )
        {
            // Generate the array of bytes representing the code cave
            List<byte> result = new List<byte>();
            foreach ( Object curCodeCavePiece in m_codeCaveDefinition )
            {
                if ( curCodeCavePiece is TVariable )
                {
                    // Retrieve the bytes representing the injected variable's address
                    TVariable varID = (TVariable) curCodeCavePiece;
                    byte [] varAddressBytes = injector.GetInjectedVariableAddressAsBytes( varID );
                    foreach ( byte curByte in varAddressBytes )
                        result.Add( curByte );
                }
                else
                {
                    result.Add( (byte) (int)curCodeCavePiece );
                }
            }

            return result.ToArray();
        }


        /** Calculates and retrieves the size of the code cave.
         * @param injector Reference to the #Injector object, used to calculate
         *    some size properties (e.g., variable sizes).
         * @return Returns the number of bytes of size for the code cave. */
        public int GetCodeCaveSize<TCodeCave, TVariable>( Injector<TCodeCave, TVariable> injector )
        {
            int pointerSizeInTargetProcess = injector.GetTargetProcessPointerSize();

            int sizeCount = 0;
            foreach ( Object curCodeCavePiece in m_codeCaveDefinition )
            {
                if ( curCodeCavePiece is TVariable )
                {
                    TVariable injVar = (TVariable) curCodeCavePiece;
                    sizeCount += pointerSizeInTargetProcess;
                }
                else
                    sizeCount++;
            }

            return sizeCount;
        }
        #endregion
    }
}
