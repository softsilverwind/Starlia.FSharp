module Starlia.Helpers.Lambda

let rec fix f x = f (fix f) x

let flip x y = y x

let doWhile (f : 'a -> bool) (list : 'a seq) : unit =
    let mutable it = list.GetEnumerator()
    let mutable flag = it.MoveNext()
    while flag do
        flag <- f (it.Current) && it.MoveNext()