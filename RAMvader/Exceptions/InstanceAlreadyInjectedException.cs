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

namespace RAMvader.CodeInjection
{
	/// <summary>
	///    Exception thrown when a method that requires the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance
	///    to be in "NOT injected" state, but this condition is not met.
	///    The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> is put in "injected" state when a call
	///    to <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject()"/>
	///    or <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}.Inject(MemoryAddress)"/> is made.
	/// </summary>
	public class InstanceAlreadyInjectedException : InjectorException
	{
		/// <summary>Constructor.</summary>
		public InstanceAlreadyInjectedException()
			: base( "The operation can only be performed if code injection procedure hasn't already happened." )
		{
		}
	}
}
