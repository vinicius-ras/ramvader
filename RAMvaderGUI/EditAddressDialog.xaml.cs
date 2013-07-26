using System.Windows;


namespace RAMvaderGUI
{
    /// <summary>
    /// Interaction logic for EditAddressDialog.xaml
    /// </summary>
    public partial class EditAddressDialog : Window
    {
        #region PUBLIC METHODS
        /** Constructor. */
        public EditAddressDialog()
        {
            InitializeComponent();
        }
        #endregion








        #region EVENT CALLBACKS
        /** Called when the user clicks the dialog's OK button. */
        private void m_btOk_Click( object sender, RoutedEventArgs e )
        {
            this.DialogResult = true;
            this.Close();
        }
        #endregion
    }
}
