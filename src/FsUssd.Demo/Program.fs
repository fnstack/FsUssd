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
                        do! context.SetValue("NAME", "MOUNGUENGUE")

                        return
                            UssdResult.con (
                                sprintf "1. Souscription\n2. Paiement de cotisation\n00. Retour au menu principal"
                            )
                    })

            add_next_entry "1" "Subscription.LastName"
            add_next_entry "(http:\/\/\S+)" "Subscription.Regex"
        }

    let menu =
        ussdMenu {
            start_state mainState

            add_states [ subscriptionLastNameState
                         regexState
                         subscriptionFirstNameState ]
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
