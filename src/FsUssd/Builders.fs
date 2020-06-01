[<AutoOpen>]
module FsUssd.Builders

type UssdMenuBuilder internal () =

    member _.Yield(_) = UssdMenu.Empty

    member __.Run(state: UssdMenu) = state |> UssdMenu.render

    [<CustomOperation("set_header")>]
    member _.SetHeader(state, header) = UssdMenu.setHeader header state

    [<CustomOperation("set_footer")>]
    member _.SetFooter(state, footer) = UssdMenu.setFooter footer state

    [<CustomOperation("set_separator")>]
    member _.SetSeparator(state, separator) = UssdMenu.setSeparator separator state

    [<CustomOperation("add_item")>]
    member _.AddItem(state, display) = UssdMenu.addItem display state

    [<CustomOperation("add_zero_item")>]
    member _.AddZeroItem(state, display) = UssdMenu.addZeroItem display state

let ussdMenu = UssdMenuBuilder()
