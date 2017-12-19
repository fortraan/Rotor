using System;

namespace Rotor.Extensions {

	public static class Vec2Extensions {

		public static Box2DX.Common.Vec2 AngleOffset (this Box2DX.Common.Vec2 vector, float theta) {
			float rotX = (float) ((vector.X * Math.Cos (theta)) - (vector.Y * Math.Sin (theta)));
			float rotY = (float) ((vector.X * Math.Sin (theta)) + (vector.Y * Math.Cos (theta)));
			return new Box2DX.Common.Vec2 (rotX, rotY);
		}

		public static void Offset (this Box2DX.Common.Vec2 vector, float x, float y) {
			vector.Set (vector.X + x, vector.Y + y);
		}
	}
}