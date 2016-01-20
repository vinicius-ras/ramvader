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

using RAMvader;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;


namespace RAMvaderGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region CONSTANTS
        /** Defines the maximum number of messages that might exist in the application's console. */
        private const int MAX_CONSOLE_ENTRIES = 100;
        /** The default interval for the application's reading and writing timer. */
        private const int DEFAULT_MEMORY_TIMER_INTERVAL = 500;
        /** Defines the test values which are used for testing writing operations on RAMvaderTestTarget. */
        private readonly Dictionary<Type, Object> WRITE_TEST_VALUES = new Dictionary<Type, object>()
        {
            { typeof( Byte ), (Byte) 100 },
            { typeof( Int16 ), (Int16) 101 },
            { typeof( Int32 ), (Int32) 102 },
            { typeof( Int64 ), (Int64) 103 },
            { typeof( UInt16 ), (UInt16) 104 },
            { typeof( UInt32 ), (UInt32) 105 },
            { typeof( UInt64 ), (UInt64) 106 },
            { typeof( Single ), (Single) 107.107f },
            { typeof( Double ), (Double) 108.108 },
            { typeof( IntPtr ), new IntPtr( (int)0x11223344 ) },
        };
        #endregion








        #region STATIC PROPERTIES
        /** Reference to the singleton instance of MainWindow. */
        private static MainWindow sm_instance;
        #endregion








        #region PROPERTIES
        /** Snapshot containing all the processes that are currently displayed on the list. */
        private Process [] m_processes;
        /** The RAMvader instance, used to gain access to the target process' memory. */
        private RAMvaderTarget m_targetProcess = new RAMvaderTarget();
        /** The timer used to execute the memory-updating functions. */
        private Timer m_memoryTimer = new Timer();
        /** The list containing the memory positions that are being edited/watched by the user. */
        private ObservableCollection<AddressEntry> m_addressEntries = new ObservableCollection<AddressEntry>();
        #endregion








        #region PUBLIC STATIC METHODS
        /** Sends the given message to the application's console.
         * @param logMsg The message to be logged. */
        public static void logToConsole( string logMsg )
        {
            // Verify if the console has reached its maximum messages limit
            ListBox consoleObj = sm_instance.m_lstConsole;
            if ( consoleObj.Items.Count >= MAX_CONSOLE_ENTRIES )
                consoleObj.Items.RemoveAt( 0 );

            // Log the given message
            consoleObj.Items.Add( logMsg );
            consoleObj.ScrollIntoView( consoleObj.Items[consoleObj.Items.Count - 1] );
        }
        #endregion








        #region PRIVATE STATIC METHODS
        /** Called when the timer responsible for reading and writing the target process' memory gets elapsed. */
        private static void onMemoryTimerElapsed( object sender, ElapsedEventArgs args )
        {
            sm_instance.Dispatcher.Invoke( sm_instance.executeMemoryOperations );
        }
        #endregion








        #region PRIVATE METHODS
        /** Detaches our application from the target process. */
        private void detachFromTargetProcess()
        {
            // Detach from target
            if ( m_targetProcess.DetachFromProcess() )
                logToConsole( "Detached from process with sucess." );
            else
                logToConsole( "Detachment has failed! Continuing." );
        }


        /** Should be called after the application attaches to or detaches from a target process to update the GUI. */
        private void onAttachmentStateChanged()
        {
            // Update GUI and start/stop memory timer
            bool isCurrentlyAttached = ( m_targetProcess.GetAttachedProcess() != null );
            m_lstProcesses.IsEnabled = !isCurrentlyAttached;
            m_btRefreshProcesses.IsEnabled = !isCurrentlyAttached;
            m_btAttachToProcess.Content = isCurrentlyAttached ? "Detach" : "Attach";

            m_memoryTimer.Enabled = ( isCurrentlyAttached );
            m_btRefreshMemory.IsEnabled = ( isCurrentlyAttached && m_chkRefreshMemory.IsChecked != true );
        }


        /** Executes the operations related to reading from and writing to the target process' memory. */
        private void executeMemoryOperations()
        {
            if ( m_targetProcess.GetAttachedProcess() == null )
                return;

            // Verify if the process has closed
            if ( m_targetProcess.GetAttachedProcess().HasExited )
            {
                m_lstProcesses.Items.Clear();

                detachFromTargetProcess();
                onAttachmentStateChanged();
                return;
            }

            // Process all target addresses
            int totalEntries = m_addressEntries.Count;
            for ( int entryIndex = 0; entryIndex < totalEntries; entryIndex++ )
            {
                AddressEntry curEntry = m_addressEntries[entryIndex];
                string curEntryName = string.IsNullOrWhiteSpace( curEntry.Description ) ? "<no identifier>" : curEntry.Description;

                // Frozen and non-frozen addresses are treated differently
                if ( curEntry.Freeze )
                {
                    // Ovewrite target process' memory
                    Object entryValue = curEntry.Value;
                    bool bWriteResult = false;
                    
                    try
                    {
                        bWriteResult = m_targetProcess.WriteToTarget( curEntry.Address, entryValue );
                    }
                    catch ( PointerDataLostException ex )
                    {
                        logToConsole( string.Format(
                            "Cannot freeze entry \"{0}\": pointer write operation failed due to a DATA LOST exception! Details follow below.",
                            curEntryName ) );
                        logToConsole( string.Format(
                            "[FAIL DESCRIPTION] {0}",
                            ex.Message ) );
                    }
                    catch ( UnsupportedPointerSizeException )
                    {
                        logToConsole( string.Format(
                            "Cannot freeze entry \"{0}\": RAMvader is running in 32-bits mode and the target process is assumed to be running in 64-bits mode! Pointers can NOT be read/written in this situation.",
                            curEntryName ) );
                        bWriteResult = false;
                    }

                    // Check for errors
                    if ( bWriteResult == false )
                        logToConsole( string.Format(
                            "Cannot write entry \"{0}\" to address 0x{1} of target process!",
                            curEntryName,
                            Converters.IntToHexStringConverter.convertIntPtrToString( curEntry.Address ) ) );
                }
                else
                {
                    // Read the target process' memory
                    Object entryValue, buffer;
                    Type entryType = curEntry.ValueType;
                    bool bReadResult = false;


                    if ( entryType == typeof( Byte ) )
                        buffer = new Byte();
                    else if ( entryType == typeof( Int16 ) )
                        buffer = new Int16();
                    else if ( entryType == typeof( Int32 ) )
                        buffer = new Int32();
                    else if ( entryType == typeof( Int64 ) )
                        buffer = new Int64();
                    else if ( entryType == typeof( UInt16 ) )
                        buffer = new UInt16();
                    else if ( entryType == typeof( UInt32 ) )
                        buffer = new UInt32();
                    else if ( entryType == typeof( UInt64 ) )
                        buffer = new UInt64();
                    else if ( entryType == typeof( Single ) )
                        buffer = new Single();
                    else if ( entryType == typeof( Double ) )
                        buffer = new Double();
                    else if ( entryType == typeof( IntPtr ) )
                        buffer = new IntPtr();
                    else
                        throw new NotSupportedException( string.Format(
                            "Cannot read from target process: data type \"{0}\" (for address entry named \"{1}\") is not supported by the application!",
                            entryType.Name, curEntryName ) );

                    try
                    {
                        bReadResult = m_targetProcess.ReadFromTarget( curEntry.Address, ref buffer );
                    }
                    catch ( UnsupportedPointerSizeException )
                    {
                        logToConsole( string.Format(
                            "Cannot read entry \"{0}\" from target process: RAMvader is running in 32-bits mode and the target process is assumed to be running in 64-bits mode! Pointers can NOT be read/written in this situation.",
                            curEntryName ) );
                        bReadResult = false;
                    }
                    entryValue = buffer;

                    // Check for errors
                    if ( bReadResult == false )
                    {
                        logToConsole( string.Format(
                            "Cannot read entry \"{0}\" to address 0x{1} of target process!",
                            curEntryName, Converters.IntToHexStringConverter.convertIntPtrToString( curEntry.Address ) ) );
                    }

                    // Update value
                    curEntry.Value = entryValue;
                }
            }
        }
        #endregion








        #region PUBLIC METHODS
        /** Constructor. */
        public MainWindow()
        {
            InitializeComponent();

            sm_instance = this;
            m_txtRefreshMemoryTime.Text = DEFAULT_MEMORY_TIMER_INTERVAL.ToString();

            m_memoryTimer.Elapsed += onMemoryTimerElapsed;
            m_memoryTimer.Interval = DEFAULT_MEMORY_TIMER_INTERVAL;

            m_memoryData.ItemsSource = m_addressEntries;

            // Fill the list of endianness values and pointer sizes
            foreach ( EEndianness curEndianness in Enum.GetValues( typeof( EEndianness ) ) )
                m_cmbTargetEndianness.Items.Add( curEndianness );
            m_cmbTargetEndianness.SelectedItem = EEndianness.evEndiannessDefault;

            foreach ( EPointerSize curPtrSize in Enum.GetValues( typeof( EPointerSize ) ) )
                m_cmbTargetPointerSize.Items.Add( curPtrSize );
            m_cmbTargetPointerSize.SelectedItem = EPointerSize.evPointerSizeDefault;

            // This application will always use "safe truncation" error handling for pointers
            m_targetProcess.SetTargetPointerSizeErrorHandling( EDifferentPointerSizeError.evSafeTruncation );

            // Tell the user about the host process' pointer size
            logToConsole( string.Format( "Process running RAMvader is using {0}-bit pointers.", IntPtr.Size * 8 ) );
        }
        #endregion








        #region EVENT CALLBACKS
        /** Called when the user clicks the "Refresh Processes" button. */
        private void m_btRefreshProcesses_Click( object sender, RoutedEventArgs e )
        {
            // Clear the list of processes
            m_lstProcesses.Items.Clear();

            // Refill the list of processes with the currently running processes
            m_processes = Process.GetProcesses();
            Array.Sort( m_processes, delegate( Process p1, Process p2 ) { return p1.ProcessName.CompareTo( p2.ProcessName ); } );
            foreach ( Process curProc in m_processes )
            {
                m_lstProcesses.Items.Add( string.Format( "[{0}] {1}", curProc.Id, curProc.ProcessName ) );
            }
        }


        /** Called when the selection of the list of processes gets changed. */
        private void m_lstProcesses_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
        {
            m_btAttachToProcess.IsEnabled = ( m_lstProcesses.SelectedIndex >= 0 );
        }


        /** Called when the user clicks the "Attach/Detach to/from process" button. */
        private void m_btAttachToProcess_Click( object sender, RoutedEventArgs e )
        {
            if ( m_targetProcess.GetAttachedProcess() == null )
            {
                // Attach to target process
                Process selectedProcess = m_processes[m_lstProcesses.SelectedIndex];
                if ( m_targetProcess.AttachToProcess( selectedProcess ) )
                    logToConsole( "Attached to process with success." );
                else
                    logToConsole( "Failed to attach to target process!" );

            }
            else
                detachFromTargetProcess();

            // Update GUI
            onAttachmentStateChanged();
        }


        /** Called when the user clicks the "Clear console" button. */
        private void m_btClearConsole_Click( object sender, RoutedEventArgs e )
        {
            m_lstConsole.Items.Clear();
        }


        /** Called when the "Refresh Memory" period textbox loses focus. */
        private void m_txtRefreshMemoryTime_LostFocus( object sender, RoutedEventArgs e )
        {
            // Verify if the given value is numeric
            int typedValue;
            try
            {
                typedValue = Int32.Parse( m_txtRefreshMemoryTime.Text );
            }
            catch( Exception ex )
            {
                // Catch format and overflow exceptions
                if ( ex is FormatException || ex is OverflowException )
                {
                    typedValue = 500;
                    m_txtRefreshMemoryTime.Text = typedValue.ToString();
                }
                else
                    throw;
            }

            // Change timer interval
            m_memoryTimer.Interval = typedValue;
        }


        /** Called when the user clicks the "Refresh memory" button. */
        private void m_btRefreshMemory_Click( object sender, RoutedEventArgs e )
        {
            executeMemoryOperations();
        }


        /** Called when the "Automatic refresh" CheckBox is checked or unchecked. */
        private void m_chkRefreshMemory_Checked( object sender, RoutedEventArgs e )
        {
            if ( sm_instance == null )
                return;

            m_btRefreshMemory.IsEnabled = (m_chkRefreshMemory.IsChecked != true);
        }


        /** Called when the user clicks the "Add Address" context menu item (in the memory data grid). */
        private void m_menuAddAddress_Click( object sender, RoutedEventArgs e )
        {
            EditAddressDialog addrDialog = new EditAddressDialog();
            addrDialog.Owner = this;

            if ( addrDialog.ShowDialog() == true )
            {
                AddressEntry newAddress = addrDialog.getResult();
                m_addressEntries.Add( newAddress );
            }
        }


        /** Called when the user clicks the "Delete Selected Addresses" context menu item (in the memory data grid). */
        private void m_menuDeleteSelectedAddresses_Click( object sender, RoutedEventArgs e )
        {
            for ( int a = m_memoryData.SelectedItems.Count - 1; a >= 0; a-- )
                m_addressEntries.Remove( (AddressEntry) m_memoryData.SelectedItems[a] );
        }


        /** Called when the user clicks the "Add RAMvaderTestTarget addresses" context menu item (in the memory data grid). */
        private void m_menuAddTestAddresses_Click( object sender, RoutedEventArgs e )
        {
            RAMvaderTestTargetAddAddressesDialog addrDialog = new RAMvaderTestTargetAddAddressesDialog();
            addrDialog.Owner = this;

            if ( addrDialog.ShowDialog() == true )
            {
                // Add all results
                foreach ( AddressEntry curAddrEntry in addrDialog.getResults() )
                    m_addressEntries.Add( curAddrEntry );
            }
        }


        /** Called when the user clicks the "Write test values" context menu item (in the memory data grid). */
        private void m_menuFreezeOnTestValues_Click( object sender, RoutedEventArgs e )
        {
            // Verify if the set of registered addresses corresponds to the expected set
            if ( m_addressEntries.Count != WRITE_TEST_VALUES.Count )
            {
                MessageBox.Show( string.Format(
                    "Cannot write test values: there are {0} registered addresses, while there should be {1} registered addresses.",
                    m_addressEntries.Count, WRITE_TEST_VALUES.Count ) );
                return;
            }

            Dictionary<Type,Object>.Enumerator writeTestValuesEnumerator = WRITE_TEST_VALUES.GetEnumerator();
            for ( int a = 0; a < m_addressEntries.Count; a++ )
            {
                // NOTE: according to MSDN's documentation, "Initially, the enumerator is
                // positioned before the first element in the collection. At this position,
                // the Current property is undefined. Therefore, you must call the MoveNext
                // method to advance the enumerator to the first element of the collection
                // before reading the value of Current."
                writeTestValuesEnumerator.MoveNext();

                // Check if this object has the right type
                AddressEntry curAddrEntry = m_addressEntries[a];
                Type expectedAddrEntryType = writeTestValuesEnumerator.Current.Key;
                if ( curAddrEntry.ValueType != expectedAddrEntryType )
                {
                    MessageBox.Show( string.Format(
                        "Address identified by \"{0}\", registered at index {1} (zero-based index) of the addresses list is of type {2}, while it was expected to be of type {3}!\n\nNOTICE: the first {1} elements registered on the list have been modified.",
                        m_addressEntries[a].Description, a, curAddrEntry.ValueType.Name,
                        expectedAddrEntryType.Name ) );
                    return;
                }

                // Update write value and enable freezing
                Object curWriteTestValue = writeTestValuesEnumerator.Current.Value;
                m_addressEntries[a].Value = curWriteTestValue;
                m_addressEntries[a].Freeze = true;
            }
        }


        /** Called when the context menu for the "Memory Data Grid" is opening. */
        private void m_memoryData_ContextMenuOpening( object sender, ContextMenuEventArgs e )
        {
            m_menuDeleteSelectedAddresses.IsEnabled = ( m_memoryData.SelectedItems.Count > 0 );
        }


        /** Catches double-clicks on the DataGrid of the #MainWindow. */
        private void m_memoryData_MouseDoubleClick( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            if ( m_memoryData.SelectedItems.Count != 1 )
                return;

            AddressEntry oldAddressEntry = (AddressEntry) m_memoryData.SelectedItem;
            int oldAddressEntryPos = m_addressEntries.IndexOf( oldAddressEntry );

            EditAddressDialog addrDialog = new EditAddressDialog( oldAddressEntry );
            addrDialog.Owner = this;

            if ( addrDialog.ShowDialog() == true )
            {
                AddressEntry newAddress = addrDialog.getResult();
                m_addressEntries.RemoveAt( oldAddressEntryPos );
                m_addressEntries.Insert( oldAddressEntryPos, newAddress );
            }

        }


        private void m_cmbTargetEndianness_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // Update target process' endianness configuration
            EEndianness selectedEndianness = (EEndianness) m_cmbTargetEndianness.SelectedValue;
            m_targetProcess.SetTargetEndianness( selectedEndianness );

            // If there was a previous selection in this combo box (that is, if this
            // method was not called because of the normal initialization of the
            // application's main window), tell the user that the endianness changed.
            if ( e.RemovedItems.Count != 0 )
                logToConsole( string.Format(
                    "Endianness for the target process is now assumed to be: \"{0}\".",
                    selectedEndianness.ToString() ) );
        }


        private void m_cmbTargetPointerSize_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // Update target process' pointer size configuration
            EPointerSize selectedPointerSize = (EPointerSize) m_cmbTargetPointerSize.SelectedValue;
            m_targetProcess.SetTargetPointerSize( selectedPointerSize );


            // If there was a previous selection in this combo box (that is, if this
            // method was not called because of the normal initialization of the
            // application's main window), tell the user that the endianness changed.
            if ( e.RemovedItems.Count != 0 )
                logToConsole( string.Format(
                    "Pointer size for the target process is now assumed to be: \"{0}\".",
                    selectedPointerSize.ToString() ) );
        }
        #endregion
    }
}
