#r @".\bin\Debug\TurboCsvFileTypeProvider.dll"
let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
csv.Data |> Seq.iter (fun x -> printfn "%A %A %A %A" x.``int field`` x.``decimal field`` x.``date field`` x.``string field``)

