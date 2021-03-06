[<AutoOpen>]
module SubscriptionState

open System
open FsUssd

let private subscriptionLastNameStateRun (context: UssdContext) =
    async {

        let! name = context.GetValue "NAME"

        printfn "%A" name

        return UssdResult.con (sprintf "Entrer votre nom:")
    }

let private subscriptionFirstNameStateRun (context: UssdContext) =
    async {

        return UssdResult.con (sprintf "Entrer votre prenom:")
    }

let subscriptionFirstNameState =
    ussdState {
        name "Subscription.FirstName"
        run subscriptionFirstNameStateRun
    //next ([[""]])
    }

let subscriptionLastNameState =
    ussdState {
        name "Subscription.LastName"
        run subscriptionLastNameStateRun

        next (
            Map.empty
            |> Map.add "john" subscriptionFirstNameState
        )
    }

let regexState =
    ussdState {
        name "Subscription.Regex"
        run (fun _ -> async { return UssdResult.terminate (sprintf "REGEX") })
    }
