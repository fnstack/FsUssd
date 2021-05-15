[<AutoOpen>]
module FsUssd.UssdState

open System

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD CONTEXT
////////////////////////////////////////////////////////////////////////////////////////////////////

type UssdContext =
    { Args: UssdArguments
      SetValue: string * string -> Async<unit>
      GetValue: string -> Async<string option>
      Session: UssdSession }

module UssdContext =
    let empty =
        { Args = UssdArguments.empty
          SetValue = fun (_, _) -> async { return () }
          GetValue = fun _ -> async { return None }
          Session = UssdSession.empty }

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD STATE
////////////////////////////////////////////////////////////////////////////////////////////////////

type UssdStateRunner = UssdContext -> Async<UssdResult>

type UssdState =
    { Name: string
      Run: UssdStateRunner
      Next: Map<string, string> }

let empty =
    { Name = String.Empty
      Run = fun _ -> async { return UssdResult.terminate (String.Empty) }
      Next = Map.empty }

type UssdStateBuilder internal () =

    member _.Yield(_) = empty

    member __.Run(state: UssdState) = state

    [<CustomOperation("name")>]
    member _.SetStateName(state: UssdState, name: string) = { state with Name = name }

    [<CustomOperation("run")>]
    member _.SetRun(state: UssdState, run: UssdStateRunner) = { state with Run = run }

    [<CustomOperation("next")>]
    member _.SetNext(state: UssdState, next: Map<string, string>) = { state with Next = next }

    [<CustomOperation("add_next_entry")>]
    member _.SetAddNextEntry(state: UssdState, key: string, value: string) =
        let next = state.Next |> Map.add key value

        { state with Next = next }

let ussdState = UssdStateBuilder()
