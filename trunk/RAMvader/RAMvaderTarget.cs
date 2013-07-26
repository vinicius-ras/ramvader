using System;
using System.ComponentModel;
using System.Diagnostics;


namespace RAMvader
{
    /** The main working class of the library. Instances of this class are able
     * to "attach" to processes and execute reading and writing operations in their
     * memories. */
    public class RAMvaderTarget
    {
        #region PROPERTIES
        /** The Process to which this instance is currently attached. */
        private Process m_process = null;
        /** The low-level Handle to the target process we are attached to. */
        private IntPtr m_targetProcessHandle = IntPtr.Zero;
        #endregion








        #region PUBLIC METHODS
        /** Constructor. */
        public RAMvaderTarget()
        {
        }


        /** Destructor. */
        ~RAMvaderTarget()
        {
            if ( m_targetProcessHandle != IntPtr.Zero )
                detachFromProcess();
        }


        /** Sets the target Process to which the instance needs to be attached.
         * @param targetProcess The target process.
         * @throws InstanceAlreadyAttachedException Indicates there is a Process
         *    currently attached to that #RAMvader object. You must detach the
         *    instance from the Process by calling #detachFromProcess() before
         *    trying to attach to another Process.
         * @return Returns true in case of success, false in case of failure. */
        public bool attachToProcess( Process targetProcess )
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
            catch ( Win32Exception ex )
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
        public bool detachFromProcess()
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
        public Process getAttachedProcess()
        {
            return m_process;
        }
        #endregion
    }
}
