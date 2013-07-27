using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Collections.ObjectModel;
using RAMvader;

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








        #region PRIVATE METHODS
        int contae = 0;
        /** Called to refresh the table which displays information about the memory of the target process. */
        private void refreshMemoryTable()
        {
            logToConsole( string.Format( "Update memory {0}", ( ++contae ) ) );
            if ( m_targetProcess.getAttachedProcess() == null )
                return;
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
            if ( m_targetProcess.getAttachedProcess() == null )
            {
                // Attach to target process
                Process selectedProcess = m_processes[m_lstProcesses.SelectedIndex];
                if ( m_targetProcess.attachToProcess( selectedProcess ) )
                    logToConsole( "Attached to process with success." );
                else
                    logToConsole( "Failed to attach to target process!" );
                
            }
            else
            {
                // Detach from target
                if ( m_targetProcess.detachFromProcess() )
                    logToConsole( "Detached from process with sucess." );
                else
                    logToConsole( "Detachment has failed! Continuing." );
            }

            // Update GUI and start/stop memory timer
            bool isCurrentlyAttached = (m_targetProcess.getAttachedProcess() != null);
            m_lstProcesses.IsEnabled = !isCurrentlyAttached;
            m_btRefreshProcesses.IsEnabled = !isCurrentlyAttached;
            m_btAttachToProcess.Content = isCurrentlyAttached ? "Detach" : "Attach";

            m_memoryTimer.Enabled = ( isCurrentlyAttached );
            m_btRefreshMemory.IsEnabled = ( isCurrentlyAttached && m_chkRefreshMemory.IsChecked != true );
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
            refreshMemoryTable();
        }


        /** Called when the timer responsible for reading and writing the target process' memory gets elapsed. */
        private static void onMemoryTimerElapsed( object sender, ElapsedEventArgs args )
        {
            sm_instance.Dispatcher.Invoke( sm_instance.refreshMemoryTable );
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
        #endregion
    }
}
