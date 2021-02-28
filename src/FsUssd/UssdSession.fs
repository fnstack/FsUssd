[<AutoOpen>]
module FsUssd.UssdSession

open System
open LazyCache

type UssdArguments =
    { SessionId: string
      ServiceCode: string
      PhoneNumber: string
      Text: string }

module UssdArguments =
    let empty =
        { SessionId = String.Empty
          ServiceCode = String.Empty
          PhoneNumber = String.Empty
          Text = String.Empty }

type UssdResultType =
    | Response
    | Release

type UssdResult =
    { Message: string
      Type: UssdResultType }

module UssdResult =
    let con message: UssdResult =
        { Message = message
          Type = UssdResultType.Response }

    let terminate message: UssdResult =
        { Message = message
          Type = UssdResultType.Release }

type UssdSessionStatus =
    | Initiated
    | Ongoing
    | Terminated
//| Timeout

type UssdSession =
    { SessionId: string
      CurrentState: string
      Status: UssdSessionStatus
      DataBag: Map<string, string> }

type UssdSessionStore =
    { SetSession: UssdSession -> Async<unit>
      GetSession: string -> Async<UssdSession option>
      IsSessionExists: string -> Async<bool>
      DeleteSession: string -> Async<unit> }

module UssdSession =
    let empty =
        { SessionId = String.Empty
          CurrentState = String.Empty
          Status = UssdSessionStatus.Initiated
          DataBag = Map.empty }

    let getSession (store: UssdSessionStore) =
        fun sessionId ->
            async {
                match! sessionId |> store.GetSession with
                | None ->
                    let session = { empty with SessionId = sessionId }

                    do! store.SetSession(session)

                    return session

                | Some session ->

                    return session
            }

    let setSession (store: UssdSessionStore) =
        fun (session: UssdSession) ->
            async {
                match! session.SessionId |> store.IsSessionExists with
                | true ->
                    do! store.SetSession(session)

                    return session

                | false ->

                    return session

            //do! store.SetSession(session)

            //return session
            }

    let setSessionValue (store: UssdSessionStore) (session: UssdSession) =
        fun (key, value) ->
            async {
                let dataBag =
                    match session.DataBag.TryFind key with
                    | None -> session.DataBag |> Map.add key value
                    | Some _ ->
                        session.DataBag
                        |> Map.remove key
                        |> Map.add key value

                let session = { session with DataBag = dataBag }

                do! store.SetSession(session)

                return ()
            }

    let getSessionValue (store: UssdSessionStore) (session: UssdSession) =
        fun key ->
            async {
                match! session.SessionId |> store.GetSession with
                | None -> return None

                | Some session ->

                    let value = session.DataBag |> Map.tryFind key

                    return value
            }

/// CACHING

let private sessionMemoryCache = CachingService()

let private deleteSessionInMemoryCache sessionId =
    async { return sessionId |> sessionMemoryCache.Remove }

let private setSessionInMemoryCache (expiresInSeconds: float) (session: UssdSession) =
    async {
        let sessionId = session.SessionId
        sessionId |> sessionMemoryCache.Remove
        sessionMemoryCache.Add<UssdSession>(sessionId, session, TimeSpan.FromSeconds(expiresInSeconds))

        let t = sessionMemoryCache

        return ()
    }

let private getSessionInMemoryCache sessionId =
    async {
        try
            match! sessionId
                   |> sessionMemoryCache.GetAsync<UssdSession>
                   |> Async.AwaitTask with
            | session when session = UssdSession.empty -> return Some session
            | session -> return Some session
        with
        | :? NullReferenceException
        | :? ArgumentOutOfRangeException -> return None
    }

let private isSessionExistsInMemoryCache sessionId =
    async {
        match! sessionId |> getSessionInMemoryCache with
        | None -> return false
        | Some _ -> return true
    }

let memorySessionStore: UssdSessionStore =
    { SetSession = setSessionInMemoryCache (600.)
      GetSession = getSessionInMemoryCache
      IsSessionExists = isSessionExistsInMemoryCache
      DeleteSession = deleteSessionInMemoryCache }
