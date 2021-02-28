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
          States =
              state
              :: (menu.States
                  |> List.filter (fun existingState -> existingState.Name <> state.Name)) }

let private addStates menu states =
    let stateNames =
        states |> List.map (fun state -> state.Name)

    { menu with
          States =
              states
              @ (menu.States
                 |> List.filter
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

// RUM IMPLEMENTATION

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

let private runState (getSession: string -> Async<UssdSession>)
                     (context: UssdContext, state: UssdState)
                     : Async<UssdResult * UssdSession> =
    async {
        let! result = state.Run(context)

        let! newSession = getSession context.Session.SessionId

        let session =
            match result.Type, newSession with
            | Response, session ->
                { session with
                      Status = Ongoing
                      CurrentState = state.Name }
            | Release, session ->
                { session with
                      Status = Terminated
                      CurrentState = state.Name }

        //let! _ = setSession (session)

        return result, session
    }

let run (menu: UssdMenuState) (args: UssdArguments) =
    async {
        let sessionStore = menu.SessionStore
        let getSession = UssdSession.getSession sessionStore
        let setSession = UssdSession.setSession sessionStore

        let! session = getSession args.SessionId

        let setSessionValue =
            UssdSession.setSessionValue sessionStore session

        let getSessionValue =
            UssdSession.getSessionValue sessionStore session

        let context, state: UssdContext * UssdState =
            match session.Status with
            | Initiated ->
                let stateName = menu.StartState.Name
                let state = findState menu.States stateName

                { UssdContext.empty with
                      Args = args
                      SetValue = setSessionValue
                      GetValue = getSessionValue
                      Session =
                          { session with
                                CurrentState = state.Name } },
                state
            | Ongoing ->
                let stateName = session.CurrentState

                let nextState =
                    findNextState menu.States stateName args.Text

                { UssdContext.empty with
                      Args = args
                      SetValue = setSessionValue
                      GetValue = getSessionValue
                      Session =
                          { session with
                                CurrentState = nextState.Name } },
                nextState
            | Terminated -> menu.Context, menu.StartState

        let! result, session = runState getSession (context, state)

        let! _ = setSession (session)
        return result.Message

    }
