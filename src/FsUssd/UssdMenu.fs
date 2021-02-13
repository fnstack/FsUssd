[<AutoOpen>]
module FsUssd.UssdMenu

open System

type UssdMenuState =
    { StartState: UssdState
      Context: UssdContext option
      States: UssdState list}

let private empty = {
    StartState = Empty
    Context = None
    States = []
}

let private setStartState menu state = { menu with StartState = state; States = state :: menu.States }

let private addState menu state =
    { menu with
          States = state :: menu.States }

type UssdMenuBuilder internal () =

    member _.Yield(_) = empty

    member __.Run(state: UssdMenuState) = state

    [<CustomOperation("start_state")>]
    member _.SetStartState(menu: UssdMenuState, state: UssdState) =

        setStartState menu state

    [<CustomOperation("add_state")>]
    member _.AddState(menu: UssdMenuState, state: UssdState) =

        addState menu state

let ussdMenu = UssdMenuBuilder()

let private getSession = UssdSession.getSession sessionStore
let private setSession = UssdSession.setSession sessionStore

let runState (context: UssdContext, state: UssdState) = async {
    let session = context.Session
    
    let! result = state.Run(context)

    let! _ = setSession(session)

    return ""
}

let run (menu: UssdMenuState) (args: UssdArguments) = async {
    let! session = getSession args.SessionId

    let context : UssdContext =
        match menu.Context with
        | None ->
            {
                Args = args
                Session = {session with CurrentState = menu.StartState.Name}
            }
        | Some context ->
            {
                context with Args = args
            }

    match menu.States |> List.tryFind (fun state -> state.Name = context.Session.CurrentState) with
    | None ->
        return String.Empty
    | Some state ->
        let! t = runState (context, state)
        return String.Empty
}
