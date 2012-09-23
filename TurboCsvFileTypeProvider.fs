module TurboCsvFileTypeProvider

open System
open System.Reflection
open System.IO
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Text.RegularExpressions

// Simple type wrapping CSV data
type CsvFile(filename) =
    // Cache the sequence of all data lines (all lines but the first)
    let data = 
        seq { for line in File.ReadAllLines(filename) |> Seq.skip 1 do
                yield line.Split(',') |> Array.map string }
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
            | (true, x) -> typeof<Int32>
            | _ -> match Decimal.TryParse(value) with
                   | (true, x) -> typeof<Decimal>
                   | _ -> match DateTime.TryParse(value) with
                          | (true, x) -> typeof<DateTime>
                          | _ -> typeof<string>

        let inferedTypes = firstLineFields |> Seq.map inferType
        
        let getterCode (fieldTy : Type) i =
            match fieldTy.Name with
            | "Int32" -> fun [row] -> <@@ Int32.Parse((%%row:string[]).[i]) @@>
            | "Decimal" -> fun [row] -> <@@ Decimal.Parse((%%row:string[]).[i]) @@>
            | "DateTime" -> fun [row] -> <@@ DateTime.Parse((%%row:string[]).[i]) @@>
            | _ -> fun [row] -> <@@(%%row:string[]).[i] @@>

        headers 
        |> Seq.zip inferedTypes
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

