using System;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;


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
            typeof( Single ), typeof( Double )
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
            Type userSelectedType = (Type) m_cmbType.SelectedItem;
            return Convert.ChangeType( m_txtValue.Text, userSelectedType, CultureInfo.InvariantCulture );
        }
        #endregion







        #region PUBLIC METHODS
        /** Constructor. */
        public EditAddressDialog()
        {
            InitializeComponent();

            // Fill the data types accepted by the application
            foreach ( Type curType in sm_appAllowedDataTypes )
                m_cmbType.Items.Add( curType );

            m_txtAddress.Text = sm_zeroPointerString;
            m_cmbType.SelectedItem = typeof( Int32 );
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
                long.Parse( m_txtAddress.Text, NumberStyles.HexNumber );
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
        #endregion
    }
}
