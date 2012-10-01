module Tests
open System
open NUnit.Framework
open NUnit.Mocks
open TurboCsvFileTypeProvider

let correctDates = [Some(new DateTime(2012, 1, 1)); 
                    Some(new DateTime(2012, 1, 1)); 
                    Some(new DateTime(2012, 1, 2)); 
                    Some(new DateTime(2012, 1, 2))]
let correctLocationIds = [Some(35); Some(289); Some(289); Some(35)]

[<TestFixture>]
type TestWithFullyPopulatedValidFile() = 
    
    [<Test>]
    member this.``Given a fully populated file, the type provider gets all lines``() = 
        let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
        Assert.AreEqual(4, csv.Data |> Seq.length) 
    
    [<Test>]
    member this.``Given a fully populated file, the type provider gets correct dates``() = 
        let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
        let parsedDates = csv.Data |> Seq.map (fun x -> x.Date) |> Seq.toList
        parsedDates 
        |> Seq.zip correctDates
        |> Seq.iter (fun x -> Assert.AreEqual((fst x), (snd x))) 
    
    [<Test>]
    member this.``Given a fully populated file, the type provider gets correct locationIds``() = 
        let csv = new TurboCsvFileTypeProvider.TurboCsv<"test.csv">()
        let parsedLocationIds = csv.Data |> Seq.map (fun x -> x.LocationId) |> Seq.toList
        parsedLocationIds 
        |> Seq.zip correctLocationIds
        |> Seq.iter (fun x -> Assert.AreEqual((fst x), (snd x))) 
    