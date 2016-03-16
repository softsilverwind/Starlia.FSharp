namespace Starlia

open System
open System.Collections.Generic
open OpenTK

open Assimp

open Starlia.Helpers.Lambda

type SModel () =
    abstract member Draw : SLayer -> unit default this.Draw (layer : SLayer) = ()