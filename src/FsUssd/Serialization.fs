[<AutoOpen>]
module Serialization

open System
open FsCodec.NewtonsoftJson
open Newtonsoft.Json

let private fableConverter = Fable.JsonConverter() :> JsonConverter

let private settings =
    Settings.Create([| fableConverter |], camelCase = true)

let serialize<'data> (data: 'data) = Serdes.Serialize(data, settings)

let deserialize<'data> (json: string): 'data = Serdes.Deserialize(json, settings)

let tryDeserialize<'data> (json: string): Result<'data, string> =
    match json with
    | json when json |> String.IsNullOrWhiteSpace -> "json is empty" |> Error
    | json ->
        try
            (deserialize<'data> json) |> Ok
        with ex -> Error ex.Message
