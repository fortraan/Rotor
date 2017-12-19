using System;
using Rotor;

namespace RotorTests {

	public class MainClass {
		public static void Main (string[] args) {
			RotorEngine engine = new RotorEngine ();
			engine.windowHeight = 200;
			engine.windowWidth = 200;
			engine.LoadStage (typeof(InputTestsLayout));
			engine.Start ();
		}
	}
}
