[<AutoOpen>]
module FsUssd.UssdMenu

open System
open System.Text

type UssdMenu =
    { StartState: UssdState option
      Context: UssdContext option
      States: UssdState list}

let Empty = {
    StartState = None
    Context = None
    States = []
}

let setStartState state startState = { state with StartState = startState }

let addState menuState state =
    { menuState with
          States = state :: menuState.States }

let run (menu: UssdMenu) (args: UssdArguments) = async {
    return String.Empty
}

type UssdMenuBuilder internal () =

    member _.Yield(_) = Empty

    member __.Run(state: UssdMenu) = state

    [<CustomOperation("start_state")>]
    member _.SetStartState(menu: UssdMenu, state: UssdState) =

        setStartState menu (Some state)

    [<CustomOperation("add_state")>]
    member _.AddState(menu: UssdMenu, stateName: string, stateRun: UssdContext -> Async<unit>) =
        let state = ussdState {
            name stateName
            run stateRun
        }

        addState menu state

let ussdMenu = UssdMenuBuilder()
