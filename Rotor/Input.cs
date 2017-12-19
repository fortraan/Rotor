using System;
using System.Collections.Generic;

namespace Rotor {

	public class Input {

		private List<Gdk.Key> lastPressed;
		private List<Gdk.Key> currentState;
		private bool mousePressed;

		[ Obsolete ("Might cause events/delegates to complain; use Input () and manually register OnKeyPressed and OnKeyReleased events instead") ]
		internal Input (Gtk.Window window) {
			lastPressed = new List<Gdk.Key> ();
			currentState = new List<Gdk.Key> ();
			window.KeyPressEvent += OnKeyPressed;
			window.KeyReleaseEvent += OnKeyReleased;
			window.ButtonPressEvent += OnButtonPressed;
			window.ButtonReleaseEvent += OnButtonReleased;
		}

		internal Input () {
			lastPressed = new List<Gdk.Key> ();
			currentState = new List<Gdk.Key> ();
		}

		internal void OnKeyPressed (object o, Gtk.KeyPressEventArgs args) {
			if (!currentState.Contains (args.Event.Key)) {
				currentState.Add (args.Event.Key);
			}
		}

		internal void OnKeyReleased (object o, Gtk.KeyReleaseEventArgs args) {
			if (currentState.Contains (args.Event.Key)) {
				currentState.Remove (args.Event.Key);
			}
		}

		internal void OnButtonPressed (object o, Gtk.ButtonPressEventArgs args) {
			RotorEngine.EngineLog ("Button pressed: {0}", args.Event.Button);
			mousePressed = true;
		}

		internal void OnButtonReleased (object o, Gtk.ButtonReleaseEventArgs args) {
			mousePressed = false;
		}

		internal void Actualize () {
			lastPressed.Clear ();
			lastPressed.AddRange (currentState);
		}

		/// <summary>
		/// Checks if a given key was pressed down this frame.
		/// </summary>
		/// <returns><c>true</c> if given key was pressed down this frame, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool GetKeyDown (Gdk.Key key) {
			return currentState.Contains (key) && !lastPressed.Contains (key);
		}

		/// <summary>
		/// Checks if a given key is curently held down.
		/// </summary>
		/// <returns><c>true</c>, if key was gotten, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool GetKey (Gdk.Key key) {
			return currentState.Contains (key);
		}

		/// <summary>
		/// Checks if a given key was released this frame.
		/// </summary>
		/// <returns><c>true</c> if given key was pressed down this frame, <c>false</c> otherwise.</returns>
		/// <param name="key">Key.</param>
		public bool GetKeyUp (Gdk.Key key) {
			return !currentState.Contains (key) && lastPressed.Contains (key);
		}

		public bool MousePressed () {
			return mousePressed;
		}
	}
}

