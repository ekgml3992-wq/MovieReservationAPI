using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

List<Movie> movies = new()
{
    new Movie
    {
        Id = 1,
        Title = "호퍼스",
        ShowTimes = new List<ShowTime>
        {
            new ShowTime
            {
                Id = 1,
                Time = "10:00",
                Seats = CreateSeats()
            },
            new ShowTime
            {
                Id = 2,
                Time = "14:00",
                Seats = CreateSeats()
            }
        }
    },
    new Movie
    {
        Id = 2,
        Title = "삼악도",
        ShowTimes = new List<ShowTime>
        {
            new ShowTime
            {
                Id = 3,
                Time = "12:00",
                Seats = CreateSeats()
            },
            new ShowTime
            {
                Id = 4,
                Time = "18:00",
                Seats = CreateSeats()
            }
        }
    }
};

app.MapGet("/movies", () =>
{
    return movies;
});

app.MapGet("/movies/{movieId}/showtimes", (int movieId) =>
{
    var movie = movies.FirstOrDefault(m => m.Id == movieId);

    if (movie == null)
        return Results.NotFound("영화를 찾을 수 없습니다");

    return Results.Ok(movie.ShowTimes);
});

app.MapGet("/showtimes/{showtimeId}/seats", (int showtimeId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    return Results.Ok(showtime.Seats);
});

app.MapPost("/reserve/{showtimeId}/{seatId}", (int showtimeId, int seatId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var seat = showtime.Seats.FirstOrDefault(s => s.Id == seatId);

    if (seat == null)
        return Results.NotFound("좌석 없음");

    if (seat.IsReserved)
        return Results.BadRequest("이미 예약된 좌석");

    seat.IsReserved = true;

    return Results.Ok("예약 완료");
});

app.MapPost("/cancel/{showtimeId}/{seatId}", (int showtimeId, int seatId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var seat = showtime.Seats.FirstOrDefault(s => s.Id == seatId);

    if (seat == null)
        return Results.NotFound("좌석 없음");

    if (!seat.IsReserved)
        return Results.BadRequest("예약되지 않은 좌석");

    seat.IsReserved = false;

    return Results.Ok("예약 취소 완료");
});

app.MapGet("/showtimes/{showtimeId}/available-seats", (int showtimeId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var availableSeats = showtime.Seats
        .Where(s => !s.IsReserved)
        .ToList();

    return Results.Ok(availableSeats);
});

app.MapGet("/showtimes/{showtimeId}/reserved-seats", (int showtimeId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var reservedSeats = showtime.Seats
        .Where(s => s.IsReserved)
        .ToList();

    return Results.Ok(reservedSeats);
});

app.MapGet("/showtimes/{showtimeId}/available-seat-count", (int showtimeId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var availableSeatCount = showtime.Seats.Count(s => !s.IsReserved);

    return Results.Ok(availableSeatCount);
});

app.MapGet("/showtimes/{showtimeId}/reserved-seat-count", (int showtimeId) =>
{
    var showtime = movies
        .SelectMany(m => m.ShowTimes)
        .FirstOrDefault(s => s.Id == showtimeId);

    if (showtime == null)
        return Results.NotFound("상영시간 없음");

    var reservedSeatCount = showtime.Seats.Count(s => s.IsReserved);

    return Results.Ok(reservedSeatCount);
});

app.Run();

List<Seat> CreateSeats()
{
    var seats = new List<Seat>();

    for (int i = 1; i <= 10; i++)
    {
        seats.Add(new Seat
        {
            Id = i,
            IsReserved = false
        });
    }

    return seats;
}

class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<ShowTime> ShowTimes { get; set; }
}

class ShowTime
{
    public int Id { get; set; }
    public string Time { get; set; }
    public List<Seat> Seats { get; set; }
}

class Seat
{
    public int Id { get; set; }
    public bool IsReserved { get; set; }
}
