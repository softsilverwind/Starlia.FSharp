module Starlia.Helpers.Image
open System
open System.Drawing
open System.Drawing.Imaging

open OpenTK
open OpenTK.Graphics.OpenGL

let loadImage (filename : string) : int = 
    let id = GL.GenTexture()
    GL.BindTexture(TextureTarget.Texture2D, id)

    GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, int TextureEnvMode.Modulate)
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
 
    use bmp = new Bitmap(filename);
    let bmp_data = bmp.LockBits(Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
 
    GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
        PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
 
    GL.GenerateMipmap(GenerateMipmapTarget.Texture2D)

    bmp.UnlockBits(bmp_data);
 
    id