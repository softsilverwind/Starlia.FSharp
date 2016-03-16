namespace Starlia

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

type SCamera =
    abstract member Projection : Matrix4
    abstract member View       : Matrix4

    abstract member DistanceFrom : Vector3 -> float32 // used for particles / blending

type SOrthoCamera (position : Vector2, size : Vector2, ?near : float32, ?far : float32) =
    inherit S2dDynObject (position, size)

    let near = defaultArg near -1.0f
    let far = defaultArg far 100.0f

    member this.Zoom (times : float32) =
        this.Size <- this.Size * 2.0f

    interface SCamera with
        member this.Projection
            with get () =
                let halfsize = this.Size / 2.0f
                let topleft = this.Position - halfsize
                let botright = this.Position + halfsize

                Matrix4.CreateOrthographicOffCenter(topleft.X, botright.X, botright.Y, topleft.Y, near, far)

        member this.View
            with get () = Matrix4.Identity

        member this.DistanceFrom (point : Vector3) : float32 =
            point.Z


type SPerspCamera (position : Vector3, angle : Vector3, ?fov : float32, ?near : float32, ?far : float32) =
    inherit S3dDynObject (position, Vector3.One, angle)

    let fov = defaultArg fov MathHelper.PiOver4
    let near = defaultArg near 1.0f
    let far = defaultArg far 100.0f

    interface SCamera with
        member this.Projection
            with get () =
                let scale = SCore.GetScale()
                let s = Vector2(float32 scale.Width, float32 scale.Height)
                Matrix4.CreatePerspectiveFieldOfView(fov, s.X / s.Y, near, far)
            
        member this.View
            with get () =
                Matrix4.CreateRotationX(-MathHelper.PiOver2)
                * Matrix4.CreateRotationY(-this.Angle.Y)
                * Matrix4.CreateRotationX(-this.Angle.X)
                * Matrix4.CreateRotationZ(-this.Angle.Z)
                * Matrix4.CreateTranslation(-this.Position)

        member this.DistanceFrom (point : Vector3) : float32 =
            (this.Position - point).Length