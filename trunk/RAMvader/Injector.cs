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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;


namespace RAMvader.CodeInjection
{
	/// <summary>
	///    Implements the logic behind the injection of code caves and variables into a target process' memory space.
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
	public partial class Injector<TMemoryAlterationSetID, TCodeCave, TVariable> : NotifyPropertyChangedAdapter
    {
		#region PRIVATE CONSTANTS
		/// <summary>
		///    Keeps both the supported types of variables that can be injected into the
		///    target process' memory space and their corresponding sizes, given in number
		///    of bytes.
		/// </summary>
		private static readonly Dictionary<Type, int> SUPPORTED_VARIABLE_TYPES_SIZE = new Dictionary<Type, int>()
        {
            { typeof( Byte ), sizeof( Byte ) },
            { typeof( Int16 ), sizeof( Int16 ) },
            { typeof( Int32 ), sizeof( Int32 ) },
            { typeof( Int64 ), sizeof( Int64 ) },
            { typeof( UInt16 ), sizeof( UInt16 ) },
            { typeof( UInt32 ), sizeof( UInt32 ) },
            { typeof( UInt64 ), sizeof( UInt64 ) },
            { typeof( Single ), sizeof( Single ) },
            { typeof( Double ), sizeof( Double ) },

            // IntPtr type is also supported, but it is treated as a special type because its size might vary depending on the target
            // process' platform (32 or 64 bits)
        };
		#endregion





		#region PRIVATE FIELDS
		/// <summary>
		///    The object used to attach to the target process, so that the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> can
		///    perform I/O operations into the target process' memory.
		/// </summary>
		private RAMvaderTarget m_targetProcess;
		/// <summary>Keeps the base address of the memory which was allocated for the target process.</summary>
		private IntPtr m_baseInjectionAddress = IntPtr.Zero;
		/// <summary>Backs the <see cref="IsInjected"/> property.</summary>
		private bool m_isInjected = false;
		/// <summary>
		///    A flag specifying if the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> has allocated memory in the target process for
		///    injecting its data. When the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> allocates memory in the target process, it is
		///    responsible for freing it whenever necessary.
		/// </summary>
		private bool m_bHasAllocatedMemory = false;
		/// <summary>The sequence of bytes which separate two consecutive code caves.</summary>
		private byte [] m_codeCavesSeparator =
        {
			LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP,
			LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP, LowLevel.OPCODE_x86_NOP,
        };
		/// <summary>The sequence of bytes which separate the code caves region from the variables region.</summary>
		private byte [] m_variablesSectionSeparator =
        {
			LowLevel.OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3,
		   LowLevel. OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3, LowLevel.OPCODE_x86_INT3,
        };
		/// <summary>Keeps all the alterations registered for a given memory alteration set.</summary>
		private Dictionary<TMemoryAlterationSetID, List<MemoryAlterationBase>> m_memoryAlterationSets = new Dictionary<TMemoryAlterationSetID, List<MemoryAlterationBase>>();
		/// <summary>
		///    Indexer field used to access the code cave offsets, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetCodeCaveOffset(TCodeCave)"/> internally.
		/// </summary>
		private NestedPropertyIndexerCodeCaveOffset m_codeCaveOffset;
        /// <summary>
		///    Indexer property used to access the address where a code cave has been injected, usually
		///    for WPF Binding purposes. Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetInjectedCodeCaveAddress(TCodeCave)"/> internally.
		/// </summary>
        private NestedPropertyIndexerInjectedCodeCaveAddress m_injectedCodeCaveAddress;
        /// <summary>
		///    Indexer property used to access variable offsets, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetVariableOffset(TVariable)"/> internally.
		/// </summary>
        private NestedPropertyIndexerVariableOffset m_variableOffset;
        /// <summary>
		///    Indexer property used to access the address where a variable has been injected, usually
		///    for WPF Binding purposes. Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetInjectedVariableAddress(TVariable)"/> internally.
		/// </summary>
        private NestedPropertyIndexerInjectedVariableAddress m_injectedVariableAddress;
        /// <summary>
		///    Indexer property used to retrieve the size of a variable, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetVariableSize(TVariable)"/> internally.
		/// </summary>
        private NestedPropertyIndexerVariableSize m_variableSize;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>
		///    Keeps the base address of the memory which was allocated for the target process.
		///    Backed by the <see cref="m_baseInjectionAddress"/> field.
		/// </summary>
		public IntPtr BaseInjectionAddress
        {
            get { return m_baseInjectionAddress; }
            private set
            {
                m_baseInjectionAddress = value;
                SendPropertyChangedNotification();

                // The following properties also need to send a "property changed"
                // notification when the Base Injection Address changes
                // IMPORTANT: Their backing fields' values are NOT altered to "null" by this operation!
                // These properties' setter methods do not alter the underlying value, they only fire
                // "property changed" notifications for WPF Bindings to work correctly..
                InjectedCodeCaveAddress = null;
                InjectedVariableAddress = null;
            }
        }
		/// <summary>
		///    A flag that is set to true whenever the <see cref="Inject()"/> (or <see cref="Inject(IntPtr)"/>) method is called and succeeds, and set to false
		///    whenever the<see cref="ResetAllocatedMemoryData"/> gets called.
		/// </summary>
		public bool IsInjected
		{
			get { return m_isInjected; }
			private set
			{
				m_isInjected = value;
				SendPropertyChangedNotification();
			}
		}
		/// <summary>
		///    The object used to attach to the target process, so that the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> can
		///    perform I/O operations into the target process' memory.
		///    Backed by the <see cref="m_targetProcess"/> field.
		/// </summary>
		public RAMvaderTarget TargetProcess
        {
            get { return m_targetProcess; }
            private set
            {
                m_targetProcess = value;
                SendPropertyChangedNotification();
            }
        }
		/// <summary>
		///    The total number of required bytes to inject the code caves and variables into the target
		///    process' memory space, as calculated by a call to the method <see cref="CalculateRequiredBytesCount"/>.
		/// </summary>
		public int RequiredBytesCount
        {
            get { return CalculateRequiredBytesCount(); }
            private set
            {
                // Simulates property updating for the Binding system to work properly
                SendPropertyChangedNotification();
            }
        }
		/// <summary>
		///    Indexer property used to access the code cave offsets, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetCodeCaveOffset(TCodeCave)"/> internally.
		///    Backed by the <see cref="m_codeCaveOffset"/> field.
		/// </summary>
		public NestedPropertyIndexerCodeCaveOffset CodeCaveOffset
        {
            get { return m_codeCaveOffset; }
            private set { SendPropertyChangedNotification(); }
        }
		/// <summary>
		///    Indexer property used to access the address where a code cave has been injected, usually
		///    for WPF Binding purposes. Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetInjectedCodeCaveAddress(TCodeCave)"/> internally.
		///    Backed by the <see cref="m_injectedCodeCaveAddress"/> field.
		/// </summary>
		public NestedPropertyIndexerInjectedCodeCaveAddress InjectedCodeCaveAddress
        {
            get { return m_injectedCodeCaveAddress; }
            private set { SendPropertyChangedNotification(); }
        }
		/// <summary>
		///    Indexer property used to access variable offsets, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetVariableOffset(TVariable)"/> internally.
		///    Backed by the <see cref="m_variableOffset"/> field.
		/// </summary>
		public NestedPropertyIndexerVariableOffset VariableOffset
        {
            get { return m_variableOffset; }
            private set { SendPropertyChangedNotification(); }
        }
		/// <summary>
		///    Indexer property used to access the address where a variable has been injected, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetInjectedVariableAddress(TVariable)"/> internally.
		///    Backed by the <see cref="m_injectedVariableAddress"/> field.
		/// </summary>
		public NestedPropertyIndexerInjectedVariableAddress InjectedVariableAddress
        {
            get { return m_injectedVariableAddress; }
            private set { SendPropertyChangedNotification(); }
        }
		/// <summary>
		///    Indexer property used to retrieve the size of a variable, usually for WPF Binding purposes.
		///    Calls <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.GetVariableSize(TVariable)"/> internally.
		///    Backed by the <see cref="m_variableSize"/> field.
		/// </summary>
		public NestedPropertyIndexerVariableSize VariableSize
        {
            get { return m_variableSize; }
            private set { SendPropertyChangedNotification(); }
        }
		#endregion





		#region PRIVATE STATIC METHODS
		/// <summary>Retrieves an array of attributes associated to the given enumerator.</summary>
		/// <typeparam name="TAttrib">The type of the Attribute to be retrieved.</typeparam>
		/// <param name="enumeratorValue">The value indicating the enumerator whose Attributes are to be retrieved.</param>
		/// <returns>Returns an array of attributes of the TAttrib type for the given enumerator value.</returns>
		private static TAttrib[] GetEnumAttributes<TAttrib>( Object enumeratorValue )
            where TAttrib : Attribute
        {
            // Ensure that the provided value is an enumerator
            Type enumeratorType = enumeratorValue.GetType();
            if ( enumeratorType.IsEnum == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve {1}s of type \"{2}\" from the provided enumerator value: the provided object is NOT an enumerator value!",
                    GetInjectorNameWithTemplateParameters(), typeof( Attribute ).Name,
                    typeof( TAttrib ).Name ) );
            }

            // Retrieve the enumerator's info
            MemberInfo [] memberInfo = enumeratorType.GetMember( enumeratorValue.ToString() );
            if ( memberInfo.Length != 1 )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve {1}s of type \"{2}\" from the provided enumerator value: multiple {3} objects have been found for the given value!",
                    GetInjectorNameWithTemplateParameters(), typeof( Attribute ).Name,
                    typeof( TAttrib ).Name, typeof( MemberInfo ).Name ) );
            }

            // Retrieve the associated attributes
            TAttrib [] attribs = (TAttrib[]) memberInfo[0].GetCustomAttributes( typeof( TAttrib ), true );
            return attribs;
        }


		/// <summary>Retrieves an attribute associated to the given enumerator.</summary>
		/// <typeparam name="TAttrib">The type of the Attribute to be retrieved.</typeparam>
		/// <param name="enumeratorValue">The value indicating the enumerator whose Attribute is to be retrieved.</param>
		/// <param name="bThrowException">
		///    A flag indicating if an exception should be thrown when the attribute is not found.
		///    If that flag is set to false, the method simply returns a null value when the attribute can't be retrieved.
		/// </param>
		/// <returns>Returns an array of attributes of the TAttrib type for the given enumerator value.</returns>
		private static TAttrib GetEnumAttribute<TAttrib>( Object enumeratorValue, bool bThrowException )
            where TAttrib : Attribute
        {
            // Retrieve the array containing all the attributes of the given type
            TAttrib [] attribs = GetEnumAttributes<TAttrib>( enumeratorValue );

            // DEBUG: Make sure at most ONE attribute can be found by this method.
            #if DEBUG
                if ( attribs.Length > 1 )
                {
                    throw new InjectorException( string.Format(
                        "[{0}] Cannot retrieve {1} of type \"{2}\" from the provided enumerator value: multiple {2} have been found for the given value, while this method is supposed to fetch only ONE {1} of the given type!",
                        GetInjectorNameWithTemplateParameters(), typeof( Attribute ).Name,
                        typeof( TAttrib ).Name ) );
                }
            #endif

            // Return the found attribute (if any)
            if ( attribs.Length == 0 )
            {
                if ( bThrowException )
                {
                    throw new InjectorException( string.Format(
                        "[{0}] Cannot retrieve {1} attribute from the enumerator identified by \"{2}\"!",
                        GetInjectorNameWithTemplateParameters(), typeof( TAttrib ).Name,
                        enumeratorValue.ToString() ) );
                }
                return null;
            }
            return attribs[0];
		}


		/// <summary>
		///    Utility method for retrieving a human-readable name for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class,
		///    including the name of its generic parameters.
		/// </summary>
		/// <returns>Returns a string containing the name of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class and its generic parameters.</returns>
		private static string GetInjectorNameWithTemplateParameters()
        {
            Type injectorType = typeof( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> );
            Type [] genericArguments = injectorType.GetGenericArguments();
            int totalGenericArguments = genericArguments.Length;

            StringBuilder builder = new StringBuilder( injectorType.Name.Remove( injectorType.Name.IndexOf('`') ) );
            builder.Append( '<' );
            for ( int a = 0; a < totalGenericArguments; a++ )
            {
                if ( a > 0 )
                    builder.Append( ", " );
                builder.Append( genericArguments[a].Name );
            }
            builder.Append( '>' );

            return builder.ToString();
        }
		#endregion





		#region PUBLIC STATIC METHODS
		/// <summary>
		///    Utility method for retrieving a sequence of bytes which represent the machine-level opcode corresponding to a 32-bits CALL instruction.
		///    64-bits CALL instructions are currently not supported by the RAMvader library.
		/// </summary>
		/// <param name="callInstructionAddress">The address of the CALL instruction itself.</param>
		/// <param name="targetCallAddress">The address which should be called by the CALL instruction.</param>
		/// <param name="instructionSize">
		///    When replacing an instruction in a target process' memory space by a CALL instruction, this parameter specifies
		///    the size of the instruction to be replaced. If this size is larger than the size of a CALL instruction, the
		///    remaining bytes are filled with NOP opcodes in the returned bytes sequence, so that the CALL instruction might
		///    replace other instructions while keeping the consistency of its surrounding instructions when a RET instruction is used
		///    to return from the CALL.
		/// </param>
		/// <param name="endianness">The endianness to be used for the offset of the CALL opcode.</param>
		/// <param name="pointerSize">The size of pointer to be used for the offset of the CALL opcode.</param>
		/// <param name="diffPointerSizeError">
		///    The policy for handling errors regarding different sizes of pointers between RAMvader process' pointers and the pointers
		///    size defined by the "pointerSize" parameter.
		/// </param>
		/// <returns>Returns a sequence of bytes representing the CALL opcode that composes the given instruction.</returns>
		public static byte [] GetX86CallOpcode( IntPtr callInstructionAddress, IntPtr targetCallAddress,
            int instructionSize = LowLevel.INSTRUCTION_SIZE_x86_CALL,
            EEndianness endianness = EEndianness.evEndiannessDefault,
            EPointerSize pointerSize = EPointerSize.evPointerSizeDefault,
            EDifferentPointerSizeError diffPointerSizeError = EDifferentPointerSizeError.evThrowException )
        {
            // Initialize defaults
            if ( endianness == EEndianness.evEndiannessDefault )
                endianness = BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

            if ( pointerSize == EPointerSize.evPointerSizeDefault )
                pointerSize = RAMvaderTarget.GetRAMvaderPointerSize();

            // Calculate the offset between the CALL instruction and the target address that it should call
            Object callOffset;

            if ( pointerSize == EPointerSize.evPointerSize32 )
                callOffset = (Int32) ( targetCallAddress.ToInt32() - callInstructionAddress.ToInt32() - LowLevel.INSTRUCTION_SIZE_x86_CALL );
            else if ( pointerSize == EPointerSize.evPointerSize64 )
                callOffset = (Int64) ( targetCallAddress.ToInt64() - callInstructionAddress.ToInt64() - LowLevel.INSTRUCTION_SIZE_x86_CALL );
            else
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to retrieve CALL instruction opcode: the specified pointer size is not supported.",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            // Build the CALL opcode
            List<byte> tmpBytes = new List<byte>( LowLevel.INSTRUCTION_SIZE_x86_CALL );
            tmpBytes.Add( LowLevel.OPCODE_x86_CALL );

            byte [] callOffsetAsBytes = RAMvaderTarget.GetValueAsBytesArray( callOffset, endianness, pointerSize, diffPointerSizeError );
            tmpBytes.AddRange( callOffsetAsBytes );

            // Fill the remaining bytes of the given instruction size with NOP opcodes
            int extraNOPs = instructionSize - LowLevel.INSTRUCTION_SIZE_x86_CALL;
            if ( extraNOPs < 0 )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to retrieve CALL instruction opcode: the CALL instruction is larger than the instruction that is going to be replaced.",
                    GetInjectorNameWithTemplateParameters() ) );
            }
            for ( int n = 0; n < extraNOPs; n++ )
                tmpBytes.Add( LowLevel.OPCODE_x86_NOP );

            // Return the result
            return tmpBytes.ToArray();
		}


		/// <summary>
		///    Utility method for retrieving a sequence of bytes which represent the machine-level opcode corresponding
		///    to a 32-bits NEAR JUMP instruction. 64-bits JUMP instructions are currently not supported by the RAMvader library.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be generated.</param>
		/// <param name="jumpInstructionAddress">The address of the JUMP instruction itself.</param>
		/// <param name="targetJumpAddress">The address to which the JUMP instruction should jump.</param>
		/// <param name="instructionSize">
		///    When replacing an instruction in a target process' memory space by a JUMP instruction, this parameter specifies the
		///    size of the instruction to be replaced. If this size is larger than the size of a JUMP instruction, the remaining bytes
		///    are filled with NOP opcodes in the returned bytes sequence, so that the JUMP instruction might replace other instructions
		///    while keeping the consistency of its surrounding instructions when the flow of code returns from the jump (if that ever
		///    happens).
		/// </param>
		/// <param name="pointerSize">The size of pointer to be used for the offset of the JUMP opcode.</param>
		/// <returns>Returns a sequence of bytes representing the JUMP opcode that composes the given instruction.</returns>
		public static byte[] GetX86NearJumpOpcode( EJumpInstructionType jumpInstructionType,
			IntPtr jumpInstructionAddress, IntPtr targetJumpAddress,
			int instructionSize = LowLevel.INSTRUCTION_SIZE_x86_NEAR_JUMP,
			EPointerSize pointerSize = EPointerSize.evPointerSizeDefault )
		{
			// Initialize defaults
			if ( pointerSize == EPointerSize.evPointerSizeDefault )
				pointerSize = RAMvaderTarget.GetRAMvaderPointerSize();

			// Calculate the offset between the JUMP instruction and the target address that it should jump to
			Object jumpOffset;
			bool offsetIsValid = false;

			if ( pointerSize == EPointerSize.evPointerSize32 )
			{
				// Calculate offset as a signed byte value and verify if offset is valid (if it fits into a single, signed byte)
				Int32 numJumpOffset = (Int32) ( targetJumpAddress.ToInt32() - jumpInstructionAddress.ToInt32() - LowLevel.INSTRUCTION_SIZE_x86_NEAR_JUMP );
				offsetIsValid = ( numJumpOffset >= SByte.MinValue && numJumpOffset <= SByte.MaxValue );

				// Convert offset to unsigned byte (if necessary)
				if ( numJumpOffset < 0 )
					numJumpOffset = Byte.MaxValue + 1 + numJumpOffset;
				jumpOffset = numJumpOffset;
			}
			else if ( pointerSize == EPointerSize.evPointerSize64 )
			{
				// Calculate offset as a signed byte value and verify if offset is valid (if it fits into a single, signed byte)
				Int64 numJumpOffset = (Int64) ( targetJumpAddress.ToInt64() - jumpInstructionAddress.ToInt64() - LowLevel.INSTRUCTION_SIZE_x86_NEAR_JUMP );
				offsetIsValid = ( numJumpOffset >= SByte.MinValue && numJumpOffset <= SByte.MaxValue );

				// Convert offset to unsigned byte (if necessary)
				if ( numJumpOffset < 0 )
					numJumpOffset = Byte.MaxValue + 1 + numJumpOffset;
				jumpOffset = numJumpOffset;
			}
			else
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve NEAR JUMP instruction opcode: the specified pointer size is not supported.",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// NEAR JUMPs can make jumps to instructions up to 0xFF bytes of distance only
			if ( offsetIsValid == false )
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve NEAR JUMP instruction opcode: offset between the JUMP instruction and its target jump address is too large for making a NEAR JUMP!",
					GetInjectorNameWithTemplateParameters() ) );

			byte byteJumpOffset = Convert.ToByte( jumpOffset );

			// Build the JUMP opcode
			List<byte> tmpBytes = new List<byte>( LowLevel.INSTRUCTION_SIZE_x86_NEAR_JUMP );
			switch ( jumpInstructionType )
			{
				case EJumpInstructionType.evJMP:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JMP );
					break;
				case EJumpInstructionType.evJA:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JA );
					break;
				case EJumpInstructionType.evJB:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JB );
					break;
				case EJumpInstructionType.evJG:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JG );
					break;
				case EJumpInstructionType.evJL:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JL );
					break;
				case EJumpInstructionType.evJE:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JE );
					break;
				case EJumpInstructionType.evJNE:
					tmpBytes.Add( LowLevel.OPCODE_x86_NEAR_JNE );
					break;
				default:
					throw new InjectorException( string.Format(
						"[{0}] Failed to retrieve NEAR JUMP instruction opcode: the specified NEAR JUMP instruction type is not supported.",
						GetInjectorNameWithTemplateParameters() ) );
			}

			tmpBytes.Add( byteJumpOffset );

			// Fill the remaining bytes of the given instruction size with NOP opcodes
			int extraNOPs = instructionSize - LowLevel.INSTRUCTION_SIZE_x86_NEAR_JUMP;
			if ( extraNOPs < 0 )
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve NEAR JUMP instruction opcode: the NEAR JUMP instruction is larger than the instruction that is going to be replaced.",
					GetInjectorNameWithTemplateParameters() ) );
			}
			for ( int n = 0; n < extraNOPs; n++ )
				tmpBytes.Add( LowLevel.OPCODE_x86_NOP );

			// Return the result
			return tmpBytes.ToArray();
		}


		/// <summary>
		///    Utility method for retrieving a sequence of bytes which represent the machine-level opcode
		///    corresponding to a x86 FAR JUMP instruction. 64-bits JUMP instructions are currently not supported
		///    by the RAMvader library.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be generated.</param>
		/// <param name="jumpInstructionAddress">The address of the JUMP instruction itself.</param>
		/// <param name="targetJumpAddress">The address to which the JUMP instruction should jump.</param>
		/// <param name="instructionSize">
		///    When replacing an instruction in a target process' memory space by a JUMP instruction, this parameter specifies
		///    the size of the instruction to be replaced. If this size is larger than the size of a JUMP instruction, the
		///    remaining bytes are filled with NOP opcodes in the returned bytes sequence, so that the JUMP instruction might
		///    replace other instructions while keeping the consistency of its surrounding instructions when the flow of code
		///    returns from the jump (if that ever happens).
		/// </param>
		/// <param name="endianness">The endianness to be used for the offset of the JUMP opcode.</param>
		/// <param name="pointerSize">The size of pointer to be used for the offset of the JUMP opcode.</param>
		/// <param name="diffPointerSizeError">
		///    The policy for handling errors regarding different sizes of pointers between RAMvader process' pointers and the
		///    pointers size defined by the "pointerSize" parameter.
		/// </param>
		/// <returns>Returns a sequence of bytes representing the JUMP opcode that composes the given instruction.</returns>
		public static byte[] GetX86FarJumpOpcode( EJumpInstructionType jumpInstructionType,
			IntPtr jumpInstructionAddress, IntPtr targetJumpAddress,
			int instructionSize,
			EEndianness endianness = EEndianness.evEndiannessDefault,
			EPointerSize pointerSize = EPointerSize.evPointerSizeDefault,
			EDifferentPointerSizeError diffPointerSizeError = EDifferentPointerSizeError.evThrowException )
		{
			// Initialize defaults
			if ( endianness == EEndianness.evEndiannessDefault )
				endianness = BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

			if ( pointerSize == EPointerSize.evPointerSizeDefault )
				pointerSize = RAMvaderTarget.GetRAMvaderPointerSize();

			// Get the bytes that compose a FAR JUMP instruction (NOTICE: unconditional x86 jump instructions have different sizes as compared to conditional jumps)
			List<byte> tmpBytes = new List<byte>();
			switch ( jumpInstructionType )
			{
				case EJumpInstructionType.evJMP:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JMP );
					break;
				case EJumpInstructionType.evJA:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JA );
					break;
				case EJumpInstructionType.evJB:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JB );
					break;
				case EJumpInstructionType.evJG:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JG );
					break;
				case EJumpInstructionType.evJL:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JL );
					break;
				case EJumpInstructionType.evJE:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JE );
					break;
				case EJumpInstructionType.evJNE:
					tmpBytes.AddRange( LowLevel.OPCODE_x86_FAR_JNE );
					break;
				default:
					throw new InjectorException( string.Format(
						"[{0}] Failed to retrieve FAR JUMP instruction opcode: the specified FAR JUMP instruction type is not supported.",
						GetInjectorNameWithTemplateParameters() ) );
			}

			// Calculate the offset between the JUMP instruction and the target address that it should call
			Object jumpOffset;
			bool offsetIsValid = false;

			int farJumpBaseInstructionSize = tmpBytes.Count + 4;   // +4 bytes for the 32-bits JUMP OFFSET

			if ( pointerSize == EPointerSize.evPointerSize32 )
			{
				// Calculate offset as a signed byte value. Using 32-bits calculations, the offset is ALWAYS valid.
				jumpOffset = (Int32) ( targetJumpAddress.ToInt32() - jumpInstructionAddress.ToInt32() - farJumpBaseInstructionSize );
				offsetIsValid = true;
			}
			else if ( pointerSize == EPointerSize.evPointerSize64 )
			{
				// Calculate offset and verify if it is valid (if it fits into a single, signed Int32)
				Int64 numJumpOffset = (Int64) ( targetJumpAddress.ToInt64() - jumpInstructionAddress.ToInt64() - farJumpBaseInstructionSize );
				jumpOffset = numJumpOffset;
				offsetIsValid = ( numJumpOffset >= Int32.MinValue && numJumpOffset <= Int32.MaxValue );
			}
			else
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve FAR JUMP instruction opcode: the specified pointer size is not supported.",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// FAR JUMPs can make jumps to instructions up to 0xFFFFFFFF bytes of distance only
			if ( offsetIsValid == false )
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve FAR JUMP instruction opcode: offset between the JUMP instruction and its target jump address is too large for making a FAR JUMP!.",
					GetInjectorNameWithTemplateParameters() ) );


			// Build the FAR JUMP opcode
			byte [] jumpOffsetAsBytes = RAMvaderTarget.GetValueAsBytesArray( jumpOffset, endianness, pointerSize, diffPointerSizeError );
			tmpBytes.AddRange( jumpOffsetAsBytes );

			// Fill the remaining bytes of the given instruction size with NOP opcodes
			int extraNOPs = instructionSize - farJumpBaseInstructionSize;
			if ( extraNOPs < 0 )
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve FAR JUMP instruction opcode: the FAR JUMP instruction is larger than the instruction that is going to be replaced.",
					GetInjectorNameWithTemplateParameters() ) );
			}
			for ( int n = 0; n < extraNOPs; n++ )
				tmpBytes.Add( LowLevel.OPCODE_x86_NOP );

			// Return the result
			return tmpBytes.ToArray();
		}
		#endregion





		#region PUBLIC METHODS
		/// <summary>
		///    Constructor. The constructor of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class checks the code caves and
		///    variables for consistency, throwing an exception if there is any error found.
		/// </summary>
		public Injector()
        {
            // Check the template parameters used to create the Injector instance: both must represent enumeration types.
			if ( typeof( TMemoryAlterationSetID ).IsEnum == false )
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to create instance. The type defined for memory alteration set identifiers was \"{1}\", while it MUST be an enumerated type!",
					GetInjectorNameWithTemplateParameters(), typeof( TMemoryAlterationSetID ).Name ) );
			}

			if ( typeof( TCodeCave ).IsEnum == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to create instance. The type defined for code cave identifiers was \"{1}\", while it MUST be an enumerated type!",
                    GetInjectorNameWithTemplateParameters(), typeof( TCodeCave ).Name ) );
            }
            
            if ( typeof( TVariable ).IsEnum == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to create instance. The type defined for variable identifiers was \"{1}\", while it MUST be an enumerated type!",
                    GetInjectorNameWithTemplateParameters(), typeof( TVariable ).Name ) );
            }

            // Check all code cave definitions
            foreach ( TCodeCave curCodeCave in Enum.GetValues( typeof( TCodeCave ) ) )
            {
                CodeCaveDefinitionAttribute caveSpecs = GetEnumAttribute<CodeCaveDefinitionAttribute>( curCodeCave, true );
                caveSpecs.PerformSafetyChecks<TVariable>();
            }

            // Check all variables definitions
            foreach ( TVariable curVariable in Enum.GetValues( typeof( TVariable ) ) )
            {
                VariableDefinitionAttribute varSpecs = GetEnumAttribute<VariableDefinitionAttribute>( curVariable, true );
                Type varType = varSpecs.InitialValue.GetType();
                if ( SUPPORTED_VARIABLE_TYPES_SIZE.ContainsKey( varType ) == false && varType != typeof( IntPtr ) )
                {
                    throw new InjectorException( string.Format(
                        "[{0}] Failed to create instance. The variable identified by \"{1}\" is of type \"{2}\", and this type of data is NOT supported by the RAMvader library.",
                        GetInjectorNameWithTemplateParameters(), curVariable.ToString(), varType.Name ) );
                }
            }

            // Initialize indexers.
            // IMPORTANT: That is the ONLY point where indexers are initialized. Their respective properties'
            // setter methods will NEVER alter their instance references - their only purpose is to raise
            // "property changed" notifications for the WPF Binding system whenever necessary
            m_codeCaveOffset = new NestedPropertyIndexerCodeCaveOffset( this );
            m_injectedCodeCaveAddress = new NestedPropertyIndexerInjectedCodeCaveAddress( this );
            m_variableOffset = new NestedPropertyIndexerVariableOffset( this );
            m_injectedVariableAddress = new NestedPropertyIndexerInjectedVariableAddress( this );
            m_variableSize = new NestedPropertyIndexerVariableSize( this );
        }


		/// <summary>
		///    Initializes or modifies the reference to the object used by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    to perform write operations to the target process' memory. The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    also uses this object to know the endianness and pointer size of the target process.
		/// </summary>
		/// <param name="targetProc">The object used for performing memory I/O operations on the target process.</param>
		/// <seealso cref="GetTargetProcess"/>
		public void SetTargetProcess( RAMvaderTarget targetProc )
        {
            TargetProcess = targetProc;
        }


		/// <summary>
		///    Retrieves the current reference to the object used by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> to
		///    perform write operations to the target process' memory.
		///    The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> also uses this object to know the endianness and pointer
		///    size of the target process.
		/// </summary>
		/// <returns>Returns the object used for performing memory I/O operations on the target process.</returns>
		/// <seealso cref="SetTargetProcess(RAMvaderTarget)"/>
		public RAMvaderTarget GetTargetProcess()
        {
            return TargetProcess;
        }


		/// <summary>Retrieves the size of the pointers used on the target process.</summary>
		/// <returns>Returns the size of the pointers used on the target process, given in bytes.</returns>
		public int GetTargetProcessPointerSize()
        {
            // Target process must've been initialized
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "The {0} class cannot retrieve the pointer size for the target process: target process has not been initialized yet!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            // Try to retrieve the pointer size
            EPointerSize targetProcessPointerSize = TargetProcess.GetActualTargetPointerSize();
            switch ( targetProcessPointerSize )
            {
                case EPointerSize.evPointerSize32:
                    return 4;
                case EPointerSize.evPointerSize64:
                    return 8;
            }

            // Pointer type not supported
            throw new InjectorException( string.Format(
                "The {0} class cannot retrieve the size of a pointer of type \"{1}.{2}.{3}\"!",
                GetInjectorNameWithTemplateParameters(), typeof( RAMvaderTarget ).Name,
                typeof( EPointerSize ).Name,
                targetProcessPointerSize.ToString() ) );
        }


		/// <summary>Modifies the sequence of bytes used to separate two consecutive code caves.</summary>
		/// <param name="byteSeq">The new sequence of bytes to use as a separator. This can be an empty array, but should not be null.</param>
		public void SetCodeCavesSeparationBytes( byte[] byteSeq )
        {
            m_codeCavesSeparator = byteSeq;
        }


		/// <summary>Retrieves the sequence of bytes used to separate two consecutive code caves.</summary>
		/// <returns>Returns the sequence of bytes used to separate two consecutive code caves in memory.</returns>
		public byte[] GetCodeCavesSeparationBytes()
        {
            return m_codeCavesSeparator;
        }


		/// <summary>Modifies the sequence of bytes used to separate the injected code caves section from the injected variables section.</summary>
		/// <param name="byteSeq">The new sequence of bytes to use as a separator. This can be an empty array, but should not be null.</param>
		public void SetVariablesSectionSeparationBytes( byte[] byteSeq )
        {
            m_variablesSectionSeparator = byteSeq;
        }


		/// <summary>Retrieves the sequence of bytes used to separate the injected code caves section from the injected variables section.</summary>
		/// <returns>Returns the sequence of bytes used to separate two consecutive code caves in memory.</returns>
		public byte[] GetVariablesSectionSeparationBytes()
        {
            return m_variablesSectionSeparator;
        }


		/// <summary>Retrieves the address where the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> has injected its data on the target process.</summary>
		/// <returns>
		///    Returns the base address where the injection has been performed.
		///    If the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> didn't perform the injection yet, the return value is IntPtr.Zero.
		/// </returns>
		/// <seealso cref="Inject()"/>
		/// <seealso cref="Inject(IntPtr)"/>
		public IntPtr GetBaseInjectionAddress()
        {
            return BaseInjectionAddress;
        }


		/// <summary>Retrieves the offset of a given code cave, relative to the base injection address into the target process' memory space.</summary>
		/// <param name="codeCaveID">The identifier of the code cave.</param>
		/// <returns>Returns the offset of the given code cave.</returns>
		public int GetCodeCaveOffset( TCodeCave codeCaveID )
        {
            int offset = 0;
            TCodeCave [] codeCaves = (TCodeCave[]) Enum.GetValues( typeof( TCodeCave ) );
            for ( int c = 0; c < codeCaves.Length; c++ )
            {
                // Has the target code cave been found?
                if ( codeCaves[c].Equals( codeCaveID ) )
                    return offset;

                // Retrieve the size of the code cave, in bytes, through its
                // specification attribute. Add the code cave's size and the
                // code cave separation bytes count to calculate the next code cave's
                // offset.
                CodeCaveDefinitionAttribute codeCaveSpecs = GetEnumAttribute<CodeCaveDefinitionAttribute>( codeCaves[c], true );
                offset += codeCaveSpecs.GetCodeCaveSize( this );
                offset += m_codeCavesSeparator.Length;
            }

            throw new InjectorException( string.Format(
                "[{0}] Cannot retrieve offset for code cave identified by \"{1}\"!",
                GetInjectorNameWithTemplateParameters(), codeCaveID.ToString() ) );
        }


		/// <summary>
		///    Retrieves the address of an injected code cave.
		///    This method should only be called after a base injection address has been defined for the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    to Inject code caves and variables.
		/// </summary>
		/// <param name="codeCaveID">The identifier of the target code cave.</param>
		/// <returns>Returns the address of the given code cave, into the target process' memory space.</returns>
		public IntPtr GetInjectedCodeCaveAddress( TCodeCave codeCaveID )
        {
            if ( IsInjected == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve injected code cave's address (\"{1}\"): the {0} has not allocated memory into the target process yet!",
                    GetInjectorNameWithTemplateParameters(), codeCaveID.ToString() ) );
            }

			return BaseInjectionAddress + GetCodeCaveOffset( codeCaveID );
        }


		/// <summary>Retrieves the offset of a given variable, relative to the base injection address into the target process' memory space.</summary>
		/// <param name="varID">The identifier of the variable whose offset is to be retrieved.</param>
		/// <returns>Returns the offset to given variable.</returns>
		public int GetVariableOffset( TVariable varID )
        {
            // Get the offset for the injected variables region in memory...
            TCodeCave [] codeCaves = (TCodeCave[]) Enum.GetValues( typeof( TCodeCave ) );
            int lastDefinedCodeCaveOffset = 0, lastDefinedCodeCaveSize = 0;
            if ( codeCaves.Length > 0 )
            {
                TCodeCave lastDefinedCodeCave = codeCaves[codeCaves.Length - 1];
                CodeCaveDefinitionAttribute lastDefinedCodeCaveSpecs = GetEnumAttribute<CodeCaveDefinitionAttribute>( lastDefinedCodeCave, true );
                lastDefinedCodeCaveOffset = GetCodeCaveOffset( lastDefinedCodeCave );
                lastDefinedCodeCaveSize = lastDefinedCodeCaveSpecs.GetCodeCaveSize( this );
            }
            
            int varOffset = lastDefinedCodeCaveOffset + lastDefinedCodeCaveSize;
			if ( codeCaves.Length > 0 )
				varOffset += m_variablesSectionSeparator.Length;

			// Calculate the given variable's offset
			foreach ( TVariable curVar in Enum.GetValues( typeof( TVariable ) ) )
            {
                if ( curVar.Equals( varID ) )
                    return varOffset;
                varOffset += this.GetVariableSize( curVar );
            }

            throw new InjectorException( string.Format(
                "[{0}] Cannot retrieve offset for variable identified by \"{1}\"!",
                GetInjectorNameWithTemplateParameters(), varID.ToString() ) );
        }


		/** 
         * @param varID 
         * @return  */
		/// <summary>
		///    Retrieves the address of an injected variable.
		///    This method should only be called after a base injection address has been defined for
		///    the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> to Inject code caves and variables.
		/// </summary>
		/// <param name="varID">The identifier of the target variable.</param>
		/// <returns>Returns the address of the given variable, into the target process' memory space.</returns>
		public IntPtr GetInjectedVariableAddress( TVariable varID )
        {
            if ( IsInjected == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve injected variable's address (\"{1}\"): the {0} has not allocated memory into the target process yet!",
                    GetInjectorNameWithTemplateParameters(), varID.ToString() ) );
            }
            return BaseInjectionAddress + GetVariableOffset( varID );
        }


		/// <summary>Retrieves the address of an injected variable, represented as bytes stored in the target process' memory space.</summary>
		/// <param name="varID">The identifier of the target variable.</param>
		/// <returns>
		///    Returns the array of bytes representing the address of the injected variable, as it is to be stored into the target process'
		///    memory space.
		/// </returns>
		public byte [] GetInjectedVariableAddressAsBytes( TVariable varID )
        {
            // The target process HAS to be specified, because it is the only one who knows the target process'
            // pointers size and endianness
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "Cannot retrieve the bytes of a code cave: the {0} object has not been initialized with a {1}!",
                    GetInjectorNameWithTemplateParameters(), typeof( RAMvaderTarget ).Name ) );
            }

            // Retrive the address (in the target process' memory space) of the injected variable and then use
            // the RAMvaderTarget object to retrieve its byte-representation into the target process' memory space
            IntPtr varAddress = this.GetInjectedVariableAddress( varID );
            return TargetProcess.GetValueAsBytesArrayInTargetProcess( varAddress );
        }


		/// <summary>Retrieves the size of a given injection variable.</summary>
		/// <param name="varID">The identifier of the variable whose size is to be retrieved.</param>
		/// <returns>Returns the size of the given injection variable, given in bytes.</returns>
		public int GetVariableSize( TVariable varID )
        {
            // Retrieve the type of the injection variable
            VariableDefinitionAttribute injVarMetadata = GetEnumAttribute<VariableDefinitionAttribute>( varID, true );
            Type varType = injVarMetadata.InitialValue.GetType();

            // Pointer types have special processing, because the target process might use either 32-bit or 64-bit pointers
            if ( varType == typeof( IntPtr ) )
            {
                // Pointer types need the target process to be initialized
                if ( TargetProcess == null )
                {
                    throw new InjectorException( string.Format(
                        "The {0} class cannot retrieve the size of an injection variable of type IntPtr before its target process is initialized!",
                        GetInjectorNameWithTemplateParameters() ) );
                }

                return GetTargetProcessPointerSize();
            }
            return SUPPORTED_VARIABLE_TYPES_SIZE[varType];
        }


		/// <summary>
		///    Calculates the total number of required bytes to inject the code caves and variables into the target process' memory space.
		///    This calculation takes in consideration the separation bytes between two consecutive code caves, the separation between the
		///    code caves section and the variables section and the size of each one of the injection variables.
		/// </summary>
		/// <returns>Returns the number of bytes required to Inject into the target process' memory.</returns>
		public int CalculateRequiredBytesCount()
        {
            int totalRequiredBytes = 0;

            // Calculate space required for all code caves
            TCodeCave [] allCodeCaves = (TCodeCave[]) Enum.GetValues( typeof( TCodeCave ) );
            foreach ( TCodeCave curCodeCave in allCodeCaves )
            {
                CodeCaveDefinitionAttribute curCodeCaveSpecs = GetEnumAttribute<CodeCaveDefinitionAttribute>( curCodeCave, true );
                totalRequiredBytes += curCodeCaveSpecs.GetCodeCaveSize( this );
            }

            // Calculate space required between (consecutive) code caves
            totalRequiredBytes += ( allCodeCaves.Length - 1 ) * m_codeCavesSeparator.Length;

            // Calculate space required between the code caves section and the
            // variables section
            totalRequiredBytes += m_variablesSectionSeparator.Length;

            // Calculate space required for variables
            TVariable [] allVariables = (TVariable[]) Enum.GetValues( typeof( TVariable ) );
            foreach ( TVariable curVariable in allVariables )
                totalRequiredBytes += this.GetVariableSize( curVariable );

            return totalRequiredBytes;
        }


		/// <summary>
		///    Adds a memory alteration to the set of alterations related to a given identifier.
		///    Memory alteration sets are kept in as list, and this method adds a memory alteration to the end of this list.
		///    The elements of a set of memory alterations are enabled/disabled in the order they get added to the list.
		///    You can then call <see cref="SetMemoryAlterationsActive(TMemoryAlterationSetID, bool)"/> to enable or disable the whole set of alterations related to an identifier.
		/// </summary>
		/// <param name="memoryAlterationSetID">The identifier that identifies the set of alterations that can be enabled/disabled all at once.</param>
		/// <param name="memoryAlteration">An object representing the memory alteration that should be added to the given set.</param>
		public void AddMemoryAlteration( TMemoryAlterationSetID memoryAlterationSetID, MemoryAlterationBase memoryAlteration )
		{
			// Retrieve the list used to keep the given memory alterations set, creating it when necessary
			if ( m_memoryAlterationSets.ContainsKey( memoryAlterationSetID ) == false )
				m_memoryAlterationSets[memoryAlterationSetID] = new List<MemoryAlterationBase>();

			List<MemoryAlterationBase> memoryAlterationSet = m_memoryAlterationSets[memoryAlterationSetID];

			// Add alteration to the list
			memoryAlterationSet.Add( memoryAlteration );
		}


		/// <summary>
		///    Removes a memory alteration from the set of alterations related to a given identifier.
		///    Memory alteration sets are kept in as list, and this method removes a memory alteration from this list.
		///    The elements of a set of memory alterations are enabled/disabled in the order they get added to the list.
		///    You can then call <see cref="SetMemoryAlterationsActive(TMemoryAlterationSetID, bool)"/> to enable or disable the whole set of alterations related to an identifier.
		/// </summary>
		/// <param name="memoryAlterationSetID">The identifier that identifies the set of alterations that can be enabled/disabled all at once.</param>
		/// <param name="memoryAlteration">The memory alteration to be removed from the given set.</param>
		/// <returns>Returns a flag specifying if the alteration has been removed from the set.</returns>
		public bool RemoveMemoryAlteration( TMemoryAlterationSetID memoryAlterationSetID, MemoryAlterationBase memoryAlteration )
		{
			// Retrieve the list used to keep the given memory alterations set, creating it when necessary
			if ( m_memoryAlterationSets.ContainsKey( memoryAlterationSetID ) == false )
				return false;

			List<MemoryAlterationBase> memoryAlterationSet = m_memoryAlterationSets[memoryAlterationSetID];

			// Remove item form the list, removing the list if it gets empty
			bool result = memoryAlterationSet.Remove( memoryAlteration );
			if ( memoryAlterationSet.Count <= 0 )
				m_memoryAlterationSets.Remove( memoryAlterationSetID );
			return result;
		}


		/// <summary>Returns an enumerable object containing all memory alterations registered for a given memory alteration set.</summary>
		/// <param name="memoryAlterationSetID">The identifier that identifies the set of alterations that can be enabled/disabled all at once.</param>
		/// <returns>Returns an enumerable list containing all the memory alterations in the given set.</returns>
		public IEnumerable<MemoryAlterationBase> GetMemoryAlterations( TMemoryAlterationSetID memoryAlterationSetID )
		{
			if ( m_memoryAlterationSets.ContainsKey( memoryAlterationSetID ) == false )
				return null;
			return m_memoryAlterationSets[memoryAlterationSetID];
		}


		/// <summary>Activates or deactivates all the memory alterations registered for a given memory alterations set.</summary>
		/// <param name="memoryAlterationSetID">The identifier that identifies the set of alterations that can be enabled/disabled all at once.</param>
		/// <param name="bActivate">A flag specifying if the alterations should be activated or deactivated.</param>
		/// <returns>
		///    Returns a flag specifying if all alterations have been activated.
		///    If any of the memory alterations in a set fail to be activated/deactivated, the returned value is false.
		/// </returns>
		public bool SetMemoryAlterationsActive( TMemoryAlterationSetID memoryAlterationSetID, bool bActivate )
		{
			// If there is no alteration set with the given identifier, return true right away (nothing else to do)
			if ( m_memoryAlterationSets.ContainsKey( memoryAlterationSetID ) == false )
				return true;

			// Activate or deactivate alterations
			bool activationResult = true;
			foreach ( MemoryAlterationBase curMemoryAlteration in m_memoryAlterationSets[memoryAlterationSetID] )
			{
				if ( curMemoryAlteration.SetEnabled( this, bActivate ) == false )
					activationResult = false;
			}

			return activationResult;
		}


		/// <summary>
		///    Allocates memory into the target process' memory space and injects the code caves and
		///    variables into that allocated memory.
		/// </summary>
		/// <seealso cref="GetBaseInjectionAddress"/>
		/// <exception cref="InjectorException">
		///    Thrown when any errors occur regarding the injection process.
		///    The data set for this exception specifies the type of error that caused it to be thrown.
		/// </exception>
		public void Inject()
        {
            // The target process should've been already defined
            if ( TargetProcess == null )
                throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureRAMvaderTargetNull );

            // Verify if there's an attachment to a target process
            Process targetProcess = TargetProcess.GetAttachedProcess();
            if ( targetProcess == null )
                throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureNotAttached );

            // Allocate READ+WRITE+EXECUTE memory into the target process' memory space
            uint totalRequiredSpace = (uint) CalculateRequiredBytesCount();
			IntPtr baseInjectionAddress = IntPtr.Zero;
			if ( totalRequiredSpace != 0 )
			{
				baseInjectionAddress = WinAPI.VirtualAllocEx(
					targetProcess.Handle, IntPtr.Zero, totalRequiredSpace,
					WinAPI.AllocationType.Reserve | WinAPI.AllocationType.Commit,
					WinAPI.MemoryProtection.ExecuteReadWrite );
				if ( baseInjectionAddress == IntPtr.Zero )
					throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureMemoryAllocation );

				// Now the Injector is responsible for deallocating the allocated memory
				m_bHasAllocatedMemory = true;
			}

            // Continue with the rest of the injection procedures
            try
            {
                this.Inject( baseInjectionAddress );
            }
            catch ( InjectorException )
            {
                // Reset the Injector's data and throw the exception up
                this.ResetAllocatedMemoryData();
                throw;
            }
        }


		/// <summary>
		///    Injects the code caves and variables into the target process' memory space.
		///    This overloaded version of the <see cref="Inject()"/> method can be used to Inject the code caves into a specific point of the
		///    target process' memory space. Notice, though, that for the code caves to work correctly, they need to be injected
		///    into a memory region with appropriate permissions. Those are usually READ+WRITE+EXECUTE permissions (READ+WRITE
		///    for injected variables and EXECUTE for allowing the target process to execute the code caves). If you need to
		///    calculate the total number of bytes required by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> to inject
		///    the code caves and variables, see <see cref="CalculateRequiredBytesCount"/>.
		/// </summary>
		/// <param name="baseInjectionAddress">
		///    The address - into the target process' memory space - where the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    will Inject the code caves and variables.
		///    A value of "IntPtr.Zero" will cause the method to exit without any effect on the target process' memory space.
		/// </param>
		/// <exception cref="InjectorException">
		///    Thrown when any errors occur regarding the injection process.
		///    The data set for this exception specifies the type of error that caused it to be thrown.
		/// </exception>
		/// <seealso cref="GetBaseInjectionAddress"/>
		public void Inject( IntPtr baseInjectionAddress )
        {
			// Cannot inject anything into another process' memory space if something has already been injected.
			if ( this.IsInjected )
				throw new InjectorException( string.Format( "[{0}] Failed to Inject into target process' memory space: already injected into process' memory space!", GetInjectorNameWithTemplateParameters() ) );

			BaseInjectionAddress = baseInjectionAddress;
			IsInjected = true;

			// Generate the bytes which constitute the data to be injected
			int totalRequiredBytes = this.CalculateRequiredBytesCount();
            List<byte> bytesToInject = new List<byte>( totalRequiredBytes );

            // Get the bytes which represent the code caves section
            bool bIsFirstCave = true;
            foreach ( TCodeCave curCodeCave in Enum.GetValues( typeof( TCodeCave ) ) )
            {
                // Add separators between two consecutive code caves
                if ( bIsFirstCave == false )
                    bytesToInject.AddRange( m_codeCavesSeparator );
                bIsFirstCave = false;

                // Add the bytes which represent the code cave
                CodeCaveDefinitionAttribute caveSpecs = GetEnumAttribute<CodeCaveDefinitionAttribute>( curCodeCave, true );
                bytesToInject.AddRange( caveSpecs.GetCodeCaveBytes( this ) );
            }

            // Add separator from the variables section (only if one or more code caves have already been injected)
			if ( bytesToInject.Count > 0 )
				bytesToInject.AddRange( m_variablesSectionSeparator );

            // Add variables
            foreach ( TVariable curVarID in Enum.GetValues( typeof( TVariable ) ) )
            {
                VariableDefinitionAttribute varSpecs = GetEnumAttribute<VariableDefinitionAttribute>( curVarID, true );
                byte [] varInitialValueAsBytes = TargetProcess.GetValueAsBytesArrayInTargetProcess( varSpecs.InitialValue );
                bytesToInject.AddRange( varInitialValueAsBytes );
            }

			// Inject the data!
			if ( TargetProcess.WriteToTarget( baseInjectionAddress, bytesToInject.ToArray() ) == false )
				throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureWriteToTarget );
		}


		/// <summary>
		///    Resets the internal data of the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> regarding the memory region where it has injected its data.
		///    This method should be called whenever the target process is terminated or whenever the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object
		///    needs to deallocate the memory it has allocated on the target process.
		/// </summary>
		public void ResetAllocatedMemoryData()
        {
            // If the Injector has allocated memory on the target process, and if the target process
            // is still running, free that allocated memory
            if ( TargetProcess != null )
            {
                Process attachedProcess = TargetProcess.GetAttachedProcess();
                if ( IsInjected && m_bHasAllocatedMemory && attachedProcess != null && attachedProcess.HasExited == false )
                {
                    WinAPI.VirtualFreeEx( attachedProcess.Handle, BaseInjectionAddress, 0,
                        WinAPI.FreeType.Release );
                }
            }

            // Return allocation address to zero
            BaseInjectionAddress = IntPtr.Zero;
            m_bHasAllocatedMemory = false;
			IsInjected = false;
		}


		/// <summary>
		///    Writes a x86 CALL instruction at a specific point of the target process' memory space to enable the process' execution flow
		///    to be detoured to a specific address.
		/// </summary>
		/// <param name="detourPoint">The address of the target process' memory space where the CALL instruction will be written.</param>
		/// <param name="targetAddress">The address to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the CALL instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, so that when the execution flows back from
		///    the CALL instruction, nothing unexpected happens.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86CallInstruction( IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
        {
            // Error checking...
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot write a x86 CALL instruction: target process has not been initialized yet!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            if ( TargetProcess.IsAttached() == false )
            {
                throw new InjectorException( string.Format(
					"[{0}] Cannot write a x86 CALL instruction: not attached to a target process!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            // Build the CALL instruction
            byte [] callOpcode = GetX86CallOpcode( detourPoint, targetAddress, instructionSize, TargetProcess.TargetProcessEndianness,
                TargetProcess.TargetPointerSize, TargetProcess.PointerSizeErrorHandling );

            // Write the instruction
            return TargetProcess.WriteToTarget( detourPoint, callOpcode );
        }


		/// <summary>
		///    Writes a x86 CALL instruction at a specific point of the target process' memory space to enable the process'
		///    execution flow to be detoured to a specific, injected code cave.
		/// </summary>
		/// <param name="detourPoint">The address of the target process' memory space where the CALL instruction will be written.</param>
		/// <param name="codeCave">The code cave to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the CALL instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, so that when the execution flows back from
		///    the CALL instruction, nothing unexpected happens.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86CallToCodeCaveInstruction( IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.WriteX86CallInstruction( detourPoint, codeCaveAddress, instructionSize );
		}


		/// <summary>
		///    Writes a x86 NEAR JUMP instruction at a specific point of the target process' memory space to enable the process'
		///    execution flow to be detoured to a specific address.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be written.</param>
		/// <param name="detourPoint">The address of the target process' memory space where the JUMP instruction will be written.</param>
		/// <param name="targetAddress">The address to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the JUMP instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, to keep the other instructions' balance
		///    unaffected by the new jump instruction.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86NearJumpInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
		{
			// Error checking...
			if ( TargetProcess == null )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a x86 NEAR JUMP instruction: target process has not been initialized yet!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			if ( TargetProcess.IsAttached() == false )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a x86 NEAR JUMP: not attached to a target process!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// Build the CALL instruction
			byte [] jumpOpcode = GetX86NearJumpOpcode( jumpInstructionType, detourPoint, targetAddress, instructionSize,
				TargetProcess.TargetPointerSize );

			// Write the instruction
			return TargetProcess.WriteToTarget( detourPoint, jumpOpcode );
		}


		/// <summary>
		///    Writes a x86 NEAR JUMP instruction at a specific point of the target process' memory space to enable the process'
		///    execution flow to be detoured to a specific, injected code cave.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be written.</param>
		/// <param name="detourPoint">The address of the target process' memory space where the JUMP instruction will be written.</param>
		/// <param name="codeCave">The code cave to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the JUMP instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, so that the target process' code remains
		///    balanced and stable for debuggers.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86NearJumpToCodeCaveInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.WriteX86NearJumpInstruction( jumpInstructionType, detourPoint, codeCaveAddress, instructionSize );
		}


		/// <summary>
		///    Writes a x86 FAR JUMP instruction at a specific point of the target process' memory space to enable the process'
		///    execution flow to be detoured to a specific address.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be written.</param>
		/// <param name="detourPoint">The address of the target process' memory space where the JUMP instruction will be written.</param>
		/// <param name="targetAddress">The address to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the JUMP instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, to keep the other instructions' balance
		///    unaffected by the new jump instruction.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86FarJumpInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
		{
			// Error checking...
			if ( TargetProcess == null )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a x86 FAR JUMP instruction: target process has not been initialized yet!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			if ( TargetProcess.IsAttached() == false )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a x86 FAR JUMP: not attached to a target process!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// Build the CALL instruction
			byte [] jumpOpcode = GetX86FarJumpOpcode( jumpInstructionType, detourPoint, targetAddress, instructionSize,
				TargetProcess.TargetProcessEndianness, TargetProcess.TargetPointerSize, TargetProcess.PointerSizeErrorHandling );

			// Write the instruction
			return TargetProcess.WriteToTarget( detourPoint, jumpOpcode );
		}


		/** 
		 * @param jumpInstructionType 
         * @param detourPoint 
         * @param codeCave 
         * @param instructionSize  */
		/// <summary>
		///    Writes a x86 FAR JUMP instruction at a specific point of the target process' memory space to enable the process'
		///    execution flow to be detoured to a specific, injected code cave.
		/// </summary>
		/// <param name="jumpInstructionType">The specific type of jump instruction to be written.</param>
		/// <param name="detourPoint">The address of the target process' memory space where the JUMP instruction will be written.</param>
		/// <param name="codeCave">The code cave to where the target process' execution should be diverted.</param>
		/// <param name="instructionSize">
		///    The size of the instruction that is going to be replaced by the JUMP instruction.
		///    This is used to fill the remaining bytes of the instruction with NOP opcodes, so that the target process' code remains
		///    balanced and stable for debuggers.
		/// </param>
		/// <returns>Returns a flag indicating the success of the operation.</returns>
		public bool WriteX86FarJumpToCodeCaveInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.WriteX86FarJumpInstruction( jumpInstructionType, detourPoint, codeCaveAddress, instructionSize );
		}


		/// <summary>
		///    Updates the value of a given variable into the target process' memory.
		///    This method is safe, as it checks the given variable's metadata against the given value's type to see if it matches
		///    the variable's type before updating the variable's value.
		/// </summary>
		/// <param name="variableID">The identifier of the injected variable whose value is to be updated.</param>
		/// <param name="newValue">The new value for the variable.</param>
		/// <returns>Returns the result of the write operation performed by a call to <see cref="RAMvaderTarget.WriteToTarget(IntPtr, object)"/>.</returns>
		public bool WriteVariableValue( TVariable variableID, object newValue )
        {
            // Error checking...
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot update variable's value: target process has not been initialized yet!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            if ( TargetProcess.IsAttached() == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot update variable's value: not attached to a target process!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            VariableDefinitionAttribute varSpecs = GetEnumAttribute<VariableDefinitionAttribute>( variableID, true );
            Type injectedVariableType = varSpecs.InitialValue.GetType();
            Type givenValueType = newValue.GetType();
            if ( injectedVariableType != givenValueType )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot update variable's value: given value's type ({1}) does not match the injected variable's type ({2})!",
                    GetInjectorNameWithTemplateParameters(), givenValueType.Name, injectedVariableType.Name ) );
            }

            // Update the value in the target process' memory space
            IntPtr varInjectedAddress = this.GetInjectedVariableAddress( variableID );
            return this.TargetProcess.WriteToTarget( varInjectedAddress, newValue );
        }


		/// <summary>
		///    Reads the current value of a given variable from the target process' memory.
		///    This method is safe, as it checks the given variable's metadata against the given output variable's type to
		///    see if it matches the injected variable's type before reading the output value.
		/// </summary>
		/// <param name="variableID">The identifier of the variable whose value is to be read from the target process' memory space.</param>
		/// <param name="outputValue">The variable which will receive the read value.</param>
		/// <returns>Returns the result of the read operation performed by a call to <see cref="RAMvaderTarget.ReadFromTarget(IntPtr, ref object)"/>.</returns>
		public bool ReadVariableValue( TVariable variableID, ref object outputValue )
        {
            // Error checking...
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot read injected variable's value: target process has not been initialized yet!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            if ( TargetProcess.IsAttached() == false )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot read injected variable's value: not attached to a target process!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            VariableDefinitionAttribute varSpecs = GetEnumAttribute<VariableDefinitionAttribute>( variableID, true );
            Type injectedVariableType = varSpecs.InitialValue.GetType();
            Type outputValueType = outputValue.GetType();
            if ( injectedVariableType != outputValueType )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot read injected variable's value: given output value's type ({1}) does not match the injected variable's type ({2})!",
                    GetInjectorNameWithTemplateParameters(), outputValueType.Name, injectedVariableType.Name ) );
            }

            // Update the value in the target process' memory space
            IntPtr varInjectedAddress = this.GetInjectedVariableAddress( variableID );
            return this.TargetProcess.ReadFromTarget( varInjectedAddress, ref outputValue );
        }
        #endregion
    }
}
