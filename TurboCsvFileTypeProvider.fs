module TurboCsvFileTypeProvider

open System
open System.Reflection
open System.IO
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

// Simple type wrapping CSV data
type CsvFile(filename) =
    // Cache the sequence of all data lines (all lines but the first)
    let data = 
        seq { for line in File.ReadAllLines(filename) |> Seq.skip 1 do
                yield 
                    if (line.Length > 0) 
                    then 
                        line.Split(',') |> Array.map string 
                    else
                        Array.empty
            }
        |> Seq.cache
    member __.Data = data

[<TypeProvider>]
type public TurboCsvFileTypeProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to house the provided types
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let ns = "TurboCsvFileTypeProvider"

    // Create the main provided type
    let csvTy = ProvidedTypeDefinition(asm, ns, "TurboCsv", Some(typeof<obj>))

    // Parameterize the type by the file to use as a template
    let filename = ProvidedStaticParameter("filename", typeof<string>)
    do csvTy.DefineStaticParameters([filename], fun tyName [| :? string as filename |] ->

        let resolvedFilename = Path.Combine(cfg.ResolutionFolder, filename)
        
        let rowTy = ProvidedTypeDefinition("Row", Some(typeof<string[]>))

        
        let headers = (File.ReadLines(resolvedFilename) |> Seq.head).Split ','
        let firstLineFields = (File.ReadLines(resolvedFilename) |> Seq.nth 1).Split ','

        let inferType value =
            match Int32.TryParse(value) with
            | (true, x) -> typeof<option<Int32>>
            | _ -> match Decimal.TryParse(value) with
                   | (true, x) -> typeof<option<Decimal>>
                   | _ -> match DateTime.TryParse(value) with
                          | (true, x) -> typeof<option<DateTime>>
                          | _ -> typeof<option<string>>

        let inferredTypes = firstLineFields |> Seq.map inferType
        
        let getterCode (fieldTy : Type) i =
            match (fieldTy.GetGenericArguments().[0]).Name with
            | "Int32" -> fun [row] -> <@@ if (%%row:string[]).Length>i then Some(Int32.Parse((%%row:string[]).[i])) else None @@>
            | "Decimal" -> fun [row] -> <@@ if (%%row:string[]).Length>i then Some(Decimal.Parse((%%row:string[]).[i])) else None @@>
            | "DateTime" -> fun [row] -> <@@ if (%%row:string[]).Length>i then Some(DateTime.Parse((%%row:string[]).[i])) else None @@>
            | _ -> fun [row] -> <@@ if (%%row:string[]).Length>i then Some((%%row:string[]).[i]) else None @@>

        headers 
        |> Seq.zip inferredTypes
        |> Seq.mapi 
            (fun i x -> ProvidedProperty((snd x), (fst x), GetterCode = (getterCode (fst x) i)))
        |> Seq.iter rowTy.AddMember
   
        // define the provided type, erasing to CsvFile
        let ty = ProvidedTypeDefinition(asm, ns, tyName, Some(typeof<CsvFile>))

        // add a parameterless constructor which loads the file that was used to define the schema
        ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ CsvFile(resolvedFilename) @@>))

        // add a constructor taking the filename to load
        ty.AddMember(ProvidedConstructor([ProvidedParameter("filename", typeof<string>)], InvokeCode = fun [filename] -> <@@ CsvFile(%%filename) @@>))
        
        // add a new, more strongly typed Data property (which uses the existing property at runtime)
        ty.AddMember(ProvidedProperty("Data", typedefof<seq<_>>.MakeGenericType(rowTy), GetterCode = fun [csvFile] -> <@@ (%%csvFile:CsvFile).Data @@>))

        // add the row type as a nested type
        ty.AddMember(rowTy)
        ty)

    // add the type to the namespace
    do this.AddNamespace(ns, [csvTy])

[<TypeProviderAssembly>]
do()

