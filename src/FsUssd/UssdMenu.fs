[<AutoOpen>]
module FsUssd.UssdMenu

open System

type UssdMenuState =
    { StartState: UssdState
      Context: UssdContext
      States: UssdState list}

let private empty = {
    StartState = Empty
    Context = UssdContext.empty
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

let private runState (context: UssdContext, state: UssdState) : Async<UssdResult> = async {
    let session = context.Session
    
    let! result = state.Run(context)

    let session =
        match result.Type, session with
        | Response, session ->
            {session with Status = Ongoing}
        | Release, session ->
            {session with Status = Terminated}

    let! _ = setSession(session)

    return result
}

let run (menu: UssdMenuState) (args: UssdArguments) = async {
    let! session = getSession args.SessionId

    let context : UssdContext =
        match session.Status with
        | Initiated ->
            {
                Args = args
                Session = {session with CurrentState = menu.StartState.Name}
            }
        | Ongoing ->
            {
                menu.Context with Args = args
            }
        | Terminated ->
            menu.Context

    match menu.States |> List.tryFind (fun state -> state.Name = context.Session.CurrentState) with
    | None ->
        return String.Empty
    | Some state ->
        let! result = runState (context, state)
        return result.Message
}
