[<AutoOpen>]
module Serialization

open System.Text.Json
open System.Text.Json.Serialization

let private options =
    
    let jsonConverter = JsonFSharpConverter(JsonUnionEncoding.InternalTag
    ||| JsonUnionEncoding.UnwrapFieldlessTags
    ||| JsonUnionEncoding.UnwrapOption)

    let options = JsonSerializerOptions(PropertyNamingPolicy = JsonNamingPolicy.CamelCase)

    options.Converters.Add(jsonConverter)

    options

let serialize<'data> (data: 'data) =
    JsonSerializer.Serialize(data,  options)

let deserialize<'data> (json: string): 'data =
    JsonSerializer.Deserialize<'data>(json, options)

//let tryDeserialize<'data> (json: string): Result<'data, string> =
//    match json with
//    | json when json |> String.IsNullOrWhiteSpace -> "json is empty" |> Error
//    | json ->
//        try
//            (deserialize<'data> json) |> Ok
//        with ex -> Error ex.Message
