namespace Starlia.Simple

open Starlia
open Starlia.Helpers

open System.Drawing
open OpenTK.Graphics.OpenGL

type ClearColorLayer(clearColor : Color) =
    inherit SLayer()

    override this.Draw() =
        GL.ClearColor(clearColor)
        GL.Clear(ClearBufferMask.ColorBufferBit)

type ClearDepthLayer() =
    inherit SLayer()

    override this.Draw() =
        GL.Clear(ClearBufferMask.DepthBufferBit)

type StaticShaderLayer(camera : SCamera, vshader : string, fshader : string) as this =
    inherit SListLayer<SObject>()
    let mutable shader = Unchecked.defaultof<SShader>

    let initialize () =
        let vs = GL.CreateShader(ShaderType.VertexShader)
        do GL.ShaderSource(vs, vshader)
        do GL.CompileShader(vs)

        let fs = GL.CreateShader(ShaderType.FragmentShader)
        do GL.ShaderSource(fs, fshader)
        do GL.CompileShader(fs)

        let program = GL.CreateProgram()
        do GL.AttachShader(program, vs)
        do GL.AttachShader(program, fs)
        do GL.LinkProgram(program)

        let mutable ok = 0
        do GL.GetProgram(program, GetProgramParameterName.LinkStatus, &ok)

        if ok = 0 then
            Log.logf Log.logger "StaticShaderLayer failed to link shader"
            Log.logf Log.logger "Vertex shader is:\n%s" vshader
            Log.logf Log.logger "Vertex shader errors:\n%s" <| Log.GetGlErrors vs
            Log.logf Log.logger "Fragment shader is:\n%s" fshader
            Log.logf Log.logger "Vertex shader errors:\n%s" <| Log.GetGlErrors vs
        else
            Log.logf Log.logger "all ok!"
            shader <- SShader(program)

        match camera with
            | :? SObject as camobj -> this.Add(camobj)
            | _ -> ()

    do initialize ()

    override this.Draw () =
        GL.UseProgram(shader.Program)
        this.Projection <- camera.Projection
        this.View <- camera.View

        base.Draw()

    member this.Camera with get() : SCamera = camera

    override this.Shader with get() : SShader = shader