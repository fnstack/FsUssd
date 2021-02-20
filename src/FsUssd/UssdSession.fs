[<AutoOpen>]
module FsUssd.UssdSession

open System
open LazyCache

type UssdArguments = {
    SessionId: string
    ServiceCode: string
    PhoneNumber: string
    Text: string
}

module UssdArguments =
    let empty = {
        SessionId = String.Empty
        ServiceCode = String.Empty
        PhoneNumber = String.Empty
        Text = String.Empty
    }

type UssdResultType =
    | Response
    | Release

type UssdResult = {
    Message: string
    Type: UssdResultType
}

module UssdResult =
    let con message : UssdResult = {
        Message = message
        Type = UssdResultType.Response
    }

    let terminate message : UssdResult = {
        Message = message
        Type = UssdResultType.Release
    }
        
type UssdSessionStatus =
    | Initiated
    | Ongoing
    | Terminated
    //| Timeout

type UssdSession = {
    SessionId: string
    CurrentState: string
    Status: UssdSessionStatus
    DataBag: Map<string, string>
}

type UssdSessionStore = {
    SetSession: string * UssdSession -> Async<unit>
    GetSession: string -> Async<UssdSession option>
    IsSessionExists: string -> Async<bool>
    DeleteSession: string -> Async<unit>
}

module UssdSession =
    let empty = {
        SessionId = String.Empty
        CurrentState = String.Empty
        Status = UssdSessionStatus.Initiated
        DataBag = Map.empty
    }

    let getSession (store: UssdSessionStore) =
        fun sessionId -> async {
            
            match! sessionId |> store.GetSession with
            | None ->
                let session = { empty with SessionId = sessionId }

                do! store.SetSession(sessionId, session)

                return session

            | Some session ->

                return session
        }

    let setSession (store: UssdSessionStore) =
        fun (session: UssdSession) -> async {
            match! session.SessionId |> store.IsSessionExists with
            | true ->
                do! store.SetSession(session.SessionId, session)

                return session

            | false ->

                return session
        }

/// CACHING

let private sessionCache = CachingService()

let private deleteSession sessionId = async { return sessionId |> sessionCache.Remove }

let private setSession (sessionId, session: UssdSession) =
    async {
        sessionId |> sessionCache.Remove
        return sessionCache.Add<UssdSession>(sessionId, session, TimeSpan.FromSeconds(600.))
    }

let private getSession sessionId =
    async {
        try
            match! sessionId |> sessionCache.GetAsync<UssdSession> |> Async.AwaitTask with
            | session when session = UssdSession.empty -> return Some session
            | session -> return Some session
        with
            | :? NullReferenceException ->
                return None
            | :? ArgumentOutOfRangeException ->
                return None
    }

let private isSessionExists key =
    async {
        match! key |> getSession with
        | None -> return false
        | Some _ -> return true
    }

let sessionStore : UssdSessionStore = {
    SetSession = setSession
    GetSession = getSession
    IsSessionExists = isSessionExists
    DeleteSession = deleteSession
}
