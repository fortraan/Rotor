using System;
using System.Collections.Generic;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace Rotor {

	/// <summary>
	/// Game object.
	/// Represents an object managed by the Rotor engine.
	/// </summary>
	public class GameObject {

		/// <summary>
		/// The position of this game object in world space.
		/// </summary>
		/// <value>The position.</value>
		public Vec2 position {
			get {
				if (rigidbody != null) {
					return rigidbody.GetPosition ();
				} else {
					return internalPos;
				}
			}
			set {
				if (rigidbody != null) {
					rigidbody.GetPosition ().Set (value.X, value.Y);
				}
				internalPos.Set (value.X, value.Y);
			}
		}

		internal Vec2 internalPos;

		/// <summary>
		/// The rotation of this game object.
		/// </summary>
		/// <value>The rotation.</value>
		public float rotation {
			get {
				if (rigidbody != null) {
					return rigidbody.GetAngle ();
				} else {
					return internalRot;
				}
			}
			set {
				if (rigidbody != null) {
					// Actually not an error.
					rigidbody.SetAngle (value);
				}
				internalRot = value;
			}
		}

		internal float internalRot;

		/// <summary>
		/// Whether or not the Rotor Engine should render the rigidbody of this game object.
		/// </summary>
		public bool renderRigidbody;

		public Gdk.Pixbuf sprite;

		public List<Controller> controllers;

		public Body rigidbody;

		public delegate void DrawEvent(Gtk.DrawingArea surface);

		public event DrawEvent OnDraw;

		internal GameObject parent;

		public GameObject () {
			renderRigidbody = true;
			internalPos = new Vec2 ();
			parent = null;
		}

		/// <summary>
		/// Sets the parent.
		/// </summary>
		/// <param name="parent">Parent.</param>
		public void SetParent (GameObject parent) {
			this.parent = parent;
		}

		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <returns>The parent.</returns>
		public GameObject GetParent () {
			return parent;
		}

		/// <summary>
		/// Draw the specified surface.
		/// </summary>
		/// <param name="surface">Surface.</param>
		public void Draw (Gtk.DrawingArea surface) {
			if (OnDraw != null) {
				OnDraw.Invoke (surface);
			}
		}

		/// <summary>
		/// Start this instance.
		/// </summary>
		public void Start (RotorEngine engine) {
			foreach (Controller controller in controllers) {
				controller.gameObject = this;
				controller.input = engine.input;
				controller.Start ();
			}
		}

		/// <summary>
		/// Update this instance.
		/// </summary>
		public void Update () {
			foreach (Controller controller in controllers) {
				controller.Update ();
			}
		}
	}
}

