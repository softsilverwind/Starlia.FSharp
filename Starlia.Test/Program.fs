open Starlia
open Starlia.Simple

open System.Drawing

open OpenTK
open OpenTK.Graphics.OpenGL

type MyObject () =
    inherit SObject()

    override this.Draw (_) =
        let scale = SCore.GetScale()
        let mutable projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, float32 scale.Width / float32 scale.Height, 1.f, 64.f)
        GL.MatrixMode(MatrixMode.Projection)
        GL.LoadMatrix(&projection)

        let mutable modelview = Matrix4.LookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY)
        GL.MatrixMode(MatrixMode.Modelview)
        GL.LoadMatrix(&modelview)
        GL.Begin(BeginMode.Triangles)
        GL.Color3(1.f, 1.f, 0.f); GL.Vertex3(-1.f, -1.f, 4.f)
        GL.Color3(1.f, 0.f, 0.f); GL.Vertex3(1.f, -1.f, 4.f)
        GL.Color3(0.2f, 0.9f, 1.f); GL.Vertex3(0.f, 1.f, 4.f)
        GL.End()

SCore.Init("Test window", Size(800, 600))

let layer = StaticShaderLayer(SOrthoCamera(Vector2.Zero, Vector2.One), Shaders.basicobject_v, Shaders.basicobject_f)

SCore.AddLast(layer)
layer.Add(MyObject())

let obj = S3dObject(Vector3.Zero, Vector3.One, Vector3.Zero, SSimpleModel("/home/nick/Programs/C++/openGL/Starlia/src/testbench/assets/zombie.obj"))
layer.Add(obj)

SCore.Loop()