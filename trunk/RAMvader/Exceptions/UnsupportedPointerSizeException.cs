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
    /** An exception which is thrown when the user tries to attach a 32-bits process to a 64-bits target process. */
    public class UnsupportedPointerSizeException : RAMvaderException
    {
        #region PUBLIC METHODS
        /** Constructor. */
        public UnsupportedPointerSizeException()
            : base( string.Format( "RAMvader library currently does not support a 32-bits host process trying to target a 64-bits process." ) )
        {
        }
        #endregion

    }
}
