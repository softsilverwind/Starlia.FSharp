namespace Starlia

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

type SShader (program : int) =
    member this.Attrib(name : string) : int =
        GL.GetAttribLocation(program, name)

    member this.Uniform(name : string) : int =
        GL.GetUniformLocation(program, name)

    member this.Program with get () : int = program