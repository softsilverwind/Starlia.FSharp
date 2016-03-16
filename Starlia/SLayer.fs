namespace Starlia

open System
open System.Collections.Generic

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

type BlockType =
  | Mouse
  | Keyboard
  | Draw
  | Update

type SLayer () =
    let mutable invalid = false
    let mutable block = Set.empty : Set<BlockType>

    let keypresses = Dictionary<Key, unit -> unit>()
    let keyreleases = Dictionary<Key, unit -> unit>()
    let mutable click = fun (x : Vector2) -> false
    let mutable mouseOver = fun (x : Vector2) -> false

    let mutable world = Matrix4()
    let mutable view = Matrix4()
    let mutable projection = Matrix4()
    let mutable wvp = Matrix4()

    let calcWVP () = wvp <- projection * view * world

    member this.Invalidate () = invalid <- true
    member this.Invalid with get () : bool = invalid

    abstract member Draw : unit -> unit default this.Draw() = ()
    abstract member Update : unit -> unit default this.Update() = ()

    member internal this.EventKeyPress (key : Key) : bool =
        match keypresses.TryGetValue(key) with
            | (true, f) ->
                f()
                true
            | (false, _) ->
                block.Contains(Keyboard)

    member internal this.EventKeyRelease (key : Key) : bool =
        match keyreleases.TryGetValue(key) with
            | (true, f) ->
                f()
                true
            | (false, _) ->
                block.Contains(Keyboard)

    member internal this.EventClick (coord : Vector2) : bool = click(coord)
    member internal this.EventMouseOver (coord : Vector2) : bool = mouseOver(coord)

    member this.KeyPresses  with get () : Dictionary<Key, unit -> unit> = keypresses
    member this.KeyReleases with get () : Dictionary<Key, unit -> unit> = keyreleases
    member this.Block       with get () : Set<BlockType> = block and set(b) = block <- b
    member this.Click       with get () : Vector2 -> bool = click and set(f : Vector2 -> bool) = click <- f
    member this.MouseOver   with get () : Vector2 -> bool = mouseOver and set(f : Vector2 -> bool) = mouseOver <- f

    member this.World
        with get () : Matrix4 = world
        and set (x : Matrix4) =
            world <- x
            calcWVP ()

    member this.View
        with get () : Matrix4 = view
        and set (x : Matrix4) =
            view <- x
            calcWVP ()

    member this.Projection
        with get () : Matrix4 = projection
        and set (x : Matrix4) =
            projection <- x
            calcWVP ()

    member this.WVP with get () : Matrix4 = wvp

    abstract member Shader : SShader default this.Shader with get() = raise <| NotImplementedException()