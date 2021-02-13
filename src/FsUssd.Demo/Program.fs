// Learn more about F# at http://fsharp.org

open System
open FsUssd

[<EntryPoint>]
let main argv =

    let subscriptionStateRun (context: UssdContext) = async {

            return UssdResult.con (sprintf "Entrer votre nom:")
        }

    let subscriptionState = ussdState {
        name "Subscription"
        run subscriptionStateRun
        //next ([[""]])
    }

    let startStateRun (context: UssdContext) = async {

            return UssdResult.con (sprintf "1. Souscription \n2. Consultation de solde")
        }

    let startState = ussdState {
        name "Menu"
        run startStateRun
        next (Map.empty |> Map.add "1" subscriptionState)
    }

    let menu = ussdMenu {
        start_state startState
        add_state subscriptionState
    }

    let mutable userInput = ""

    while not (userInput = "quit") do

        Console.Write("Ussd Input: ");

        userInput <- Console.ReadLine()

        let args = userInput.Split("&")

        let args = {
            UssdArguments.PhoneNumber = args.[0]
            ServiceCode = "000"
            SessionId = "126"
            Text = args.[1]
        }

        let result = (run menu args) |> Async.RunSynchronously

        Console.WriteLine(result);

        ()

    0 // return an integer exit code
