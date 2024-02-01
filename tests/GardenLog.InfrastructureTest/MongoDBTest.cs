using GardenLog.SharedKernel;
using GardenLog.SharedKernel.Interfaces;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PlantHarvest.Contract.Base;
using PlantHarvest.Contract.Commands;
using PlantHarvest.Contract.Enum;
using PlantHarvest.Domain.HarvestAggregate;
using PlantHarvest.Domain.HarvestAggregate.Events;
using PlantHarvest.Domain.HarvestAggregate.Events.Meta;
using System.Reflection;

namespace GardenLog.InfrastructureTest;

public class MongoDBTest
{

    [Fact]
    public void SplitHarvestAndPalntIntoSeparateCollections()
    {
        var client = new MongoClient("mongodb+srv://<user>:<pass>@gardenlog2024-cluster.v95zx5v.mongodb.net/");
        var database = client.GetDatabase("GardenLogDB");

        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            p.SetIgnoreExtraElements(true);
            p.UnmapMember(m => m.DomainEvents);
        });


        BsonClassMap.RegisterClassMap<HarvestCycle>(p =>
        {
            p.AutoMap();
            p.SetIgnoreExtraElements(true);
            p.SetDiscriminator("harvest-cycle");
            p.UnmapMember(m => m.Plants);
            //p.MapProperty(m => m.Plants).SetDefaultValue(new List<PlantHarvestCycle>());


            var nonPublicCtors = p.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            p.MapConstructor(longestCtor, p.ClassType.GetProperties().Where(c => c.Name != "Id" && c.Name != "Plants").Select(c => c.Name).ToArray());

        });

        BsonClassMap.RegisterClassMap<PlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);

            g.UnmapMember(m => m.GardenBedLayout);
            g.MapMember(m => m.PlantingMethod).SetSerializer(new EnumSerializer<PlantingMethodEnum>(BsonType.String));

            g.MapProperty(m => m.PlantCalendar).SetDefaultValue(new List<PlantSchedule>());
            //g.MapProperty(m => m.GardenBedLayout).SetDefaultValue(new List<GardenBedPlantHarvestCycle>());
            g.MapProperty(m => m.SpacingInInches).SetDefaultValue(0);
            g.MapProperty(m => m.PlantsPerFoot).SetDefaultValue(0.0);


            var nonPublicCtors = g.ClassType.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            var longestCtor = nonPublicCtors.OrderByDescending(ctor => ctor.GetParameters().Length).FirstOrDefault();
            g.MapConstructor(longestCtor, g.ClassType.GetProperties().Where(c => c.Name != "Id" && c.Name != "GardenBedLayout").Select(c => c.Name).ToArray());

        });


        BsonClassMap.RegisterClassMap<GardenBedPlantHarvestCycle>(g =>
        {
            g.AutoMap();
            g.SetIgnoreExtraElements(true);
        });


        // Get references to collections
        var gardenCollection = database.GetCollection<GardenBedPlantHarvestCycle>("GardenBedUsage-Collection");
        var plantCollection = database.GetCollection<PlantHarvestCycle>("PlantHarvestCycle-Collection");
        var harvestCollection = database.GetCollection<HarvestCycle>("HarvestCycle-Collection");
       

        var harvest = HarvestCycle.Create("testUser", "testGarden", DateTime.Now, null, "test note", "garden1");

        harvest.AddPlantHarvestCycle(new CreatePlantHarvestCycleCommand()
        {
            DesiredNumberOfPlants = 1,
            PlantId = "plant1",
            PlantName = "plant name 1",
        });

        // Insert author (without books)
        harvestCollection.InsertOne(harvest);

        // Insert books
        //plantCollection.InsertMany(harvest.Plants);



    }
    
    [Fact]
    public void SplitAuthorAndBooksIntoSeparateCollections()
    {
        // Connect to MongoDB
        var client = new MongoClient("mongodb+srv://glUser:Mglu123@gardenlog2024-cluster.v95zx5v.mongodb.net/");
        var database = client.GetDatabase("GardenLogDB");

        BsonClassMap.RegisterClassMap<BaseEntity>(p =>
        {
            p.AutoMap();
            p.SetIgnoreExtraElements(true);
            p.UnmapMember(m => m.DomainEvents);
        });

        // Configure BsonClassMap to ignore the Books property of Author
        BsonClassMap.RegisterClassMap<Author>(cm =>
        {
            cm.AutoMap();
            cm.UnmapMember(c => c.Books);
        });

        BsonClassMap.RegisterClassMap<Book>(cm =>
        {
            cm.AutoMap();
            cm.UnmapMember(c => c.Editors);
        });


        // Get references to collections
        var authorsCollection = database.GetCollection<Author>("HarvestCycle-Collection");
        var booksCollection = database.GetCollection<Book>("PlantHarvestCycle-Collection");

        // Create an author
        var author = Author.Create("J.K. Rowling");

        author.AddBook(Book.Create("Harry Potter and the Philosopher's Stone", "Fantasy"));
        author.AddBook(Book.Create("Harry Potter and the Chamber of Secrets", "Fantasy"));
        author.AddBook(Book.Create("Harry Potter and the Prisoner of Azkaban", "Fantasy"));

        // Insert author (without books)
        authorsCollection.InsertOne(author);

        // Insert books
       // booksCollection.InsertMany(author.Books);

    }
}


public class Author : BaseEntity, IAggregateRoot
{
    private Author()
    {

    }

    public string? Name { get; set; }

    private readonly List<Book> _books = new();
    public IReadOnlyCollection<Book> Books => _books.AsReadOnly();

    public static Author Create(string name)
    {
        return new Author()
        {
            Id = Guid.NewGuid().ToString(),
            Name = name
        };
    }

    public void AddBook(Book book)
    {
        _books.Add(book);
    }

    protected override void AddDomainEvent(string attributeName)
    {
        throw new NotImplementedException();
    }
    
}

public class Book : BaseEntity, IEntity
{   
    public string? Title { get; set; }
    public string? Genre { get; set; }
    private readonly List<Editor> _editors = new();
    public IReadOnlyCollection<Editor> Editors => _editors.AsReadOnly();

    private Book()
    {

    }   

    public static Book Create(string title, string genre)
    {
        return new Book()
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            Genre = genre
        };
    }

    protected override void AddDomainEvent(string attributeName)
    {
        throw new NotImplementedException();
    }
}

public class Editor : BaseEntity, IEntity
{
   
    public string? Name { get; set; }

    protected override void AddDomainEvent(string attributeName)
    {
        throw new NotImplementedException();
    }
}

