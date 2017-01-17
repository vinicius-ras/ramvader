using RAMvader;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RAMvaderGUI
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml.
	/// </summary>
	public partial class MainWindow : Window
	{
		#region PRIVATE FIELDS
		/// <summary>The <see cref="Timer"/> used for refreshing the addresses the user has registered.</summary>
		private Timer m_autoRefreshTimer = null;
		#endregion





		#region PUBLIC PROPERTIES
		/// <summary>Reference to the object used to access and operate on the target process' memory space.</summary>
		public RAMvaderTarget TargetProcessIO { get; set; } = new RAMvaderTarget();
		/// <summary>
		///    The list of addresses that are displayed in the <see cref="DataGrid"/> of the
		///    <see cref="MainWindow"/>.
		/// </summary>
		public ObservableCollection<AddressData> RegisteredAddresses { get; } = new ObservableCollection<AddressData>();
		#endregion





		#region PRIVATE METHODS
		/// <summary>
		///    Makes the program detach from the target process, by calling <see cref="RAMvaderTarget.DetachFromProcess()"/> on
		///    the object <see cref="TargetProcessIO"/>.
		/// </summary>
		private void DetachFromTargetProcess()
		{
			Process targetProcess = TargetProcessIO.TargetProcess;
			if ( TargetProcessIO.IsAttached() && TargetProcessIO.DetachFromProcess() == false )
				MessageBox.Show( this, Properties.Resources.strErrorDuringDetachmentMsg, Properties.Resources.strErrorDuringDetachmentTitle, MessageBoxButton.OK, MessageBoxImage.Error );
			else
			{
				// Output for the user
				this.PrintLineToConsole( Properties.Resources.strConsoleMsgDetachedFromProcess, targetProcess.ProcessName, targetProcess.Id );

				// Automatically refresh the list of running processes
				this.RefreshProcessesList();
			}
		}


		/// <summary>Refreshes the list which displays the processes that are currently running on the user's system.</summary>
		private void RefreshProcessesList()
		{
			// Find all processes and sort
			List<Process> processes = new List<Process>();
			foreach ( Process curProcess in Process.GetProcesses() )
				processes.Add( curProcess );
			processes.Sort( ( p1, p2 ) => string.Compare( p1.ProcessName, p2.ProcessName, true ) );

			// Clear the list and refill it with the current processes
			m_lstProcesses.Items.Clear();
			foreach ( Process curProcess in processes )
				m_lstProcesses.Items.Add( curProcess );

			// Output for the user
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgProcessesListUpdated, processes.Count );
		}


		/// <summary>
		///    Called to read the contents on all registered addresses on the target process' memory space, and update
		///    their values to be displayed to the user in the <see cref="DataGrid"/> containing 
		///    the registered addresses.
		///    Notice that frozen values are NEVER updated, as they must remain unchanged (with the contents typed by
		///    the user).
		/// </summary>
		private void RefreshRegisteredAddresses()
		{
			if ( TargetProcessIO.IsAttached() == false )
				return;

			foreach ( AddressData curData in RegisteredAddresses )
			{
				// Perform read operations
				object newValue = null;
				if ( curData.Freeze == false && curData.IsValueBeingEdited == false && TargetProcessIO.ReadFromTarget( new AbsoluteMemoryAddress( curData.Address ), curData.Type, ref newValue ) )
				{
					// For unfrozen values, we should just read the contents of the address in the target process and update
					// our local process' variables
					curData.Value = newValue;
				}
				else if ( curData.Freeze )
				{
					// For frozen values, we should just overwrite the contents of the address in the target process with the "frozen value"
					TargetProcessIO.WriteToTarget( new AbsoluteMemoryAddress( curData.Address ), curData.Value );
				}
			}
		}


		/// <summary>
		///    Starts the timer which automatically refreshes the registered addresses (in
		///    the <see cref="DataGrid"/> displayed for the user)
		/// </summary>
		private void StartRegisteredAddressesRefreshTimer()
		{
			// Configure the method that will be repeated (only needed once)
			if ( m_autoRefreshTimer == null )
			{
				// Instantiate the timer
				m_autoRefreshTimer = new Timer();

				// Configure the method
				Dispatcher mainWndDispatcher = this.Dispatcher;
				m_autoRefreshTimer.Elapsed += ( sender, evtArgs ) =>
				{
					// The code here is executed in the timer's thread.
					// The code below will be executed in the main thread.
					try
					{
						mainWndDispatcher.Invoke( () =>
						{
							// Refresh registered addresses
							this.RefreshRegisteredAddresses();
						} );
					}
					catch ( TaskCanceledException )
					{
						// Ignored
					}
				};
			}

			// Start the timer!
			m_autoRefreshTimer.Start();
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgAutomaticRefreshmentOn );
		}
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		public MainWindow()
		{
			// Use the "invariant culture" so that Single and Double variables use the "dot" as
			// decimal separator by default.
			CultureInfo defaultCulture = CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentCulture = defaultCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = defaultCulture;

			// Initialize components of the MainWindow
			InitializeComponent();

			// Output to the user
			PrintLineToConsole( Properties.Resources.strConsoleMsgAppStarted );
		}


		/// <summary>Prints a message to the console displayed in the <see cref="MainWindow"/>.</summary>
		/// <param name="format">
		///    The format string to be printed to the console.
		///    This format is the same as passed to the <see cref="string.Format(string, object[])"/> method.
		/// </param>
		/// <param name="args">
		///    The arguments to be used in the console whe formating the message.
		///    These are the same arguments passed to the <see cref="string.Format(string, object[])"/> method.
		/// </param>
		public void PrintToConsole( string format, params object[] args )
		{
			if ( m_txtConsole == null )
				return;

			string strTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
			string strPayload = string.Format( format, args );
			m_txtConsole.AppendText( string.Format( "[{0}] {1}", strTime, strPayload ) );
			m_txtConsole.ScrollToEnd();
		}


		/// <summary>Prints a message with a newline character to the console displayed in the <see cref="MainWindow"/>.</summary>
		/// <param name="format">
		///    The format string to be printed to the console.
		///    This format is the same as passed to the <see cref="string.Format(string, object[])"/> method.
		/// </param>
		/// <param name="args">
		///    The arguments to be used in the console whe formating the message.
		///    These are the same arguments passed to the <see cref="string.Format(string, object[])"/> method.
		/// </param>
		public void PrintLineToConsole( string format, params object[] args )
		{
			this.PrintToConsole( format + "\n", args );
		}
		#endregion





		#region EVENT CALLBACKS
		/// <summary>Called when the "Refresh (processes list)" button gets clicked.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void ButtonRefreshProcessesClick( object sender, RoutedEventArgs e )
		{
			this.RefreshProcessesList();
		}


		/// <summary>Called when the "Attach/Detach" button gets clicked.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void ButtonAttachDetachClick( object sender, RoutedEventArgs e )
		{
			if ( TargetProcessIO.IsAttached() == false )
			{
				Process selectedProcess = (Process) m_lstProcesses.SelectedItem;
				if ( TargetProcessIO.AttachToProcess( selectedProcess ) == false )
				{
					MessageBox.Show( this, Properties.Resources.strErrorDuringAttachmentMsg, Properties.Resources.strErrorDuringAttachmentTitle, MessageBoxButton.OK, MessageBoxImage.Error );
					return;
				}
				else
				{
					// Output for the user
					this.PrintLineToConsole( Properties.Resources.strConsoleMsgAttachedToProcess, selectedProcess.ProcessName, selectedProcess.Id );

					// Enable the program to know when the target process exits
					TargetProcessIO.TargetProcess.EnableRaisingEvents = true;
					TargetProcessIO.TargetProcess.Exited += ( evtSender, evtArgs ) =>
					{
						// Execute this in the Main (GUI) thread
						this.Dispatcher.Invoke( () =>
						{
							this.DetachFromTargetProcess();
						} );
					};
				}
			}
			else
				this.DetachFromTargetProcess();
		}


		/// <summary>Called when the "Clear (console)" button gets clicked.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void ButtonClearConsoleClick( object sender, RoutedEventArgs e )
		{
			m_txtConsole.Clear();
		}


		/// <summary>
		///    Called when the user clicks the "Add New Address" context menu item (from
		///    the <see cref="DataGrid"/> that keeps the registered memory addresses).
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void MenuItemAddAddress( object sender, RoutedEventArgs e )
		{
			// Create a dummy AddressData object and add it to the list
			AddressData newEntry = new AddressData( Properties.Resources.strNewAddressId, IntPtr.Zero,
				(Int32) 0, false );
			RegisteredAddresses.Add( newEntry );

			// Output for the user
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgRegisteredNewEntry );
		}


		/// <summary>
		///    Called when the user clicks the "Delete address(es)" context menu item (from
		///    the <see cref="DataGrid"/> that keeps the registered memory addresses).
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void MenuItemDeleteAddress( object sender, RoutedEventArgs e )
		{
			// Copy the list of selected items to a buffer (as we might not acess the "SelectedItems" and remove elements
			// from it at the same time in a foreach loop)
			List<AddressData> selectedItems = new List<AddressData>( m_dataGridAddresses.SelectedItems.Count );
			foreach ( object curSelectionRaw in m_dataGridAddresses.SelectedItems )
				selectedItems.Add( (AddressData) curSelectionRaw );

			// Remove the elements from the list
			foreach ( AddressData curSelectedObj in selectedItems )
				RegisteredAddresses.Remove( curSelectedObj );

			// Output for the user
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgDeletedEntries );
		}


		/// <summary>
		///    Called when the user clicks the "Add RAMvaderTestTarget addresses" context menu item (from
		///    the <see cref="DataGrid"/> that keeps the registered memory addresses).
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void MenuItemAddRAMvaderTestTargetAddresses( object sender, RoutedEventArgs e )
		{
			AddRAMvaderTestTargetAddressesDialog dialog = new AddRAMvaderTestTargetAddressesDialog();
			if ( dialog.ShowDialog() == true )
			{
				// Calculate the addresses to be added
				AddressData [] addressesToAdd = new AddressData[]
				{
					new AddressData( "Var Byte", dialog.RetrieveTypedAddress( typeof(Byte) ), (Byte) 0, false ),
					new AddressData( "Var Int16", dialog.RetrieveTypedAddress( typeof(Int16) ), (Int16) 0, false ),
					new AddressData( "Var Int32", dialog.RetrieveTypedAddress( typeof(Int32) ), (Int32) 0, false ),
					new AddressData( "Var Int64", dialog.RetrieveTypedAddress( typeof(Int64) ), (Int64) 0, false ),
					new AddressData( "Var UInt16", dialog.RetrieveTypedAddress( typeof(UInt16) ), (UInt16) 0, false ),
					new AddressData( "Var UInt32", dialog.RetrieveTypedAddress( typeof(UInt32) ), (UInt32) 0, false ),
					new AddressData( "Var UInt64", dialog.RetrieveTypedAddress( typeof(UInt64) ), (UInt64) 0, false ),
					new AddressData( "Var Single", dialog.RetrieveTypedAddress( typeof(Single) ), (Single) 0, false ),
					new AddressData( "Var Double", dialog.RetrieveTypedAddress( typeof(Double) ), (Double) 0, false ),
				};

				// Add the addresses to the list
				foreach ( AddressData curAddr in addressesToAdd )
					RegisteredAddresses.Add( curAddr );

				// Output for the user
				this.PrintLineToConsole( Properties.Resources.strConsoleMsgRAMvaderTestTargetAddressesAdded );
			}
		}


		/// <summary>
		///    Called when the user clicks the "Freeze RAMvaderTestTarget addresses" context menu item (from
		///    the <see cref="DataGrid"/> that keeps the registered memory addresses).
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void MenuItemFreezeRAMvaderTestTargetAddresses( object sender, RoutedEventArgs e )
		{
			// The number of entries must match the expected number of entries for this feature to work
			if ( RegisteredAddresses.Count != RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder.Length )
			{
				string errorMsg = string.Format(Properties.Resources.strErrorRAMvaderTestTargetFreezeCountUnmatchedMsg,
					RegisteredAddresses.Count, RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder.Length );
				MessageBox.Show( this, errorMsg, Properties.Resources.strErrorRAMvaderTestTargetFreezeCountUnmatchedTitle,
					MessageBoxButton.OK, MessageBoxImage.Error );
				return;
			}

			// The entries must be added in the correct order for this feature to work
			int totalExpectedEntries = RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder.Length;
			for ( int entryIndex = 0; entryIndex < totalExpectedEntries; entryIndex++ )
			{
				Type expectedEntryType = RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder[entryIndex];
				if ( RegisteredAddresses[entryIndex].Type != expectedEntryType )
				{
					string errorMsg = string.Format( Properties.Resources.strErrorRAMvaderTestTargetFreezeTypesUnmatchedMsg,
						entryIndex,
						RegisteredAddresses[entryIndex].Identifier, RegisteredAddresses[entryIndex].Type.Name,
						expectedEntryType.Name );
					MessageBox.Show( this, errorMsg, Properties.Resources.strErrorRAMvaderTestTargetFreezeTypesUnmatchedTitle, MessageBoxButton.OK, MessageBoxImage.Error );
					return;
				}
			}

			// Set the test values and freeze the entries
			for ( int entryIndex = 0; entryIndex < totalExpectedEntries; entryIndex++ )
			{
				Type curType = RegisteredAddresses[entryIndex].Type;
				object freezeValue = RAMvaderTestTargetData.GetFreezeValue( curType );

				RegisteredAddresses[entryIndex].Value = freezeValue;
				RegisteredAddresses[entryIndex].Freeze = true;
			}

			// Output for the user
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgRAMvaderTestTargetAddressesFrozen );
		}

		/// <summary>Called when the user clicks the "Refresh (registered addresses)" button.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void ButtonRefreshRegisteredAddressesClick( object sender, RoutedEventArgs e )
		{
			this.RefreshRegisteredAddresses();
			this.PrintLineToConsole( Properties.Resources.strConsoleMsgManualRefreshment );
		}


		/// <summary>Called when the user checks or unchecks the "Automatically refresh (registered addresses)" checkbox.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void CheckBoxAutomaticallyRefreshCheckedOrUnchecked( object sender, RoutedEventArgs e )
		{
			if ( m_chkAutomaticRefresh.IsChecked == true )
				this.StartRegisteredAddressesRefreshTimer();
			else
			{
				m_autoRefreshTimer.Stop();
				this.PrintLineToConsole( Properties.Resources.strConsoleMsgAutomaticRefreshmentOff );
			}
		}


		/// <summary>Called when the user adjusts the "Auto refresh time" slider.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void SliderRefreshTimeValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
		{
			Slider slider = (Slider) sender;
			m_autoRefreshTimer.Interval = (int) slider.Value;
		}


		/// <summary>
		///    Called when the user is beginning the edition of a cell in the <see cref="DataGrid"/> of registered
		///    addresses.
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void DataGridCellEditBeginning( object sender, DataGridBeginningEditEventArgs e )
		{
			// Mark the address as "being edited", preventing the "automatic refreshment" routine from
			// overwriting this value while it is being edited.
			AddressData editedData = ((AddressData) e.Row.Item);
			editedData.IsValueBeingEdited = true;
		}


		/// <summary>
		///    Called when the user is finalizing the edition of a cell in the <see cref="DataGrid"/> of registered
		///    addresses.
		/// </summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void DataGridCellEditEnding( object sender, DataGridCellEditEndingEventArgs e )
		{
			// Mark the data as "not being edited" anymore
			AddressData editedData = ((AddressData) e.Row.Item);
			editedData.IsValueBeingEdited = false;

			// When editing the "Value" datagrid column...
			if ( e.Column == m_registeredAddressesDataGridValueColumn && e.EditAction == DataGridEditAction.Commit )
			{
				// Retrieve the new value from the textbox used to update the data grid cell
				TextBox editElement = (TextBox) e.EditingElement;
				object newValueRaw = editElement.Text;   // this will be a string

				// Try to convert the typed value to the type expected for the registered address
				Type expectedDataType = editedData.Type;
				object newValue = null;
				try
				{
					newValue = Convert.ChangeType( newValueRaw, expectedDataType );
				}
				catch ( OverflowException )
				{
					e.Cancel = true;
				}
				catch ( FormatException )
				{
					e.Cancel = true;
				}

				// If an error ocurred because invalid input was entered, the event is cancelled.
				// In that case, we will display an error message to the user.
				// NOTE: when the event is cancelled, the textbox used to edit the value is not hidden and the user can
				// type a new (correct) value on it.
				if ( e.Cancel )
				{
					MessageBox.Show( this,
						Properties.Resources.strErrorInvalidInputValueMsg,
						Properties.Resources.strErrorInvalidInputValueTitle,
						MessageBoxButton.OK, MessageBoxImage.Error );
					return;
				}

				// Write the value to the target process' memory space
				if ( TargetProcessIO.IsAttached() && TargetProcessIO.WriteToTarget( new AbsoluteMemoryAddress( editedData.Address ), newValue ) == false )
					MessageBox.Show( this,
						Properties.Resources.strErrorCannotUpdateValueMsg,
						Properties.Resources.strErrorCannotUpdateValueTitle,
						MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}
		#endregion

		private void ButtonTeste( object sender, RoutedEventArgs e )
		{
			PrintLineToConsole( "{0}, {1}", TargetProcessIO.TargetProcessEndianness, TargetProcessIO.TargetPointerSize );
		}
	}
}
