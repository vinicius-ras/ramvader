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

namespace RAMvader.CodeInjection
{
	/// <summary>
	///    This class represents the artifacts that can be added to a code cave.
	///    Artifacts may include: byte sequences, addresses of injected variables, assembly instructions, etc.
	///    Futurely, new kinds of artifacts might be created for making the process of building of code caves easier,
	///    more flexible and more powerful.
	///    During the injection process, the <see cref="CodeInjection.Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
	///    will use the artifacts of each code cave to build the byte codes of each one of the code caves that need to be
	///    injected.
	/// </summary>
	public abstract class CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable>
	{
		#region PRIVATE FIELDS
		/// <summary>This field is set to a reference of the injector</summary>
		private Injector<TMemoryAlterationSetID, TCodeCave, TVariable> m_injector;
		#endregion





		#region PUBLIC METHODS
		/// <summary>
		///    <para>
		///       Sets the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance which is currently using
		///       the <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance.
		///       This method should be called only by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>,
		///       during the injection process.
		///    </para>
		///    <para>ATTENTION: This method is currently NOT thread safe.</para>
		/// </summary>
		/// <param name="injectorRef">A reference to the injector wich will be using this instance.</param>
		/// <exception cref="InvalidOperationException">
		///    Thrown when this instance is already locked by an <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>,
		///    which hasn't called ReleaseFromInjector yet in order to release the instance to be used by
		///    another <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.
		/// </exception>
		public void LockWithInjector( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injectorRef )
		{
			if ( m_injector != null )
				throw new InvalidOperationException( string.Format(
					"The {0} instance is alrady locked by a {1} object!",
					typeof( CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable> ).Name,
					typeof( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> ).Name ) );
			m_injector = injectorRef;
		}


		/// <summary>
		///    Retrieves the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance that is currently
		///    locking this object, during an injection procedure.
		/// </summary>
		/// <returns>
		///    Returns the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> which is trying to inject
		///    the <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance.
		/// </returns>
		public Injector<TMemoryAlterationSetID, TCodeCave, TVariable> GetLockingInjector()
		{
			return m_injector;
		}


		/// <summary>
		///    Releases this instance from the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> that is
		///    currently using it. During the injection process, the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    that needs to use a <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> locks it for its
		///    own use, and after the injection it releases it by calling this method, allowing other <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>s
		///    to lock and use this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		///    Thrown when there are currently no <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> instance
		///    locking the object.
		/// </exception>
		public void ReleaseFromInjector()
		{
			if ( m_injector == null )
				throw new InvalidOperationException( string.Format(
					"There is currently no {0} instance locking this {1} object!",
					typeof(Injector<TMemoryAlterationSetID, TCodeCave, TVariable>).Name,
					typeof(CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable>).Name ) );
			m_injector = null;
		}
		#endregion





		#region ABSTRACT METHODS
		/// <summary>
		///    Generates the bytes which correspond to the artifact instance.
		///    These bytes are the ones to be actually written to the target process' memory space by
		///    the <see cref="CodeInjection.Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///    during the injection procedure.
		/// </summary>
		/// <returns>
		///    Returns an array of bytes corresponding to the artifact when it is injected in the target
		///    process' memory space.
		/// </returns>
		public abstract byte[] GenerateArtifactBytes();
		/// <summary>Retrieves the total size of a given artifact, in bytes.</summary>
		/// <param name="target">
		///    The instance of <see cref="RAMvaderTarget"/> that is setup to access the target process' memory space.
		///    This instance is used to know properties of the target process, such as its pointers size.
		/// </param>
		/// <returns>Returns the total size of the artifact, in bytes.</returns>
		public abstract int GetTotalSize( RAMvaderTarget target );
		#endregion
	}
}
