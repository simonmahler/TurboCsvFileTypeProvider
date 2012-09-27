TurboCsvFileTypeProvider
========================
Modification of sample [csv file type provider](http://fsharp3sample.codeplex.com/SourceControl/changeset/view/8670#195462) by Microsft. Compared to the original MiniScvProvider, TurboCsvFileTypeProvider supports other data types than just floats. The provider uses the first line of data to infer the types of each column, currently supporting the following types:
-int
-decimal
-string
-DateTime.

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
|> Seq.iter (fun x -> printfn "%s %d %f %s" (x.Date.Value.ToShortDateString()) x.LocationId.Value x.Temperature.Value x.Description.Value)
````

Note the use of the .Value property. This is because the provider specifies individual fields as options (nullable, for those not as familiar with F#).
When a field is missing, the value is simply None. It is important to realize the significance of the very first data row as any field that is missing will currently prevent the provider from corretly inferring the type.

What's next?
============
Using the first line of data to infer the type of each column is not exactly safe. Unfortunatelly, you will not be able to tell that an entire column fits the inferred type without reading in the entire column first. This may be undesirable when working with large files. The other option is to allow the user to specify the definition of each column explicitely.
That being said, the type provider as it is has been quite useful to me so far. Let me know if you have any suggestions or advice.

License
=======
The original mini csv provider type and ProvidedTypes-0.2.fs by Microsoft are covered by [Apache License 2.0] (http://fsharp3sample.codeplex.com/license).

Pre-requisites
==============
Visual Studio 2012, F# 3.0, .NET 4.5
