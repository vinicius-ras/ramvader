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

using System.Diagnostics;


namespace RAMvader
{
    /** Exception thrown when the user tries to attach a #RAMvader instance to a
     * process, but the instance is already attached to another process. Before
     * attaching to a process, the #RAMvader instance must be detached from any
     * other process. */
    public class InstanceAlreadyAttachedException : RAMvaderException
    {
        /** Constructor.
         * @param oldProcess The process to which the #RAMvader instance is
         *    currently attached. */
        public InstanceAlreadyAttachedException( Process oldProcess )
            : base( string.Format(
                "{0} instance already attached to process with PID {1}.",
                typeof( RAMvaderTarget ).Name,
                oldProcess.Id ) )
        {
        }
    }
}
