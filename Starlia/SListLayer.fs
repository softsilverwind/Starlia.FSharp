namespace Starlia

open System.Collections.Generic

type SListLayer<'T when 'T :> SObject> () =
    inherit SLayer ()
    let elements = LinkedList<'T>()

    override this.Draw () : unit =
        base.Draw ()

        let mutable it = elements.First
        while it <> null do
            let it2 = it.Next
            if it.Value.Invalid then
                elements.Remove(it)
            else
                it.Value.Draw(this)
            it <- it2

    override this.Update () =
        base.Update ()

        let mutable it = elements.First
        while it <> null do
            let it2 = it.Next
            if it.Value.Invalid then
                elements.Remove(it)
            else
                it.Value.Update()
            it <- it2

    member this.Add (o : 'T) : unit =
        elements.AddLast(o) |> ignore