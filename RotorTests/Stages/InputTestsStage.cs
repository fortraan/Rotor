using System;
using Rotor;
using Gtk;
using Box2DX.Dynamics;
using Box2DX.Common;

namespace RotorTests {

	public class InputTestsLayout : Rotor.Stage {

		public InputTestsLayout () {
			gameObjects = new System.Collections.Generic.List<GameObject> ();
			staticControllers = new System.Collections.Generic.List<Controller> ();
			name = "Input tests";
			aabb = new Box2DX.Collision.AABB ();
			aabb.LowerBound.Set (-100, -100);
			aabb.UpperBound.Set (100, 100);
			gravity = new Vec2 (0, 0);
			allowSleep = true;
		}

		public override void Construct (ref World world) {
			staticControllers.Add (new InputTestsController ());
		}
	}
}

