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
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with RAMvader.  If not, see <http://www.gnu.org/licenses/>.
 */

namespace RAMvader
{
	/// <summary>
	///    An exception which is thrown when trying to perform an I/O operation with pointers between
	///    two processes with different pointer sizes.
	/// </summary>
	public class PointerDataLostException : RAMvaderException
    {
		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="bIsReadOperation">A flag specifying if the exception has been thrown during a read operation (true) or a write operation (false).</param>
		public PointerDataLostException( bool bIsReadOperation )
            : base( string.Format(
                "{0} operation failed: the size of pointers on the target process is different from the size of pointers on the process which runs RAMvader!",
                bIsReadOperation ? "READ" : "WRITE" ) )
        {
        }
        #endregion
    }
}
