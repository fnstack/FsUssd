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
    { Header: string
      Footer: string
      Separator: string
      Items: UssdMenuItem list
      EmptyLinesBetweenHeaderAndItems: int
      EmptyLinesBetweenFooterAndItems: int
      ZeroItem: UssdMenuItem }

let Empty =
    { Header = String.Empty
      Footer = String.Empty
      Separator = ")"
      Items = List.Empty
      EmptyLinesBetweenHeaderAndItems = 0
      EmptyLinesBetweenFooterAndItems = 0
      ZeroItem = UssdMenuItem.Empty }

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
    { state with ZeroItem = item }

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

    if (state.Header |> String.IsNullOrWhiteSpace |> not) then

        builder.AppendLine(state.Header) |> ignore

        if (state.EmptyLinesBetweenHeaderAndItems > 0) then
            [ 1 .. state.EmptyLinesBetweenHeaderAndItems ]
            |> List.iter (fun _ -> builder.AppendLine() |> ignore)
            |> ignore

    state.Items
    |> List.sortBy (fun item -> item.Position)
    |> List.iter (fun item ->
        builder.AppendLine((sprintf "%s%s %s" (item.Position.ToString()) state.Separator item.Display))
        |> ignore)

    if (state.ZeroItem = UssdMenuItem.Empty |> not) then

        builder.AppendLine(sprintf "0%s %s" state.Separator (state.ZeroItem.Display))
        |> ignore

    if (state.Footer |> String.IsNullOrWhiteSpace |> not) then

        if (state.EmptyLinesBetweenFooterAndItems > 0) then
            [ 1 .. state.EmptyLinesBetweenFooterAndItems ]
            |> List.iter (fun _ -> builder.AppendLine() |> ignore)
            |> ignore

        builder.AppendLine(state.Footer) |> ignore

    let result = builder.ToString()
    result

type UssdMenuBuilder internal () =

    member _.Yield(_) = Empty

    member __.Run(state: UssdMenu) = state |> render

    [<CustomOperation("set_header")>]
    member _.SetHeader(state, header) = setHeader state header

    [<CustomOperation("set_footer")>]
    member _.SetFooter(state, footer) = setFooter state footer

    [<CustomOperation("set_separator")>]
    member _.SetSeparator(state, separator) = setSeparator state separator

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
