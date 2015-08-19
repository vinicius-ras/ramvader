using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace RAMvader
{
    /** An adapter class to make the implementation of the INotifyPropertyChanged
     * interface easier for any class willing to provide that implementation. */
    public abstract class NotifyPropertyChangedAdapter : INotifyPropertyChanged
    {
        #region INTERFACE IMPLEMENTATION: INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion





        #region PROTECTED STATIC METHODS
        /** This method should be called inside PROPERTY SETTER METHODS to notify
         * listeners of the "property changed" event that the property has been updated.
         * @param propertyName This parameter is automatically filled with the name of the
         *    updated property by the compiler, as long as it is called with no parameters
         *    inside a property-setter method. */
        protected void SendPropertyChangedNotification( [CallerMemberName] string propertyName = "" )
        {
            if ( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion
    }
}
