using System.Text.Json;
using SkyNet.Domain.Entities;
using SkyNet.Domain.Enums;

namespace SkyNet.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(SkyNetDbContext context, string wwwrootPath)
    {
        if (context.Airports.Any()) return;

        // ── Seed Airports from airports.json ─────────────────────
        var airportsJson = Path.Combine(wwwrootPath, "data", "airports.json");
        if (File.Exists(airportsJson))
        {
            var json = await File.ReadAllTextAsync(airportsJson);
            var raw  = JsonSerializer.Deserialize<AirportSeedDto[]>(json,
                       new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (raw != null)
            {
                var airports = raw
                    .Where(a => !string.IsNullOrWhiteSpace(a.Iata) && a.Iata.Length == 3)
                    .Select((a, i) => new Airport
                    {
                        Id        = i + 1,
                        IataCode  = a.Iata.ToUpper(),
                        Name      = a.Name,
                        City      = a.City,
                        Country   = a.Country,
                        Latitude  = a.Lat,
                        Longitude = a.Lon,
                        Capacity  = Random.Shared.Next(5_000_000, 100_000_000),
                        IsActive  = true,
                        UsageCount = Random.Shared.Next(0, 50000)
                    }).ToList();

                await context.Airports.AddRangeAsync(airports);
            }
        }

        // ── Seed sample Flights ───────────────────────────────────
        var routes = new (string From, string To, double Dist, double Price)[]
        {
            // Central Asia hub (TAS)
            ("TAS","LHR",6700,420), ("TAS","IST",3600,180), ("TAS","DXB",3200,150),
            ("TAS","FRA",5200,280), ("TAS","AMS",5500,290), ("TAS","CDG",5800,310),
            ("TAS","DOH",3500,160), ("TAS","AUH",3400,155), ("TAS","BKK",4800,240),
            ("TAS","KUL",5100,260), ("TAS","PEK",4300,210), ("TAS","DEL",2200,120),
            ("TAS","IST",3600,180), ("TAS","GYD",1800,90),  ("TAS","ALA", 600, 45),
            ("TAS","NQZ",1800, 90), ("TAS","SVO",3600,180), ("TAS","EVN",2200,100),
            ("TAS","TBS",2400,110), ("TAS","BKK",4800,240), ("TAS","SIN",5200,270),
            // Europe hub
            ("LHR","JFK",5540,380), ("LHR","DXB",5500,300), ("LHR","SIN",10830,580),
            ("LHR","NRT",9540,520), ("LHR","ICN",9000,500), ("LHR","CDG",340,80),
            ("LHR","AMS",370,75),   ("LHR","FRA",650,95),   ("LHR","MUC",1440,110),
            ("LHR","MAD",1260,105), ("LHR","BCN",1140,100), ("LHR","IST",2850,150),
            ("LHR","CAI",3500,190), ("LHR","JNB",9000,510), ("LHR","ATL",6840,370),
            ("LHR","ORD",6340,350), ("LHR","LAX",8740,480), ("LHR","SFO",8630,470),
            ("CDG","JFK",5830,370), ("CDG","LHR",340,80),   ("CDG","FRA",480,85),
            ("CDG","AMS",430,80),   ("CDG","IST",2240,160), ("CDG","MAD",1070,95),
            ("CDG","BCN",830,85),   ("CDG","FCO",1100,95),  ("CDG","NRT",9700,530),
            ("FRA","JFK",6200,360), ("FRA","DXB",5160,280), ("FRA","SIN",10170,560),
            ("FRA","PEK",8150,440), ("FRA","NRT",9310,510), ("FRA","MUC",300,60),
            ("FRA","AMS",370,70),   ("FRA","VIE",600,85),   ("FRA","WAW",920,95),
            ("AMS","JFK",5860,340), ("AMS","LHR",370,75),   ("AMS","DXB",5560,290),
            ("IST","JFK",8050,420), ("IST","CDG",2240,160), ("IST","FRA",2440,155),
            ("IST","DXB",2720,145), ("IST","SIN",8570,460), ("IST","DOH",2210,120),
            ("IST","BKK",7500,400), ("IST","KUL",8100,440), ("IST","GYD",1800,95),
            ("IST","TBS",1600,85),  ("IST","EVN",1700,88),  ("IST","KBP",1400,80),
            ("IST","WAW",2150,120), ("IST","VIE",1810,110), ("IST","ATH",700,70),
            // Middle East hub
            ("DXB","JFK",11010,580), ("DXB","LHR",5500,300), ("DXB","SIN",5841,310),
            ("DXB","NRT",7948,430),  ("DXB","ICN",7255,390), ("DXB","PEK",6700,360),
            ("DXB","BOM",1924,120),  ("DXB","DEL",2200,130), ("DXB","KHI",1320,85),
            ("DXB","DOH",360,45),    ("DXB","AUH",140,30),   ("DXB","MCT",390,50),
            ("DXB","CAI",2420,130),  ("DXB","JNB",7350,400), ("DXB","NBO",4560,260),
            ("DXB","ADD",2800,155),  ("DXB","LAX",13400,680),
            ("DOH","LHR",6790,370),  ("DOH","CDG",6680,360), ("DOH","FRA",6040,330),
            ("DOH","BOM",2050,120),  ("DOH","DEL",2520,135), ("DOH","KUL",6330,350),
            ("DOH","SIN",6350,350),  ("DOH","NRT",9200,500),
            // Asia Pacific
            ("SIN","NRT",5315,290),  ("SIN","ICN",4690,260), ("SIN","PEK",4470,250),
            ("SIN","SYD",6300,340),  ("SIN","MEL",6050,330), ("SIN","BKK",1400,90),
            ("SIN","KUL",350,40),    ("SIN","CGK",890,70),   ("SIN","MNL",2390,140),
            ("SIN","HKG",2570,150),  ("SIN","LAX",14100,720),
            ("NRT","ICN",1160,90),   ("NRT","PEK",2100,140), ("NRT","HKG",2900,180),
            ("NRT","SFO",8270,500),  ("NRT","LAX",8800,510), ("NRT","SYD",7800,440),
            ("BKK","SIN",1400,90),   ("BKK","KUL",1180,80),  ("BKK","HKG",1730,110),
            ("BKK","NRT",4600,260),  ("BKK","ICN",3720,220), ("BKK","DEL",2900,175),
            // Americas
            ("JFK","LAX",3983,180),  ("JFK","ORD",1200,90),  ("JFK","MIA",2055,130),
            ("JFK","SFO",4140,200),  ("JFK","ATL",1200,95),  ("JFK","DFW",2200,140),
            ("JFK","YYZ",570,65),    ("JFK","GRU",7600,420), ("JFK","MEX",3360,185),
            ("LAX","SFO",559,80),    ("LAX","ORD",2986,160), ("LAX","SEA",1535,100),
            ("LAX","NRT",8800,510),  ("LAX","SYD",12070,620),("LAX","MEX",2570,145),
            ("SFO","ORD",3240,165),  ("SFO","SEA",1093,85),  ("SFO","NRT",8270,500),
            ("ATL","ORD",1070,85),   ("ATL","MIA",1090,80),  ("ATL","DFW",1140,88),
            ("ORD","DFW",1440,105),  ("ORD","MIA",2100,135), ("ORD","DEN",1473,100),
            ("MIA","GRU",7685,420),  ("MIA","BOG",3480,195), ("MIA","LIM",4380,240),
            ("GRU","EZE",2860,200),  ("GRU","BOG",4700,260), ("GRU","SCL",3100,190),
            // Africa
            ("JNB","NBO",3200,185),  ("JNB","ADD",3700,210), ("JNB","LOS",5100,290),
            ("JNB","CAI",6100,340),  ("NBO","ADD",1150,80),  ("NBO","CAI",4100,230),
            ("CAI","IST",1450,95),   ("CAI","DXB",2420,130), ("CAI","LHR",3500,190),
            // India subcontinent
            ("DEL","BOM",1148,85),   ("DEL","SIN",5600,310), ("DEL","NRT",5900,330),
            ("DEL","DXB",2200,130),  ("DEL","LHR",6700,380), ("DEL","CDG",6700,380),
            ("BOM","DEL",1148,85),   ("BOM","DXB",1924,120), ("BOM","SIN",5200,290),
            ("BOM","LHR",7200,400),
            // MUC/VIE/WAW Europe interior
            ("MUC","VIE",330,65),    ("MUC","FRA",300,60),   ("MUC","ZRH",315,60),
            ("MUC","IST",2060,130),  ("MUC","DXB",5100,280), ("VIE","WAW",520,100),
            ("VIE","IST",1810,110),  ("VIE","DXB",4800,270), ("WAW","KBP",780,70),
            ("WAW","FRA",920,95),    ("WAW","AMS",1110,100), ("KBP","IST",1400,80),
            ("KBP","FRA",2000,120),  ("KBP","AMS",2190,125),
            // Caucasus / Central Asia
            ("GYD","IST",1800,95),   ("GYD","DXB",2400,130), ("GYD","TAS",1800,90),
            ("GYD","FRA",3600,200),  ("TBS","IST",1600,85),  ("TBS","DXB",2600,140),
            ("EVN","IST",1700,88),   ("EVN","DXB",2700,145), ("ALA","DXB",4200,230),
            ("ALA","TAS",600,45),    ("ALA","FRA",5300,290),  ("NQZ","TAS",1800,90),
        };

        var flights = new List<Flight>();
        int fid = 1;
        var baseDate = DateTime.Today;

        foreach (var r in routes)
        {
            for (int day = 0; day < 7; day++)
            {
                flights.Add(new Flight
                {
                    Id              = fid,
                    FlightNumber    = $"SK{fid:D4}",
                    OriginIata      = r.From,
                    DestinationIata = r.To,
                    DepartureTime   = baseDate.AddDays(day).AddHours(Random.Shared.Next(5, 22)),
                    ArrivalTime     = baseDate.AddDays(day).AddHours(Random.Shared.Next(5, 22)).AddHours(r.Dist / 850.0),
                    Price           = r.Price * (0.8 + Random.Shared.NextDouble() * 0.4),
                    Distance        = r.Dist,
                    FuelEfficiency  = 3.0 + Random.Shared.NextDouble() * 2.0,
                    Status          = FlightStatus.Scheduled,
                    SeatsTotal      = 180,
                    SeatsAvailable  = Random.Shared.Next(0, 180)
                });
                fid++;
            }
        }

        await context.Flights.AddRangeAsync(flights);

        // ── Seed sample Passengers ────────────────────────────────
        var names = new[] {
            ("Asilbek","Khujanazarov"), ("John","Smith"), ("Maria","Garcia"),
            ("Ahmed","Al-Rashid"), ("Yuki","Tanaka"), ("Sarah","Johnson"),
            ("Ivan","Petrov"), ("Fatima","Hassan"), ("Carlos","Silva"), ("Emma","Wilson")
        };

        var passengers = names.Select((n, i) => new Passenger
        {
            Id          = i + 1,
            PNR         = $"SKY-{(i + 1) * 100000 + Random.Shared.Next(9999):D6}",
            PassportId  = $"AA{(i + 100) * 1234:D7}",
            FirstName   = n.Item1,
            LastName    = n.Item2,
            Nationality = "Uzbekistan",
            TicketClass = (TicketClass)((i % 3) + 1),
            FlightNumber = $"SK{(i + 1) * 10:D4}",
            IsCheckedIn  = false,
            IsBoarded    = false
        }).ToList();

        await context.Passengers.AddRangeAsync(passengers);
        await context.SaveChangesAsync();
    }

    private class AirportSeedDto
    {
        public string Iata    { get; set; } = string.Empty;
        public string Name    { get; set; } = string.Empty;
        public string City    { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public double Lat     { get; set; }
        public double Lon     { get; set; }
    }
}
