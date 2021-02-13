namespace FsUssd

//type UssdRequestType =
//    | Initiation
//    | Response
//    | Release
//    | Timeout

//module UssdRequestType =

//    let create (value: string) =
//        match value.ToLower().Trim() with
//        | "initiation" -> UssdRequestType.Initiation
//        | "response" -> UssdRequestType.Response
//        | "release" -> UssdRequestType.Release
//        | _ -> UssdRequestType.Timeout

//type UssdRequest =
//    { Msisdn: string
//      SessionId: string
//      ServiceCode: string
//      Type: UssdRequestType
//      Input: string }
