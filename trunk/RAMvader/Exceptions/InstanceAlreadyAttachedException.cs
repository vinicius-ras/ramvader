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

using System.Diagnostics;


namespace RAMvader
{
	/// <summary>
	///    Exception thrown when the user tries to attach a <see cref="RAMvaderTarget"/> instance to a process, but the instance
	///    is already attached to another process. Before attaching to a process, the <see cref="RAMvaderTarget"/> instance must
	///    be detached from any other process.
	/// </summary>
	public class InstanceAlreadyAttachedException : RAMvaderException
    {
		/// <summary>Constructor.</summary>
		/// <param name="oldProcess">The process to which the <see cref="RAMvaderTarget"/> instance is currently attached.</param>
		public InstanceAlreadyAttachedException( Process oldProcess )
            : base( string.Format(
                "{0} instance already attached to process with PID {1}.",
                typeof( RAMvaderTarget ).Name,
                oldProcess.Id ) )
        {
        }
    }
}
