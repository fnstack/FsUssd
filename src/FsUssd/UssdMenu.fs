[<AutoOpen>]
module FsUssd.UssdMenu

open System

type UssdMenuState =
    { StartState: UssdState
      Context: UssdContext
      States: UssdState list}

let private empty = {
    StartState = UssdState.empty
    Context = UssdContext.empty
    States = []
}

let private setStartState menu state = { menu with StartState = state; States = state :: menu.States }

let private addState menu state =
    { menu with
          States = state :: menu.States }

type UssdMenuBuilder internal () =

    member _.Yield(_) = empty

    member __.Run(state: UssdMenuState) =
        //match state with
        //| state when state.StartState = UssdState.Empty && state.States |> List.isEmpty |> not ->
        //    {state with StartState = state.States.[0]}
        //| _ ->
        //    state

        state

    [<CustomOperation("start_state")>]
    member _.SetStartState(menu: UssdMenuState, state: UssdState) =

        setStartState menu state

    [<CustomOperation("add_state")>]
    member _.AddState(menu: UssdMenuState, state: UssdState) =

        addState menu state

let ussdMenu = UssdMenuBuilder()

let private getSession = UssdSession.getSession sessionStore
let private setSession = UssdSession.setSession sessionStore

let findState (states: UssdState list) stateName =
    
    match states |> List.tryFind (fun state -> state.Name = stateName) with
    | None ->
        states.[1]
    | Some state ->
        state

let findNextState (states: UssdState list) stateName userText =
    
    let state = findState states stateName

    let state = match state.Next.TryFind(userText) with
                | None ->
                    state
                | Some state ->
                    state

    state

let private runState (context: UssdContext, state: UssdState) : Async<UssdResult> = async {
    let session = context.Session
    
    let! result = state.Run(context)

    let session =
        match result.Type, session with
        | Response, session ->
            {session with Status = Ongoing; CurrentState = state.Name}
        | Release, session ->
            {session with Status = Terminated; CurrentState = state.Name}

    let! _ = setSession(session)

    return result
}

let run (menu: UssdMenuState) (args: UssdArguments) = async {
    let! session = getSession args.SessionId

    let context, state : UssdContext * UssdState =
        match session.Status with
        | Initiated ->
            let stateName = menu.StartState.Name
            let state = findState menu.States stateName

            {
                Args = args
                Session = {session with CurrentState = state.Name}
            }, state
        | Ongoing ->
            let stateName = menu.StartState.Name
            let state = findNextState menu.States stateName args.Text

            {
                menu.Context with Args = args
            }, state
        | Terminated ->
            menu.Context, menu.StartState

    let! result = runState (context, state)
    return result.Message
        
}
