using System;
using System.Linq;
using MovieReservationAPI;
using Xunit;

namespace MovieReservationAPI.Tests
{
    public class MovieReservationServiceTests
    {
        private MovieReservationService CreateService()
        {
            return new MovieReservationService(MovieReservationService.CreateDefaultMovies());
        }

        [Fact]
        public void Movies_ReturnsAllMovies()
        {
            var service = CreateService();

            var movies = service.Movies();

            Assert.Equal(2, movies.Count);
            Assert.Contains(movies, m => m.Id == 1 && m.Title == "호퍼스");
            Assert.Contains(movies, m => m.Id == 2 && m.Title == "삼악도");
        }

        [Fact]
        public void Movies_EachMovieHasTwoShowTimes()
        {
            var service = CreateService();

            var movies = service.Movies();

            Assert.All(movies, movie => Assert.Equal(2, movie.ShowTimes.Count));
        }

        [Fact]
        public void CreateSeats_ReturnsTenSeats()
        {
            var seats = MovieReservationService.CreateSeats();

            Assert.Equal(10, seats.Count);
        }

        [Fact]
        public void CreateSeats_ReturnsSeatIdsFrom1To10()
        {
            var seats = MovieReservationService.CreateSeats();

            Assert.Equal(Enumerable.Range(1, 10), seats.Select(s => s.Id));
        }

        [Fact]
        public void CreateSeats_AllSeatsAreInitiallyUnreserved()
        {
            var seats = MovieReservationService.CreateSeats();

            Assert.All(seats, seat => Assert.False(seat.IsReserved));
        }

        [Fact]
        public void ShowTimes_ValidMovieId_ReturnsExpectedShowTimes()
        {
            var service = CreateService();

            var showTimes = service.ShowTimes(1);

            Assert.Equal(2, showTimes.Count);
            Assert.Contains(showTimes, s => s.Id == 1 && s.Time == "10:00");
            Assert.Contains(showTimes, s => s.Id == 2 && s.Time == "14:00");
        }

        [Fact]
        public void ShowTimes_SecondMovie_ReturnsExpectedShowTimes()
        {
            var service = CreateService();

            var showTimes = service.ShowTimes(2);

            Assert.Equal(2, showTimes.Count);
            Assert.Contains(showTimes, s => s.Id == 3 && s.Time == "12:00");
            Assert.Contains(showTimes, s => s.Id == 4 && s.Time == "18:00");
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void ShowTimes_InvalidMovieId_ThrowsException(int movieId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ShowTimes(movieId));

            Assert.Equal("영화를 찾을 수 없습니다", ex.Message);
        }

        [Fact]
        public void Seats_ValidShowTimeId_ReturnsTenSeats()
        {
            var service = CreateService();

            var seats = service.Seats(1);

            Assert.Equal(10, seats.Count);
            Assert.All(seats, seat => Assert.False(seat.IsReserved));
        }

        [Fact]
        public void Seats_ReturnsSeatIdsFrom1To10()
        {
            var service = CreateService();

            var seats = service.Seats(1);

            Assert.Equal(Enumerable.Range(1, 10), seats.Select(s => s.Id));
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void Seats_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.Seats(showtimeId));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_ValidSeat_ReturnsSuccessMessage()
        {
            var service = CreateService();

            var result = service.ReserveSeat(1, 3);

            Assert.Equal("예약 완료", result);
        }

        [Fact]
        public void ReserveSeat_ValidSeat_ChangesSeatToReserved()
        {
            var service = CreateService();

            service.ReserveSeat(1, 3);

            var seat = service.Seats(1).First(s => s.Id == 3);
            Assert.True(seat.IsReserved);
        }

        [Fact]
        public void ReserveSeat_FirstSeatBoundary_ReservesSuccessfully()
        {
            var service = CreateService();

            var result = service.ReserveSeat(1, 1);

            Assert.Equal("예약 완료", result);
            Assert.True(service.Seats(1).First(s => s.Id == 1).IsReserved);
        }

        [Fact]
        public void ReserveSeat_LastSeatBoundary_ReservesSuccessfully()
        {
            var service = CreateService();

            var result = service.ReserveSeat(1, 10);

            Assert.Equal("예약 완료", result);
            Assert.True(service.Seats(1).First(s => s.Id == 10).IsReserved);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void ReserveSeat_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(showtimeId, 1));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-1)]
        public void ReserveSeat_InvalidSeatId_ThrowsException(int seatId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, seatId));

            Assert.Equal("좌석 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_AlreadyReservedSeat_ThrowsException()
        {
            var service = CreateService();

            service.ReserveSeat(1, 5);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, 5));

            Assert.Equal("이미 예약된 좌석", ex.Message);
        }

        [Fact]
        public void ReserveSeat_ReservingOneSeat_DoesNotAffectOtherSeats()
        {
            var service = CreateService();

            service.ReserveSeat(1, 4);

            var seats = service.Seats(1);

            Assert.True(seats.First(s => s.Id == 4).IsReserved);
            Assert.All(seats.Where(s => s.Id != 4), seat => Assert.False(seat.IsReserved));
        }

        [Fact]
        public void ReserveSeat_ReservingSeatInOneShowTime_DoesNotAffectAnotherShowTime()
        {
            var service = CreateService();

            service.ReserveSeat(1, 1);

            var firstShowTimeSeats = service.Seats(1);
            var secondShowTimeSeats = service.Seats(2);

            Assert.True(firstShowTimeSeats.First(s => s.Id == 1).IsReserved);
            Assert.False(secondShowTimeSeats.First(s => s.Id == 1).IsReserved);
        }

        [Fact]
        public void CancelReservation_ValidReservedSeat_ReturnsSuccessMessage()
        {
            var service = CreateService();

            service.ReserveSeat(1, 3);

            var result = service.CancelReservation(1, 3);

            Assert.Equal("예약 취소 완료", result);
        }

        [Fact]
        public void CancelReservation_ValidReservedSeat_ChangesSeatToUnreserved()
        {
            var service = CreateService();

            service.ReserveSeat(1, 3);
            service.CancelReservation(1, 3);

            var seat = service.Seats(1).First(s => s.Id == 3);
            Assert.False(seat.IsReserved);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void CancelReservation_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.CancelReservation(showtimeId, 1));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(11)]
        [InlineData(-1)]
        public void CancelReservation_InvalidSeatId_ThrowsException(int seatId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.CancelReservation(1, seatId));

            Assert.Equal("좌석 없음", ex.Message);
        }

        [Fact]
        public void CancelReservation_NotReservedSeat_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.CancelReservation(1, 3));

            Assert.Equal("예약되지 않은 좌석", ex.Message);
        }

        [Fact]
        public void CancelReservation_AfterCancel_CanReserveAgain()
        {
            var service = CreateService();

            service.ReserveSeat(1, 3);
            service.CancelReservation(1, 3);

            var result = service.ReserveSeat(1, 3);

            Assert.Equal("예약 완료", result);
            Assert.True(service.Seats(1).First(s => s.Id == 3).IsReserved);
        }

        [Fact]
        public void CancelReservation_CancelingSeatInOneShowTime_DoesNotAffectAnotherShowTime()
        {
            var service = CreateService();

            service.ReserveSeat(1, 1);
            service.ReserveSeat(2, 1);

            service.CancelReservation(1, 1);

            Assert.False(service.Seats(1).First(s => s.Id == 1).IsReserved);
            Assert.True(service.Seats(2).First(s => s.Id == 1).IsReserved);
        }

        [Fact]
        public void AvailableSeats_InitialState_ReturnsAllSeats()
        {
            var service = CreateService();

            var availableSeats = service.AvailableSeats(1);

            Assert.Equal(10, availableSeats.Count);
            Assert.All(availableSeats, seat => Assert.False(seat.IsReserved));
        }

        [Fact]
        public void AvailableSeats_AfterOneReservation_ExcludesReservedSeat()
        {
            var service = CreateService();

            service.ReserveSeat(1, 2);

            var availableSeats = service.AvailableSeats(1);

            Assert.Equal(9, availableSeats.Count);
            Assert.DoesNotContain(availableSeats, seat => seat.Id == 2);
        }

        [Fact]
        public void AvailableSeats_AfterCancel_ReturnsSeatAgain()
        {
            var service = CreateService();

            service.ReserveSeat(1, 2);
            service.CancelReservation(1, 2);

            var availableSeats = service.AvailableSeats(1);

            Assert.Equal(10, availableSeats.Count);
            Assert.Contains(availableSeats, seat => seat.Id == 2);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void AvailableSeats_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.AvailableSeats(showtimeId));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void ReservedSeats_InitialState_ReturnsEmptyList()
        {
            var service = CreateService();

            var reservedSeats = service.ReservedSeats(1);

            Assert.Empty(reservedSeats);
        }

        [Fact]
        public void ReservedSeats_AfterReservations_ReturnsOnlyReservedSeats()
        {
            var service = CreateService();

            service.ReserveSeat(1, 2);
            service.ReserveSeat(1, 5);

            var reservedSeats = service.ReservedSeats(1);

            Assert.Equal(2, reservedSeats.Count);
            Assert.Contains(reservedSeats, seat => seat.Id == 2);
            Assert.Contains(reservedSeats, seat => seat.Id == 5);
            Assert.All(reservedSeats, seat => Assert.True(seat.IsReserved));
        }

        [Fact]
        public void ReservedSeats_AfterCancel_RemovesCanceledSeat()
        {
            var service = CreateService();

            service.ReserveSeat(1, 2);
            service.ReserveSeat(1, 5);
            service.CancelReservation(1, 2);

            var reservedSeats = service.ReservedSeats(1);

            Assert.Single(reservedSeats);
            Assert.DoesNotContain(reservedSeats, seat => seat.Id == 2);
            Assert.Contains(reservedSeats, seat => seat.Id == 5);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void ReservedSeats_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReservedSeats(showtimeId));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void ReservedSeatCount_InitialState_ReturnsZero()
        {
            var service = CreateService();

            var count = service.ReservedSeatCount(1);

            Assert.Equal(0, count);
        }

        [Fact]
        public void AvailableSeatCount_InitialState_ReturnsTen()
        {
            var service = CreateService();

            var count = service.AvailableSeatCount(1);

            Assert.Equal(10, count);
        }

        [Fact]
        public void ReservedSeatCount_AfterOneReservation_ReturnsOne()
        {
            var service = CreateService();

            service.ReserveSeat(1, 4);

            var count = service.ReservedSeatCount(1);

            Assert.Equal(1, count);
        }

        [Fact]
        public void AvailableSeatCount_AfterOneReservation_ReturnsNine()
        {
            var service = CreateService();

            service.ReserveSeat(1, 4);

            var count = service.AvailableSeatCount(1);

            Assert.Equal(9, count);
        }

        [Fact]
        public void SeatCounts_AfterMultipleReservations_AreAccurate()
        {
            var service = CreateService();

            service.ReserveSeat(1, 1);
            service.ReserveSeat(1, 2);
            service.ReserveSeat(1, 3);

            Assert.Equal(3, service.ReservedSeatCount(1));
            Assert.Equal(7, service.AvailableSeatCount(1));
        }

        [Fact]
        public void SeatCounts_AfterCancel_ReturnToOriginalValues()
        {
            var service = CreateService();

            service.ReserveSeat(1, 4);
            service.CancelReservation(1, 4);

            Assert.Equal(0, service.ReservedSeatCount(1));
            Assert.Equal(10, service.AvailableSeatCount(1));
        }

        [Fact]
        public void SeatCounts_InDifferentShowTimes_AreIndependent()
        {
            var service = CreateService();

            service.ReserveSeat(1, 1);
            service.ReserveSeat(1, 2);
            service.ReserveSeat(2, 1);

            Assert.Equal(2, service.ReservedSeatCount(1));
            Assert.Equal(8, service.AvailableSeatCount(1));
            Assert.Equal(1, service.ReservedSeatCount(2));
            Assert.Equal(9, service.AvailableSeatCount(2));
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void AvailableSeatCount_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.AvailableSeatCount(showtimeId));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Theory]
        [InlineData(999)]
        [InlineData(0)]
        [InlineData(-1)]
        public void ReservedSeatCount_InvalidShowTimeId_ThrowsException(int showtimeId)
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReservedSeatCount(showtimeId));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void Constructor_WithCustomMovies_UsesProvidedMovieList()
        {
            var customMovies = new[]
            {
                new Movie
                {
                    Id = 100,
                    Title = "테스트영화",
                    ShowTimes =
                    {
                        new ShowTime
                        {
                            Id = 1000,
                            Time = "20:00",
                            Seats = MovieReservationService.CreateSeats()
                        }
                    }
                }
            }.ToList();

            var service = new MovieReservationService(customMovies);

            var movies = service.Movies();

            Assert.Single(movies);
            Assert.Equal(100, movies[0].Id);
            Assert.Equal("테스트영화", movies[0].Title);
        }

        [Fact]
        public void Constructor_WithEmptyMovies_ReturnsEmptyMovieList()
        {
            var service = new MovieReservationService(new System.Collections.Generic.List<Movie>());

            var movies = service.Movies();

            Assert.Empty(movies);
        }

        [Fact]
        public void EmptyMovies_ShowTimes_ThrowsException()
        {
            var service = new MovieReservationService(new System.Collections.Generic.List<Movie>());

            var ex = Assert.Throws<InvalidOperationException>(() => service.ShowTimes(1));

            Assert.Equal("영화를 찾을 수 없습니다", ex.Message);
        }

        [Fact]
        public void EmptyMovies_Seats_ThrowsException()
        {
            var service = new MovieReservationService(new System.Collections.Generic.List<Movie>());

            var ex = Assert.Throws<InvalidOperationException>(() => service.Seats(1));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void EmptyMovies_ReserveSeat_ThrowsException()
        {
            var service = new MovieReservationService(new System.Collections.Generic.List<Movie>());

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, 1));

            Assert.Equal("상영시간 없음", ex.Message);
        }
    }
}
