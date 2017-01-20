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
	/// <summary>
	///    RAMvader library's core class.
	///    Instances of this class are able to "attach" to processes and execute reading and writing operations in their memory spaces.
	/// </summary>
	public class RAMvaderTarget : NotifyPropertyChangedAdapter
	{
		#region PRIVATE CONSTANTS
		/// <summary>
		///    A dictionary containing both all basic data types supported by the RAMvader library and their respective
		///    sizes. Notice, though, that the IntPtr type IS supported by the library but is not listed in this Dictionary,
		///    because it is treated in a special way by the library, due to its variant-size nature.
		/// </summary>
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
		/// <summary>The Process to which this instance is currently attached.</summary>
		private Process m_process = null;
		/// <summary>The low-level Handle to the target process we are attached to.</summary>
		private IntPtr m_targetProcessHandle = IntPtr.Zero;
		/// <summary>The current endianness that the <see cref="RAMvaderTarget"/> is operating on. The default is
		/// for RAMvader to assume the target process runs in the same endianness as the process that
		/// is running RAMvader.</summary>
		private EEndianness m_targetProcessEndianness = EEndianness.evEndiannessDefault;
		/// <summary>Keeps the pointer size of the target process. Default configuration is to use
		/// the same pointer size of the process which runs RAMvader.</summary>
		private EPointerSize m_targetPointerSize = EPointerSize.evPointerSizeDefault;
		/// <summary>Determines the type of error handling which is used when the target process runs with a
		/// different pointer size configuration, as compared to the process which runs RAMvader.</summary>
		private EDifferentPointerSizeError m_diffPointerSizeError = EDifferentPointerSizeError.evThrowException;
		/// <summary>
		///	   For each of the modules of the <see cref="Process"/> this instance is attached to,
		///	   maps the name of the module to its base address.
		///	   This map might be updated by a call to <see cref="RefreshTargetProcessModulesBaseAddresses"/>
		/// </summary>
		private Dictionary<String,IntPtr> m_modulesBaseAddresses = null;
		#endregion








		#region PUBLIC PROPERTIES
		/// <summary>
		///    The Process object which this RAMvaderTarget instance is attached to.
		///    The <see cref="TargetProcess"/> property can only be changed by calls to <see cref="AttachToProcess(Process)"/>
		///    or <see cref="DetachFromProcess"/>.
		///    Backed by the <see cref="m_process"/> field.
		/// </summary>
		public Process TargetProcess
		{
			get { return m_process; }
			private set
			{
				m_process = value;
				SendPropertyChangedNotification();

				Attached = ( value != null );
			}
		}
		/// <summary>
		///    The handle to the Process object which this RAMvaderTarget instance is attached to.
		///    The <see cref="ProcessHandle"/> property can only be changed by calls to <see cref="AttachToProcess(Process)"/> or <see cref="DetachFromProcess"/>.
		///    Backed by the <see cref="m_targetProcessHandle"/> field.
		/// </summary>
		public IntPtr ProcessHandle
		{
			get { return m_targetProcessHandle; }
			private set { m_targetProcessHandle = value; SendPropertyChangedNotification(); }
		}
		/// <summary>
		///    A flag specifying if this instance is currently attached to a target process.
		///    Returns the same result as the <see cref="IsAttached"/> method.
		/// </summary>
		public bool Attached
		{
			get { return IsAttached(); }
			private set
			{
				// Simulate a property set event
				SendPropertyChangedNotification();
			}
		}
		/// <summary>
		///    The endianness configured for the target process.
		///    This property can also be accessed through the methods <see cref="SetTargetEndianness(EEndianness)"/>
		///    and <see cref="GetTargetEndianness"/>.
		///    Backed by the <see cref="m_targetProcessEndianness"/> field.
		/// </summary>
		public EEndianness TargetProcessEndianness
		{
			get { return m_targetProcessEndianness; }
			set
			{
				m_targetProcessEndianness = value;
				SendPropertyChangedNotification();

				// Simulate a change to the ActualTargetProcessEndianness property, so that
				// it can send its own "property changed" notification
				ActualTargetProcessEndianness = value;
			}
		}
		/// <summary>
		///    The actual endianness that the <see cref="RAMvaderTarget"/> instance is currently assuming that
		///    the target process is using. This is the same value returned by
		///    the <see cref="GetActualTargetEndianness"/> method - see its description for more details.
		/// </summary>
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
		/// <summary>
		///    The pointer size configured for the target process.
		///    This property can also be accessed through the methods <see cref="SetTargetPointerSize(EPointerSize)"/>
		///    and <see cref="GetTargetPointerSize"/>.
		///    Backed by the <see cref="m_targetPointerSize"/> field.
		/// </summary>
		public EPointerSize TargetPointerSize
		{
			get { return m_targetPointerSize; }
			set
			{
				m_targetPointerSize = value;
				SendPropertyChangedNotification();

				// Simulate a change to the ActualTargetPointerSize property, so that
				// it can send its own "property changed" notification
				ActualTargetPointerSize = value;
			}
		}
		/// <summary>
		///    The actual pointer size that the <see cref="RAMvaderTarget"/> instance is currently assuming that
		///    the target process is using. This is the same value returned by
		///    the <see cref="GetActualTargetPointerSize"/> method - see its description for more details.
		/// </summary>
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
		/// <summary>
		///    The type of error handling which is used when the target process runs with a different
		///    pointer size configuration, as compared to the process which runs RAMvader.
		///    This property can also be accessed through the methods <see cref="SetTargetPointerSizeErrorHandling(EDifferentPointerSizeError)"/>
		///    and <see cref="GetTargetPointerSizeErrorHandling"/>.
		///    Backed by the <see cref="m_diffPointerSizeError"/> field.
		/// </summary>
		public EDifferentPointerSizeError PointerSizeErrorHandling
		{
			get { return m_diffPointerSizeError; }
			set { m_diffPointerSizeError = value; SendPropertyChangedNotification(); }
		}
		#endregion








		#region DELEGATES
		/// <summary>Delegate used for handling the event which is fired when the <see cref="RAMvaderTarget"/> object is attached to a process.</summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="args">The arguments (data) of the event.</param>
		public delegate void AttachedEventHandler( object sender, EventArgs args );
		/// <summary>Delegate used for handling the event which is fired when the <see cref="RAMvaderTarget"/> object is detached from a process.</summary>
		/// <param name="sender">The object that sent the event.</param>
		/// <param name="args">The arguments (data) of the event.</param>
		public delegate void DetachedEventHandler( object sender, EventArgs args );
		#endregion








		#region EVENTS
		/// <summary>Handles the event that gets fired when the <see cref="RAMvaderTarget"/> gets attached to a process.</summary>
		public event AttachedEventHandler AttachedEvent;
		/// <summary>Handles the event that gets fired when the <see cref="RAMvaderTarget"/> gets detached from a process.</summary>
		public event DetachedEventHandler DetachedEvent;
		#endregion








		#region PUBLIC STATIC METHODS
		/// <summary>Retrieves the pointer size for the process which runs RAMvader.</summary>
		/// <returns>Returns a <see cref="EPointerSize"/> value, specifying the pointer size of the process.</returns>
		public static EPointerSize GetRAMvaderPointerSize()
		{
			if ( IntPtr.Size == 4 )
				return EPointerSize.evPointerSize32;
			else if ( IntPtr.Size == 8 )
				return EPointerSize.evPointerSize64;
			else
				throw new UnsupportedPointerSizeException( string.Format(
					"[{0}] The following pointer size (returned by IntPtr.Size) is not supported by RAMvader: {1} bytes.",
					typeof( RAMvaderTarget ).Name, IntPtr.Size ) );
		}


		/// <summary>Utility method for retrieving a given value as an array of bytes, respecting the specified endianness.</summary>
		/// <param name="value">The value to be retrieved as a sequence of bytes.</param>
		/// <param name="endianness">The endianness to be used when retrieving the sequence of bytes.</param>
		/// <param name="pointerSize">
		///    The size of pointer to be used when retrieving the sequence of bytes.
		///    That parameter is only used when retrieving the bytes representation of IntPtr values.
		/// </param>
		/// <param name="diffPointerSizeError">
		///    The policy for handling errors regarding different sizes of pointers between RAMvader process'
		///    pointers and the pointers size defined by the "pointerSize" parameter. That parameter is only used when retrieving the
		///    bytes representation of IntPtr values.
		/// </param>
		/// <returns>Returns a sequence of bytes representing the value in the given endianness (and pointer sizes, if applicable).</returns>
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


		/// <summary>Reverts the given array of bytes, if the specified endianness is different from the endianness
		/// used by the process which runs RAMvader.</summary>
		/// <param name="bytesArray">The array to be set to the target process' endianness.</param>
		/// <param name="endianness">The endianness to compare agains the RAMvader process' endianness.</param>
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


		/// <summary>Verifies if a given data type is supported by the RAMvader library.</summary>
		/// <param name="type">The type to be verified.</param>
		/// <returns>Returns a flag indicating if the given type is supported by the RAMvader library.</returns>
		public static bool IsDataTypeSupported( Type type )
		{
			return SUPPORTED_DATA_TYPES_SIZE.ContainsKey( type ) || type == typeof( IntPtr );
		}


		/// <summary>
		///    <para>
		///       Retrieves the size (as considered by the RAMvader library) of a specific data type that is
		///       supported by the library, given in bytes.
		///    </para>
		///    <para>ATTENTION: this method does NOT support the <see cref="IntPtr"/> (see remarks).</para>
		/// </summary>
		/// <param name="type">The type to be verified.</param>
		/// <returns>Retrieves the size of the type, in bytes, as considered by the RAMvader library.</returns>
		/// <exception cref="UnsupportedDataTypeException">
		///    Thrown when the given data type is <see cref="IntPtr"/> or when the given type is not supported by the
		///    library.
		///    To check if a type is supported (except for the <see cref="IntPtr"/> type), please refer
		///    to <see cref="IsDataTypeSupported(Type)"/>.
		/// </exception>
		/// <remarks>
		///    The <see cref="IntPtr"/> type is not supported by this method, as it is a type whose size changes might
		///    change depending on the process being a 32-bit or 64-bit process.
		///    To retrieve the size considered for an <see cref="IntPtr"/>, you must use the
		///    method <see cref="RAMvaderTarget.GetActualTargetPointerSizeInBytes()"/>, which requires an
		///    already-configured <see cref="RAMvaderTarget"/> object.
		/// </remarks>
		/// 
		public static int GetSupportedDataTypeSizeInBytes( Type type )
		{
			int returnedSize;
			if ( SUPPORTED_DATA_TYPES_SIZE.TryGetValue( type, out returnedSize ) == false )
				throw new UnsupportedDataTypeException( type );
			return returnedSize;
		}
		#endregion








		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
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
				throw new UnexpectedDataTypeSizeException( builder.ToString() );
#endif
		}


		/// <summary>Destructor.</summary>
		~RAMvaderTarget()
		{
			if ( ProcessHandle != IntPtr.Zero )
				DetachFromProcess();
		}


		/// <summary>
		///    Makes the <see cref="RAMvaderTarget"/> instance assume that the target process is using a specific endianness to store its
		///    values. The default endianness assumed by a <see cref="RAMvaderTarget"/> instance is the same endianness as the process that is
		///    running RAMvader.
		/// </summary>
		/// <param name="endianness">The new endianness to be assumed as the target process' endianness.</param>
		/// <seealso cref="GetTargetEndianness"/>
		public void SetTargetEndianness( EEndianness endianness )
		{
			TargetProcessEndianness = endianness;
		}


		/// <summary>Retrieves the endianness that the <see cref="RAMvaderTarget"/> instance is currently assuming that the target process is using.</summary>
		/// <returns>Returns the (assumed) target process' endianness.</returns>
		/// <seealso cref="SetTargetEndianness(EEndianness)"/>
		public EEndianness GetTargetEndianness()
		{
			return TargetProcessEndianness;
		}


		/// <summary>
		///    Retrieves the actual endianness that the <see cref="RAMvaderTarget"/> instance is currently assuming that the target process is using.
		///    This method converts the <see cref="EEndianness.evEndiannessDefault"/> value into either <see cref="EEndianness.evEndiannessBig"/> or
		///    <see cref="EEndianness.evEndiannessLittle"/>.
		/// </summary>
		/// <returns>Returns the (assumed) target process' endianness.</returns>
		/// <seealso cref="SetTargetEndianness(EEndianness)"/>
		public EEndianness GetActualTargetEndianness()
		{
			return ActualTargetProcessEndianness;
		}


		/// <summary>
		///    Makes the <see cref="RAMvaderTarget"/> instance assume that the target process is using a specific pointer size (32 or 64 bits)
		///    configuration. The default pointer size assumed by a <see cref="RAMvaderTarget"/> instance is the same pointer size as the process
		///    that is running RAMvader.
		/// </summary>
		/// <param name="pointerSize">The new pointer size to be assumed for the target process.</param>
		/// <seealso cref="GetTargetPointerSize"/>
		public void SetTargetPointerSize( EPointerSize pointerSize )
		{
			TargetPointerSize = pointerSize;
		}


		/// <summary>Retrieves the pointer size that the <see cref="RAMvaderTarget"/> instance is currently assuming
		/// that the target process is using.</summary>
		/// <returns>Returns the (assumed) target process' pointer size.</returns>
		/// <seealso cref="SetTargetPointerSize(EPointerSize)"/>
		public EPointerSize GetTargetPointerSize()
		{
			return TargetPointerSize;
		}


		/// <summary>
		///    Retrieves the actual pointer size that the <see cref="RAMvaderTarget"/> instance is currently assuming that the target process is using.
		///    This method converts the <see cref="EPointerSize.evPointerSizeDefault"/> value into either <see cref="EPointerSize.evPointerSize32"/>
		///    or <see cref="EPointerSize.evPointerSize64"/>.
		/// </summary>
		/// <returns>Returns the (assumed) target process' pointer size.</returns>
		/// <seealso cref="SetTargetPointerSize(EPointerSize)"/>
		public EPointerSize GetActualTargetPointerSize()
		{
			return ActualTargetPointerSize;
		}


		/// <summary>
		///    Calls the <see cref="GetActualTargetPointerSize"/> method and converts its returned value into
		///    its corresponding number of bytes.
		/// </summary>
		/// <returns>Returns the number of bytes that represents the return value of <see cref="GetActualTargetPointerSize"/>.</returns>
		/// <seealso cref="GetActualTargetPointerSize"/>
		public int GetActualTargetPointerSizeInBytes()
		{
			// Try to retrieve the pointer size
			EPointerSize targetProcessPointerSize = this.GetActualTargetPointerSize();
			switch ( targetProcessPointerSize )
			{
				case EPointerSize.evPointerSize32:
					return 4;
				case EPointerSize.evPointerSize64:
					return 8;
			}

			// Pointer type not supported
			throw new UnsupportedPointerSizeException( string.Format(
				"Cannot retrieve the size of a pointer of type \"{0}.{1}.{2}\": support for this type in this method has not been implemented!",
				typeof( RAMvaderTarget ).Name, typeof( EPointerSize ).Name,
				targetProcessPointerSize.ToString() ) );
		}


		/// <summary>Defines how to handle errors related to different pointer sizes between the target process and the process which runs the RAMvader library.</summary>
		/// <param name="pointerSizeErrorHandling">How different pointer-size-related errors are to be handled.</param>
		/// <seealso cref="GetTargetPointerSizeErrorHandling"/>
		public void SetTargetPointerSizeErrorHandling( EDifferentPointerSizeError pointerSizeErrorHandling )
		{
			PointerSizeErrorHandling = pointerSizeErrorHandling;
		}


		/// <summary>Retrieves the pointer size that the <see cref="RAMvaderTarget"/> instance is currently assuming that the target process is using.</summary>
		/// <returns>Returns the (assumed) target process' pointer size.</returns>
		/// <seealso cref="SetTargetPointerSizeErrorHandling(EDifferentPointerSizeError)"/>
		public EDifferentPointerSizeError GetTargetPointerSizeErrorHandling()
		{
			return PointerSizeErrorHandling;
		}


		/// <summary>
		///    Reverts the given array of bytes, if the target process' endianness is different
		///    from the endianness used by the process which runs RAMvader.
		///    The target process' endianness can be configured through the <see cref="SetTargetEndianness(EEndianness)"/> method.
		/// </summary>
		/// <param name="bytesArray">The array to be set to the target process' endianness.</param>
		public void RevertArrayOnEndiannessDifference( byte[] bytesArray )
		{
			// Transfer execution to the static version of the method
			RevertArrayOnEndiannessDifference( bytesArray, TargetProcessEndianness );
		}


		/// <summary>Sets the target Process to which the instance needs to be attached.</summary>
		/// <param name="targetProcess">The target process.</param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		/// <exception cref="InstanceAlreadyAttachedException">
		///    Indicates there is a Process currently attached to that <see cref="RAMvaderTarget"/> object. You must detach the
		///    instance from the Process by calling <see cref="DetachFromProcess"/> before trying to attach to another Process.
		/// </exception>
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


		/// <summary>Detaches this instance from its target process.</summary>
		/// <returns>
		///    Returns true if the instance detached successfully.
		///    Returns false if something went wrong when detaching from the target process.
		/// </returns>
		/// <exception cref="InstanceNotAttachedException">Indicates this instance of the <see cref="RAMvaderTarget"/> class is currently not attached to any Process.</exception>
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


		/// <summary>Retrieves the Process to which this instance is attached.</summary>
		/// <returns>
		///    Returns a Process object, indicating the process to which this instance is attached.
		///    If the instance is not attached to any process, this method returns null.
		/// </returns>
		public Process GetAttachedProcess()
		{
			return TargetProcess;
		}


		/// <summary>
		///    Verify if the <see cref="RAMvaderTarget"/> is currently attached to any Process.
		///    This is just a shorthand method for checking if <see cref="GetAttachedProcess"/> returns a null value.
		/// </summary>
		/// <returns>Returns a flag indicating if the <see cref="RAMvaderTarget"/> instance is currently attached to any process.</returns>
		public bool IsAttached()
		{
			return ( this.GetAttachedProcess() != null );
		}


		/// <summary>
		///    Refreshes the internal data which keeps the base addresses of each module of the <see cref="Process"/> this instance is currently attached to.
		///    If new modules have been loaded by the target process, this method should be called to update the internal data about these new modules, if you need to use that data.
		/// </summary>
		public void RefreshTargetProcessModulesBaseAddresses()
		{
			// This method requires an attachment to the target process
			if ( this.IsAttached() == false )
				throw new InstanceNotAttachedException();

			// Get the list of modules of the process this instance is attached to
			ProcessModuleCollection modules = this.TargetProcess.Modules;

			// Refresh the data about the modules
			if ( m_modulesBaseAddresses == null )
				m_modulesBaseAddresses = new Dictionary<string, IntPtr>( modules.Count );
			else
				m_modulesBaseAddresses.Clear();

			foreach ( ProcessModule curModule in modules )
				m_modulesBaseAddresses.Add( curModule.ModuleName, curModule.BaseAddress );
		}


		/// <summary>Retrieves the base address of the given target process' module (<see cref="ProcessModule"/>).</summary>
		/// <param name="moduleName">The name of the module of the target process whose base address is to be retrieved.</param>
		/// <returns>
		///    In case of success, returns the base address of the process' module whose name has been specified.
		///    Otherwise (e.g., there is no module with the given name in the target process' list of modules, or in case of any error), this method returns <see cref="IntPtr.Zero"/>.
		/// </returns>
		public IntPtr GetTargetProcessModuleBaseAddress( String moduleName )
		{
			// This method requires attachment to a target process
			if ( this.IsAttached() == false )
				throw new InstanceNotAttachedException();

			// Retrieve the requested data
			IntPtr result = IntPtr.Zero;
			if ( m_modulesBaseAddresses == null || m_modulesBaseAddresses.TryGetValue( moduleName, out result ) == false )
			{
				this.RefreshTargetProcessModulesBaseAddresses();
				if ( m_modulesBaseAddresses.TryGetValue( moduleName, out result ) == false )
					return IntPtr.Zero;
			}
			return result;
		}


		/// <summary>
		///    Retrives a byte array representing the given numeric value as it would appear into
		///    the target process' memory space (considering its endianness).
		/// </summary>
		/// <param name="objVal">
		///    An Object representing the value to be converted to its (endianness correct) bytes array representation.
		///    This object should be one of the basic data types supported by the RAMvader library.
		/// </param>
		/// <returns>
		///    Returns an array of bytes representing the given value as it would be stored into the target process' memory,
		///    considering the target process' endianness configurations.
		/// </returns>
		public byte[] GetValueAsBytesArrayInTargetProcess( Object objVal )
		{
			return GetValueAsBytesArray( objVal, this.TargetProcessEndianness, this.TargetPointerSize, this.PointerSizeErrorHandling );
		}


		/// <summary>
		///    Writes a Byte Array into the target process' memory.
		///    All other writing methods convert their corresponding input data to a byte sequence
		///    and then call this method to execute the actual writing operation.
		/// </summary>
		/// <param name="address">The address on the target process' memory where the data is to be written.</param>
		/// <param name="writeData">The data to be written to the target process.</param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		public bool WriteToTarget( MemoryAddress address, byte[] writeData )
		{
			// RAMvader doesn't support a 32-bits host trying to target a 64-bits process.
			if ( TargetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
				throw new UnsupportedPointerSizeException( "RAMvader library currently does not support a 32-bits host process trying to target a 64-bits process." );

			// Nothing needs to be written?
			if ( writeData.Length == 0 )
				return true;

			// Perform the writing operation, and check its results
			IntPtr totalBytesWritten;

			bool writeResult = WinAPI.WriteProcessMemory( ProcessHandle, address.Address, writeData,
				writeData.Length, out totalBytesWritten );

			return ( writeResult && totalBytesWritten == new IntPtr( writeData.Length ) );
		}


		/// <summary>Writes a value into the target process' memory.</summary>
		/// <param name="address">The address on the target process' memory where the data is to be written.</param>
		/// <param name="writeData">
		///    The data to be written to the target process.
		///    This data must be one of the basic data types supported by the RAMvader library.
		/// </param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		public bool WriteToTarget( MemoryAddress address, Object writeData )
		{
			// Does the RAMvader library support the given data type?
			Type writeDataType = writeData.GetType();
			if ( SUPPORTED_DATA_TYPES_SIZE.ContainsKey( writeDataType ) == false && writeData is IntPtr == false )
				throw new UnsupportedDataTypeException( writeDataType );

			// Perform the writing operation
			byte [] dataBuffer = GetValueAsBytesArrayInTargetProcess( writeData );
			return WriteToTarget( address, dataBuffer );
		}


		/// <summary>
		///    Reads a sequence of bytes from the target process' memory, filling the given output
		///    array with the read bytes. All other reading methods call this method to read the desired
		///    data from the target process, and convert the returned bytes into the target data type.
		/// </summary>
		/// <param name="address">The address on the target process' memory where the data will be read from.</param>
		/// <param name="outDestiny">
		///    The destiny buffer, where the read data will be copied to. The number of elements in the passed
		///    array determines the number of bytes that will be read from the target process.
		/// </param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		public bool ReadFromTarget( MemoryAddress address, byte[] outDestiny )
		{
			// RAMvader doesn't support a 32-bits host trying to target a 64-bits process.
			if ( TargetPointerSize == EPointerSize.evPointerSize64 && GetRAMvaderPointerSize() != EPointerSize.evPointerSize64 )
				throw new UnsupportedPointerSizeException( "RAMvader library currently does not support a 32-bits host process trying to target a 64-bits process." );

			// Perform the reading operation
			IntPtr totalBytesRead;
			int expectedReadBytes = outDestiny.Length;
			bool readResult = WinAPI.ReadProcessMemory( ProcessHandle, address.Address, outDestiny,
				expectedReadBytes, out totalBytesRead );
			return ( readResult && totalBytesRead == new IntPtr( expectedReadBytes ) );
		}


		/// <summary>Reads a value from the target process' memory, given the type of the object to be read.</summary>
		/// <param name="address">The address on the target process' memory where the data will be read from.</param>
		/// <param name="typeToRead">The type to be read from the target process' memory space.</param>
		/// <param name="outDestiny">
		///    The result of the reading will be stored in this variable.
		///    The output data will be one of the basic data types supported by the RAMvader library.
		/// </param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		public bool ReadFromTarget(MemoryAddress address, Type typeToRead, ref object outDestiny )
		{
			// Determine the size for pointers into the target process
			EPointerSize curProcessPtrSize = RAMvaderTarget.GetRAMvaderPointerSize();
			EPointerSize targetProcessPtrSize = GetActualTargetPointerSize();

			// Does the RAMvader library support the given data type?
			int destinySizeInBytes;
			if ( SUPPORTED_DATA_TYPES_SIZE.ContainsKey( typeToRead ) == false )
			{
				if ( typeToRead == typeof( IntPtr ) )
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
							throw new UnsupportedPointerSizeException( string.Format(
								"[{0}] The following pointer size (returned by IntPtr.Size) is not supported by RAMvader: {1} bytes.",
								typeof( RAMvaderTarget ).Name, IntPtr.Size ) );
					}
				}
				else
					throw new UnsupportedDataTypeException( typeToRead );
			}
			else
				destinySizeInBytes = SUPPORTED_DATA_TYPES_SIZE[typeToRead];

			// Try to perform the reading operation
			byte [] byteBuff = new byte[destinySizeInBytes];
			if ( ReadFromTarget( address, byteBuff ) == false )
				return false;

			RevertArrayOnEndiannessDifference( byteBuff );

			// Convert the read bytes to their corresponding format
			Object outDestinyRaw = null;
			if ( typeToRead == typeof( Byte ) )
				outDestinyRaw = byteBuff[0];
			else if ( typeToRead == typeof( Int16 ) )
				outDestinyRaw = BitConverter.ToInt16( byteBuff, 0 );
			else if ( typeToRead == typeof( Int32 ) )
				outDestinyRaw = BitConverter.ToInt32( byteBuff, 0 );
			else if ( typeToRead == typeof( Int64 ) )
				outDestinyRaw = BitConverter.ToInt64( byteBuff, 0 );
			else if ( typeToRead == typeof( UInt16 ) )
				outDestinyRaw = BitConverter.ToUInt16( byteBuff, 0 );
			else if ( typeToRead == typeof( UInt32 ) )
				outDestinyRaw = BitConverter.ToUInt32( byteBuff, 0 );
			else if ( typeToRead == typeof( UInt64 ) )
				outDestinyRaw = BitConverter.ToUInt64( byteBuff, 0 );
			else if ( typeToRead == typeof( Single ) )
				outDestinyRaw = BitConverter.ToSingle( byteBuff, 0 );
			else if ( typeToRead == typeof( Double ) )
				outDestinyRaw = BitConverter.ToDouble( byteBuff, 0 );
			else if ( typeToRead == typeof( IntPtr ) )
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

									outDestinyRaw = new IntPtr( ptrInTargetProcessTruncated );
								}
								else
								{
									// Expand 32-bit pointer to 64-bits
									UInt32 ptrInTargetProcess = BitConverter.ToUInt32( byteBuff, 0 );
									UInt64 ptrInTargetProcessExpanded = (UInt64) ptrInTargetProcess;
									bComparisonFailed = ( ptrInTargetProcess != ptrInTargetProcessExpanded );

									outDestinyRaw = new IntPtr( ptrInTargetProcess );
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
						outDestinyRaw = new IntPtr( (Int32) BitConverter.ToInt32( byteBuff, 0 ) );
					else
						outDestinyRaw = new IntPtr( (Int64) BitConverter.ToInt64( byteBuff, 0 ) );
				}
			}
			else
			{
				// Code should never really reach this point...
				// If it does, this means an "else if" statement needs to be written to
				// treat the data type given by the "readDataType" variable
				throw new UnsupportedDataTypeException( typeToRead );
			}

			// Return the result
			outDestiny = outDestinyRaw;
			return true;
		}


		/// <summary>Reads a value from the target process' memory.</summary>
		/// <typeparam name="T">The data type expected to be read from the target process' memory space.</typeparam>
		/// <param name="address">The address on the target process' memory where the data will be read from.</param>
		/// <param name="outDestiny">
		///    The result of the reading will be stored in this variable.
		///    The referenced variable's data must be one of the basic data types supported by the RAMvader library.
		/// </param>
		/// <returns>Returns true in case of success, false in case of failure.</returns>
		public bool ReadFromTarget<T>( MemoryAddress address, ref T outDestiny )
		{
			// Try to perform the read on a temporary object
			object tempBuffer = null;
			bool readResult = ReadFromTarget( address, typeof( T ), ref tempBuffer );

			// Upon read success, cast the read object and use it as the output for outDestiny
			if ( readResult )
				outDestiny = (T) tempBuffer;

			// Return the result
			return readResult;
		}
		#endregion








#if DEBUG
		#region INTERNAL STRUCTURES
		/// <summary>
		///    Utility structure for storing used in debug mode for verifying if the sizes of the basic
		///    types are the same as expected by the library.
		/// </summary>
		private struct BasicTypesSizeChecker
		{
			#region PRIVATE PROPERTIES
			/// <summary>The Type that is being checked.</summary>
			private Type m_type;
			/// <summary>The actual size reported by the compiler for the basic type.</summary>
			private int m_reportedSize;
			/// <summary>The expected size for the basic type.</summary>
			private int m_expectedSize;
			#endregion








			#region PUBLIC METHODS
			/// <summary>Constructor.</summary>
			/// <param name="type">The Type that is being tested.</param>
			/// <param name="reportedSize">The size that has been reported for the given type.</param>
			/// <param name="expectedSize">The expected size for the given type.</param>
			public BasicTypesSizeChecker( Type type, int reportedSize, int expectedSize )
			{
				m_type = type;
				m_reportedSize = reportedSize;
				m_expectedSize = expectedSize;
			}


			/// <summary>Retrieves the name of the type that is being checked.</summary>
			/// <returns>Returns a string containing the name of the checked type.</returns>
			public string GetTypeName()
			{
				return m_type.Name;
			}


			/// <summary>Verifies if the Type size matches the expected size.</summary>
			/// <returns>Returns true case the expected size matches the reported size.</returns>
			public bool IsTypeSizeValid()
			{
				return ( m_expectedSize == m_reportedSize );
			}


			/// <summary>Retrieves the message that indicates that the type size is invalid.</summary>
			/// <returns>Returns a string containing a message that indicates the type size is invalid.</returns>
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
