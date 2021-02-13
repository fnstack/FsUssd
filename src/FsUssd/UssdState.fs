[<AutoOpen>]
module FsUssd.UssdState

open System

type UssdState = {
    Name: string
    Run: UssdContext -> Async<unit>
}

let Empty = {
    Name = String.Empty
    Run = fun _ -> async { return () }
}

type UssdStateBuilder internal () =

    member _.Yield(_) = Empty

    member __.Run(state: UssdState) = state

    [<CustomOperation("name")>]
    member _.SetStateName(state: UssdState, name: string) =
        {state with Name = name}

    [<CustomOperation("run")>]
    member _.SetRun(state: UssdState, run: UssdContext -> Async<unit>) =
        {state with Run = run}

let ussdState = UssdStateBuilder()
