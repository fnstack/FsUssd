// Learn more about F# at http://fsharp.org

open System
open FsUssd

[<EntryPoint>]
let main argv =

    let mainState =
        ussdState {
            name "Menu"

            run
                (fun (context: UssdContext) ->
                    async {
                        return
                            UssdResult.con (
                                sprintf "1. Souscription\n2. Paiement de cotisation\n00. Retour au menu principal"
                            )
                    })

            next (
                Map.empty
                |> Map.add "1" subscriptionLastNameState
                |> Map.add "2" subscriptionLastNameState
            )
        }

    let menu =
        ussdMenu {
            start_state mainState
            add_state subscriptionLastNameState
            add_state subscriptionFirstNameState
        }

    let mutable userInput = ""

    while not (userInput = "quit") do

        Console.Write("Ussd Input: ")

        userInput <- Console.ReadLine()

        let args = userInput.Split("&")

        let args =
            { UssdArguments.PhoneNumber = args.[0]
              ServiceCode = "000"
              SessionId = "126"
              Text = args.[1] }

        let result =
            (run menu args) |> Async.RunSynchronously

        Console.WriteLine(result)

        ()

    0 // return an integer exit code
