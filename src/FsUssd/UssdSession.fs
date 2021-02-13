[<AutoOpen>]
module FsUssd.UssdSession

type UssdArguments = {
    SessionId: string
    ServiceCode: string
    PhoneNumber: string
    Text: string
}

type UssdSession = {
    SessionId: string
    Values: (string * string) list
}

module UssdSession = 
    let getSession (store: Store) =
        fun sessionId -> async {
            match! sessionId |> store.GetValue with
            | None ->
                let session = {UssdSession.SessionId = sessionId; Values = []}

                let data = serialize(session)

                do! store.SetValue(sessionId, data)

                return session

            | Some session ->
                let! session = deserialize(session)

                return session
        }
