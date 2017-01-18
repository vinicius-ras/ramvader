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
	///    Specialization of the <see cref="CodeCaveArtifact{TMemoryAlterationSetID, TCodeCave, TVariable}"/> class used to add raw, unsigned bytes
	///    to a code cave.
	/// </summary>
	public class CodeCaveArtifactUnsignedBytesSequence<TMemoryAlterationSetID, TCodeCave, TVariable> : CodeCaveArtifact<TMemoryAlterationSetID, TCodeCave, TVariable>
	{
		#region PRIVATE FIELDS
		/// <summary>The raw bytes to be added through this artifact.</summary>
		private byte [] m_bytes;
		#endregion





		#region PUBLIC METHODS
		/// <summary>Constructor.</summary>
		/// <param name="bytes">The raw bytes to be added through this artifact.</param>
		public CodeCaveArtifactUnsignedBytesSequence( params byte[] bytes )
		{
			m_bytes = bytes;
		}
		#endregion




		#region OVERRIDEN METHODS: CodeCaveArtifact
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
		public override byte[] GenerateArtifactBytes()
		{
			return m_bytes;
		}


		/// <summary>Retrieves the total size of a given artifact, in bytes.</summary>
		/// <param name="target">
		///    The instance of <see cref="RAMvaderTarget"/> that is setup to access the target process' memory space.
		///    This instance is used to know properties of the target process, such as its pointers size.
		/// </param>
		/// <returns>Returns the total size of the artifact, in bytes.</returns>
		public override int GetTotalSize( RAMvaderTarget target )
		{
			return m_bytes.Length;
		}
		#endregion
	}
}
