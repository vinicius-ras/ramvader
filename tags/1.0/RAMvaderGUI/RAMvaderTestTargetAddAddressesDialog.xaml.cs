﻿using RAMvaderGUI.Converters;
using System;
using System.Windows;

namespace RAMvaderGUI
{
    /** Implements the logic behind the dialog used to add all the addresses for
     * the RAMvaderTestTarget program simultaneously. */
    public partial class RAMvaderTestTargetAddAddressesDialog : Window
    {
        #region PRIVATE CONSTANTS
        /** The list of expected types of addresses to be registered for the application.
         * Addresses are expected to appear in the order as they are declared in this list. */
        private readonly Type [] EXPECTED_TYPES_LIST =
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
            typeof( IntPtr ),
        };
        #endregion





        #region PRIVATE FIELDS
        /** Keeps the resulting set of #AddressEntry objects, which are created when the user clicks the dialog's "Ok" button. */
        private AddressEntry [] m_dialogResult;
        #endregion





        #region PUBLIC METHODS
        /** Constructor. */
        public RAMvaderTestTargetAddAddressesDialog()
        {
            InitializeComponent();
        }


        /** Retrieves the list of the addresses typed by the user, in the order they
         * were typed.
         * @return Returns an array of typed addresses, ready to be registered in the
         *    application. In case of any errors (e.g., incorrect/malformed user's input),
         *    returns null. */
        public AddressEntry [] getResults()
        {
            return m_dialogResult;
        }
        #endregion





        #region EVENT CALLBACKS
        /** Called when the user clicks the "OK" button. */
        private void m_btOk_Click( object sender, RoutedEventArgs e )
        {
            // Check typed values: did the user enter the correct number of necessary addresses?
            string [] typedAddresses = m_txtAddresses.Text.Split( new string [] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries );
            if ( typedAddresses.Length != EXPECTED_TYPES_LIST.Length )
            {
                MessageBox.Show( string.Format( "Expected {0} addresses to be types. You have typed {1} address(es)!", EXPECTED_TYPES_LIST.Length, typedAddresses.Length ) );
                return;
            }

            // Create the results
            string strFailedToReadInputError = null;
            m_dialogResult = new AddressEntry[typedAddresses.Length];
            for ( int a = 0; a < typedAddresses.Length; a++ )
            {
                m_dialogResult[a] = new AddressEntry();
                try
                {
                    m_dialogResult[a].Address = IntToHexStringConverter.convertStringToIntPtr( typedAddresses[a] );
                }
                catch ( FormatException )
                {
                    strFailedToReadInputError = string.Format( "Cannot read IntPtr value: \"{0}\"", typedAddresses[a] );
                    break;
                }
                m_dialogResult[a].Description = EXPECTED_TYPES_LIST[a].Name;
                m_dialogResult[a].ValueType = EXPECTED_TYPES_LIST[a];
                m_dialogResult[a].DisplayAsHex = true;
            }

            if ( strFailedToReadInputError != null )
            {
                m_dialogResult = null;
                MessageBox.Show( strFailedToReadInputError );
                return;
            }

            // Close dialog
            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}