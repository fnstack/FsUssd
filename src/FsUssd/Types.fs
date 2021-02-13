namespace FsUssd

type Store = {
    SetValue: string * string -> Async<unit>
    GetValue: string -> Async<string option>
    ValueExists: string -> Async<bool>
    DeleteValue: string -> Async<unit>
}


