﻿/*
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
    /** Implements the logic behind the injection of code caves and variables into a
     * target process' memory space.
     * @tparam TCodeCave An enumerated type which specifies the identifiers for code caves.
     *    Each enumerator belonging to this enumeration should have the #CodeCaveDefinitionAttribute attribute.
     * @tparam TVariable An enumerated type which specifies the identifiers for variables to be injected at the target process.
     *    Each enumerator belonging to this enumeration should have the #VariableDefinitionAttribute attribute. */
    public partial class Injector<TCodeCave, TVariable> : NotifyPropertyChangedAdapter
    {
        #region PRIVATE CONSTANTS
        /** Keeps both the supported types of variables that can be injected into the
         * target process' memory space and their corresponding sizes, given in number
         * of bytes. */
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





        #region PUBLIC CONSTANTS
        /** The byte value for the NOP opcode. */
        public const byte OPCODE_NOP = 0x90;
        /** The byte value for the INT3 opcode. */
        public const byte OPCODE_INT3 = 0xCC;
		/** Represents the byte value for the 32-bits CALL opcode. */
		public const byte OPCODE_32_BITS_CALL = 0xE8;
		/** Represents the byte value for the 32-bits NEAR JMP opcode. */
		public const byte OPCODE_32_BITS_NEAR_JMP = 0xEB;
		/** Represents the byte value for the 32-bits NEAR JA opcode. */
		public const byte OPCODE_32_BITS_NEAR_JA  = 0x77;
		/** Represents the byte value for the 32-bits NEAR JB opcode. */
		public const byte OPCODE_32_BITS_NEAR_JB  = 0x72;
		/** Represents the byte value for the 32-bits NEAR JG opcode. */
		public const byte OPCODE_32_BITS_NEAR_JG  = 0x7F;
		/** Represents the byte value for the 32-bits NEAR JL opcode. */
		public const byte OPCODE_32_BITS_NEAR_JL  = 0x7C;
		/** Represents the byte value for the 32-bits NEAR JE opcode. */
		public const byte OPCODE_32_BITS_NEAR_JE  = 0x74;
		/** Represents the byte value for the 32-bits NEAR JNE opcode. */
		public const byte OPCODE_32_BITS_NEAR_JNE = 0x75;
		/** Represents the byte value for the 32-bits FAR JMP opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JMP = { 0xE9 };
		/** Represents the byte value for the 32-bits FAR JA opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JA  = { 0x0F, 0x87 };
		/** Represents the byte value for the 32-bits FAR JB opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JB  = { 0x0F, 0x82 };
		/** Represents the byte value for the 32-bits FAR JG opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JG  = { 0x0F, 0x8F };
		/** Represents the byte value for the 32-bits FAR JL opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JL  = { 0x0F, 0x8C };
		/** Represents the byte value for the 32-bits FAR JE opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JE  = { 0x0F, 0x84 };
		/** Represents the byte value for the 32-bits FAR JNE opcode. */
		public static readonly byte [] OPCODE_32_BITS_FAR_JNE = { 0x0F, 0x85 };
		/** The size of a 32-bits CALL instruction, given in bytes. */
		public const int INSTRUCTION_SIZE_32_BITS_CALL = 5;
		/** The size of a 32-bits NEAR JUMP instruction, given in bytes. Near jumps allow jumps to instructions up to a distance of 0xFF bytes. */
		public const int INSTRUCTION_SIZE_32_BITS_NEAR_JUMP = 2;
		/** The size of a 32-bits FAR JUMP instruction, given in bytes. Far jumps allow jumps to instructions up to a distance of 0xFFFFFFFF bytes. */
		public const int INSTRUCTION_SIZE_32_BITS_FAR_JUMP = 5;
		#endregion






		#region PRIVATE FIELDS
		/** The object used to attach to the target process, so that the
         * #Injector can perform I/O operations into the target process' memory. */
		private RAMvaderTarget m_targetProcess;
        /** Keeps the base address of the memory which was allocated for the target
         * process. */
        private IntPtr m_baseInjectionAddress = IntPtr.Zero;
        /** A flag specifying if the #Injector has allocated memory in the target process for
         * injecting its data. When the #Injector allocates memory in the target process, it is
         * responsible for freing it whenever necessary. */
        private bool m_bHasAllocatedMemory = false;
        /** The sequence of bytes which separate two consecutive code caves. */
        private byte [] m_codeCavesSeparator =
        {
            OPCODE_NOP, OPCODE_NOP, OPCODE_NOP, OPCODE_NOP,
            OPCODE_NOP, OPCODE_NOP, OPCODE_NOP, OPCODE_NOP,
        };
        /** The sequence of bytes which separate the code caves region from the variables region. */
        private byte [] m_variablesSectionSeparator =
        {
            OPCODE_INT3, OPCODE_INT3, OPCODE_INT3, OPCODE_INT3,
            OPCODE_INT3, OPCODE_INT3, OPCODE_INT3, OPCODE_INT3,
        };
        /** Indexer field used to access the code cave offsets, usually for WPF Binding purposes.
         * Calls #Injector.GetCodeCaveOffset() internally. */
        private NestedPropertyIndexerCodeCaveOffset m_codeCaveOffset;
        /** Indexer property used to access the address where a code cave has been injected, usually
         * for WPF Binding purposes.
         * Calls #Injector.GetInjectedCodeCaveAddress() internally.
         * Backed by the #m_injectedCodeCaveAddress field. */
        private NestedPropertyIndexerInjectedCodeCaveAddress m_injectedCodeCaveAddress;
        /** Indexer property used to access variable offsets, usually for WPF Binding purposes.
         * Calls #Injector.GetVariableOffset() internally.
         * Backed by the #m_variableOffset field. */
        private NestedPropertyIndexerVariableOffset m_variableOffset;
        /** Indexer property used to access the address where a variable has been injected, usually
         * for WPF Binding purposes.
         * Calls #Injector.GetInjectedVariableAddress() internally.
         * Backed by the #m_injectedVariableAddress field. */
        private NestedPropertyIndexerInjectedVariableAddress m_injectedVariableAddress;
        /** Indexer property used to retrieve the size of a variable, usually for WPF Binding purposes.
         * Calls #Injector.GetVariableSize() internally.
         * Backed by the #m_variableSize field. */
        private NestedPropertyIndexerVariableSize m_variableSize;
        #endregion





        #region PUBLIC PROPERTIES
        /** Keeps the base address of the memory which was allocated for the target
         * process. Backed by the #m_baseInjectionAddress field. */
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
        /** The object used to attach to the target process, so that the
         * #Injector can perform I/O operations into the target process' memory.
         * Backed by the #m_targetProcess field. */
        public RAMvaderTarget TargetProcess
        {
            get { return m_targetProcess; }
            private set
            {
                m_targetProcess = value;
                SendPropertyChangedNotification();
            }
        }
        /** The total number of required bytes to inject the code caves and variables into the target
         * process' memory space, as calculated by a call to the method #CalculateRequiredBytesCount(). */
        public int RequiredBytesCount
        {
            get { return CalculateRequiredBytesCount(); }
            private set
            {
                // Simulates property updating for the Binding system to work properly
                SendPropertyChangedNotification();
            }
        }
        /** Indexer property used to access the code cave offsets, usually for WPF Binding purposes.
         * Calls #Injector.GetCodeCaveOffset() internally.
         * Backed by the #m_codeCaveOffset field. */
        public NestedPropertyIndexerCodeCaveOffset CodeCaveOffset
        {
            get { return m_codeCaveOffset; }
            private set { SendPropertyChangedNotification(); }
        }
        /** Indexer property used to access the address where a code cave has been injected, usually
         * for WPF Binding purposes.
         * Calls #Injector.GetInjectedCodeCaveAddress() internally.
         * Backed by the #m_injectedCodeCaveAddress field. */
        public NestedPropertyIndexerInjectedCodeCaveAddress InjectedCodeCaveAddress
        {
            get { return m_injectedCodeCaveAddress; }
            private set { SendPropertyChangedNotification(); }
        }
        /** Indexer property used to access variable offsets, usually for WPF Binding purposes.
         * Calls #Injector.GetVariableOffset() internally.
         * Backed by the #m_variableOffset field. */
        public NestedPropertyIndexerVariableOffset VariableOffset
        {
            get { return m_variableOffset; }
            private set { SendPropertyChangedNotification(); }
        }
        /** Indexer property used to access the address where a variable has been injected, usually
         * for WPF Binding purposes.
         * Calls #Injector.GetInjectedVariableAddress() internally.
         * Backed by the #m_injectedVariableAddress field. */
        public NestedPropertyIndexerInjectedVariableAddress InjectedVariableAddress
        {
            get { return m_injectedVariableAddress; }
            private set { SendPropertyChangedNotification(); }
        }
        /** Indexer property used to retrieve the size of a variable, usually for WPF Binding purposes.
         * Calls #Injector.GetVariableSize() internally.
         * Backed by the #m_variableSize field. */
        public NestedPropertyIndexerVariableSize VariableSize
        {
            get { return m_variableSize; }
            private set { SendPropertyChangedNotification(); }
        }
        #endregion





        #region PRIVATE STATIC METHODS
        /** Retrieves an array of attributes associated to the given enumerator.
         * @tparam TAttrib The type of the Attribute to be retrieved.
         * @param enumeratorValue The value indicating the enumerator whose Attributes are to be
         *    retrieved.
         * @return Returns an array of attributes of the TAttrib type for the given enumerator
         *    value. */
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


        /** Retrieves an attribute associated to the given enumerator.
         * @tparam TAttrib The type of the Attribute to be retrieved.
         * @param enumeratorValue The value indicating the enumerator whose Attribute is to be
         *    retrieved.
         * @param bThrowException A flag indicating if an exception should be thrown when the attribute
         *    is not found. If that flag is set to false, the method simply returns a null value when the
         *    attribute can't be retrieved.
         * @return Returns an array of attributes of the TAttrib type for the given enumerator
         *    value. */
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


        /*** Utility method for retrieving a human-readable name for the #Injector class, including the name of
         * its generic parameters.
         * @return Returns a string containing the name of the #Injector class and its generic parameters. */
        private static string GetInjectorNameWithTemplateParameters()
        {
            Type injectorType = typeof( Injector<TCodeCave, TVariable> );
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
        /** Utility method for retrieving a sequence of bytes which represent the machine-level opcode corresponding to a 32-bits CALL instruction.
		 * 64-bits CALL instructions are currently not supported by the RAMvader library.
         * @param callInstructionAddress The address of the CALL instruction itself.
         * @param targetCallAddress The address which should be called by the CALL instruction.
         * @param instructionSize When replacing an instruction in a target process' memory space by a CALL instruction, this parameter specifies
         *    the size of the instruction to be replaced. If this size is larger than the size of a CALL instruction, the remaining bytes are filled
         *    with NOP opcodes in the returned bytes sequence, so that the CALL instruction might replace other instructions while keeping the consistency
         *    of its surrounding instructions when a RET instruction is used to return from the CALL.
         * @param endianness The endianness to be used for the offset of the CALL opcode.
         * @param pointerSize The size of pointer to be used for the offset of the CALL opcode.
         * @param diffPointerSizeError The policy for handling errors regarding different sizes of pointers between RAMvader process'
         *    pointers and the pointers size defined by the "pointerSize" parameter.
         * @return Returns a sequence of bytes representing the CALL opcode that composes the given instruction. */
        public static byte [] Get32BitsCallOpcode( IntPtr callInstructionAddress, IntPtr targetCallAddress,
            int instructionSize = INSTRUCTION_SIZE_32_BITS_CALL,
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
                callOffset = (Int32) ( targetCallAddress.ToInt32() - callInstructionAddress.ToInt32() - INSTRUCTION_SIZE_32_BITS_CALL );
            else if ( pointerSize == EPointerSize.evPointerSize64 )
                callOffset = (Int64) ( targetCallAddress.ToInt64() - callInstructionAddress.ToInt64() - INSTRUCTION_SIZE_32_BITS_CALL );
            else
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to retrieve CALL instruction opcode: the specified pointer size is not supported.",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            // Build the CALL opcode
            List<byte> tmpBytes = new List<byte>( INSTRUCTION_SIZE_32_BITS_CALL );
            tmpBytes.Add( OPCODE_32_BITS_CALL );

            byte [] callOffsetAsBytes = RAMvaderTarget.GetValueAsBytesArray( callOffset, endianness, pointerSize, diffPointerSizeError );
            tmpBytes.AddRange( callOffsetAsBytes );

            // Fill the remaining bytes of the given instruction size with NOP opcodes
            int extraNOPs = instructionSize - INSTRUCTION_SIZE_32_BITS_CALL;
            if ( extraNOPs < 0 )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Failed to retrieve CALL instruction opcode: the CALL instruction is larger than the instruction that is going to be replaced.",
                    GetInjectorNameWithTemplateParameters() ) );
            }
            for ( int n = 0; n < extraNOPs; n++ )
                tmpBytes.Add( OPCODE_NOP );

            // Return the result
            return tmpBytes.ToArray();
        }


		/** Utility method for retrieving a sequence of bytes which represent the machine-level opcode corresponding to a 32-bits NEAR JUMP instruction.
		 * 64-bits JUMP instructions are currently not supported by the RAMvader library.
		 * @param jumpInstructionType The specific type of jump instruction to be generated.
         * @param jumpInstructionAddress The address of the JUMP instruction itself.
         * @param targetJumpAddress The address to which the JUMP instruction should jump.
         * @param instructionSize When replacing an instruction in a target process' memory space by a JUMP instruction, this parameter specifies
         *    the size of the instruction to be replaced. If this size is larger than the size of a JUMP instruction, the remaining bytes are filled
         *    with NOP opcodes in the returned bytes sequence, so that the JUMP instruction might replace other instructions while keeping the
		 *    consistency of its surrounding instructions when the flow of code returns from the jump (if that ever happens).
         * @param pointerSize The size of pointer to be used for the offset of the JUMP opcode.
         * @return Returns a sequence of bytes representing the JUMP opcode that composes the given instruction. */
		public static byte[] Get32BitsNearJumpOpcode( EJumpInstructionType jumpInstructionType,
			IntPtr jumpInstructionAddress, IntPtr targetJumpAddress,
			int instructionSize = INSTRUCTION_SIZE_32_BITS_NEAR_JUMP,
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
				Int32 numJumpOffset = (Int32) ( targetJumpAddress.ToInt32() - jumpInstructionAddress.ToInt32() - INSTRUCTION_SIZE_32_BITS_NEAR_JUMP );
				offsetIsValid = ( numJumpOffset >= SByte.MinValue && numJumpOffset <= SByte.MaxValue );

				// Convert offset to unsigned byte (if necessary)
				if ( numJumpOffset < 0 )
					numJumpOffset = Byte.MaxValue + 1 + numJumpOffset;
				jumpOffset = numJumpOffset;
			}
			else if ( pointerSize == EPointerSize.evPointerSize64 )
			{
				// Calculate offset as a signed byte value and verify if offset is valid (if it fits into a single, signed byte)
				Int64 numJumpOffset = (Int64) ( targetJumpAddress.ToInt64() - jumpInstructionAddress.ToInt64() - INSTRUCTION_SIZE_32_BITS_NEAR_JUMP );
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
			List<byte> tmpBytes = new List<byte>( INSTRUCTION_SIZE_32_BITS_NEAR_JUMP );
			switch ( jumpInstructionType )
			{
				case EJumpInstructionType.evJMP:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JMP );
					break;
				case EJumpInstructionType.evJA:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JA );
					break;
				case EJumpInstructionType.evJB:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JB );
					break;
				case EJumpInstructionType.evJG:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JG );
					break;
				case EJumpInstructionType.evJL:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JL );
					break;
				case EJumpInstructionType.evJE:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JE );
					break;
				case EJumpInstructionType.evJNE:
					tmpBytes.Add( OPCODE_32_BITS_NEAR_JNE );
					break;
				default:
					throw new InjectorException( string.Format(
						"[{0}] Failed to retrieve NEAR JUMP instruction opcode: the specified NEAR JUMP instruction type is not supported.",
						GetInjectorNameWithTemplateParameters() ) );
			}

			tmpBytes.Add( byteJumpOffset );

			// Fill the remaining bytes of the given instruction size with NOP opcodes
			int extraNOPs = instructionSize - INSTRUCTION_SIZE_32_BITS_NEAR_JUMP;
			if ( extraNOPs < 0 )
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve NEAR JUMP instruction opcode: the NEAR JUMP instruction is larger than the instruction that is going to be replaced.",
					GetInjectorNameWithTemplateParameters() ) );
			}
			for ( int n = 0; n < extraNOPs; n++ )
				tmpBytes.Add( OPCODE_NOP );

			// Return the result
			return tmpBytes.ToArray();
		}


		/** Utility method for retrieving a sequence of bytes which represent the machine-level opcode corresponding to a 32-bits FAR JUMP instruction.
		 * 64-bits JUMP instructions are currently not supported by the RAMvader library.
		 * @param jumpInstructionType The specific type of jump instruction to be generated.
         * @param jumpInstructionAddress The address of the JUMP instruction itself.
         * @param targetJumpAddress The address to which the JUMP instruction should jump.
         * @param instructionSize When replacing an instruction in a target process' memory space by a JUMP instruction, this parameter specifies
         *    the size of the instruction to be replaced. If this size is larger than the size of a JUMP instruction, the remaining bytes are filled
         *    with NOP opcodes in the returned bytes sequence, so that the JUMP instruction might replace other instructions while keeping the
		 *    consistency of its surrounding instructions when the flow of code returns from the jump (if that ever happens).
         * @param endianness The endianness to be used for the offset of the JUMP opcode.
         * @param pointerSize The size of pointer to be used for the offset of the JUMP opcode.
         * @param diffPointerSizeError The policy for handling errors regarding different sizes of pointers between RAMvader process'
         *    pointers and the pointers size defined by the "pointerSize" parameter.
         * @return Returns a sequence of bytes representing the JUMP opcode that composes the given instruction. */
		public static byte[] Get32BitsFarJumpOpcode( EJumpInstructionType jumpInstructionType,
			IntPtr jumpInstructionAddress, IntPtr targetJumpAddress,
			int instructionSize = INSTRUCTION_SIZE_32_BITS_FAR_JUMP,
			EEndianness endianness = EEndianness.evEndiannessDefault,
			EPointerSize pointerSize = EPointerSize.evPointerSizeDefault,
			EDifferentPointerSizeError diffPointerSizeError = EDifferentPointerSizeError.evThrowException )
		{
			// Initialize defaults
			if ( endianness == EEndianness.evEndiannessDefault )
				endianness = BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

			if ( pointerSize == EPointerSize.evPointerSizeDefault )
				pointerSize = RAMvaderTarget.GetRAMvaderPointerSize();

			// Calculate the offset between the JUMP instruction and the target address that it should call
			Object jumpOffset;
			bool offsetIsValid = false;

			if ( pointerSize == EPointerSize.evPointerSize32 )
			{
				// Calculate offset as a signed byte value. Using 32-bits calculations, the offset is ALWAYS valid.
				jumpOffset = (Int32) ( targetJumpAddress.ToInt32() - jumpInstructionAddress.ToInt32() - INSTRUCTION_SIZE_32_BITS_FAR_JUMP );
				offsetIsValid = true;
			}
			else if ( pointerSize == EPointerSize.evPointerSize64 )
			{
				// Calculate offset and verify if it is valid (if it fits into a single, signed Int32)
				Int64 numJumpOffset = (Int64) ( targetJumpAddress.ToInt64() - jumpInstructionAddress.ToInt64() - INSTRUCTION_SIZE_32_BITS_FAR_JUMP );
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
			List<byte> tmpBytes = new List<byte>( INSTRUCTION_SIZE_32_BITS_FAR_JUMP );
			switch ( jumpInstructionType )
			{
				case EJumpInstructionType.evJMP:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JMP );
					break;
				case EJumpInstructionType.evJA:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JA );
					break;
				case EJumpInstructionType.evJB:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JB );
					break;
				case EJumpInstructionType.evJG:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JG );
					break;
				case EJumpInstructionType.evJL:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JL );
					break;
				case EJumpInstructionType.evJE:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JE );
					break;
				case EJumpInstructionType.evJNE:
					tmpBytes.AddRange( OPCODE_32_BITS_FAR_JNE );
					break;
				default:
					throw new InjectorException( string.Format(
						"[{0}] Failed to retrieve FAR JUMP instruction opcode: the specified FAR JUMP instruction type is not supported.",
						GetInjectorNameWithTemplateParameters() ) );
			}

			byte [] jumpOffsetAsBytes = RAMvaderTarget.GetValueAsBytesArray( jumpOffset, endianness, pointerSize, diffPointerSizeError );
			tmpBytes.AddRange( jumpOffsetAsBytes );

			// Fill the remaining bytes of the given instruction size with NOP opcodes
			int extraNOPs = instructionSize - INSTRUCTION_SIZE_32_BITS_FAR_JUMP;
			if ( extraNOPs < 0 )
			{
				throw new InjectorException( string.Format(
					"[{0}] Failed to retrieve FAR JUMP instruction opcode: the FAR JUMP instruction is larger than the instruction that is going to be replaced.",
					GetInjectorNameWithTemplateParameters() ) );
			}
			for ( int n = 0; n < extraNOPs; n++ )
				tmpBytes.Add( OPCODE_NOP );

			// Return the result
			return tmpBytes.ToArray();
		}
		#endregion





		#region PUBLIC METHODS
		/** Constructor. The constructor of the #Injector class checks the code caves and variables for consistency, throwing an exception if
         * there is any error found.
         * @throw InjectorException Thrown when there is an inconsistency in the definition of code caves and/or variables. The inconsistency
         *    is specified in the exception's message. */
		public Injector()
        {
            // Check the template parameters used to create the Injector instance: both must represent enumeration types.
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


        /** Initializes or modifies the reference to the object used by the #Injector to perform write operations to the
         * target process' memory. The #Injector also uses this object to know the endianness and pointer size of the target
         * process.
         * @param targetProc The object used for performing memory I/O operations on the target process.
         * @see #GetTargetProcess() */
        public void SetTargetProcess( RAMvaderTarget targetProc )
        {
            TargetProcess = targetProc;
        }


        /** Retrieves the current reference to the object used by the #Injector to perform write operations to the
         * target process' memory. The #Injector also uses this object to know the endianness and pointer size of the target
         * process.
         * @return Returns the object used for performing memory I/O operations on the target process.
         * @see #SetTargetProcess() */
        public RAMvaderTarget GetTargetProcess()
        {
            return TargetProcess;
        }


        /** Retrieves the size of the pointers used on the target process.
         * @return Returns the size of the pointers used on the target process, given in bytes. */
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


        /** Modifies the sequence of bytes used to separate two consecutive code caves.
         * @param byteSeq The new sequence of bytes to use as a separator. This can be an empty array, but
         *    should not be null. */
        public void SetCodeCavesSeparationBytes( byte[] byteSeq )
        {
            m_codeCavesSeparator = byteSeq;
        }


        /** Retrieves the sequence of bytes used to separate two consecutive code caves.
         * @return Returns the sequence of bytes used to separate two consecutive code caves
         *    in memory. */
        public byte[] GetCodeCavesSeparationBytes()
        {
            return m_codeCavesSeparator;
        }


        /** Modifies the sequence of bytes used to separate the injected code caves
         * section from the injected variables section.
         * @param byteSeq The new sequence of bytes to use as a separator. This can be an empty array, but
         *    should not be null. */
        public void SetVariablesSectionSeparationBytes( byte[] byteSeq )
        {
            m_variablesSectionSeparator = byteSeq;
        }


        /** Retrieves the sequence of bytes used to separate the injected code caves
         * section from the injected variables section.
         * @return Returns the sequence of bytes used to separate two consecutive code caves
         *    in memory. */
        public byte[] GetVariablesSectionSeparationBytes()
        {
            return m_variablesSectionSeparator;
        }


        /** Retrieves the address where the #Injector has injected its data on the target process.
         * @return Returns the base address where the injection has been performed.
         *    If the #Injector didn't perform the injection yet, the return value is IntPtr.Zero.
         * @see #Inject() */
        public IntPtr GetBaseInjectionAddress()
        {
            return BaseInjectionAddress;
        }


        /** Retrieves the offset of a given code cave, relative to the base injection
         * address into the target process' memory space.
         * @param codeCaveID The identifier of the code cave.
         * @return Returns the offset of the given code cave. */
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


        /** Retrieves the address of an injected code cave. This method should only be
         * called after a base injection address has been defined for the #Injector to
         * Inject code caves and variables.
         * @param codeCaveID The identifier of the target code cave.
         * @return Returns the address of the given code cave, into the target process'
         *    memory space. */
        public IntPtr GetInjectedCodeCaveAddress( TCodeCave codeCaveID )
        {
            if ( BaseInjectionAddress == IntPtr.Zero )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve injected code cave's address (\"{1}\"): the {0} has not allocated memory into the target process yet!",
                    GetInjectorNameWithTemplateParameters(), codeCaveID.ToString() ) );
            }

            return BaseInjectionAddress + GetCodeCaveOffset( codeCaveID );
        }


        /** Retrieves the offset of a given variable, relative to the base injection
         * address into the target process' memory space.
         * @param varID The identifier of the variable whose offset is to be retrieved.
         * @return Returns the offset to given variable. */
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
            
            int varOffset = lastDefinedCodeCaveOffset + lastDefinedCodeCaveSize + m_variablesSectionSeparator.Length;

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


        /** Retrieves the address of an injected variable. This method should only be
         * called after a base injection address has been defined for the #Injector to
         * Inject code caves and variables.
         * @param varID The identifier of the target variable.
         * @return Returns the address of the given variable, into the target process'
         *    memory space. */
        public IntPtr GetInjectedVariableAddress( TVariable varID )
        {
            if ( BaseInjectionAddress == IntPtr.Zero )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot retrieve injected variable's address (\"{1}\"): the {0} has not allocated memory into the target process yet!",
                    GetInjectorNameWithTemplateParameters(), varID.ToString() ) );
            }
            return BaseInjectionAddress + GetVariableOffset( varID );
        }


        /** Retrieves the address of an injected variable, represented as bytes stored in the target process' memory space.
         * @param varID The identifier of the target variable.
         * @return Returns the array of bytes representing the address of the injected variable, as it is to be stored into
         *    the target process' memory space. */
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


        /** Retrieves the size of a given injection variable.
         * @param varID The identifier of the variable whose size is to be retrieved.
         * @return Returns the size of the given injection variable, given in bytes. */
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


        /** Calculates the total number of required bytes to inject the code caves and
         * variables into the target process' memory space. This calculation takes in
         * consideration the separation bytes between two consecutive code caves, the separation
         * between the code caves section and the variables section and the size of each one of
         * the injection variables.
         * @return Returns the number of bytes required to Inject into the target process' memory. */
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


        /** Allocates memory into the target process' memory space and injects the code
         * caves and variables into that allocated memory.
         * @see #GetBaseInjectionAddress()
         * @throw #InjectorException Thrown when any errors occur regarding the injection process.
         *    The data set for this exception specifies the type of error that caused it to be
         *    thrown. */
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
            IntPtr baseInjectionAddress = WinAPI.VirtualAllocEx(
                targetProcess.Handle, IntPtr.Zero, totalRequiredSpace,
                WinAPI.AllocationType.Reserve | WinAPI.AllocationType.Commit,
                WinAPI.MemoryProtection.ExecuteReadWrite );
            if ( baseInjectionAddress == IntPtr.Zero )
                throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureMemoryAllocation );

            // Now the Injector is responsible for deallocating the allocated memory
            m_bHasAllocatedMemory = true;

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


        /** Injects the code caves and variables into the target process' memory space. This overloaded version of
         * the #Inject() method can be used to Inject the code caves into a specific point of the target process'
         * memory space. Notice, though, that for the code caves to work correctly, they need to be injected into a
         * memory region with appropriate permissions. Those are usually READ+WRITE+EXECUTE permissions (READ+WRITE
         * for injected variables and EXECUTE for allowing the target process to execute the code caves). If you need
         * to calculate the total number of bytes required by the #Injector to Inject the code caves and variables,
         * see #CalculateRequiredBytesCount().
         * @param baseInjectionAddress The address - into the target process' memory
         *    space - where the #Injector will Inject the code caves and variables.
         * @see #GetBaseInjectionAddress()
         * @throw #InjectorException Thrown when any errors occur regarding the injection process.
         *    The data set for this exception specifies the type of error that caused it to be
         *    thrown. */
        public void Inject( IntPtr baseInjectionAddress )
        {
            BaseInjectionAddress = baseInjectionAddress;

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

            // Add separator from the variables section
            bytesToInject.AddRange( m_variablesSectionSeparator );

            // Add variables
            foreach ( TVariable curVarID in Enum.GetValues( typeof( TVariable ) ) )
            {
                VariableDefinitionAttribute varSpecs = GetEnumAttribute<VariableDefinitionAttribute>( curVarID, true );
                byte [] varInitialValueAsBytes = TargetProcess.GetValueAsBytesArrayInTargetProcess( varSpecs.InitialValue );
                bytesToInject.AddRange( varInitialValueAsBytes );
            }

            // Inject the data!
            if ( TargetProcess.WriteToTarget( BaseInjectionAddress, bytesToInject.ToArray() ) == false )
                throw new InjectionFailureException( InjectionFailureException.EFailureType.evFailureWriteToTarget );
        }


        /** Resets the internal data of the #Injector regarding the memory region where it has injected its data.
         * This method should be called whenever the target process is terminated or whenever the #Injector object
         * needs to deallocate the memory it has allocated on the target process. */
        public void ResetAllocatedMemoryData()
        {
            // If the Injector has allocated memory on the target process, and if the target process
            // is still running, free that allocated memory
            if ( TargetProcess != null )
            {
                Process attachedProcess = TargetProcess.GetAttachedProcess();
                if ( m_bHasAllocatedMemory && attachedProcess != null && attachedProcess.HasExited == false )
                {
                    WinAPI.VirtualFreeEx( attachedProcess.Handle, BaseInjectionAddress, 0,
                        WinAPI.FreeType.Release );
                }
            }

            // Return allocation address to zero
            BaseInjectionAddress = IntPtr.Zero;
            m_bHasAllocatedMemory = false;
        }


		/** Writes a 32-BITS CALL instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific address.
         * @param detourPoint The address of the target process' memory space where the
         *    CALL instruction will be written.
         * @param targetAddress The address to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the CALL instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, so that when the execution flows back from the CALL instruction,
         *    nothing unexpected happens. */
		public bool Write32BitsCallInstruction( IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
        {
            // Error checking...
            if ( TargetProcess == null )
            {
                throw new InjectorException( string.Format(
                    "[{0}] Cannot write a 32-BITS CALL instruction: target process has not been initialized yet!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            if ( TargetProcess.IsAttached() == false )
            {
                throw new InjectorException( string.Format(
					"[{0}] Cannot write a 32-BITS CALL instruction: not attached to a target process!",
                    GetInjectorNameWithTemplateParameters() ) );
            }

            // Build the CALL instruction
            byte [] callOpcode = Get32BitsCallOpcode( detourPoint, targetAddress, instructionSize, TargetProcess.TargetProcessEndianness,
                TargetProcess.TargetPointerSize, TargetProcess.PointerSizeErrorHandling );

            // Write the instruction
            return TargetProcess.WriteToTarget( detourPoint, callOpcode );
        }


		/** Writes a 32-BITS CALL instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific,
         * injected code cave.
         * @param detourPoint The address of the target process' memory space where the
         *    CALL instruction will be written.
         * @param codeCave The code cave to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the CALL instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, so that when the execution flows back from the CALL instruction,
         *    nothing unexpected happens. */
		public bool Write32BitsCallToCodeCaveInstruction( IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.Write32BitsCallInstruction( detourPoint, codeCaveAddress, instructionSize );
		}


		/** Writes a 32-BITS NEAR JUMP instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific address.
		 * @param jumpInstructionType The specific type of jump instruction to be written.
         * @param detourPoint The address of the target process' memory space where the
         *    JUMP instruction will be written.
         * @param targetAddress The address to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the JUMP instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, to keep the other instructions' balance unaffected by the new jump
		 *    instruction. */
		public bool Write32BitsNearJumpInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
		{
			// Error checking...
			if ( TargetProcess == null )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a 32-BITS NEAR JUMP instruction: target process has not been initialized yet!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			if ( TargetProcess.IsAttached() == false )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a 32-BITS NEAR JUMP: not attached to a target process!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// Build the CALL instruction
			byte [] jumpOpcode = Get32BitsNearJumpOpcode( jumpInstructionType, detourPoint, targetAddress, instructionSize,
				TargetProcess.TargetPointerSize );

			// Write the instruction
			return TargetProcess.WriteToTarget( detourPoint, jumpOpcode );
		}


		/** Writes a 32-BITS NEAR JUMP instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific,
         * injected code cave.
		 * @param jumpInstructionType The specific type of jump instruction to be written.
         * @param detourPoint The address of the target process' memory space where the
         *    JUMP instruction will be written.
         * @param codeCave The code cave to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the JUMP instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, so that the target process' code remains balanced and stable for debuggers. */
		public bool Write32BitsNearJumpToCodeCaveInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.Write32BitsNearJumpInstruction( jumpInstructionType, detourPoint, codeCaveAddress, instructionSize );
		}


		/** Writes a 32-BITS FAR JUMP instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific address.
		 * @param jumpInstructionType The specific type of jump instruction to be written.
         * @param detourPoint The address of the target process' memory space where the
         *    JUMP instruction will be written.
         * @param targetAddress The address to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the JUMP instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, to keep the other instructions' balance unaffected by the new jump
		 *    instruction. */
		public bool Write32BitsFarJumpInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, IntPtr targetAddress, int instructionSize )
		{
			// Error checking...
			if ( TargetProcess == null )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a 32-BITS FAR JUMP instruction: target process has not been initialized yet!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			if ( TargetProcess.IsAttached() == false )
			{
				throw new InjectorException( string.Format(
					"[{0}] Cannot write a 32-BITS FAR JUMP: not attached to a target process!",
					GetInjectorNameWithTemplateParameters() ) );
			}

			// Build the CALL instruction
			byte [] jumpOpcode = Get32BitsFarJumpOpcode( jumpInstructionType, detourPoint, targetAddress, instructionSize,
				TargetProcess.TargetProcessEndianness, TargetProcess.TargetPointerSize, TargetProcess.PointerSizeErrorHandling );

			// Write the instruction
			return TargetProcess.WriteToTarget( detourPoint, jumpOpcode );
		}


		/** Writes a 32-BITS FAR JUMP instruction at a specific point of the target process' memory
         * space to enable the process' execution flow to be detoured to a specific,
         * injected code cave.
		 * @param jumpInstructionType The specific type of jump instruction to be written.
         * @param detourPoint The address of the target process' memory space where the
         *    JUMP instruction will be written.
         * @param codeCave The code cave to where the target process' execution should be
         *    diverted.
         * @param instructionSize The size of the instruction that is going to be replaced by
         *    the JUMP instruction. This is used to fill the remaining bytes of the instruction
         *    with NOP opcodes, so that the target process' code remains balanced and stable for debuggers. */
		public bool Write32BitsFarJumpToCodeCaveInstruction( EJumpInstructionType jumpInstructionType,
			IntPtr detourPoint, TCodeCave codeCave, int instructionSize )
		{
			IntPtr codeCaveAddress = this.GetInjectedCodeCaveAddress( codeCave );
			return this.Write32BitsFarJumpInstruction( jumpInstructionType, detourPoint, codeCaveAddress, instructionSize );
		}


		/** Updates the value of a given variable into the target process' memory.
         * This method is safe, as it checks the given variable's metadata against the given value's type to see if
         * it matches the variable's type before updating the variable's value.
         * @param variableID The identifier of the injected variable whose value is to be updated.
         * @param newValue The new value for the variable.
         * @return Returns the result of the write operation performed by a call to #RAMvaderTarget.WriteToTarget(). */
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


        /** Reads the current value of a given variable from the target process' memory.
         * This method is safe, as it checks the given variable's metadata against the given output variable's type to see if
         * it matches the injected variable's type before reading the output value.
         * @param variableID The identifier of the variable whose value is to be read from the target process' memory space.
         * @param outputValue The variable which will receive the read value.
         * @return Returns the result of the read operation performed by a call to #RAMvaderTarget.ReadFromTarget(). */
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