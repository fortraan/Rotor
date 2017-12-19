using System;
using Rotor;

namespace RotorTests {

	public class InputTestsController : Controller {

		public override void Start () {
			
		}

		public override void Update () {
			if (input.MousePressed ()) {
				Console.WriteLine ("Mouse pressed");
			}
			if (input.GetKeyDown (Gdk.Key.a)) {
				Console.WriteLine ("A down");
			}
			if (input.GetKeyUp (Gdk.Key.a)) {
				Console.WriteLine ("A up");
			}
		}
	}
}

