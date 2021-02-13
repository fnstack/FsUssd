[<AutoOpen>]
module FsUssd.UssdMenu

open System
open System.Text

type UssdMenuItem = { Position: int; Display: string }

module UssdMenuItem =

    let Empty =
        { UssdMenuItem.Display = String.Empty
          Position = 0 }

    let create pos display =
        { UssdMenuItem.Display = display
          Position = pos }

type UssdMenu =
    { Header: string option
      Footer: string option
      Separator: string
      Items: UssdMenuItem list
      EmptyLinesBetweenHeaderAndItems: int option
      EmptyLinesBetweenFooterAndItems: int option
      ZeroItem: UssdMenuItem option }

let Empty =
    { Header = None
      Footer = None
      Separator = "."
      Items = List.Empty
      EmptyLinesBetweenHeaderAndItems = None
      EmptyLinesBetweenFooterAndItems = None
      ZeroItem = None }

let setHeader state header = { state with Header = header }

let setFooter state footer = { state with Footer = footer }

let setSeparator state separator = { state with Separator = separator }

let addItem state display =
    let item =
        display
        |> UssdMenuItem.create (state.Items.Length + 1)

    { state with
          Items = item :: state.Items }

let addZeroItem state display =
    let item = display |> UssdMenuItem.create 0
    { state with ZeroItem = Some item }

let addItems state (items: UssdMenuItem list) =

    { state with
          Items = items @ state.Items }

let addEmptyLinesBetweenHeaderAndItems state lines =
    { state with
          EmptyLinesBetweenHeaderAndItems = lines }

let addEmptyLinesBetweenFooterAndItems state lines =
    { state with
          EmptyLinesBetweenFooterAndItems = lines }

let render (state: UssdMenu) =
    let builder = new StringBuilder()

    let setHeader header = header |> builder.AppendLine |> ignore

    match state.Header, state.EmptyLinesBetweenHeaderAndItems with
    | Some header, None ->
        header |> setHeader
    | Some header, Some emptyLinesBetweenHeaderAndItems when emptyLinesBetweenHeaderAndItems > 0 ->
        header |> setHeader

        [ 1 .. emptyLinesBetweenHeaderAndItems ]
        |> List.iter (fun _ -> builder.AppendLine() |> ignore)
        |> ignore
    | _, _ -> ()

    state.Items
    |> List.sortBy (fun item -> item.Position)
    |> List.iter (fun item ->
        builder.AppendLine((sprintf "%s%s %s" (item.Position.ToString()) state.Separator item.Display))
        |> ignore)

    match state.ZeroItem, state.Footer, state.EmptyLinesBetweenFooterAndItems with
    | Some zeroItem, None, None ->
        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display))
        |> ignore
    | Some zeroItem, Some footer, None ->
        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display)) |> ignore

        builder.AppendLine(footer) |> ignore
    | Some zeroItem, Some footer, Some emptyLinesBetweenFooterAndItems when emptyLinesBetweenFooterAndItems > 0 ->
        builder.AppendLine(sprintf "0%s %s" state.Separator (zeroItem.Display)) |> ignore

        [ 1 .. emptyLinesBetweenFooterAndItems ]
        |> List.iter (fun _ -> builder.AppendLine() |> ignore)
        |> ignore
        
        builder.AppendLine(footer) |> ignore
    | _, _, _ -> ()

    let result = builder.ToString()
    result

type UssdMenuBuilder internal () =

    member _.Yield(_) = Empty

    member __.Run(state: UssdMenu) = state |> render

    [<CustomOperation("set_header")>]
    member _.SetHeader(state, header: string) =
        match header.Trim() with
        | header when header |> String.IsNullOrWhiteSpace ->
            state
        | header ->
            setHeader state (Some header)

    [<CustomOperation("set_footer")>]
    member _.SetFooter(state, footer: string) =
        match footer.Trim() with
        | footer when footer |> String.IsNullOrWhiteSpace ->
            state
        | footer ->
            setFooter state (Some footer)

    [<CustomOperation("set_separator")>]
    member _.SetSeparator(state, separator: string) =
        match separator.Trim() with
        | separator when separator |> String.IsNullOrWhiteSpace ->
            state
        | separator ->
            setSeparator state separator

    [<CustomOperation("add_item")>]
    member _.AddItem(state, display) = addItem state display

    [<CustomOperation("add_items")>]
    member _.AddItems(state, items) = addItems state items

    [<CustomOperation("add_zero_item")>]
    member _.AddZeroItem(state, display) = addZeroItem state display

    [<CustomOperation("add_empty_lines_between_header_and_items")>]
    member _.AddEmptyLinesBetweenHeaderAndItems(state, lines) =
        addEmptyLinesBetweenHeaderAndItems state lines

    [<CustomOperation("add_empty_lines_between_footer_and_items")>]
    member _.AddEmptyLinesBetweenFooterAndItems(state, lines) =
        addEmptyLinesBetweenFooterAndItems state lines

let ussdMenu = UssdMenuBuilder()
