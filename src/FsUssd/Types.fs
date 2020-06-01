namespace FsUssd

open System

type UssdRequestTypes =
    | Initiation
    | Response
    | Release
    | Timeout

module UssdRequestTypes =

    let create (value : string) =
        match value.ToLower() with
        | "initiation" -> UssdRequestTypes.Initiation
        | "response" -> UssdRequestTypes.Response
        | "release" -> UssdRequestTypes.Release
        | _ -> UssdRequestTypes.Timeout

type UssdResponseTypes =
    | Response
    | Release

module UssdResponseTypes =

    let toString (value : UssdResponseTypes) =
        match value with
        | Response -> "Response"
        | Release -> "Release"

type UssdRequest = {
        Mobile : string
        SessionId : string
        ServiceCode : string
        Type : string
        Message : string
        Operator : string
    }

type UssdResponse = {
        Type : string
        Message : string
        ClientState : string
        Exception : Exception
        NextRoute : string
        IsRedirect : bool
    }

module UssdResponse =

    let isRelease (response : UssdResponse) =
        response.NextRoute |> String.IsNullOrWhiteSpace

    let render message nextRoute =
        let responseType = (match nextRoute |> String.IsNullOrWhiteSpace with true -> UssdResponseTypes.Release | false -> UssdResponseTypes.Response) |> UssdResponseTypes.toString

        {UssdResponse.NextRoute = nextRoute; Message = message; Type = responseType; ClientState = String.Empty; IsRedirect = false; Exception = null}

    let redirect nextRoute =
        {UssdResponse.NextRoute = nextRoute; Message = String.Empty; Type = String.Empty; ClientState = String.Empty; IsRedirect = true; Exception = null}

    let setException error state =
        {state with Exception = error}
