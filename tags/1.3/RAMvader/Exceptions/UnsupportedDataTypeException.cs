/*
 * Copyright (C) 2014 Vinicius Rogério Araujo Silva
 *
 * This file is part of RAMvader.
 * 
 * RAMvader is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
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

namespace RAMvader
{
	/// <summary>
	///    An exception which is thrown when the user tries to perform a read or write operation using
	///    a data type that is not supported by the library.
	/// </summary>
	public class UnsupportedDataTypeException : RAMvaderException
	{
		/// <summary>Constructor.</summary>
		/// <param name="dataType">The data type for which RAMvader does not offer support to.</param>
		public UnsupportedDataTypeException( Type dataType )
			: base( string.Format(
				"RAMvader library does not support reading/writing operations on the data type \"{0}\"!",
				dataType.Name ) )
		{
		}
	}
}
