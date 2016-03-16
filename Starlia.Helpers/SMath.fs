module Starlia.Helpers.SMath

let clamp x min max =
    match x with
        | x when x < min -> min
        | x when x > max -> max
        | _ -> x

let fmod (x : float32) (y : float32) : float32 =
    x - float32 (int x / int y) * y

let dmod (x : float) (y : float) : float =
    x - float (int x / int y) * y