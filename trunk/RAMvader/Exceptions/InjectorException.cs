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

using System;


namespace RAMvader.CodeInjection
{
    /** A generic expection that might be thrown by the #Injector class. */
    public class InjectorException : RAMvaderException
    {
        #region PUBLIC METHODS
        /** Constructor.
         * @param msg The message used to initialize the Exception. */
        public InjectorException( string msg )
            : base( msg )
        {
        }
        #endregion
    }
}
