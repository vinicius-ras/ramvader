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

namespace RAMvaderGUI
{
	/// <summary>
	///    Holds data specifically for using RAMvaderGUI to test the RAMvaderTestTarget program.
	/// </summary>
	public class RAMvaderTestTargetData
	{
		#region PRIVATE STATIC FIELDS
		/// <summary>
		///    Determines the freeze value of each variable on the RAMvaderTestTarget program, as well as order
		///    expected for these values to be input.
		/// </summary>
		private static readonly object [] sm_expectedInputAddressesFreezeValue =
		{
			( Byte ) 111,
			( Int16 ) 222,
			( Int32 ) 333,
			( Int64 ) 444,
			( UInt16 ) 555,
			( UInt32 ) 666,
			( UInt64 ) 777,
			( Single ) 888.888f,
			( Double ) 999.999,
			new IntPtr( 0x1A2B3C4D ),
		};
		/// <summary>
		///    Caches the Type of expected input values for the RAMvaderTestTarget program.
		///    This cache is obtained from the <see cref="sm_expectedInputAddressesFreezeValue"/> member.
		/// </summary>
		private static readonly Type [] sm_expectedInputAddressesTypes;
		#endregion





		#region STATIC CONSTRUCTOR
		/// <summary>Class' static constructor.</summary>
		static RAMvaderTestTargetData()
		{
			// Build the array with expected input types
			int totalExpectedTypes = sm_expectedInputAddressesFreezeValue.Length;
			List<Type> expectedTypes = new List<Type>( totalExpectedTypes );
			for ( int t = 0; t < totalExpectedTypes; t++ )
				expectedTypes.Add( sm_expectedInputAddressesFreezeValue[t].GetType() );

			sm_expectedInputAddressesTypes = expectedTypes.ToArray();
		}
		#endregion





		#region PUBLIC STATIC PROPERTIES
		/// <summary>
		///    The order in which the program expects the user to put the addresses of
		///    the RAMvaderTestTarget program.
		/// </summary>
		public static Type[] ExpectedAddressesInputTypeOrder { get { return sm_expectedInputAddressesTypes; } }
		#endregion





		#region PUBLIC METHODS
		/// <summary>Retrieves the value used to freeze the variable of the given type on the RAMvaderTestTarget program.</summary>
		/// <param name="t">The type of variable whose freeze value is to be retrieved.</param>
		/// <returns>The freeze value for the given type variable.</returns>
		public static object GetFreezeValue( Type t )
		{
			int totalTypes = sm_expectedInputAddressesFreezeValue.Length;
			while ( --totalTypes >= 0 )
			{
				if ( sm_expectedInputAddressesFreezeValue[totalTypes].GetType() == t )
					return sm_expectedInputAddressesFreezeValue[totalTypes];
			}
			throw new NotImplementedException( string.Format(
				"Cannot retrieve the freeze value of type \"{0}\" on the RAMvaderTestTarget program: the given type's freeze value has not been implemented!",
				t.Name ) );
		}
		#endregion
	}
}
