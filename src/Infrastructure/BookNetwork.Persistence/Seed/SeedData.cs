using BookNetwork.Domain.Entities;

namespace BookNetwork.Persistence.Seed;

internal static class SeedData
{
    public static readonly Publisher[] Publishers =
    [
        new()
        {
            Id = Guid.Parse("5f2c3d90-9a8e-4cf0-8ef7-7e2bc9a5a111"),
            Name = "Yapı Kredi Yayınları",
            Country = "Türkiye",
            Website = "https://kitap.ykykultur.com.tr"
        },
        new()
        {
            Id = Guid.Parse("3a0d9b34-1c9a-4c57-8c31-8897e5b45ef2"),
            Name = "İş Bankası Kültür Yayınları",
            Country = "Türkiye",
            Website = "https://isbankyayinevi.com"
        }
    ];

    public static readonly Author[] Authors =
    [
        new()
        {
            Id = Guid.Parse("2f5fb6ae-0561-4b65-92f5-5a6f0ca6c7d1"),
            FirstName = "Sabahattin",
            LastName = "Ali",
            Biography = "Türk edebiyatının unutulmaz yazarlarından; toplumsal ve psikolojik derinliğiyle bilinir.",
            BirthDate = new DateTime(1907, 2, 25)
        },
        new()
        {
            Id = Guid.Parse("4a3f34c3-8a7c-4cbc-9c24-1ff717a8ba4a"),
            FirstName = "Orhan",
            LastName = "Pamuk",
            Biography = "Nobel ödüllü yazar; İstanbul ve kimlik temalarıyla öne çıkar.",
            BirthDate = new DateTime(1952, 6, 7)
        },
        new()
        {
            Id = Guid.Parse("8c3efaa3-3b94-4f17-bc2e-98e035a668cd"),
            FirstName = "Reşat Nuri",
            LastName = "Güntekin",
            Biography = "Anadolu insanını ve idealizmi merkeze alan klasik romanların yazarı.",
            BirthDate = new DateTime(1889, 11, 25)
        }
    ];

    public static readonly Category[] Categories =
    [
        new()
        {
            Id = Guid.Parse("2b5d5b82-2c80-4c03-8f4f-5a9b9d6b5c77"),
            Name = "Klasik",
            Description = "Türk edebiyatının klasik kabul edilen eserleri."
        },
        new()
        {
            Id = Guid.Parse("1c3a3f8e-b6d4-4ef8-8e7f-56ad52c57d8f"),
            Name = "Roman",
            Description = "Uzun soluklu kurgu eserler."
        },
        new()
        {
            Id = Guid.Parse("35ef77fa-5f76-48ec-9f84-6cc81857232e"),
            Name = "Türk Edebiyatı",
            Description = "Yerel yazarların eserleri."
        }
    ];

    public static readonly Book[] Books =
    [
        new()
        {
            Id = Guid.Parse("8b1c1b79-fc6e-4d3a-9e55-6f2e2e4b3c10"),
            Title = "Kürk Mantolu Madonna",
            Description = "Sabahattin Ali’nin aşk ve yalnızlık temalarını işleyen klasiği.",
            Isbn = "9789750805000",
            PublicationYear = 1943,
            PublisherId = Guid.Parse("5f2c3d90-9a8e-4cf0-8ef7-7e2bc9a5a111")
        },
        new()
        {
            Id = Guid.Parse("0a81f345-62fb-4fbe-9cde-52d69f964bce"),
            Title = "Masumiyet Müzesi",
            Description = "Orhan Pamuk’tan İstanbul’da geçen modern bir aşk romanı.",
            Isbn = "9789750809572",
            PublicationYear = 2008,
            PublisherId = Guid.Parse("5f2c3d90-9a8e-4cf0-8ef7-7e2bc9a5a111")
        },
        new()
        {
            Id = Guid.Parse("e2c73d7f-5123-4b7f-9cf7-9f1d0105d01c"),
            Title = "Çalıkuşu",
            Description = "Reşat Nuri Güntekin’in idealist öğretmen Feride’nin hikâyesi.",
            Isbn = "9789754700110",
            PublicationYear = 1922,
            PublisherId = Guid.Parse("3a0d9b34-1c9a-4c57-8c31-8897e5b45ef2")
        }
    ];

    public static readonly BookAuthor[] BookAuthors =
    [
        new()
        {
            BookId = Guid.Parse("8b1c1b79-fc6e-4d3a-9e55-6f2e2e4b3c10"),
            AuthorId = Guid.Parse("2f5fb6ae-0561-4b65-92f5-5a6f0ca6c7d1")
        },
        new()
        {
            BookId = Guid.Parse("0a81f345-62fb-4fbe-9cde-52d69f964bce"),
            AuthorId = Guid.Parse("4a3f34c3-8a7c-4cbc-9c24-1ff717a8ba4a")
        },
        new()
        {
            BookId = Guid.Parse("e2c73d7f-5123-4b7f-9cf7-9f1d0105d01c"),
            AuthorId = Guid.Parse("8c3efaa3-3b94-4f17-bc2e-98e035a668cd")
        }
    ];

    public static readonly BookCategory[] BookCategories =
    [
        new()
        {
            BookId = Guid.Parse("8b1c1b79-fc6e-4d3a-9e55-6f2e2e4b3c10"),
            CategoryId = Guid.Parse("2b5d5b82-2c80-4c03-8f4f-5a9b9d6b5c77")
        },
        new()
        {
            BookId = Guid.Parse("8b1c1b79-fc6e-4d3a-9e55-6f2e2e4b3c10"),
            CategoryId = Guid.Parse("35ef77fa-5f76-48ec-9f84-6cc81857232e")
        },
        new()
        {
            BookId = Guid.Parse("0a81f345-62fb-4fbe-9cde-52d69f964bce"),
            CategoryId = Guid.Parse("1c3a3f8e-b6d4-4ef8-8e7f-56ad52c57d8f")
        },
        new()
        {
            BookId = Guid.Parse("0a81f345-62fb-4fbe-9cde-52d69f964bce"),
            CategoryId = Guid.Parse("35ef77fa-5f76-48ec-9f84-6cc81857232e")
        },
        new()
        {
            BookId = Guid.Parse("e2c73d7f-5123-4b7f-9cf7-9f1d0105d01c"),
            CategoryId = Guid.Parse("2b5d5b82-2c80-4c03-8f4f-5a9b9d6b5c77")
        },
        new()
        {
            BookId = Guid.Parse("e2c73d7f-5123-4b7f-9cf7-9f1d0105d01c"),
            CategoryId = Guid.Parse("35ef77fa-5f76-48ec-9f84-6cc81857232e")
        }
    ];
}
