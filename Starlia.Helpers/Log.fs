module Starlia.Helpers.Log

open System

open OpenTK.Graphics.OpenGL

let GetGlErrors(obj : int) =
    if GL.IsShader(obj) then
        GL.GetShaderInfoLog(obj)
    else if GL.IsProgram(obj) then
        GL.GetProgramInfoLog(obj)
    else
        "Not a shader or program"

let mutable logger = eprintfn "%A: %s" DateTime.Now

let logf logger format = Printf.kprintf logger format