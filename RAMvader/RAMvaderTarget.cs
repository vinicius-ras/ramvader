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

#if DEBUG
using System.Text;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;


namespace RAMvader
{
    /** The main working class of the library. Instances of this class are able
     * to "attach" to processes and execute reading and writing operations in their
     * memories. */
    public class RAMvaderTarget : NotifyPropertyChangedAdapter
    {
        #region PRIVATE CONSTANTS
        /** A dictionary containing both all basic data types supported by the RAMvader library and their respective sizes.
         * Notice, though, that the IntPtr type IS supported by the library but is not listed in this Dictionary, because it
         * is treated in a special way by the library, due to its variant-size nature. */
        private static readonly Dictionary<Type, int> SUPPORTED_DATA_TYPES_SIZE = new Dictionary<Type, int>()
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
        };
        #endregion








        #region PRIVATE FIELDS
        /** The Process to which this instance is currently attached. */
        private Process m_process = null;
        /** The low-level Handle to the target process we are attached to. */
        private IntPtr m_targetProcessHandle = IntPtr.Zero;
        /** The current endianness that the #RAMvaderTarget is operating on. The default is
         * for RAMvader to assume the target process runs in the same endianness as the process
         * that is running RAMvader. */
        private EEndianness m_targetProcessEndianness = EEndianness.evEndiannessDefault;
        /** Keeps the pointer size of the target process. Default configuration is to use
         * the same pointer size of the process which runs RAMvader. */
        private EPointerSize m_targetPointerSize = EPointerSize.evPointerSizeDefault;
        /** Determines the type of error handling which is used when the target process runs with a different
         * pointer size configuration, as compared to the process which runs RAMvader. */
        private EDifferentPointerSizeError m_diffPointerSizeError = EDifferentPointerSizeError.evThrowException;
        #endregion








        #region PUBLIC PROPERTIES
        /** The Process object which this RAMvaderTarget instance is attached to.
         * The #TargetProcess property can only be changed by calls to #AttachToProcess()
         * or #DetachFromProcess().
         * Backed by the #m_process field. */
        public Process TargetProcess
        {
            get { return m_process; }
            private set {
                m_process = value;
                SendPropertyChangedNotification();

                Attached = ( value != null );
            }
        }
        /** The handle to the Process object which this RAMvaderTarget instance is attached to.
         * The #ProcessHandle property can only be changed by calls to #AttachToProcess() or #DetachFromProcess().
         * Backed by the #m_targetProcessHandle field. */
        public IntPtr ProcessHandle
        {
            get { return m_targetProcessHandle; }
            private set { m_targetProcessHandle = value; SendPropertyChangedNotification(); }
        }
        /** A flag specifying if this instance is currently attached to a target process.
         * Returns the same result as the #IsAttached() method. */
        public bool Attached
        {
            get { return IsAttached(); }
            private set
            {
                // Simulate a property set event
                SendPropertyChangedNotification();
            }
        }
        /** The endianness configured for the target process.
         * This property can also be accessed through the methods #SetTargetEndianness()
         * and #GetTargetEndianness().
         * Backed by the #m_targetProcessEndianness field. */
        public EEndianness TargetProcessEndianness
        {
            get { return m_targetProcessEndianness; }
            set {
                m_targetProcessEndianness = value;
                SendPropertyChangedNotification();

                // Simulate a change to the ActualTargetProcessEndianness property, so that
                // it can send its own "property changed" notification
                ActualTargetProcessEndianness = value;
            }
        }
        /** The actual endianness that the #RAMvaderTarget instance is currently assuming that
         * the target process is using. This is the same value returned by
         * the #GetActualTargetEndianness() method - see its description for more details. */
        public EEndianness ActualTargetProcessEndianness
        {
            get
            {
                if ( TargetProcessEndianness == EEndianness.evEndiannessDefault )
                    return BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;
                return TargetProcessEndianness;
            }
            private set
            {
                // This property has no backing field, but it should notify whenever it
                // gets "changed" (which might happen when the TargetProcessEndianness
                // property changes)
                SendPropertyChangedNotification();
            }
        }
        /** The pointer size configured for the target process.
         * This property can also be accessed through the methods #SetTargetPointerSize()
         * and #GetTargetPointerSize().
         * Backed by the #m_targetPointerSize field. */
        public EPointerSize TargetPointerSize
        {
            get { return m_targetPointerSize; }
            set {
                m_targetPointerSize = value;
                SendPropertyChangedNotification();

                // Simulate a change to the ActualTargetPointerSize property, so that
                // it can send its own "property changed" notification
                ActualTargetPointerSize = value;
            }
        }
        /** The actual pointer size that the #RAMvaderTarget instance is currently assuming that
         * the target process is using. This is the same value returned by
         * the #GetActualTargetPointerSize() method - see its description for more details. */
        public EPointerSize ActualTargetPointerSize
        {
            get
            {
                if ( TargetPointerSize == EPointerSize.evPointerSizeDefault )
                    return GetRAMvaderPointerSize();
                return TargetPointerSize;
            }
            private set
            {
                // This property has no backing field, but it should notify whenever it
                // gets "changed" (which might happen when the TargetPointerSize
                // property changes)
                SendPropertyChangedNotification();
            }
        }
        /** The type of error handling which is used when the target process runs with a different
         * pointer size configuration, as compared to the process which runs RAMvader.
         * This property can also be accessed through the methods #SetTargetPointerSizeErrorHandling()
         * and #GetTargetPointerSizeErrorHandling().
         * Backed by the #m_diffPointerSizeError field. */
        public EDifferentPointerSizeError PointerSizeErrorHandling
        {
            get { return m_diffPointerSizeError; }
            set { m_diffPointerSizeError = value; SendPropertyChangedNotification(); }
        }
        #endregion








        #region DELEGATES
        /** Delegate used for handling the event which is fired when the #RAMvaderTarget object is attached to a process. */
        public delegate void AttachedEventHandler( object sender, EventArgs args );
        /** Delegate used for handling the event which is fired when the #RAMvaderTarget object is detached from a process. */
        public delegate void DetachedEventHandler( object sender, EventArgs args );
        #endregion








        #region EVENTS
        /** Handles the event that gets fired when the #RAMvaderTarget gets attached to a process. */
        public event AttachedEventHandler AttachedEvent;
        /** Handles the event that gets fired when the #RAMvaderTarget gets detached from a process. */
        public event DetachedEventHandler DetachedEvent;
        #endregion








        #region PUBLIC STATIC METHODS
        /** Retrieves the pointer size for the process which runs RAMvader.
         * @return Returns a #EPointerSize value, specifying the pointer size of the process. */
        public static EPointerSize GetRAMvaderPointerSize()
        {
            if ( IntPtr.Size == 4 )
                return EPointerSize.evPointerSize32;
            else if ( IntPtr.Size == 8 )
                return EPointerSize.evPointerSize64;
            else
                throw new RAMvaderException( string.Format(
                    "[{0}] The following pointer size (returned by IntPtr.Size) is not supported by RAMvader: {1} bytes.",
                    typeof( RAMvaderTarget ).Name, IntPtr.Size ) );
        }


        /** Utility method for retrieving a given value as an array of bytes, respecting the specified endianness.
         * @param value The value to be retrieved as a sequence of bytes.
         * @param endianness The endianness to be used when retrieving the sequence of bytes.
         * @param pointerSize The size of pointer to be used when retrieving the sequence of bytes. That parameter is
         *    only used when retrieving the bytes representation of IntPtr values.
         * @param diffPointerSizeError The policy for handling errors regarding different sizes of pointers between RAMvader process'
         *    pointers and the pointers size defined by the "pointerSize" parameter. That parameter is only used when retrieving the
         *    bytes representation of IntPtr values.
         * @return Returns a sequence of bytes representing the value in the given endianness (and pointer sizes, if applicable). */
        public static byte[] GetValueAsBytesArray( Object value, EEndianness endianness = EEndianness.evEndiannessDefault,
            EPointerSize pointerSize = EPointerSize.evPointerSizeDefault,
            EDifferentPointerSizeError diffPointerSizeError = EDifferentPointerSizeError.evThrowException )
        {
            // Initialize defaults
            if ( endianness == EEndianness.evEndiannessDefault )
                endianness = BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

            if ( pointerSize == EPointerSize.evPointerSizeDefault )
                pointerSize = RAMvaderTarget.GetRAMvaderPointerSize();

            // Is the value a single-byte value?
            if ( value is Byte )
                return new byte[] { (byte) value };

            // If this point is reached, the value is represented by multiple bytes,
            // and we need to consider the specified endianness (and possibli pointer sizes) to turn it into
            // a correct array of bytes.

            // For pointers, we need to verify how they will be written in the specified pointers size.
            // This depends on the configuration for the specified pointer size
            if ( value is IntPtr )
            {
                // Handle the cases where the size specified for pointers are different than RAMvader's pointer size
                EPointerSize curProcessPtrSize = GetRAMvaderPointerSize();

                Object ptrInSpecifiedSize = null;
                if ( curProcessPtrSize != pointerSize )
                {
                    switch ( diffPointerSizeError )
                    {
                        case EDifferentPointerSizeError.evThrowException:
                            throw new PointerDataLostException( false );
                        case EDifferentPointerSizeError.evSafeTruncation:
                        case EDifferentPointerSizeError.evUnsafeTruncation:
                            {
                                bool bIsSafeTruncation = ( diffPointerSizeError == EDifferentPointerSizeError.evSafeTruncation );

                                // Expand or truncate accordingly, for the write operation
                                bool bComparisonFailed = false;
                                if ( curProcessPtrSize == EPointerSize.evPointerSize32 )
                                {
                                    // Expand 32-bit pointer to 64-bits
                                    UInt32 ptrInCurProcess = (UInt32) (IntPtr) value;
                                    UInt64 ptrInTargetProcessExpanded = (UInt64) ptrInCurProcess;
                                    ptrInSpecifiedSize = (UInt64) ptrInCurProcess;

                                    bComparisonFailed = ( ptrInCurProcess != ptrInTargetProcessExpanded );
                                }
                                else
                                {
                                    // Truncate 64-bit pointer to 32-bits
                                    UInt64 ptrInCurProcess = (UInt64) (IntPtr) value;
                                    UInt32 ptrInTargetProcessTruncated = (UInt32) ptrInCurProcess;
                                    ptrInSpecifiedSize = (UInt32) ptrInCurProcess;

                                    bComparisonFailed = ( ptrInCurProcess != ptrInTargetProcessTruncated );
                                }

                                // Perform safety check, as needed
                                if ( bIsSafeTruncation && bComparisonFailed )
                                    throw new PointerDataLostException( false );
                                break;
                            }
                    }
                }
                else
                {
                    if ( curProcessPtrSize == EPointerSize.evPointerSize32 )
                        ptrInSpecifiedSize = (Int32) ( (IntPtr) value ).ToInt32();
                    else
                        ptrInSpecifiedSize = (Int64) ( (IntPtr) value ).ToInt64();
                }

                // Return the pointer's bytes
                return GetValueAsBytesArray( ptrInSpecifiedSize );
            }

            // Call "BitConverter.GetBytes()" through reflection, to convert the object
            // into its specific bytes representation (using the same endianness as the 
            // RAMvader library)
            MethodInfo getBytesMethod = typeof( BitConverter ).GetMethod( "GetBytes", new Type[] { value.GetType() } );
            Object getBytesInvokeResult = getBytesMethod.Invoke( null, new Object[] { value } ); ;
            byte [] resultingBytesArray = (byte[]) getBytesInvokeResult;

            // Correct endianness when necessary (transform OUR endianness into the SPECIFIED endianness).
            RevertArrayOnEndiannessDifference( resultingBytesArray, endianness );
            return resultingBytesArray;
        }


        /** Reverts the given array of bytes, if the specified endianness is different
         * from the endianness used by the process which runs RAMvader.
         * @param bytesArray The array to be set to the target process' endianness.
         * @param endianness The endianness to compare agains the RAMvader process' endianness. */
        public static void RevertArrayOnEndiannessDifference( byte[] bytesArray, EEndianness endianness )
        {
            // Default endianness configuration? No need to to anything.
            if ( endianness == EEndianness.evEndiannessDefault )
                return;

            // Verify if RAMvader's process runs in a different endianness configuration as compared to
            // the specified endianness
            EEndianness ramVaderEndianness = BitConverter.IsLittleEndian ?
                EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

            // If the endianness configuration is different, reverse bytes order
            if ( ramVaderEndianness != endianness )
                Array.Reverse( bytesArray );
        }
        #endregion








        #region PUBLIC METHODS
        /** Constructor. */
        public RAMvaderTarget()
        {
            #if DEBUG
                // Check the sizes of the basic types, in debug mode
                StringBuilder builder = null;
                BasicTypesSizeChecker [] typesToCheck = new BasicTypesSizeChecker[] {
                    new BasicTypesSizeChecker( typeof( Byte ),   sizeof( Byte ),   1 ),
                    new BasicTypesSizeChecker( typeof( Int16 ),  sizeof( Int16 ),  2 ),
                    new BasicTypesSizeChecker( typeof( Int32 ),  sizeof( Int32 ),  4 ),
                    new BasicTypesSizeChecker( typeof( Int64 ),  sizeof( Int64 ),  8 ),
                    new BasicTypesSizeChecker( typeof( UInt16 ), sizeof( UInt16 ), 2 ),
                    new BasicTypesSizeChecker( typeof( UInt32 ), sizeof( UInt32 ), 4 ),
                    new BasicTypesSizeChecker( typeof( UInt64 ), sizeof( UInt64 ), 8 ),
                    new BasicTypesSizeChecker( typeof( Single ), sizeof( Single ), 4 ),
                    new BasicTypesSizeChecker( typeof( Double ), sizeof( Double ), 8 ),
                };

                foreach ( BasicTypesSizeChecker curType in typesToCheck )
                {
                    if ( curType.IsTypeSizeValid() == false )
                    {
                        if ( builder == null )
                            builder = new StringBuilder( string.Format( "[{0}] The following types have reported unexpected sizes:", this.GetType().Name ) );
                    
                        builder.AppendLine();
                        builder.AppendFormat( curType.GetTypeInvalidMessage() );
                    }
                }

                if ( builder != null )
                    throw new RAMvaderException( builder.ToString() );
            #endif
        }


        /** Destructor. */
        ~RAMvaderTarget()
        {
            if ( ProcessHandle != IntPtr.Zero )
                DetachFromProcess();
        }


        /** Makes the #RAMvaderTarget instance assume that the target process is using a specific endianness to store its
         * values. The default endianness assumed by a #RAMvaderTarget instance is the same endianness as the process that is
         * running RAMvader.
         * @param endianness The new endianness to be assumed as the target process' endianness.
         * @see #GetTargetEndianness() */
        public void SetTargetEndianness( EEndianness endianness )
        {
            TargetProcessEndianness = endianness;
        }


        /** Retrieves the endianness that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' endianness.
         * @see #SetTargetEndianness() */
        public EEndianness GetTargetEndianness()
        {
            return TargetProcessEndianness;
        }


        /** Retrieves the actual endianness that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * This method converts the #EEndianness.evEndiannessDefault value into either #EEndianness.evEndiannessBig or #EEndianness.evEndiannessLittle.
         * @return Returns the (assumed) target process' endianness.
         * @see #SetTargetEndianness() */
        public EEndianness GetActualTargetEndianness()
        {
            return ActualTargetProcessEndianness;
        }


        /** Makes the #RAMvaderTarget instance assume that the target process is using a specific pointer size (32 or 64 bits)
         * configuration. The default pointer size assumed by a #RAMvaderTarget instance is the same pointer size as the process
         * that is running RAMvader.
         * @param pointerSize The new pointer size to be assumed for the target process.
         * @see #GetTargetPointerSize() */
        public void SetTargetPointerSize( EPointerSize pointerSize )
        {
            TargetPointerSize = pointerSize;
        }


        /** Retrieves the pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSize() */
        public EPointerSize GetTargetPointerSize()
        {
            return TargetPointerSize;
        }


        /** Retrieves the actual pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * This method converts the #EPointerSize.evPointerSizeDefault value into either #EPointerSize.evPointerSize32 or #EPointerSize.evPointerSize64.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSize() */
        public EPointerSize GetActualTargetPointerSize()
        {
            return ActualTargetPointerSize;
        }


        /** Defines how to handle errors related to different pointer sizes between the target process
         * and the process which runs the RAMvader library.
         * @param pointerSizeErrorHandling How different pointer-size-related errors are to be handled.
         * @see #GetTargetPointerSizeErrorHandling() */
        public void SetTargetPointerSizeErrorHandling( EDifferentPointerSizeError pointerSizeErrorHandling )
        {
            PointerSizeErrorHandling = pointerSizeErrorHandling;
        }


        /** Retrieves the pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSizeErrorHandling() */
        public EDifferentPointerSizeError GetTargetPointerSizeErrorHandling()
        {
            return PointerSizeErrorHandling;
        }


        /** Reverts the given array of bytes, if the target process' endianness is different
         * from the endianness used by the process which runs RAMvader.
         * The target process' endianness can be configured through the #SetTargetEndianness() method.
         * @param bytesArray The array to be set to the target process' endianness. */
        public void RevertArrayOnEndiannessDifference( byte[] bytesArray )
        {
            // Transfer execution to the static version of the method
            RevertArrayOnEndiannessDifference( bytesArray, TargetProcessEndianness );
        }


        /** Sets the target Process to which the instance needs to be attached.
         * @param targetProcess The target process.
         * @throws InstanceAlreadyAttachedException Indicates there is a Process
         *    currently attached to that #RAMvader object. You must detach the
         *    instance from the Process by calling #DetachFromProcess() before
         *    trying to attach to another Process.
         * @return Returns true in case of success, false in case of failure. */
        public bool AttachToProcess( Process targetProcess )
        {
            // Is this instance already attached to a process?
            if ( TargetProcess != null )
                throw new InstanceAlreadyAttachedException( TargetProcess );

            // Certify this process is attachable
            try
            {
                /* This might fail if the Process object has been retrieved via
                 * the Process.GetProcesses() method. This doesn't always mean that
                 * we have no access to the Process state, though... This condition
                 * is treated in the Win32Exception catch block. */
                targetProcess.WaitForExit( 0 );
            }
            catch ( Win32Exception )
            {
                /* Try to retrieve the Process' state again, but this time by
                 * directly opening the Process through its identifier. If this
                 * does not succeed, then our process probably has no access
                 * to the target process. */
                // "Reopen" the process (this is what happens in the
                // low-level WinAPI, when we call the following line of code)
                targetProcess = Process.GetProcessById( targetProcess.Id );
                
                /* Try to check for the process state again, to see if we fail again.
                 * If we fail here, an exception will be thrown back to the caller.
                 * In this case, there's nothing else we can do, so we don't catch
                 * the thrown exception. */
                targetProcess.WaitForExit( 0 );
            }

            // Try to open the target process
            WinAPI.ProcessAccessFlags processOpenFlags =
                WinAPI.ProcessAccessFlags.VMOperation |
                WinAPI.ProcessAccessFlags.QueryInformation |
                WinAPI.ProcessAccessFlags.VMRead |
                WinAPI.ProcessAccessFlags.VMWrite;
            ProcessHandle = WinAPI.OpenProcess( processOpenFlags, false, targetProcess.Id );
            if ( ProcessHandle == IntPtr.Zero )
                return false;

            // If everything went well, update our internal data.
            TargetProcess = targetProcess;

            // Fire "Attached" event
            if ( AttachedEvent != null )
                AttachedEvent( this, EventArgs.Empty );

            // Return true (everything went well)
            return true;
        }


        /** Detaches this instance from its target process.
         * @throws InstanceNotAttachedException Indicates this instance of the
         *    #RAMvader class is currently not attached to any Process.
         * @return Returns true if the instance detached successfully.
         *    Returns false if something went wrong when detaching from the
         *    target process. */
        public bool DetachFromProcess()
        {
            if ( TargetProcess == null )
                throw new InstanceNotAttachedException();

            // Close process' Handle
            bool detachResult = WinAPI.CloseHandle( ProcessHandle );

            // Update internal data
            TargetProcess = null;
            ProcessHandle = IntPtr.Zero;

            // Fire "Detached" event
            if ( detachResult && DetachedEvent != null )
                DetachedEvent( this, EventArgs.Empty );

            // Return the status for the detachment process
            return detachResult;
        }


        /** Retrieves the Process to which this instance is attached.
         * @return Returns a Process object, indicating the process to which this
         *    instance is attached. If the instance is not attached to any process,
         *    this method returns null. */
        public Process GetAttachedProcess()
        {
            return TargetProcess;
        }


        /** Verify if the #RAMvaderTarget is currently attached to any Process.
         * This is just a shorthand method for checking if #GetAttachedProcess() returns
         * a null value.
         * @return Returns a flag indicating if the #RAMvaderTarget instance is currently
         *    attached to any process. */
        public bool IsAttached()
        {
            return ( this.GetAttachedProcess() != null );
        }


        /** Retrives a byte array representing the given numeric value as it would
         * appear into the target process' memory space (considering its endianness).
         * @param objVal An Object representing the value to be converted to its
         *    (endianness correct) bytes array representation. This object should be one
         *    of the basic data types supported by the RAMvader library.
         * @return Returns an array of bytes representing the given value as it would
         *    be stored into the target process' memory, considering the target process'
         *    endianness configurations. */
        public byte [] GetValueAsBytesArrayInTargetProcess( Object objVal )
        {
            return GetValueAsBytesArray( objVal, this.TargetProcessEndianness, this.TargetPointerSize, this.PointerSizeErrorHandling );
        }


        /** Writes a Byte Array into the target process' memory. All other writing methods
         * convert their corresponding input data to a byte sequence and then call this method
         * to execute the actual writing operation.
         * @param address The address on the target process' memory where the data is to be written.
         * @param writeData The data to be written to the target process.
         * @return Returns true in case of success, false in case of failure. */
        public bool WriteToTarget( IntPtr address, byte [] writeData )
        {
            // RAMvader doesn't support a 32-bits host trying to target a 64-bits process.
            if ( TargetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
                throw new UnsupportedPointerSizeException();
            
            // Perform the writing operation, and check its results
            IntPtr totalBytesWritten;

            bool writeResult = WinAPI.WriteProcessMemory( ProcessHandle, address, writeData,
                writeData.Length, out totalBytesWritten );

            return ( writeResult && totalBytesWritten == new IntPtr( writeData.Length ) );
        }


        /** Writes a value into the target process' memory.
         * @param address The address on the target process' memory where the data is to be written.
         * @param writeData The data to be written to the target process. This data must be one of the
         *    basic data types supported by the RAMvader library.
         * @return Returns true in case of success, false in case of failure. */
        public bool WriteToTarget( IntPtr address, Object writeData )
        {
            // Does the RAMvader library support the given data type?
            Type writeDataType = writeData.GetType();
            if ( SUPPORTED_DATA_TYPES_SIZE.ContainsKey( writeDataType ) == false && writeData is IntPtr == false )
                throw new UnsupportedDataTypeException( writeDataType );

            // Perform the writing operation
            byte [] dataBuffer = GetValueAsBytesArrayInTargetProcess( writeData );
            return WriteToTarget( address, dataBuffer );
        }


        /** Reads a sequence of bytes from the target process' memory, filling the given output
         * array with the read bytes. All other reading methods call this method to read the desired
         * data from the target process, and convert the returned bytes into the target data type.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The destiny buffer, where the read data will be copied to. The number of
         *    elements in the passed array determines the number of bytes that will be read from the
         *    target process.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, byte[] outDestiny )
        {
            // RAMvader doesn't support a 32-bits host trying to target a 64-bits process.
            if ( TargetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
                throw new UnsupportedPointerSizeException();

            // Perform the reading operation
            IntPtr totalBytesRead;
            int expectedReadBytes = outDestiny.Length;
            bool readResult = WinAPI.ReadProcessMemory( ProcessHandle, address, outDestiny,
                expectedReadBytes, out totalBytesRead );
            return ( readResult && totalBytesRead == new IntPtr( expectedReadBytes ) );
        }


        /** Reads a value from the target process' memory.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         *    The referenced variable's data must be one of the basic data types supported by the RAMvader library.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Object outDestiny )
        {
            // Determine the size for pointers into the target process
            EPointerSize curProcessPtrSize = RAMvaderTarget.GetRAMvaderPointerSize();
            EPointerSize targetProcessPtrSize = GetActualTargetPointerSize();

            // Does the RAMvader library support the given data type?
            Type readDataType = outDestiny.GetType();
            int destinySizeInBytes;
            if ( SUPPORTED_DATA_TYPES_SIZE.ContainsKey( readDataType ) == false )
            {
                if ( readDataType == typeof( IntPtr ) )
                {
                    switch ( targetProcessPtrSize )
                    {
                        case EPointerSize.evPointerSize32:
                            destinySizeInBytes = 4;
                            break;
                        case EPointerSize.evPointerSize64:
                            destinySizeInBytes = 8;
                            break;
                        default:
                            throw new RAMvaderException( string.Format(
                                "[{0}] The following pointer size (returned by IntPtr.Size) is not supported by RAMvader: {1} bytes.",
                                typeof( RAMvaderTarget ).Name, IntPtr.Size ) );
                    }
                }
                else
                    throw new UnsupportedDataTypeException( readDataType );
            }
            else
                destinySizeInBytes = SUPPORTED_DATA_TYPES_SIZE[readDataType];

            // Try to perform the reading operation
            byte [] byteBuff = new byte[destinySizeInBytes];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );

            // Convert the read bytes to their corresponding format
            if ( outDestiny is Byte )
                outDestiny = byteBuff[0];
            else if ( outDestiny is Int16 )
                outDestiny = BitConverter.ToInt16( byteBuff, 0 );
            else if ( outDestiny is Int32 )
                outDestiny = BitConverter.ToInt32( byteBuff, 0 );
            else if ( outDestiny is Int64 )
                outDestiny = BitConverter.ToInt64( byteBuff, 0 );
            else if ( outDestiny is UInt16 )
                outDestiny = BitConverter.ToUInt16( byteBuff, 0 );
            else if ( outDestiny is UInt32 )
                outDestiny = BitConverter.ToUInt32( byteBuff, 0 );
            else if ( outDestiny is UInt64 )
                outDestiny = BitConverter.ToUInt64( byteBuff, 0 );
            else if ( outDestiny is Single )
                outDestiny = BitConverter.ToSingle( byteBuff, 0 );
            else if ( outDestiny is Double )
                outDestiny = BitConverter.ToDouble( byteBuff, 0 );
            else if ( outDestiny is IntPtr )
            {
                // Handle the cases where the size of pointers in the processes are different
                if ( curProcessPtrSize != targetProcessPtrSize )
                {
                    switch ( PointerSizeErrorHandling )
                    {
                        case EDifferentPointerSizeError.evThrowException:
                            throw new PointerDataLostException( true );
                        case EDifferentPointerSizeError.evSafeTruncation:
                        case EDifferentPointerSizeError.evUnsafeTruncation:
                            {
                                bool bIsSafeTruncation = ( PointerSizeErrorHandling == EDifferentPointerSizeError.evSafeTruncation );

                                // Expand or truncate accordingly, for the write operation
                                bool bComparisonFailed = false;
                                if ( curProcessPtrSize == EPointerSize.evPointerSize32 )
                                {
                                    // Truncate 64-bit pointer to 32-bits
                                    Int64 ptrInTargetProcess = BitConverter.ToInt64( byteBuff, 0 );
                                    Int32 ptrInTargetProcessTruncated = (Int32) ptrInTargetProcess;
                                    bComparisonFailed = ( ptrInTargetProcess != ptrInTargetProcessTruncated );
                                    
                                    outDestiny = new IntPtr( ptrInTargetProcessTruncated );
                                }
                                else
                                {
                                    // Expand 32-bit pointer to 64-bits
                                    UInt32 ptrInTargetProcess = BitConverter.ToUInt32( byteBuff, 0 );
                                    UInt64 ptrInTargetProcessExpanded = (UInt64) ptrInTargetProcess;
                                    bComparisonFailed = ( ptrInTargetProcess != ptrInTargetProcessExpanded );

                                    outDestiny = new IntPtr( ptrInTargetProcess );
                                }

                                // Perform safety check, as needed
                                if ( bIsSafeTruncation && bComparisonFailed )
                                    throw new PointerDataLostException( false );
                                break;
                            }
                    }
                }
                else
                {
                    if ( curProcessPtrSize == EPointerSize.evPointerSize32 )
                        outDestiny = new IntPtr( (Int32) BitConverter.ToInt32( byteBuff, 0 ) );
                    else
                        outDestiny = new IntPtr( (Int64) BitConverter.ToInt64( byteBuff, 0 ) );
                }
            }
            else
            {
                // Code should never really reach this point...
                // If it does, this means an "else if" statement needs to be written to
                // treat the data type given by the "readDataType" variable
                throw new UnsupportedDataTypeException( readDataType );
            }
            return true;
        }
        #endregion








        #if DEBUG
            #region INTERNAL STRUCTURES
            /** Utility structure for storing used in debug mode for verifying if the sizes of the basic
             * types are the same as expected by the library. */
            private struct BasicTypesSizeChecker
            {
                #region PRIVATE PROPERTIES
                /** The Type that is being checked. */
                private Type m_type;
                /** The actual size reported by the compiler for the basic type. */
                private int m_reportedSize;
                /** The expected size for the basic type. */
                private int m_expectedSize;
                #endregion








                #region PUBLIC METHODS
                /** Constructor.
                 * @param type The Type that is being tested.
                 * @param reportedSize The size that has been reported for the given type.
                 * @param expectedSize The expected size for the given type. */
                public BasicTypesSizeChecker( Type type, int reportedSize, int expectedSize )
                {
                    m_type = type;
                    m_reportedSize = reportedSize;
                    m_expectedSize = expectedSize;
                }


                /** Retrieves the name of the type that is being checked.
                 * @return Returns a string containing the name of the checked type. */
                public string GetTypeName()
                {
                    return m_type.Name;
                }


                /** Verifies if the Type size matches the expected size.
                 * @return Returns true case the expected size matches the reported size. */
                public bool IsTypeSizeValid()
                {
                    return ( m_expectedSize == m_reportedSize );
                }


                /** Retrieves the message that indicates that the type size is invalid.
                 * @return Returns a string containing a message that indicates the type size is invalid. */
                public string GetTypeInvalidMessage()
                {
                    return string.Format( "- {0}: expected size {1}, reported size {2}", m_type.Name,
                        m_expectedSize, m_reportedSize );
                }
                #endregion
            }
            #endregion
        #endif
    }
}