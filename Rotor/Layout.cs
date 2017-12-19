using System;
using Gtk;

namespace Rotor {

	public class Layout {
		
		public string name { get; protected set; }

		/// <summary>
		/// Construct the specified window.
		/// </summary>
		/// <param name="window">Window.</param>
		public virtual void Construct (ref Window window) {}
	}
}

