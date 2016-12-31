/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 * 
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * RAMvader is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Reflection;


namespace RAMvader.CodeInjection
{
	/// <summary>
	///    This attribute is required to be applied to all of the enumerator values that identify a Code Cave which needs
	///    to be injected by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class.
	///    Failing to apply that attribute to a code cave causes an error during the call
	///    to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject()"/>.
	/// </summary>
    [AttributeUsage( AttributeTargets.Field, AllowMultiple=false )]
    public class CodeCaveDefinitionAttribute : Attribute
    {
		#region PRIVATE FIELDS
		/// <summary>Keeps the code cave definition which is initialized into this class' constructor method.</summary>
		private Object [] m_codeCaveDefinition;
		#endregion





		#region PUBLIC STATIC METHODS
		/// <summary>Utility method which retrieves the <see cref="CodeCaveDefinitionAttribute"/> from the given enumerator value.</summary>
		/// <param name="elm">The enumerator from which the <see cref="CodeCaveDefinitionAttribute"/> should be retrieved.</param>
		/// <returns>
		///    Returns the <see cref="CodeCaveDefinitionAttribute"/> associated with the given enumerator, if any.
		///    Returns null if no <see cref="CodeCaveDefinitionAttribute"/> is associated with the given enumerator.
		/// </returns>
		public static CodeCaveDefinitionAttribute GetCodeCaveDefinitionAttributeFromEnum( Enum elm )
        {
            Type enumType = elm.GetType();
            FieldInfo fieldInfo = enumType.GetField( elm.ToString() );
            return fieldInfo.GetCustomAttribute<CodeCaveDefinitionAttribute>();
        }
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="codeCaveDefinition">
		///    An array of objects representing the parts which constitute the code cave.
		///    Acceptable values are byte values (this method accepts int values from 0 to 255 and converts them to the byte type internally)
		///    and Injection Variable enumerator values. When an Injection Variable enumerator value is found, it is replaced by the address of
		///    the corresponding injected variable.
		/// </param>
		public CodeCaveDefinitionAttribute( params Object [] codeCaveDefinition )
        {
            m_codeCaveDefinition = codeCaveDefinition;
        }


		/// <summary>Checks the definitions for the code caves, throwing an exception when there's anything wrong with the definition.</summary>
		/// <typeparam name="TVariable">
		///    An enumerated type which specifies the identifiers for variables to be injected at the target process.
		///    Each enumerator belonging to this enumeration should have the <see cref="VariableDefinitionAttribute"/> attribute.
		/// </typeparam>
		/// <exception cref="AttributeRetrievalException">
		///    Thrown when the code cave definition is malformed (i.e., unsupported data types have been passed to the definition).
		/// </exception>
		public void PerformSafetyChecks<TVariable>()
        {
            // SAFETY CHECK: values must either be byte-convertible (integers ranging
            // from 0 to 255, both inclusive) or Injection Variable enumerator values
            foreach ( Object curValue in m_codeCaveDefinition )
            {
                if ( curValue is TVariable == false && curValue is int == false )
                {
                    throw new AttributeRetrievalException( string.Format(
                        "[{0}] Invalid value type specified for a {0} attribute! Values must either be integers ranging from 0 to 255 or enumerators identifying variables to be injected in the target process' memory space.",
                        typeof( CodeCaveDefinitionAttribute ).Name ) );
                }
                else if ( curValue is int )
                {
                    int iCurValue = (int) curValue;
                    if ( iCurValue < 0 || iCurValue > 255 )
                    {
                        throw new AttributeRetrievalException( string.Format(
                            "[{0}] Invalid value type specified for a {0} attribute! Values must either be integers ranging from 0 to 255 or enumerators identifying variables to be injected in the target process' memory space.",
                            typeof( CodeCaveDefinitionAttribute ).Name ) );
                    }
                }
            }
        }


		/// <summary>
		///    Retrives a sequence of bytes representing the code cave that this attribute is associated to.
		///    This method preserves the byte values passed to the <see cref="CodeCaveDefinitionAttribute"/> constructor and replaces any Injection Variable enumerator value
		///    passed to this constructor by the injected variable's address. NOTICE: This method should only be called after the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    class has calculated the addresses for injected variables.
		/// </summary>
		/// <typeparam name="TMemoryAlterationID">The enumeration of Memory Alteration Sets used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TCodeCave">The enumeration of Code Caves used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TVariable">The enumeration of Injection Variables used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <param name="injector">The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve the bytes of the code cave.</param>
		/// <returns>Returns a byte sequence representing the code cave, ready to be injected into the game's memory.</returns>
		public byte[] GetCodeCaveBytes<TMemoryAlterationID, TCodeCave, TVariable>( Injector<TMemoryAlterationID, TCodeCave, TVariable> injector )
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


		/// <summary>Calculates and retrieves the size of the code cave.</summary>
		/// <typeparam name="TMemoryAlterationID">The enumeration of Memory Alteration Sets used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TCodeCave">The enumeration of Code Caves used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <typeparam name="TVariable">The enumeration of Injection Variables used for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.</typeparam>
		/// <param name="injector">Reference to the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object, used to calculate some size properties (e.g., variable sizes).</param>
		/// <returns>Returns the number of bytes of size for the code cave.</returns>
		public int GetCodeCaveSize<TMemoryAlterationID, TCodeCave, TVariable>( Injector<TMemoryAlterationID, TCodeCave, TVariable> injector )
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
