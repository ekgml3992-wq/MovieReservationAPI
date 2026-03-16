using System;
using System.Collections.Generic;
using System.Linq;

namespace MovieReservationAPI
{
    public class MovieReservationService
    {
        private readonly List<Movie> _movies;

        public MovieReservationService()
        {
            _movies = CreateDefaultMovies();
        }

        public MovieReservationService(List<Movie> movies)
        {
            _movies = movies;
        }

        public List<Movie> Movies()
        {
            return _movies;
        }

        public List<ShowTime> ShowTimes(int movieId)
        {
            var movie = _movies.FirstOrDefault(m => m.Id == movieId);

            if (movie == null)
                throw new InvalidOperationException("영화를 찾을 수 없습니다");

            return movie.ShowTimes;
        }

        public List<Seat> Seats(int showtimeId)
        {
            var showtime = _movies
                .SelectMany(m => m.ShowTimes)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showtime == null)
                throw new InvalidOperationException("상영시간 없음");

            return showtime.Seats;
        }

        public string ReserveSeat(int showtimeId, int seatId)
        {
            var showtime = _movies
                .SelectMany(m => m.ShowTimes)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showtime == null)
                throw new InvalidOperationException("상영시간 없음");

            var seat = showtime.Seats.FirstOrDefault(s => s.Id == seatId);

            if (seat == null)
                throw new InvalidOperationException("좌석 없음");

            if (seat.IsReserved)
                throw new InvalidOperationException("이미 예약된 좌석");

            seat.IsReserved = true;

            return "예약 완료";
        }

        public string CancelReservation(int showtimeId, int seatId)
        {
            var showtime = _movies
                .SelectMany(m => m.ShowTimes)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showtime == null)
                throw new InvalidOperationException("상영시간 없음");

            var seat = showtime.Seats.FirstOrDefault(s => s.Id == seatId);

            if (seat == null)
                throw new InvalidOperationException("좌석 없음");

            if (!seat.IsReserved)
                throw new InvalidOperationException("예약되지 않은 좌석");

            seat.IsReserved = false;

            return "예약 취소 완료";
        }

        public List<Seat> AvailableSeats(int showtimeId)
        {
            var showtime = _movies
                .SelectMany(m => m.ShowTimes)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showtime == null)
                throw new InvalidOperationException("상영시간 없음");

            return showtime.Seats
                .Where(s => !s.IsReserved)
                .ToList();
        }

        public List<Seat> ReservedSeats(int showtimeId)
        {
            var showtime = _movies
                .SelectMany(m => m.ShowTimes)
                .FirstOrDefault(s => s.Id == showtimeId);

            if (showtime == null)
                throw new InvalidOperationException("상영시간 없음");

            return showtime.Seats
                .Where(s => s.IsReserved)
                .ToList();
        }

        public int AvailableSeatCount(int showtimeId)
        {
            return AvailableSeats(showtimeId).Count;
        }

        public int ReservedSeatCount(int showtimeId)
        {
            return ReservedSeats(showtimeId).Count;
        }

        public static List<Movie> CreateDefaultMovies()
        {
            return new List<Movie>
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
        }

        public static List<Seat> CreateSeats()
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
    }

    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<ShowTime> ShowTimes { get; set; } = new();
    }

    public class ShowTime
    {
        public int Id { get; set; }
        public string Time { get; set; } = string.Empty;
        public List<Seat> Seats { get; set; } = new();
    }

    public class Seat
    {
        public int Id { get; set; }
        public bool IsReserved { get; set; }
    }
}
