# Basic setup

## Config swagger til å kjøre på http lokalt
Høyre trykk solution filen i file explorer inne i visual studio.
Deretter trykk Properties > Debug > Open debug launch profiles UI
Sett launch browser til på og sett App URL til http://localhost:5000

## Koble til MongoDB
Gå till appsetting.json og edit apiUrl til database Uri'en din.
Kjør ```dotnet add package MongoDB.Driver``` for å laste ned mongoDB sine drivers og bli kvitt feilmedlingene dine.

## Rediger database og collections
Tester database og collection er kodet inn som dette (linje nr før koden):
```csharp
12  private readonly IMongoCollection<BsonDocument> _testCollection;
20 var userDatabase = client.GetDatabase("databaseName");
21 _testCollection = userDatabase.GetCollection<BsonDocument>("collectionName");
```

Hvis du skal legge til en collection eller database må du bare duplisere den relevante koden.
For å lage en ny collection må du også lage en ny instanse av den som det er gjort på linje 12.
