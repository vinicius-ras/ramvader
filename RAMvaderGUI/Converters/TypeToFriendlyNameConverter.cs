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
using System.Globalization;
using System.Windows.Data;

namespace RAMvaderGUI
{
	/// <summary>
	///    Converts a <see cref="Type"/> object into a friendly name to be read by the user.
	/// </summary>
	[ValueConversion( typeof( Type ), typeof( string ) )]
	public class TypeToFriendlyNameConverter : IValueConverter
	{
		#region PRIVATE STATIC FIELDS
		/// <summary>A dictionary containing all the friendly names for <see cref="Type"/> objects this converter knows.</summary>
		private Dictionary<Type,string> sm_typeNames = new Dictionary<Type, string>()
		{
			{ typeof( Byte ), "Byte" },
			{ typeof( Int16 ), "Int16 (WORD)" },
			{ typeof( Int32 ), "Int32 (DWORD)" },
			{ typeof( Int64 ), "Int64 (QWORD)" },
			{ typeof( UInt16 ), "UInt16 (Unsigned WORD)" },
			{ typeof( UInt32 ), "UInt32 (Unsigned DWORD)" },
			{ typeof( UInt64 ), "UInt64 (Unsigned QWORD)" },
			{ typeof( Single ), "Single (Float)" },
			{ typeof( Double ), "Double" },
			{ typeof( IntPtr ), "IntPtr" },
		};
		#endregion





		#region INTERFACE IMPLEMENTATION: IValueConverter
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			Type typeObj = (Type) value;
			string result;
			if ( sm_typeNames.TryGetValue( typeObj, out result ) == false )
				result = typeObj.FullName;
			return result;
		}


		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			throw new NotImplementedException();
		}
		#endregion
	}
}
