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
	///    Exception thrown when the RAMvader library fails to find a module in the
	///    target process' modules list.
	/// </summary>
	public class ModuleNotFoundException : RAMvaderException
	{
		/// <summary>Constructor.</summary>
		/// <param name="moduleName">The name of the module which has not been found.</param>
		public ModuleNotFoundException( string moduleName )
			: base( string.Format( "Cannot find a module named \"{0}\" in the target process!", moduleName ) )
		{
		}
	}
}
