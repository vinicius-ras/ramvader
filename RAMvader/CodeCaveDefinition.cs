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
using System.Collections.Generic;

namespace RAMvader.CodeInjection
{
	/// <summary>
	///    This class holds the definition of a Code Cave that can be injected into a process' memory space
	///    by using the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>.
	///    A code cave is made up of a list of <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> objects,
	///    which hold all the information used by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> to
	///    generate the bytes of the code cave, which are then injected into the target process' memory space.
	/// </summary>
	public class CodeCaveDefinition<TMemoryAlterationSetID, TCodeCave, TVariable>
	{
		#region PRIVATE FIELDS
		/// <summary>The artifacts that compose the code cave.</summary>
		private CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable> [] m_codeCaveArtifacts;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		public CodeCaveDefinition()
		{
		}


		/// <summary>
		///    Allows you to manually set the <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> objects
		///    to be used to generate the code cave.
		///    Code caves are usually created by using a <see cref="CodeCaveBuilder{TMemoryAlterationSetID, TCodeCave, TVariable}"/>,
		///    which is the recommended way. This method can be used as an alternative to using the builder approach.
		/// </summary>
		/// <param name="artifacts">The array containing the artifacts that will be used to generate the code cave.</param>
		public void SetArtifactsArray( CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable> [] artifacts )
		{
			m_codeCaveArtifacts = artifacts;
		}


		/// <summary>
		///    Retrieves the array of <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> objects used to
		///    generate the code cave.
		/// </summary>
		/// <returns>Returns the array of artifacts used to generate the code cave.</returns>
		public CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable>[] GetArtifactsArray()
		{
			return m_codeCaveArtifacts;
		}


		/// <summary>
		///	   <para>
		///       Processes the list of <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> objects that
		///       define the code cave, and generates the bytes that represent the code cave, as it should be injected in the target process'
		///       memory space.
		///    </para>
		///	   <para>
		///	      This method should be called by the <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/>
		///       during the code injection process only.
		///    </para>
		/// </summary>
		/// <param name="injector">The <see cref="Injector{TMemoryAlterationSetID, TCodeCave, TVariable}"/> object used to retrieve the bytes of the code cave.</param>
		/// <returns>Returns a byte sequence representing the code cave, ready to be injected into the game's memory.</returns>
		public byte[] GenerateCodeCaveBytes( Injector<TMemoryAlterationSetID, TCodeCave, TVariable> injector )
		{
			// If there are no artifacts to process, return an empty result
			if ( m_codeCaveArtifacts == null )
				return new byte[0];

			// Generate the array of bytes representing the code cave
			List<byte> result = new List<byte>();
			foreach ( CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable> curCodeCaveArtifact in m_codeCaveArtifacts )
			{
				curCodeCaveArtifact.LockWithInjector( injector );

				byte [] curArtifactGeneratedBytes = curCodeCaveArtifact.GenerateArtifactBytes();
				result.AddRange( curArtifactGeneratedBytes );

				curCodeCaveArtifact.ReleaseFromInjector();
			}

			return result.ToArray();
		}


		/// <summary>Calculates and retrieves the size of the code cave.</summary>
		/// <param name="target">
		///    The instance of <see cref="RAMvaderTarget"/> that is setup to access the target process' memory space.
		///    This instance is used to know properties of the target process, such as its pointers size.
		/// </param>
		/// <returns>Returns the number of bytes of size for the code cave.</returns>
		public int GetCodeCaveSize( RAMvaderTarget target )
		{
			if ( m_codeCaveArtifacts == null )
				return 0;

			int sizeCount = 0;
			foreach ( CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable> curArtifact in m_codeCaveArtifacts )
				sizeCount += curArtifact.GetTotalSize( target );

			return sizeCount;
		}
		#endregion
	}
}
