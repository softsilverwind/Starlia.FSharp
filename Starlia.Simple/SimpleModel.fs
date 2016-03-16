namespace Starlia.Simple

open System
open System.IO
open System.Drawing
open System.Collections.Generic

open Assimp

open OpenTK
open OpenTK.Graphics
open OpenTK.Graphics.OpenGL

open Starlia
open Starlia.Helpers.Lambda
open Starlia.Helpers.Image

type SSimpleModel (filename : string) =
    inherit SModel ()

    let mutable vertices = Unchecked.defaultof<Vector3[]>
    let mutable normals  = Unchecked.defaultof<Vector3[]>
    let mutable textures = Unchecked.defaultof<Vector2[]>
    let mutable tex      = 0

    let initialize () =
        use importer = new AssimpContext()
        let model = importer.ImportFile(filename, PostProcessPreset.TargetRealTimeMaximumQuality)
        let v = List<Vector3>()
        let n = List<Vector3>()
        let t = List<Vector2>()

        fun (init : (Scene * Node) -> unit) (scene : Scene, node : Node) ->
            for mesh in scene.Meshes do
                for face in mesh.Faces do
                    if face.IndexCount = 3 then
                        for k in 0..face.IndexCount - 1 do
                            let index = face.Indices.[k]
                            if mesh.Normals <> null then
                                n.Add(Vector3(mesh.Normals.[index].X, mesh.Normals.[index].Y, mesh.Normals.[index].Z))
                            v.Add(Vector3(mesh.Vertices.[index].X, mesh.Vertices.[index].Y, mesh.Vertices.[index].Z))
                            t.Add(Vector2(mesh.TextureCoordinateChannels.[0].[index].X, 1.0f - mesh.TextureCoordinateChannels.[0].[index].Y))
            
            for child in node.Children do
                init (scene, child)
        |> fix
        |> flip (model, model.RootNode)

        vertices <- v.ToArray()
        normals <- n.ToArray()
        textures <- t.ToArray()

        let path = model.Materials.[0].GetMaterialTextures(Assimp.TextureType.Diffuse).[0].FilePath // Get your free exception today!

        let fullpath = FileInfo(filename).DirectoryName + "/" + path
        tex <- loadImage(fullpath)

    do initialize ()

    override this.Draw (layer : SLayer) =
        let attrib_pos = layer.Shader.Attrib("pos")
        let attrib_texcoord = layer.Shader.Attrib("texcoord")
        let uniform_wvp = layer.Shader.Uniform("wvp")
        let uniform_tex = layer.Shader.Uniform("tex")

        GL.ActiveTexture(TextureUnit.Texture0)
        GL.BindTexture(TextureTarget.Texture2D, tex)
        GL.Uniform1(uniform_tex, 0)

        let mutable wvp = layer.WVP
        GL.UniformMatrix4(uniform_wvp, false, &wvp)

        GL.EnableVertexAttribArray(attrib_pos)
        GL.VertexAttribPointer(attrib_pos, 3, VertexAttribPointerType.Float, false, 0, vertices)
        GL.EnableVertexAttribArray(attrib_texcoord)
        GL.VertexAttribPointer(attrib_texcoord, 2, VertexAttribPointerType.Float, false, 0, textures)

        GL.DrawArrays(PrimitiveType.Triangles, 0, vertices.Length)

        GL.DisableVertexAttribArray(attrib_pos)
        GL.DisableVertexAttribArray(attrib_texcoord)