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

namespace RAMvader
{
	/// <summary>
	///    Exception thrown when the user code tries to detach a <see cref="RAMvaderTarget"/> instance
	///    from its target process, but the <see cref="RAMvaderTarget"/> instance has not been attached to
	///    any process.
	/// </summary>
	public class InstanceNotAttachedException : RAMvaderException
    {
		/// <summary>Constructor.</summary>
		public InstanceNotAttachedException()
            : base( "This instance is not attached to any process." )
        {
        }
    }
}
