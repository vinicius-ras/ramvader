using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;


namespace RAMvader
{
    /** The main working class of the library. Instances of this class are able
     * to "attach" to processes and execute reading and writing operations in their
     * memories. */
    public class RAMvaderTarget
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








        #region PUBLIC ENUMERATIONS
        /** Defines the possible endianness options which RAMvader can operate on. */
        public enum EEndianness
        {
            /** A value indicating that RAMvader should operate in the same endianness as the
             * process that RAMvader is running on. */
            evEndiannessDefault,
            /** A value indicating that RAMvader should operate in Little-Endian byte order. */
            evEndiannessLittle,
            /** A value indicating that RAMvader should operate in Big-Endian byte order. */
            evEndiannessBig,
        }


        /** Defines the supported pointer sizes for the target process. */
        public enum EPointerSize
        {
            /** The default pointer size configuration, where the target process' pointer size
             * is assumed to be the same as the pointer size of the process which runs RAMvader.
             * The pointer size can be retrieved through IntPtr.Size. */
            evPointerSizeDefault,
            /** Explicitly identifies a 32-bit pointer. */
            evPointerSize32,
            /** Explicitly identifies a 64-bit pointer. */
            evPointerSize64,
        }


        /** Defines how errors with different pointer sizes are handled by the library. */
        public enum EDifferentPointerSizeError
        {
            /** Throws an exception if the target process and the process which runs RAMvader have
             * different pointer sizes. This is the default behaviour, for safety reasons. */
            evThrowException,
            /** If the target process and the process which uses RAMvader have different pointer sizes,
             * operations with pointers truncate the pointers to 32-bits when necessary. If any data is
             * lost during the truncation process, a #PointerDataLostException is thrown. */
            evSafeTruncation,
            /** If the target process and the process which uses RAMvader have different pointer sizes,
             * operations with pointers truncate the pointers to 32-bits when necessary. If any data is lost
             * during the truncation process, nothing happens. Thus, this is the less recommended option and
             * should be used with caution. */
            evUnsafeTruncation,
        }
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








        #region PRIVATE STATIC METHODS
        /** Retrieves the pointer size for the process which runs RAMvader.
         * @return Returns a #EPointerSize value, specifying the pointer size of the process. */
        private static EPointerSize GetRAMvaderPointerSize()
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
            if ( m_targetProcessHandle != IntPtr.Zero )
                DetachFromProcess();
        }


        /** Makes the #RAMvaderTarget instance assume that the target process is using a specific endianness to store its
         * values. The default endianness assumed by a #RAMvaderTarget instance is the same endianness as the process that is
         * running RAMvader.
         * @param endianness The new endianness to be assumed as the target process' endianness.
         * @see #GetTargetEndianness() */
        public void SetTargetEndianness( EEndianness endianness )
        {
            m_targetProcessEndianness = endianness;
        }


        /** Retrieves the endianness that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' endianness.
         * @see #SetTargetEndianness() */
        public EEndianness GetTargetEndianness()
        {
            return m_targetProcessEndianness;
        }


        /** Retrieves the actual endianness that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * This method converts the #EEndianness.evEndiannessDefault value into either #EEndianness.evEndiannessBig or #EEndianness.evEndiannessLittle.
         * @return Returns the (assumed) target process' endianness.
         * @see #SetTargetEndianness() */
        public EEndianness GetActualTargetEndianness()
        {
            if ( m_targetProcessEndianness == EEndianness.evEndiannessDefault )
                return BitConverter.IsLittleEndian ? EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;
            return m_targetProcessEndianness;
        }


        /** Makes the #RAMvaderTarget instance assume that the target process is using a specific pointer size (32 or 64 bits)
         * configuration. The default pointer size assumed by a #RAMvaderTarget instance is the same pointer size as the process
         * that is running RAMvader.
         * @param pointerSize The new pointer size to be assumed for the target process.
         * @see #GetTargetPointerSize() */
        public void SetTargetPointerSize( EPointerSize pointerSize )
        {
            m_targetPointerSize = pointerSize;
        }


        /** Retrieves the pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSize() */
        public EPointerSize GetTargetPointerSize()
        {
            return m_targetPointerSize;
        }


        /** Retrieves the actual pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * This method converts the #EPointerSize.evPointerSizeDefault value into either #EPointerSize.evPointerSize32 or #EPointerSize.evPointerSize64.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSize() */
        public EPointerSize GetActualTargetPointerSize()
        {
            if ( m_targetPointerSize == EPointerSize.evPointerSizeDefault )
                return GetRAMvaderPointerSize();
            return m_targetPointerSize;
        }


        /** Defines how to handle errors related to different pointer sizes between the target process
         * and the process which runs the RAMvader library.
         * @param pointerSizeErrorHandling How different pointer-size-related errors are to be handled.
         * @see #GetTargetPointerSizeErrorHandling() */
        public void SetTargetPointerSizeErrorHandling( EDifferentPointerSizeError pointerSizeErrorHandling )
        {
            m_diffPointerSizeError = pointerSizeErrorHandling;
        }


        /** Retrieves the pointer size that the #RAMvaderTarget instance is currently assuming that the target process is using.
         * @return Returns the (assumed) target process' pointer size.
         * @see #SetTargetPointerSizeErrorHandling() */
        public EDifferentPointerSizeError GetTargetPointerSizeErrorHandling()
        {
            return m_diffPointerSizeError;
        }


        /** Reverts the given array of bytes, if the target process' endianness is different
         * from the endianness used by the process which runs RAMvader.
         * The target process' endianness can be configured through the #SetTargetEndianness() method.
         * @param bytesArray The array to be set to the target process' endianness. */
        public void RevertArrayOnEndiannessDifference( byte[] bytesArray )
        {
            // Default endianness configuration? No need to to anything.
            if ( m_targetProcessEndianness == EEndianness.evEndiannessDefault )
                return;

            // Verify if RAMvader's process runs in a different endianness configuration as compared to
            // the target process
            EEndianness ramVaderEndianness = BitConverter.IsLittleEndian ?
                EEndianness.evEndiannessLittle : EEndianness.evEndiannessBig;

            // If both processes run on different endianness configurations, reverse bytes order
            if ( ramVaderEndianness != m_targetProcessEndianness )
                Array.Reverse( bytesArray );
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
            if ( m_process != null )
                throw new InstanceAlreadyAttachedException( m_process );

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
            m_targetProcessHandle = WinAPI.OpenProcess( processOpenFlags, false, targetProcess.Id );
            if ( m_targetProcessHandle == IntPtr.Zero )
                return false;

            // If everything went well, update our internal data.
            m_process = targetProcess;

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
            if ( m_process == null )
                throw new InstanceNotAttachedException();

            // Close process' Handle
            bool detachResult = WinAPI.CloseHandle( m_targetProcessHandle );

            // Update internal data
            m_process = null;
            m_targetProcessHandle = IntPtr.Zero;

            return detachResult;
        }


        /** Retrieves the Process to which this instance is attached.
         * @return Returns a Process object, indicating the process to which this
         *    instance is attached. If the instance is not attached to any process,
         *    this method returns null. */
        public Process GetAttachedProcess()
        {
            return m_process;
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
            // Is the value a single-byte value?
            if ( objVal is Byte )
                return new byte[] { (byte) objVal };

            // If this point is reached, the value is represented by multiple bytes,
            // and we need to consider the target process' endianness to turn it into
            // a correct array of bytes.

            // For pointers, we need to verify how they will be written to the target process.
            // This depends on the configuration for the target process' pointer size
            if ( objVal is IntPtr )
            {
                // Handle the cases where the size of pointers in the processes are different
                EPointerSize curProcessPtrSize = GetRAMvaderPointerSize();
                EPointerSize targetProcessPtrSize = GetActualTargetPointerSize();

                Object ptrInTargetProcess = null;
                if ( curProcessPtrSize != targetProcessPtrSize )
                {
                    switch ( m_diffPointerSizeError )
                    {
                        case EDifferentPointerSizeError.evThrowException:
                            throw new PointerDataLostException( false );
                        case EDifferentPointerSizeError.evSafeTruncation:
                        case EDifferentPointerSizeError.evUnsafeTruncation:
                            {
                                bool bIsSafeTruncation = ( m_diffPointerSizeError == EDifferentPointerSizeError.evSafeTruncation );

                                // Expand or truncate accordingly, for the write operation
                                bool bComparisonFailed = false;
                                if ( curProcessPtrSize == EPointerSize.evPointerSize32 )
                                {
                                    // Expand 32-bit pointer to 64-bits
                                    UInt32 ptrInCurProcess = (UInt32) (IntPtr) objVal;
                                    UInt64 ptrInTargetProcessExpanded = (UInt64) ptrInCurProcess;
                                    ptrInTargetProcess = (UInt64) ptrInCurProcess;

                                    bComparisonFailed = ( ptrInCurProcess != ptrInTargetProcessExpanded );
                                }
                                else
                                {
                                    // Truncate 64-bit pointer to 32-bits
                                    UInt64 ptrInCurProcess = (UInt64) (IntPtr) objVal;
                                    UInt32 ptrInTargetProcessTruncated = (UInt32) ptrInCurProcess;
                                    ptrInTargetProcess = (UInt32) ptrInCurProcess;

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
                        ptrInTargetProcess = (Int32) ( (IntPtr) objVal ).ToInt32();
                    else
                        ptrInTargetProcess = (Int64) ( (IntPtr) objVal ).ToInt64();
                }

                // Return the pointer's bytes
                return GetValueAsBytesArrayInTargetProcess( ptrInTargetProcess );
            }
            
            // Call "BitConverter.GetBytes()" through reflection, to convert the object
            // into its specific bytes representation (using the same endianness as the 
            // RAMvader library)
            MethodInfo getBytesMethod = typeof( BitConverter ).GetMethod( "GetBytes", new Type[] { objVal.GetType() } );
            Object getBytesInvokeResult = getBytesMethod.Invoke( null, new Object[] { objVal } ); ;
            byte [] bytesArrayInTargetProcess = (byte[]) getBytesInvokeResult;

            // Correct endianness when necessary (transform OUR endianness into the TARGET PROCESS' endianness).
            RevertArrayOnEndiannessDifference( bytesArrayInTargetProcess );
            return bytesArrayInTargetProcess;
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
            if ( m_targetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
                throw new UnsupportedPointerSizeException();
            
            // Perform the writing operation, and check its results
            IntPtr totalBytesWritten;

            bool writeResult = WinAPI.WriteProcessMemory( m_targetProcessHandle, address, writeData,
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
            if ( m_targetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
                throw new UnsupportedPointerSizeException();

            // Perform the reading operation
            IntPtr totalBytesRead;
            int expectedReadBytes = outDestiny.Length;
            bool readResult = WinAPI.ReadProcessMemory( m_targetProcessHandle, address, outDestiny,
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
                    switch ( m_diffPointerSizeError )
                    {
                        case EDifferentPointerSizeError.evThrowException:
                            throw new PointerDataLostException( true );
                        case EDifferentPointerSizeError.evSafeTruncation:
                        case EDifferentPointerSizeError.evUnsafeTruncation:
                            {
                                bool bIsSafeTruncation = ( m_diffPointerSizeError == EDifferentPointerSizeError.evSafeTruncation );

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
