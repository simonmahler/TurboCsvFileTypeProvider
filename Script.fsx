#r @".\bin\Debug\TurboCsvFileTypeProvider.dll"
let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
csv.Data |> Seq.iter (fun x -> printfn "%s %d %f %s" (x.Date.ToShortDateString()) x.LocationId x.Temperature x.Description)

