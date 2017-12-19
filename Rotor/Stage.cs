using System;
using System.Collections.Generic;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace Rotor {

	public class Stage {
		
		public string name { get; protected set; }
		public AABB aabb { get; protected set; }
		public Vec2 gravity { get; protected set; }
		public bool allowSleep { get; protected set; }
		public List<GameObject> gameObjects { get; protected set; }
		public List<Controller> staticControllers { get; protected set; }

		/// <summary>
		/// Construct the specified world.
		/// </summary>
		/// <param name="world">World.</param>
		public virtual void Construct (ref World world) {}
	}
}

