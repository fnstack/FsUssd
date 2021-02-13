[<AutoOpen>]
module FsUssd.MemoryStore

open System
open LazyCache

let private cache = CachingService()

let private deleteValue key = async { return key |> cache.Remove }

let private setValue (key, value) =
    async {
        key |> cache.Remove
        return cache.Add<string>(key, value, TimeSpan.FromSeconds(60.))
    }

let private getValue key =
    async {
        match! key |> cache.GetAsync<string> |> Async.AwaitTask with
        | value when value |> String.IsNullOrWhiteSpace -> return None
        | value -> return value |> Some
    }

let private isValueExists key =
    async {
        match! key |> getValue with
        | None -> return false
        | Some _ -> return true
    }

let memoryStore = {
    Store.ValueExists = isValueExists
    SetValue = setValue
    GetValue = getValue
    DeleteValue = deleteValue
}
