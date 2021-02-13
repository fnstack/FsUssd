namespace FsUssd

open System

//type UssdResponseType =
//    | Response
//    | Release

//module UssdResponseType =

//    let toString (value: UssdResponseType) =
//        match value with
//        | Response -> "Response"
//        | Release -> "Release"

//type UssdResponse =
//    { Type: string
//      Message: string
//      ClientState: string
//      Exception: Exception
//      NextRoute: string
//      IsRedirect: bool }

//module UssdResponse =

//    let isRelease (response: UssdResponse) = response.NextRoute |> String.IsNullOrWhiteSpace

//    let render message nextRoute =
//        let responseType =
//            (match nextRoute |> String.IsNullOrWhiteSpace with
//             | true -> UssdResponseType.Release
//             | false -> UssdResponseType.Response)
//            |> UssdResponseType.toString

//        { UssdResponse.NextRoute = nextRoute
//          Message = message
//          Type = responseType
//          ClientState = String.Empty
//          IsRedirect = false
//          Exception = null }

//    let redirect nextRoute =
//        { UssdResponse.NextRoute = nextRoute
//          Message = String.Empty
//          Type = String.Empty
//          ClientState = String.Empty
//          IsRedirect = true
//          Exception = null }

//    let setException error state = { state with Exception = error }
