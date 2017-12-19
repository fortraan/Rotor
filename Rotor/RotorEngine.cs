using System;
using System.Collections.Generic;
using Gtk;
using Box2DX.Common;
using Box2DX.Collision;
using Box2DX.Dynamics;
using Cairo;
using System.Threading.Tasks;

namespace Rotor {

	public class RotorEngine {

		internal Window window;

		private DrawingArea surface;

		private OperationMode opMode;

		internal Input input;

		private World world;
		private List<GameObject> gameObjects;
		private List<Controller> staticControllers;

		public uint targetFramerate;

		public string windowTitle;
		public int windowWidth = 640;
		public int windowHeight = 420;

		internal int cameraX;
		internal int cameraY;

		private object updateLock;

		public LogLevel logLevel;

		public bool exitRequested;

		private static string[] logTags = new string[] {
			"V",
			"I",
			"W",
			"E"
		};

		public enum LogLevel {
			Verbose,
			Info,
			Warning,
			Error,
			Silence
		}

		//string anotherFalsePositive = "Starting static";

		public enum OperationMode {
			GTKGUI,
			DIMEN2,
			DIMEN3
		}

		public RotorEngine () {
			logLevel = LogLevel.Warning;
			updateLock = new object ();
			opMode = OperationMode.DIMEN2;
			targetFramerate = 120;
			windowTitle = "[ //// Rotor Engine 0 //// ]";
			staticControllers = new List<Controller> ();
			gameObjects = new List<GameObject> ();
			input = new Input ();
			window = new Window (windowTitle);
			Console.WriteLine (window.Screen == null);
			window.DeleteEvent += delegate {
				Stop ();
			};

			/*AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e) {
				DisplayErrorMessage ((Exception) e.ExceptionObject);
			};*/

			cameraX = 0;
			cameraY = 0;

			window.KeyPressEvent += input.OnKeyPressed;
			window.KeyReleaseEvent += input.OnKeyReleased;
			window.ButtonPressEvent += input.OnButtonPressed;
			window.ButtonReleaseEvent += input.OnButtonReleased;

			window.Events = window.Events |
			Gdk.EventMask.ButtonPressMask |
			Gdk.EventMask.ButtonReleaseMask;
		}

		#region Core engine internals

		private void Init2D () {
			if (surface != null) {
				surface.Destroy ();
			}
			surface = new DrawingArea ();
			surface.ExposeEvent += OnSurfaceExpose;
			window.Add (surface);
			surface.Show ();
		}

		private void InitControllers () {
			Log (LogLevel.Info, "Starting controllers");
			foreach (Controller staticController in staticControllers) {
				//string avastFalsePositive = "Starting static";

				Log ("Starting static controllers");
				staticController.input = input;
				staticController.Start ();
			}
			foreach (GameObject obj in gameObjects) {
				obj.Start (this);
			}
		}

		private void Update () {
			lock (updateLock) {
				try {
					Log (LogLevel.Info, "Locking for update");
					if (opMode == OperationMode.DIMEN2) {
						world.Step (1f / targetFramerate, 8, 1);
						foreach (Controller staticController in staticControllers) {
							staticController.Update ();
						}
						foreach (GameObject gameObject in gameObjects) {
							gameObject.Update ();
						}
						input.Actualize ();
						surface.QueueDraw ();
					}
					Log (LogLevel.Info, "Update complete; unlocking");
				} catch (Exception e) {
					DisplayErrorMessage (e);
				}
			}
		}

		private void OnSurfaceExpose (object o, ExposeEventArgs args) {
			lock (updateLock) {
				Log (LogLevel.Info, "Locking for draw");
				if (opMode == OperationMode.DIMEN2) {
					Gdk.GC context = new Gdk.GC (surface.GdkWindow);
					surface.GdkWindow.BeginPaintRegion (args.Event.Region);
					Cairo.Context cairoContext = Gdk.CairoHelper.Create (surface.GdkWindow);
					/*Body bodies = world.GetBodyList ();
			while (bodies != null) {
				using (Body body = bodies) {
					Vec2 pos = body.GetPosition ();
					Console.WriteLine ("Drawing body at X: {0} Y: {1}", pos.X, pos.Y);
					Shape shape = body.GetShapeList ();
					while (shape != null) {
						Console.WriteLine ("Drawing shape");
						ShapeType type = shape.GetType ();
						Console.WriteLine (type.ToString ());
						switch (type) {
							case ShapeType.PolygonShape:
								Console.WriteLine ("Polygon ({0} vertices)", ((PolygonShape) shape).VertexCount);
								surface.GdkWindow.DrawPolygon (context, true, PhysicsSpaceToScreenSpace (ShapifyPolygon ((PolygonShape) shape)).ToArray ());
								break;
							case ShapeType.CircleShape:
								break;
							default:
								break;
						}
						shape = shape.GetNext ();
					}
				}
				bodies = bodies.GetNext ();
				if (bodies == null) {
					break;
				}
			}*/
					foreach (GameObject gameObject in gameObjects) {
						gameObject.Draw (surface);
						if (gameObject.renderRigidbody) {
							using (Body body = gameObject.rigidbody) {
								Vec2 pos = body.GetPosition ();
								Log (LogLevel.Verbose, "Drawing body at X: {0} Y: {1}", pos.X, pos.Y);
								Shape shape = body.GetShapeList ();
								while (shape != null) {
									ShapeType type = shape.GetType ();
									switch (type) {
										case ShapeType.PolygonShape:
											Log (LogLevel.Verbose, "Polygon ({0} vertices)", ((PolygonShape) shape).VertexCount);
											surface.GdkWindow.DrawPolygon (context, true, ApplyCameraOffset (PhysicsSpaceToScreenSpace (ShapifyPolygon ((PolygonShape) shape))).ToArray ());
											break;
										case ShapeType.CircleShape:
											break;
										default:
											break;
									}
									shape = shape.GetNext ();
								}
							}
						}
						if (gameObject.sprite != null) {
							cairoContext.Save ();

							cairoContext.Translate (PhysicsXToScreenX (gameObject.position.X) - cameraX, PhysicsYToScreenY (gameObject.position.Y) + cameraY);
							if (gameObject.rotation != 0) {
								cairoContext.Rotate (gameObject.rotation);
							}
							cairoContext.Translate (-0.5 * gameObject.sprite.Width, -0.5 * gameObject.sprite.Height);
							Gdk.CairoHelper.SetSourcePixbuf (cairoContext, gameObject.sprite, 0, 0);
							cairoContext.Paint ();

							cairoContext.Restore ();
							//surface.GdkWindow.DrawPixbuf (context, gameObject.sprite, 0, 0, PhysicsXToScreenX (gameObject.position.X) - (int) (0.5 * gameObject.sprite.Width), , -1, -1, Gdk.RgbDither.Normal, 0, 0);
						}
					}
					surface.GdkWindow.EndPaint ();
					cairoContext.Dispose ();
				}
				Log (LogLevel.Info, "Draw complete, unlocking");
			}
		}

		#endregion

		#region Utility functions

		private List<Gdk.Point> ApplyCameraOffset(List<Gdk.Point> points) {
			points.ForEach (delegate(Gdk.Point obj) {
				obj.X -= cameraX;
				obj.Y += cameraY;
			});
			return points;
		}

		private List<Gdk.Point> ShapifyPolygon (PolygonShape shape) {
			List<Gdk.Point> points = new List<Gdk.Point> ();
			for (int i = 0; i < shape.VertexCount; i++) {
				Vec2 vert = shape.GetVertices () [i];
				ApplyRotationMatrix (shape.GetBody ().GetAngle (), ref vert);
				Log (LogLevel.Verbose, "Vertex - X: {0} Y: {1}", vert.X, vert.Y);
				points.Add (new Gdk.Point ((int) (vert.X + shape.GetBody ().GetPosition ().X), (int) (vert.Y + shape.GetBody ().GetPosition ().Y)));
			}
			return points;
		}

		private Gdk.Rectangle MinimumRotatedBoundingRect (Gdk.Point upLeftCorner, Gdk.Point upRightCorner, Gdk.Point downLeftCorner, Gdk.Point downRightCorner, float theta) {
			Gdk.Point rotated1 = ApplyRotationMatrix (theta, upLeftCorner);
			Gdk.Point rotated2 = ApplyRotationMatrix (theta, upRightCorner);
			Gdk.Point rotated3 = ApplyRotationMatrix (theta, downRightCorner);
			Gdk.Point rotated4 = ApplyRotationMatrix (theta, downLeftCorner);

			int left = System.Math.Min (System.Math.Min (rotated1.X, rotated2.X), System.Math.Min (rotated3.X, rotated4.X));
			int right = System.Math.Max (System.Math.Max (rotated1.X, rotated2.X), System.Math.Max (rotated3.X, rotated4.X));
			int top = System.Math.Max (System.Math.Max (rotated1.Y, rotated2.Y), System.Math.Max (rotated3.Y, rotated4.Y));
			int bottom = System.Math.Min (System.Math.Min (rotated1.Y, rotated2.Y), System.Math.Min (rotated3.Y, rotated4.Y));
			return new Gdk.Rectangle (0, 0, System.Math.Abs (left) + System.Math.Abs (right), System.Math.Abs (top) + System.Math.Abs (bottom));
		}

		private int PhysicsXToScreenX (float x) {
			return ((int) x) + ((int) (0.5 * window.Allocation.Width));
		}

		private int PhysicsYToScreenY (float y) {
			return ((int) y) + ((int) (0.5 * window.Allocation.Height));
		}

		private List<Gdk.Point> PhysicsSpaceToScreenSpace (List<Gdk.Point> points) {
			return points.ConvertAll<Gdk.Point> (delegate(Gdk.Point input) {
				input.X += (int) (0.5 * windowWidth);
				input.Y += (int) (0.5 * windowHeight);
				return input;
			});
		}

		private void ApplyRotationMatrix (float theta, ref Vec2 vector) {
			float rotatedX = (float) ((vector.X * System.Math.Cos (theta)) - (vector.Y * System.Math.Sin (theta)));
			float rotatedY = (float) ((vector.X * System.Math.Sin (theta)) + (vector.Y * System.Math.Cos (theta)));
			vector.Set (rotatedX, rotatedY);
		}

		private Gdk.Point ApplyRotationMatrix (float theta, Gdk.Point original) {
			return new Gdk.Point ((int) ((original.X * System.Math.Cos (theta)) - (original.Y * System.Math.Sin (theta))), (int) ((original.X * System.Math.Sin (theta)) + (original.Y * System.Math.Cos (theta))));
		}

		internal static void EngineLog (string format, params object[] args) {
			Console.WriteLine ("[ Rotor \\L ]: " + format, args);
		}

		internal static void EngineLog (LogLevel level, string format, params object[] args) {
			if (level == LogLevel.Silence) {
				throw new ArgumentException ("\"Silence\" is not an allowed logging level.");
			}
			Console.WriteLine ("[ Rotor \\" + logTags [(int) level] + " ]: " + format, args);
		}

		internal void Log (string format, params object[] args) {
			if (logLevel != LogLevel.Silence) {
				Console.WriteLine ("[ Rotor \\L ]: " + format, args);
			}
		}

		internal void Log (LogLevel level, string format, params object[] args) {
			if (level == LogLevel.Silence) {
				throw new ArgumentException ("\"Silence\" is not an allowed logging level.");
			}
			if (level == LogLevel.Error) {
				Console.Error.WriteLine ("[ Rotor \\" + logTags [(int) level] + " ]: " + format, args);
			} else if ((int) level >= (int) logLevel) {
				Console.WriteLine ("[ Rotor \\" + logTags [(int) level] + " ]: " + format, args);
			}
		}

		internal void DisplayErrorMessage (Exception e) {
			Log (LogLevel.Error, "{0}\nStack trace:\n{1}", e.Message, e.StackTrace);
			MessageDialog errorPopup = new MessageDialog (window, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, "");
			errorPopup.UseMarkup = false;
			errorPopup.Text = "Error!";
			errorPopup.SecondaryUseMarkup = false;
			errorPopup.SecondaryText = string.Format ("{0}\nStack trace:\n{1}", e.Message, e.StackTrace);
			errorPopup.Response += delegate {
				errorPopup.Destroy ();
			};
			errorPopup.Run ();
			errorPopup.Destroy ();
		}

		#endregion

		#region Exposed API

		/// <summary>
		/// Start this instance of the Rotor Engine.
		/// </summary>
		public void Start () {
			Log ("Starting main loop");
			Application.Init ();

			window.Title = windowTitle;

			if (opMode == OperationMode.DIMEN2) {
				Init2D ();
			} else  if (opMode == OperationMode.DIMEN3) {
				throw new NotImplementedException ();
			}

			GLib.Timeout.Add ((uint) (1000f / targetFramerate), new GLib.TimeoutHandler (delegate {
				Update ();
				return true;
			}));
			ResizeWindow (windowWidth, windowHeight);
			window.Screen = surface.Screen;
			window.ShowAll ();
			while (!exitRequested) {
				try {
					Application.RunIteration (true);
				} catch (Exception e) {
					DisplayErrorMessage (e);
				}
			}
			Stop ();
		}

		/// <summary>
		/// Stop this instance and quit the Gtk application.
		/// </summary>
		public void Stop () {
			Log ("Stopping");
			lock (updateLock) {
				if (opMode == OperationMode.DIMEN2 && world != null) {
					world.Dispose ();
				}
				Application.Quit ();
			}
			System.Environment.Exit (0);
		}

		/// <summary>
		/// Resizes the window.
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void ResizeWindow (int width, int height) {
			window.Resize (width, height);
		}

		/// <summary>
		/// Loads the stage.
		/// </summary>
		/// <param name="stageType">Type of the stage to load. stageType must indirectly or directly derive from Rotor.Stage.</param>
		public void LoadStage (Type stageType) {
			if (!typeof(Stage).IsAssignableFrom (stageType)) {
				throw new ArgumentException (string.Format ("Type \"{0}\" cannot be cast to Rotor.Stage", stageType.Name));
			}
			Stage stage = (Stage) Activator.CreateInstance (stageType);
			LoadStage (stage);
		}

		/// <summary>
		/// Loads the stage.
		/// </summary>
		/// <param name="stage">Stage.</param>
		public void LoadStage (Stage stage) {
			lock (updateLock) {
				Log ("Loading stage \"{0}\"", stage.name);
				Log (LogLevel.Info, "Locking for stage load");
				if (world != null)
					world.Dispose ();
				if (opMode != OperationMode.DIMEN2) {
					Init2D ();
				}
				opMode = OperationMode.DIMEN2;
				world = new World (stage.aabb, stage.gravity, stage.allowSleep);
				stage.Construct (ref world);
				gameObjects.Clear ();
				gameObjects.AddRange (stage.gameObjects);
				staticControllers.Clear ();
				staticControllers.AddRange (stage.staticControllers);
				InitControllers ();
				Log (LogLevel.Info, "Stage load complete; unlocking");
			}
		}

		/// <summary>
		/// Loads a stage asynchronously. For use in controllers.
		/// </summary>
		/// <param name="stageType">Stage type.</param>
		public async void  LoadStageAsync (Type stageType) {
			await Task.Run (() => {
				LoadStage (stageType);
			});
		}

		/// <summary>
		/// Loads a stage asynchronously. For use in controllers.
		/// </summary>
		/// <param name="stage">Stage.</param>
		public async void LoadStageAsync (Stage stage) {
			await Task.Run (() => {
				LoadStage (stage);
			});
		}

		/// <summary>
		/// Loads the layout.
		/// </summary>
		/// <param name="layoutType">Type of the layout to load. layoutType must indirectly or directly derive from Rotor.Layout.</param>
		public void LoadLayout (Type layoutType) {
			if (!typeof(Rotor.Layout).IsAssignableFrom (layoutType)) {
				throw new ArgumentException (string.Format ("Type \"{0}\" cannot be cast to Rotor.Layout", layoutType.Name));
			}
			Rotor.Layout layout = (Rotor.Layout) Activator.CreateInstance (layoutType);
			LoadLayout (layout);
		}

		/// <summary>
		/// Loads the layout.
		/// </summary>
		/// <param name="layout">Layout.</param>
		public void LoadLayout (Rotor.Layout layout) {
			lock (updateLock) {
				opMode = OperationMode.GTKGUI;
				foreach (Widget widget in window.AllChildren) {
					window.Remove (widget);
				}
				layout.Construct (ref window);
			}
		}

		/// <summary>
		/// Sets the operation mode.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public void SetOperationMode (OperationMode mode) {
			if (mode == OperationMode.DIMEN3) {
				throw new NotImplementedException ();
			}
			opMode = mode;
		}

		/// <summary>
		/// Adds a static controller.
		/// </summary>
		/// <param name="controller">Controller.</param>
		public void AddStaticController (Controller controller) {
			controller.input = input;
			staticControllers.Add (controller);
		}

		/// <summary>
		/// Adds a game object.
		/// </summary>
		/// <param name="obj">Object.</param>
		public void AddGameObject (GameObject obj) {
			gameObjects.Add (obj);
		}

		#endregion
	}
}

