namespace Starlia

open System
open System.Collections.Generic
open OpenTK

open Starlia.Helpers


type Axis =
  | Axis_X
  | Axis_Y
  | Axis_Z

type SObject () =
    let emittedSignals = HashSet<string>()
    let handlers = Dictionary<string, unit -> unit>()

    let mutable invalid = false

    member this.Invalidate () = invalid <- true
    member this.Invalid with get () : bool = invalid

    abstract member Draw : SLayer -> unit default this.Draw(layer : SLayer) = ()
    abstract member Update : unit -> unit default this.Update() = ()

    member internal this.DispatchSignals () =
        Seq.iter (fun x -> handlers.[x]()) emittedSignals
        emittedSignals.Clear()

    member this.Emit (name : string) = emittedSignals.Add(name)

    member this.SetHandler (name : string, f : unit -> unit) = handlers.Add(name, f)

type S2dObject (?position : Vector2, ?size : Vector2, ?angle : float32, ?model : SModel) =
    inherit SObject ()

    let mutable position = defaultArg position Vector2.Zero
    let mutable size = defaultArg size Vector2.One
    let mutable angle = defaultArg angle 0.0f
    let mutable model = defaultArg model <| SModel()

    member this.Position with get () : Vector2 = position and set (x : Vector2) = position <- x
    member this.Size     with get () : Vector2 = size     and set (x : Vector2) = size <- x
    member this.Angle    with get () : float32 = angle    and set (x : float32) = angle <- x
    member this.Model    with get () : SModel  = model    and set (x : SModel)  = model <- x

    override this.Draw (layer : SLayer) =
        layer.World <- Matrix4.CreateTranslation(Vector3(position.X, position.Y, 0.0f))
            * Matrix4.CreateRotationZ(angle)
            * Matrix4.CreateScale(Vector3(size.X, size.Y, 0.0f))
           
        model.Draw (layer)

    member this.GetNormal (axis : Axis) : Vector2 = // TODO this was written shotgun, check if it is correct
        let angled = float angle

        match axis with
            | Axis_X -> Vector2d(Math.Cos(angled), Math.Sin(angled)) |> Vector2d.op_Explicit
            | Axis_Y -> Vector2d(Math.Sin(angled), Math.Cos(angled)) |> Vector2d.op_Explicit
            | Axis_Z -> Vector2.Zero


type S3dObject (?position : Vector3, ?size : Vector3, ?angle : Vector3, ?model : SModel) =
    inherit SObject ()

    let mutable position = defaultArg position Vector3.Zero
    let mutable size = defaultArg size Vector3.One
    let mutable angle = defaultArg angle Vector3.Zero
    let mutable model = defaultArg model <| SModel()

    member this.Position with get () : Vector3 = position and set (x : Vector3) = position <- x
    member this.Size     with get () : Vector3 = size     and set (x : Vector3) = size     <- x
    member this.Angle    with get () : Vector3 = angle    and set (x : Vector3) = angle    <- x
    member this.Model    with get () : SModel  = model    and set (x : SModel)  = model    <- x

    override this.Draw (layer : SLayer) =
        layer.World <- Matrix4.CreateTranslation(Vector3(position.X, position.Y, position.Z))
            * Matrix4.CreateRotationZ(angle.Z)
            * Matrix4.CreateRotationX(angle.X)
            * Matrix4.CreateRotationY(angle.Y)
            * Matrix4.CreateScale(Vector3(size.X, size.Y, size.Z))
           
        model.Draw (layer)
    
    member this.GetNormal (axis : Axis) : Vector3 =
        let angled = Vector3d.op_Explicit(angle)
        let (cx, cy, cz) = (Math.Cos(angled.X), Math.Cos(angled.Y), Math.Cos(angled.Z))
        let (sx, sy, sz) = (Math.Sin(angled.X), Math.Sin(angled.Y), Math.Sin(angled.Z))

        match axis with
            | Axis_X -> Vector3d(cy*cz - sx*sy*sz, cy*sz + cz*sx*sy, -cx*sy) |> Vector3d.op_Explicit
            | Axis_Y -> Vector3d(-cx*sz, cx*cz, sx) |> Vector3d.op_Explicit
            | Axis_Z -> Vector3d(cz*sy + cy*sx*sz, sy*sz - cy*cz*sx, cx*cy) |> Vector3d.op_Explicit

type S2dDynObject (?position : Vector2, ?size : Vector2, ?angle : float32, ?model : SModel) =
    inherit S2dObject (defaultArg position Vector2.Zero, defaultArg size Vector2.One, defaultArg angle 0.0f, defaultArg model <| SModel())

    let mutable velocity = Vector2.Zero
    let mutable angvelocity = 0.0f
    let mutable thrust = Vector2.Zero

    member this.Velocity with get () : Vector2 = velocity and set (x : Vector2) = velocity <- x
    member this.AngVelocity with get () : float32 = angvelocity and set (x : float32) = angvelocity <- x
    member this.Thrust with get () : Vector2 = thrust and set (x : Vector2) = thrust <- x

    override this.Update () =
        this.Angle <- SMath.fmod (this.Angle + angvelocity) (MathHelper.TwoPi);
        let actualVelocity = velocity + this.GetNormal(Axis_X) * thrust.X + this.GetNormal(Axis_Y) * thrust.Y
        this.Position <- this.Position + actualVelocity

type S3dDynObject (?position : Vector3, ?size : Vector3, ?angle : Vector3, ?model : SModel) =
    inherit S3dObject (defaultArg position Vector3.Zero, defaultArg size Vector3.One, defaultArg angle Vector3.Zero, defaultArg model <| SModel())

    let mutable velocity = Vector3.Zero
    let mutable angvelocity = Vector3.Zero
    let mutable thrust = Vector3.Zero

    member this.Velocity with get () : Vector3 = velocity and set (x : Vector3) = velocity <- x
    member this.AngVelocity with get () : Vector3 = angvelocity and set (x : Vector3) = angvelocity <- x
    member this.Thrust with get () : Vector3 = thrust and set (x : Vector3) = thrust <- x

    override this.Update () =
        this.Angle <- Vector3 (
            SMath.clamp (this.Angle.X + angvelocity.X) (-MathHelper.PiOver2) (MathHelper.PiOver2),
            SMath.fmod (this.Angle.Y + angvelocity.Y) (MathHelper.PiOver2),
            SMath.fmod (this.Angle.Z + angvelocity.Z) (MathHelper.PiOver2)
        )

        let actualVelocity =
            velocity
            + this.GetNormal(Axis_X) * thrust.X
            + this.GetNormal(Axis_Y) * thrust.Y
            + this.GetNormal(Axis_Z) * thrust.Z

        this.Position <- this.Position + actualVelocity
