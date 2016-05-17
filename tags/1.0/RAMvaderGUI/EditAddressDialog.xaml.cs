using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.Reflection;


namespace RAMvaderGUI
{
    /// <summary>
    /// Interaction logic for EditAddressDialog.xaml
    /// </summary>
    public partial class EditAddressDialog : Window
    {
        #region STATIC PROPERTIES
        /** The types of data the application can manipulate. */
        private static Type [] sm_appAllowedDataTypes = new Type[] {
            typeof( Byte ),
            typeof( Int16 ), typeof( Int32 ), typeof( Int64 ),
            typeof( UInt16 ), typeof( UInt32 ), typeof( UInt64 ),
            typeof( Single ), typeof( Double ), typeof( IntPtr )
        };
        /** A string representing a pointer, which points to the position "zero". */
        private static string sm_zeroPointerString = Converters.IntToHexStringConverter.convertIntPtrToString( IntPtr.Zero );
        #endregion








        #region PRIVATE METHODS
        /** Retrieves an Object representing the value typed by the user. The Object has the same Type
         * as selected by the user in the dialog's Type ComboBox.
         * @return Returns the resulting object. 
         * @throws OverflowException When there is an overflow while trying to convert the number to the
         *    type of data the user has selected in the Available Types ComboBox.
         * @throws FormatException When the user input is malformed. */
        private Object getValueObject()
        {
            // Process according to numeric types
            Type userSelectedType = (Type) m_cmbType.SelectedItem;
            if ( userSelectedType == typeof( Single ) || userSelectedType == typeof ( Double ) )
            {
                // Floating point types
                return Convert.ChangeType( m_txtValue.Text, userSelectedType, CultureInfo.InvariantCulture );
            }
            else if ( userSelectedType == typeof( IntPtr ) )
            {
                // Pointers
                return Converters.IntToHexStringConverter.convertStringToIntPtr( m_txtValue.Text );
            }
            else
            {
                // Verify if the user has specified an hex number or not
                string textToParse = m_txtValue.Text.Trim();
                object [] invokeParams = null;
                if ( textToParse.StartsWith( "0x", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    textToParse = textToParse.Substring( 2 );
                    invokeParams = new object[] { textToParse, NumberStyles.HexNumber };
                }
                else
                    invokeParams = new object[] { textToParse };

                // Other numeric types
                return userSelectedType.InvokeMember( "Parse",
                    BindingFlags.InvokeMethod, null, null, invokeParams );
            }
        }


        /** Retrieves a string representing an IntPtr, based on the current architecture (32 or 64 bits).
         * @param pVal The IntPtr value to be transformed into a string.
         * @return Returns a string representing the givben IntPtr. */
        private string getIntPtrAsString( IntPtr pVal )
        {
            int pointerSize = IntPtr.Size;
            switch ( pointerSize )
            {
                case 4:
                case 8:
                    {
                        StringBuilder builder = new StringBuilder( "0x" );
                        string valFormat = string.Format( "X{0}", pointerSize * 2 );
                        builder.Append( pVal.ToString( valFormat ) );
                        return builder.ToString();
                    }
                default:
                    throw new Exception( string.Format(
                        "The size of pointers returned by IntPtr.Size is not supported! Returned size: {0}.",
                        IntPtr.Size ) );
            }
        }
        #endregion







        #region PUBLIC METHODS
        /** Constructor.
         * @param entry The #AddressEntry to be edited, if any. */
        public EditAddressDialog( AddressEntry entry = null )
        {
            InitializeComponent();

            // Fill the data types accepted by the application
            foreach ( Type curType in sm_appAllowedDataTypes )
                m_cmbType.Items.Add( curType );

            // Populate controls
            if ( entry == null )
            {
                m_txtAddress.Text = sm_zeroPointerString;
                m_cmbType.SelectedItem = typeof( Int32 );
            }
            else
            {
                m_txtDescription.Text = entry.Description;
                m_txtAddress.Text = Converters.IntToHexStringConverter.convertIntPtrToString( entry.Address );
                m_cmbType.SelectedItem = entry.ValueType;
                if ( entry.ValueType != typeof( IntPtr ) )
                    m_txtValue.Text = (String) Convert.ChangeType( entry.Value, typeof( String ), CultureInfo.InvariantCulture );
                else
                    m_txtValue.Text = getIntPtrAsString( (IntPtr) entry.Value );
                m_chkFreezeValue.IsChecked = entry.Freeze;
            }
        }


        /** Retrieves the resulting #AddressEntry configured by the user by using the dialog.
         * @return Returns an #AddressEntry object, representing the configuration the user made
         *    in the dialog. */
        public AddressEntry getResult()
        {
            // Get the identifier of the address
            AddressEntry result = new AddressEntry();

            result.Description = m_txtDescription.Text;
            result.ValueType = (Type) m_cmbType.SelectedItem;
            result.Freeze = ( m_chkFreezeValue.IsChecked == true );
            result.Address = Converters.IntToHexStringConverter.convertStringToIntPtr( m_txtAddress.Text );
            result.Value = getValueObject();
            result.DisplayAsHex = m_txtValue.Text.StartsWith( "0x", StringComparison.InvariantCultureIgnoreCase );
            
            return result;
        }
        #endregion








        #region EVENT CALLBACKS
        /** Called when the user clicks the dialog's OK button. */
        private void m_btOk_Click( object sender, RoutedEventArgs e )
        {
            this.DialogResult = true;
            this.Close();
        }


        /** Called when the "Address" TextBox loses focus. */
        private void m_txtAddress_LostFocus( object sender, RoutedEventArgs e )
        {
            // Verify if this is a valid address
            try
            {
                Converters.IntToHexStringConverter.convertStringToIntPtr( m_txtAddress.Text );
            }
            catch ( Exception ex )
            {
                if ( ex is FormatException || ex is OverflowException )
                    m_txtAddress.Text = sm_zeroPointerString;
                else
                    throw;
            }
        }


        /** Called when the "Value" TextBox loses focus. */
        private void m_txtValue_LostFocus( object sender, RoutedEventArgs e )
        {
            try
            {
                getValueObject();
            }
            catch ( Exception ex )
            {
                if ( ex is OverflowException || ex is FormatException )
                    m_txtValue.Text = "0";
                else
                    throw;
            }
        }

        
        /** Called when the user changes the data type selected in the available data types ComboBox. */
        private void m_cmbType_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // Prevents this method from executing during the initialization of the combobox
            if ( e.RemovedItems.Count <= 0 )
                return;

            // Try to cast the currently typed value to the new Data type
            Type oldType = (Type) e.RemovedItems[0];
            Type newType = (Type) m_cmbType.SelectedValue;

            if ( newType == typeof( IntPtr ) )
                m_txtValue.Text = string.Format( "0x{0}", IntPtr.Zero.ToString( string.Format( "X{0}", IntPtr.Size * 2 ) ) );
            else if ( oldType == typeof( IntPtr ) )
                m_txtValue.Text = "0";
            else
            {
                Object newValue = null;
                while ( newValue == null )
                {
                    Object oldValue = Convert.ChangeType( m_txtValue.Text, oldType, CultureInfo.InvariantCulture );
                    try
                    {
                        newValue = Convert.ChangeType( oldValue, newType, CultureInfo.InvariantCulture );
                    }
                    catch ( OverflowException )
                    {
                        m_txtValue.Text = "0";
                    }
                }

                m_txtValue.Text = (String) Convert.ChangeType( newValue, typeof( string ), CultureInfo.InvariantCulture );
            }
        }
        #endregion
    }
}
