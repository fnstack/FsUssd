namespace FsUssd

open System
open System.Text

////////////////////////////////////////////////////////////////////////////////////////////////////
// USSD MENUS
////////////////////////////////////////////////////////////////////////////////////////////////////

type UssdMenuItem =
    { Display: string }

module UssdMenuItem =

    let Empty = { UssdMenuItem.Display = String.Empty }
    let create display = { UssdMenuItem.Display = display }

type UssdMenu =
    { Header: string
      Footer: string
      Separator: string
      Items: UssdMenuItem list
      ZeroItem: UssdMenuItem }

module UssdMenu =

    let Empty =
        { Header = String.Empty
          Footer = String.Empty
          Separator = ")"
          Items = List.Empty
          ZeroItem = UssdMenuItem.Empty }

    let setHeader header state = { state with Header = header }

    let setFooter footer state = { state with Footer = footer }

    let setSeparator separator state = { state with Separator = separator }

    let addItem display state =
        let item = display |> UssdMenuItem.create
        { state with Items = item :: state.Items }

    let addZeroItem display state =
        let item = display |> UssdMenuItem.create
        { state with ZeroItem = item }

    let render (state: UssdMenu) =
        let builder = new StringBuilder()

        if (state.Header
            |> String.IsNullOrWhiteSpace
            |> not)
        then

            builder.AppendLine(state.Header) |> ignore

        state.Items
        |> List.iteri (fun i item ->
            builder.AppendLine((sprintf "%s%s %s" ((i + 1).ToString()) state.Separator item.Display)) |> ignore)

        if (state.ZeroItem = UssdMenuItem.Empty |> not) then

            builder.AppendLine(sprintf "0%s %s" state.Separator (state.ZeroItem.Display)) |> ignore

        if (state.Footer
            |> String.IsNullOrWhiteSpace
            |> not)
        then

            builder.AppendLine(state.Footer) |> ignore

        let result = builder.ToString()
        result
