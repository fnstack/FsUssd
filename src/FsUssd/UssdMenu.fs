[<AutoOpen>]
module FsUssd.UssdMenu

open System

type UssdMenuState =
    { StartState: UssdState
      Context: UssdContext
      SessionStore: UssdSessionStore
      States: UssdState list }

let private empty =
    { StartState = UssdState.empty
      Context = UssdContext.empty
      SessionStore = memorySessionStore
      States = [] }

let private setStartState menu state =
    { menu with
          StartState = state
          States = state :: menu.States }

let private addState menu state =
    { menu with
          States = state :: menu.States }

let private addStates menu states =
    let stateNames =
        states |> List.map (fun state -> state.Name)

    { menu with
          States =
              states
              @ (menu.States
                 |> List.where
                     (fun state ->
                         stateNames
                         |> List.exists (fun name -> name <> state.Name))) }

let private useSessionStore menu store = { menu with SessionStore = store }

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

    [<CustomOperation("add_states")>]
    member _.AddStates(menu: UssdMenuState, states: UssdState list) =

        addStates menu states

    [<CustomOperation("use_session_store")>]
    member _.UseSessionStore(menu: UssdMenuState, store: UssdSessionStore) =

        useSessionStore menu store

let ussdMenu = UssdMenuBuilder()

//let private getSession = UssdSession.getSession sessionStore
//let private setSession = UssdSession.setSession sessionStore

let private findState (states: UssdState list) stateName =

    match states
          |> List.tryFind (fun state -> state.Name = stateName) with
    | None -> states.[1]
    | Some state -> state

let private findNextState (states: UssdState list) stateName userText =

    let state = findState states stateName

    let state =
        match state.Next.TryFind(userText) with
        | None -> state
        | Some state -> state

    state

let private runState (setSession: UssdSession -> Async<UssdSession>)
                     (context: UssdContext, state: UssdState)
                     : Async<UssdResult> =
    async {
        let session = context.Session

        let! result = state.Run(context)

        let session =
            match result.Type, session with
            | Response, session ->
                { session with
                      Status = Ongoing
                      CurrentState = state.Name }
            | Release, session ->
                { session with
                      Status = Terminated
                      CurrentState = state.Name }

        let! _ = setSession (session)

        return result
    }

let run (menu: UssdMenuState) (args: UssdArguments) =
    async {
        let sessionStore = menu.SessionStore
        let getSession = UssdSession.getSession sessionStore
        let setSession = UssdSession.setSession sessionStore

        let! session = getSession args.SessionId

        let context, state: UssdContext * UssdState =
            match session.Status with
            | Initiated ->
                let stateName = menu.StartState.Name
                let state = findState menu.States stateName

                { Args = args
                  Session =
                      { session with
                            CurrentState = state.Name } },
                state
            | Ongoing ->
                let stateName = session.CurrentState

                let nextState =
                    findNextState menu.States stateName args.Text

                { Args = args
                  Session =
                      { session with
                            CurrentState = nextState.Name } },
                nextState
            | Terminated -> menu.Context, menu.StartState

        let! result = runState setSession (context, state)
        return result.Message

    }
