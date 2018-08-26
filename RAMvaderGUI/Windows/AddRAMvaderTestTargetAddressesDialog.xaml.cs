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
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Windows;

namespace RAMvaderGUI
{
	/// <summary>
	///    Interaction logic for AddRAMvaderTestTargetAddressesDialog.xaml.
	/// </summary>
	public partial class AddRAMvaderTestTargetAddressesDialog : Window
	{
		#region PRIVATE FIELDS
		/// <summary>
		///    Keeps the addresses of the variables of the RAMvaderTestTarget program, which can be querried by
		///    their respective <see cref="Type"/>.
		/// </summary>
		private Dictionary<Type,IntPtr> m_variableAddresses = new Dictionary<Type, IntPtr>();
		#endregion





		#region PRIVATE METHODS
		/// <summary>Parses the given string for its corresponding hex-value.</summary>
		/// <param name="str">The string to be parsed.</param>
		/// <returns>
		///    In case of success, returns the hex-value corresponding to the given string.
		///    In case of failure, returns <code>null</code>.
		/// </returns>
		private Int32? GetHexValueFromString( string str )
		{
			// Prepare the string for testing
			str = str.Trim().ToLower();
			if ( str.StartsWith( "0x" ) )
				str = str.Substring( 2 );

			if ( string.IsNullOrEmpty( str ) == false )
			{
				try
				{
					Int32 typedValue = Convert.ToInt32( str, 16 );
					return typedValue;
				}
				catch ( ArgumentException ) { }
				catch ( FormatException ) { }
				catch ( OverflowException ) { }
			}

			return null;
		}
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		public AddRAMvaderTestTargetAddressesDialog()
		{
			InitializeComponent();
		}


		/// <summary>Retrieves the address the user typed for a given variable of the RAMvaderTestTarget program.</summary>
		/// <param name="targetType">The type of the variable from RAMvaderTestTarget program whose address is to be retrieved.</param>
		/// <returns>Returns the address of the variable, input by the user.</returns>
		public IntPtr RetrieveTypedAddress( Type targetType )
		{
			IntPtr result;
			if ( m_variableAddresses.TryGetValue( targetType, out result ) == false )
				result = IntPtr.Zero;
			return result;
		}
		#endregion





		#region EVENT CALLBACKS
		/// <summary>Called when the user clicks the "OK" button of the dialog.</summary>
		/// <param name="sender">The object which sent the event.</param>
		/// <param name="e">Holds data about the event.</param>
		private void ButtonOkClick( object sender, RoutedEventArgs e )
		{
			string typedText = m_txtAddresses.Text.Replace( "\r", string.Empty );

			// Verify if there are the exact number of expected lines in the user's input
			string [] linesToRead = typedText.Split( new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries );
			int totalExpectedInputAddresses = RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder.Length;
			if ( linesToRead.Length != totalExpectedInputAddresses )
			{
				string errorMsg = string.Format( Properties.Resources.strErrorRAMvaderTestTargetNotEnoughLinesInInputMsg,
					totalExpectedInputAddresses, linesToRead.Length );
				MessageBox.Show( this, errorMsg, Properties.Resources.strErrorMalformedInput, MessageBoxButton.OK,
					MessageBoxImage.Error );
				return;
			}

			// Obtain the input addresses of the variables
			m_variableAddresses.Clear();
			for ( int lineIndex = 0; lineIndex < linesToRead.Length; lineIndex++ )
			{
				// Try to parse the hex value
				Int32? hexValue = this.GetHexValueFromString( linesToRead[lineIndex] );
				if ( hexValue.HasValue == false )
				{
					string errorMsg = string.Format( Properties.Resources.strErrorRAMvaderTestTargetInvalidLine,
						lineIndex+1, linesToRead[lineIndex] );
					MessageBox.Show( this, errorMsg, Properties.Resources.strErrorMalformedInput, MessageBoxButton.OK,
						MessageBoxImage.Error );
					return;
				}

				// Add to the result
				Type curType = RAMvaderTestTargetData.ExpectedAddressesInputTypeOrder[lineIndex];
				m_variableAddresses.Add( curType, new IntPtr( hexValue.Value ) );
			}

			// Everything ok: close the dialog, returning true
			this.DialogResult = true;
			this.Close();
		}
		#endregion
	}
}
