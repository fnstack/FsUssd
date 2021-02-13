// Learn more about F# at http://fsharp.org

open System
open FsUssd

[<EntryPoint>]
let main argv =

    let startStateRun context = async {
            return ()
        }

    let state = ussdState {
        name "Menu"
        run startStateRun
    }

    let menu = ussdMenu {
        start_state state
    }

    let args = {
        UssdArguments.PhoneNumber = "242069753244"
        ServiceCode = "000"
        SessionId = "126"
        Text = ""
    }

    let result = (run menu args) |> Async.RunSynchronously

    0 // return an integer exit code
