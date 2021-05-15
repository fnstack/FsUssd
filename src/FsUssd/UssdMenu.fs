[<AutoOpen>]
module FsUssd.UssdMenu

open System
open System.Text.RegularExpressions

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD MENU ITEM
////////////////////////////////////////////////////////////////////////////////////////////////////

//type UssdMenuItem = { Position: int; Display: string }

//module UssdMenuItem =

//    let Empty =
//        { UssdMenuItem.Display = String.Empty
//          Position = 0 }

//    let create pos display =
//        { UssdMenuItem.Display = display
//          Position = pos }

//type UssdMenu =
//    { Header: string option
//      Footer: string option
//      Separator: string
//      Items: UssdMenuItem list
//      EmptyLinesBetweenHeaderAndItems: int option
//      EmptyLinesBetweenFooterAndItems: int option
//      ZeroItem: UssdMenuItem option }

//let Empty =
//    { Header = None
//      Footer = None
//      Separator = "."
//      Items = List.Empty
//      EmptyLinesBetweenHeaderAndItems = None
//      EmptyLinesBetweenFooterAndItems = None
//      ZeroItem = None }

//let setHeader state header = { state with Header = header }

//let setFooter state footer = { state with Footer = footer }

//let setSeparator state separator = { state with Separator = separator }

//let addItem state display =
//    let item =
//        display
//        |> UssdMenuItem.create (state.Items.Length + 1)

//    { state with
//          Items = item :: state.Items }

//let addZeroItem state display =
//    let item = display |> UssdMenuItem.create 0
//    { state with ZeroItem = Some item }

//let addItems state (items: UssdMenuItem list) =

//    { state with
//          Items = items @ state.Items }

//let addEmptyLinesBetweenHeaderAndItems state lines =
//    { state with
//          EmptyLinesBetweenHeaderAndItems = lines }

//let addEmptyLinesBetweenFooterAndItems state lines =
//    { state with
//          EmptyLinesBetweenFooterAndItems = lines }

//let render (state: UssdMenu) =
//    let builder = new StringBuilder()

//    let setHeader header = header |> builder.AppendLine |> ignore

//    match state.Header, state.EmptyLinesBetweenHeaderAndItems with
//    | Some header, None ->
//        header |> setHeader
//    | Some header, Some emptyLinesBetweenHeaderAndItems when emptyLinesBetweenHeaderAndItems > 0 ->
//        header |> setHeader

//        [ 1 .. emptyLinesBetweenHeaderAndItems ]
//        |> List.iter (fun _ -> builder.AppendLine() |> ignore)
//        |> ignore
//    | _, _ -> ()

//    state.Items
//    |> List.sortBy (fun item -> item.Position)
//    |> List.iter (fun item ->
//        builder.AppendLine((sprintf "%s%s %s" (item.Position.ToString()) state.Separator item.Display))
//        |> ignore)

//    match state.ZeroItem, state.Footer, state.EmptyLinesBetweenFooterAndItems with
//    | Some zeroItem, None, None ->
//        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display))
//        |> ignore
//    | Some zeroItem, Some footer, None ->
//        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display)) |> ignore

//        builder.AppendLine(footer) |> ignore
//    | Some zeroItem, Some footer, Some emptyLinesBetweenFooterAndItems when emptyLinesBetweenFooterAndItems > 0 ->
//        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display)) |> ignore

//        [ 1 .. emptyLinesBetweenFooterAndItems ]
//        |> List.iter (fun _ -> builder.AppendLine() |> ignore)
//        |> ignore

//        builder.AppendLine(footer) |> ignore
//    | _, _, _ -> ()

//    let result = builder.ToString()
//    result

//type UssdMenuBuilder internal () =

//    member _.Yield(_) = Empty

//    member __.Run(state: UssdMenu) = state |> render

//    [<CustomOperation("set_header")>]
//    member _.SetHeader(state, header: string) =
//        match header.Trim() with
//        | header when header |> String.IsNullOrWhiteSpace ->
//            state
//        | header ->
//            setHeader state (Some header)

//    [<CustomOperation("set_footer")>]
//    member _.SetFooter(state, footer: string) =
//        match footer.Trim() with
//        | footer when footer |> String.IsNullOrWhiteSpace ->
//            state
//        | footer ->
//            setFooter state (Some footer)

//    [<CustomOperation("set_separator")>]
//    member _.SetSeparator(state, separator: string) =
//        match separator.Trim() with
//        | separator when separator |> String.IsNullOrWhiteSpace ->
//            state
//        | separator ->
//            setSeparator state separator

//    [<CustomOperation("add_item")>]
//    member _.AddItem(state, display) = addItem state display

//    [<CustomOperation("add_items")>]
//    member _.AddItems(state, items) = addItems state items

//    [<CustomOperation("add_zero_item")>]
//    member _.AddZeroItem(state, display) = addZeroItem state display

//    [<CustomOperation("add_empty_lines_between_header_and_items")>]
//    member _.AddEmptyLinesBetweenHeaderAndItems(state, lines) =
//        addEmptyLinesBetweenHeaderAndItems state lines

//    [<CustomOperation("add_empty_lines_between_footer_and_items")>]
//    member _.AddEmptyLinesBetweenFooterAndItems(state, lines) =
//        addEmptyLinesBetweenFooterAndItems state lines

//let ussdMenu = UssdMenuBuilder()

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD MENU BUILDER
////////////////////////////////////////////////////////////////////////////////////////////////////

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

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD MENU RUN
////////////////////////////////////////////////////////////////////////////////////////////////////

let private findState (states: UssdState list) stateName =

    match states
          |> List.tryFind (fun state -> state.Name = stateName) with
    | None -> states.[1]
    | Some state -> state

let private findNextState (states: UssdState list) stateName userText =

    let state = findState states stateName

    let state =
        match state.Next with
        | next when next |> Map.isEmpty -> state
        | next when next.ContainsKey(userText) ->
            match next.TryGetValue userText with
            | true, state -> findState states state
            | false, _ -> state
        | next ->
            match next
                  |> Map.toList
                  |> List.tryFind (fun (key, _) -> Regex.IsMatch(userText, key)) with
            | None -> state
            | Some (_, state) -> findState states state

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

        return result, session
    }

let run (menu: UssdMenuState) (args: UssdArguments) =
    async {
        let sessionStore = menu.SessionStore
        let getSession = UssdSession.getSession sessionStore
        let setSession = UssdSession.setSession sessionStore
        let removeSession = UssdSession.removeSession sessionStore

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

        match session with
        | session when session.Status = UssdSessionStatus.Ongoing ->
            let! _ = setSession (session)
            ()
        | session when session.Status = UssdSessionStatus.Terminated ->
            let! _ = removeSession (session)
            ()
        | _ -> ()

        return result.Message

    }
