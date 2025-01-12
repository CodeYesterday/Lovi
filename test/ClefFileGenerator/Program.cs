using Serilog;
using Serilog.Formatting.Compact;

var path = Path.GetFullPath($"../../../../../LogData/{Cfg.Session}/{Cfg.FileName}.clef");

await using var fileLog = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File(new CompactJsonFormatter(), path)
    .CreateLogger();

LogskiBase[] logskis =
[
    new Logski1(fileLog, "App1"),
    new Logski2(fileLog, "App1"),
    new Logski3(fileLog, "App1"),
    new Logski3(fileLog, "App2")
];

var tasks = new List<Task>();

foreach (var logski in logskis)
{
    tasks.Add(Task.Run(() => logski.DoLgskiAsync()));
}

await Task.WhenAll(tasks);

class Logski1 : LogskiBase
{
    public Logski1(ILogger logger, string app)
    {
        _logger = logger.ForContext<Logski1>().ForContext("Application", app);
    }
}

class Logski2 : LogskiBase
{
    public Logski2(ILogger logger, string app)
    {
        _logger = logger.ForContext<Logski2>().ForContext("Application", app);
    }
}

class Logski3 : LogskiBase
{
    public Logski3(ILogger logger, string app)
    {
        _logger = logger.ForContext<Logski3>().ForContext("Application", app);
    }
}

abstract class LogskiBase
{
    private static Person[] Persons =
        [
            new()
            {
                FirstName = "John",
                LastName = "Smith",
                Address = new()
                {
                    ZipCode = "012345",
                    City = "Cleansigton",
                    Street = "Noneroad",
                    HouseNumber = 10
                },
                Phones =
                    [
                        new()
                        {
                            Number = "+01 555 123 4532",
                            Type = PhoneType.Home
                        },
                        new()
                        {
                            Number = "+01 555 945 5365",
                            Type = PhoneType.Mobile
                        },
                        new()
                        {
                            Number = "+01 555 263 7463",
                            Type = PhoneType.Office
                        }
                    ]
            },
            new()
            {
                FirstName = "Maria",
                LastName = "Smith",
                Address = new()
                {
                    ZipCode = "012345",
                    City = "Cleansigton",
                    Street = "Noneroad",
                    HouseNumber = 10
                },
                Phones =
                [
                    new()
                    {
                        Number = "+01 555 123 4532",
                        Type = PhoneType.Home
                    },
                    new()
                    {
                        Number = "+01 555 432 4256",
                        Type = PhoneType.Mobile
                    }
                ]
            },
            new()
            {
                FirstName = "Jane",
                LastName = "Doe",
                Address = new()
                {
                    ZipCode = "27645",
                    City = "Nowhere",
                    Street = "Tobefound",
                    HouseNumber = 1
                }
            },
            new()
            {
                FirstName = "Timo",
                LastName = "Beil",
                Address = new()
                {
                    ZipCode = "74635",
                    City = "Phony",
                    Street = "Mobileway",
                    HouseNumber = 12
                },
                Phones =
                [
                    new()
                    {
                        Number = "+01 555 826 475",
                        Type = PhoneType.Mobile
                    },
                    new()
                    {
                        Number = "+01 555 284 9735",
                        Type = PhoneType.Office
                    }
                ]
            }
        ];

    protected ILogger _logger;

    public virtual async Task DoLgskiAsync()
    {
        for (int n = 0; n < Cfg.Count; ++n)
        {
            _logger.Information("Hello, {@User} in log no {Numero}", new { Name = "nblumhardt", Id = 101 }, n);
            _logger.Information("Number {N:x8}", 42);
            _logger.Information("Non-formatted number {Numero}", n);

            _logger.Information("Mixed types {Mixed}", n % 2 == 0 ? n : $"Number {n}");

            _logger.Warning("Tags are {Tags}", new[] { "test", "orange" });
            if (n % 2 == 0)
                _logger.Error("Some generic error");
            _logger.Debug("Additional debug info");
            _logger.Verbose("This is some ver verbose message\nIt has multiple lines");
            _logger.Verbose("And it is not alone");
            if (n % 3 == 0)
                _logger.Fatal("Fatality");

            if (n % 5 == 0)
            {
                _logger.Information("A Person: {@Person}", Persons[(n / 5) % Persons.Length]);
            }

            if (n % 100 == 0)
            {
                try
                {
                    throw new DivideByZeroException("Well, not actually");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Something failed: {Message}", ex.Message);
                }
            }
        }

        await Task.Yield();
    }
}

class Address
{
    public required string ZipCode { get; set; }

    public required string City { get; set; }

    public required string Street { get; set; }

    public required int HouseNumber { get; set; }
}

enum PhoneType
{
    Home,
    Mobile,
    Office
}

class Phone
{
    public required string Number { get; set; }

    public required PhoneType Type { get; set; }
}

class Person
{
    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required Address Address { get; set; }

    public List<Phone> Phones { get; set; } = [];
}

public static class Cfg
{
    public const int Count = 50;

    public const string Session = "SmallSession";

    public const string FileName = "log2";
}