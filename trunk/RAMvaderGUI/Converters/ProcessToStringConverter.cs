using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts a <see cref="Process"/> to a string representing it to be displayed in a list of processes.
	/// </summary>
	[ValueConversion( typeof( Process ), typeof( String ) )]
	public class ProcessToStringConverter : IValueConverter
	{
		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Process proc = (Process) value;
			return string.Format( "[{0}] {1}", proc.Id.ToString("D6"), proc.ProcessName );
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
