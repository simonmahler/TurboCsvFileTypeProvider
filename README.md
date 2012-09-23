TurboCsvFileTypeProvider
=======
Modification of sample [csv file type provider](http://fsharp3sample.codeplex.com/SourceControl/changeset/view/8670#195462) by Microsft. Compared to the original MiniScvProvider, TurboCsvFileTypeProvider supports other data types than just floats. The provider uses the first line of data to infer the types of each column, currently supporting int, decimal, string and DateTime.


How do I use it?
================
This type provider gives you access to data stored in a csv file.
Consider the following csv file (test.csv):

````
Date,LocationId,Temperature,Description
2012-1-1,35,12.3,sunny
2012-1-1,289,-0.4,sunny
2012-1-2,289,34.2,rainy
2012-1-2,35,31.1,snowy
````
We can now read in the file using the following code:

````
#r @".\bin\Debug\TurboCsvFileTypeProvider.dll"
let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
csv.Data 
|> Seq.iter (fun x -> printfn "%s %d %f %s" (x.Date.ToShortDateString()) x.LocationId x.Temperature x.Description)
````

License
=======
The original mini csv provider type and ProvidedTypes-0.2.fs by Microsoft are covered by [Apache License 2.0] (http://fsharp3sample.codeplex.com/license).

Pre-requisites
==============
Visual Studio 2012, F# 3.0, .NET 4.5
