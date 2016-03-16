module Starlia.SCore

open System.Drawing
open System.Diagnostics
open System.Collections.Generic

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL
open OpenTK.Input

open Starlia.Helpers.Lambda

let private layers = LinkedList<SLayer>()
let mutable private lastUpdate = 0L
let private clearColor = Color.CornflowerBlue

let mutable private scale = Size(1,1)

let GetScale () = scale
let private SetScale (s : Size) =
    scale <- s
    GL.Viewport(Point(0,0), scale)

let mutable private fps = 100

let private Draw () =
    GL.ClearColor(clearColor)
    GL.Enable(EnableCap.DepthTest)
    GL.Clear(ClearBufferMask.ColorBufferBit ||| ClearBufferMask.DepthBufferBit)

    let mutable node = layers.First

    while node <> null do
        let next = node.Next
        if node.Value.Invalid then
            layers.Remove(node)
        node <- next

    node <- layers.First

    while node <> null && (not <| node.Value.Block.Contains(Draw)) do
        node <- node.Next

    if node = null then
        node <- layers.Last

    while node <> null do
        node.Value.Draw()
        node <- node.Previous




let private Update () =
    let timeNow = Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000L)

    let mspf = 1000 / fps

    let times = int (timeNow / int64 mspf - lastUpdate / int64 mspf)

    let mutable node = layers.First
    while node <> null do
        let next = node.Next
        if node.Value.Invalid then
            layers.Remove(node)
        node <- next

    for i in 0..times do
        let mutable blocked = false
        let mutable node = layers.First

        while node <> null && not blocked do
            node.Value.Update()
            if node.Value.Block.Contains(Update) then
                blocked <- true
            node <- node.Next

    lastUpdate <- timeNow

                
let private eventClick (pos : Point) =
    let scaled = Vector2(float32 pos.X / float32 scale.Height, float32 pos.Y / float32 scale.Width)
    doWhile (fun (x : SLayer) -> not <| x.EventClick(scaled)) layers

let private eventMouseOver (pos : Point) =
    let scaled = Vector2(float32 pos.X / float32 scale.Height, float32 pos.Y / float32 scale.Width)
    doWhile (fun (x : SLayer) -> not <| x.EventMouseOver(scaled)) layers

let private eventKeyPress (c : Key) =
    doWhile (fun (x : SLayer) -> not <| x.EventKeyPress(c)) layers

let private eventKeyRelease (c : Key) =
    doWhile (fun (x : SLayer) -> not <| x.EventKeyRelease(c)) layers

let AddFirst (layer : SLayer) : unit = layers.AddFirst(layer) |> ignore
let AddLast (layer : SLayer) : unit = layers.AddLast(layer) |> ignore

let mutable private game : OpenTK.GameWindow = null

let Init (title : string, size : Size) =
    game <- new GameWindow(size.Width, size.Height, GraphicsMode.Default, title)

    game.Load.Add(fun _ ->
        game.VSync <- VSyncMode.On
        game.TargetRenderFrequency <- float fps
        game.TargetUpdateFrequency <- float fps
        SetScale(size)
    )
    game.Resize.Add(fun _ -> SetScale(game.Size))
    game.UpdateFrame.Add(fun _ -> Update() )
    game.RenderFrame.Add(fun _ -> Draw(); game.SwapBuffers())
    game.KeyDown.Add(fun e -> eventKeyPress(e.Key))
    game.KeyUp.Add(fun e -> eventKeyRelease(e.Key))
    game.MouseDown.Add(fun e -> let mouse = Mouse.GetState() in eventClick(Point(mouse.X, mouse.Y)))
    game.MouseMove.Add(fun e -> let mouse = Mouse.GetState() in eventMouseOver(Point(mouse.X, mouse.Y)))


let GetFPS () = fps
let SetFPS (x : int) =
    fps <- x
    game.TargetRenderFrequency <- float fps
    game.TargetUpdateFrequency <- float fps

let Loop () =
    lastUpdate <- Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000L)
    game.Run()