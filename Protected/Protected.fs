namespace Starlia.Protected

open System

[<AttributeUsage(AttributeTargets.Method ||| AttributeTargets.Constructor, AllowMultiple=false, Inherited=true)>]
type Protected() =
    inherit System.Attribute()