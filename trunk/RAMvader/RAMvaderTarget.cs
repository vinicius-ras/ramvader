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
        /** A set containing all basic data types supported by the RAMvader library. */
        private static readonly SortedSet<Type> SUPPORTED_DATA_TYPES = new SortedSet<Type>()
        {
            typeof( Byte ),
            typeof( Int16 ),
            typeof( Int32 ),
            typeof( Int64 ),
            typeof( UInt16 ),
            typeof( UInt32 ),
            typeof( UInt64 ),
            typeof( Single ),
            typeof( Double ),
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
                            builder = new StringBuilder( "The following types have reported unexpected sizes:" );
                    
                        builder.AppendLine();
                        builder.AppendFormat( curType.GetTypeInvalidMessage() );
                    }
                }

                if ( builder != null )
                    throw new PlatformNotSupportedException( builder.ToString() );
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
            uint expectedWrittenBytes = (uint) writeData.Length;
            UIntPtr totalBytesWritten;

            bool writeResult = WinAPI.WriteProcessMemory( m_targetProcessHandle, address, writeData,
                expectedWrittenBytes, out totalBytesWritten );

            return ( writeResult && totalBytesWritten == new UIntPtr( expectedWrittenBytes ) );
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
            if ( SUPPORTED_DATA_TYPES.Contains( writeDataType ) == false )
                throw new UnsupportedDataType( writeDataType );

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
            IntPtr totalBytesRead;
            int expectedReadBytes = outDestiny.Length;
            bool readResult = WinAPI.ReadProcessMemory( m_targetProcessHandle, address, outDestiny,
                expectedReadBytes, out totalBytesRead );
            return ( readResult && totalBytesRead == new IntPtr( expectedReadBytes ) );
        }


        /** Reads a Byte value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Byte outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Byte )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            outDestiny = byteBuff[0];
            return true;
        }


        /** Reads an Int16 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Int16 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Int16 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToInt16( byteBuff, 0 );
            return true;
        }


        /** Reads an Int32 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Int32 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Int32 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToInt32( byteBuff, 0 );
            return true;
        }


        /** Reads an Int64 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Int64 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Int64 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToInt64( byteBuff, 0 );
            return true;
        }


        /** Reads an UInt16 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref UInt16 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( UInt16 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToUInt16( byteBuff, 0 );
            return true;
        }


        /** Reads an UInt32 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref UInt32 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( UInt32 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToUInt32( byteBuff, 0 );
            return true;
        }


        /** Reads an UInt64 value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref UInt64 outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( UInt64 )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToUInt64( byteBuff, 0 );
            return true;
        }


        /** Reads a Single (float) value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Single outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Single )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToSingle( byteBuff, 0 );
            return true;
        }


        /** Reads a Double value from the target process.
         * @param address The address on the target process' memory where the data will be read from.
         * @param outDestiny The result of the reading will be stored in this variable.
         * @return Returns true in case of success, false in case of failure. */
        public bool ReadFromTarget( IntPtr address, ref Double outDestiny )
        {
            byte [] byteBuff = new byte[sizeof( Double )];
            if ( ReadFromTarget( address, byteBuff ) == false )
                return false;

            RevertArrayOnEndiannessDifference( byteBuff );
            outDestiny = BitConverter.ToDouble( byteBuff, 0 );
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
