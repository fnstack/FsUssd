[<AutoOpen>]
module FsUssd.UssdMenu

open System

type UssdMenu =
    { StartState: UssdState option
      Context: UssdContext option
      States: UssdState list}

let private empty = {
    StartState = None
    Context = None
    States = []
}

let private setStartState state startState = { state with StartState = startState }

let private addState menu state =
    { menu with
          States = state :: menu.States }

type UssdMenuBuilder internal () =

    member _.Yield(_) = empty

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

let private getSession = UssdSession.getSession sessionStore

let run (menu: UssdMenu) (args: UssdArguments) = async {
    let! session = getSession args.SessionId

    return String.Empty
}
