namespace FsUssd

type Store = {
    SetValue: string * string -> Async<unit>
    GetValue: string -> Async<string>
    ValueExists: string -> Async<bool>
    DeleteValue: string -> Async<unit>
}

type UssdArguments = {
    SessionId: string
    ServiceCode: string
    PhoneNumber: string
    Text: string
}

//type UssdSession = {
    
//}

type UssdContext = {
    Args: UssdArguments
}
