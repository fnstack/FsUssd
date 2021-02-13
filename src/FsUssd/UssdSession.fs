[<AutoOpen>]
module FsUssd.UssdSession

type UssdArguments = {
    SessionId: string
    ServiceCode: string
    PhoneNumber: string
    Text: string
}

[<CLIMutable>]
type UssdSession = {
    SessionId: string
    Values: Map<string, string>
}

module UssdSession = 
    let getSession (store: Store) =
        fun sessionId -> async {
            match! sessionId |> store.GetValue with
            | None ->
                let session = {UssdSession.SessionId = sessionId; Values = Map.empty.Add("CREATED_AT", System.DateTime.Now.ToString())}

                let data = serialize(session)

                do! store.SetValue(sessionId, data)

                return session

            | Some session ->
                let! session = deserialize(session)

                return session
        }
