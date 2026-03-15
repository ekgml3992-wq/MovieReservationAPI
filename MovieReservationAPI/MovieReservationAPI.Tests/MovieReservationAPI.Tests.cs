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
        public void GetMovies_ReturnsAllMovies()
        {
            var service = CreateService();

            var movies = service.Movies();

            Assert.Equal(2, movies.Count);
            Assert.Contains(movies, m => m.Title == "호퍼스");
            Assert.Contains(movies, m => m.Title == "삼악도");
        }

        [Fact]
        public void GetMovies_ReturnsExpectedMovieIdsAndTitles()
        {
            var service = CreateService();

            var movies = service.Movies();

            Assert.Contains(movies, m => m.Id == 1 && m.Title == "호퍼스");
            Assert.Contains(movies, m => m.Id == 2 && m.Title == "삼악도");
        }

        [Fact]
        public void GetShowTimes_ValidMovieId_ReturnsShowTimes()
        {
            var service = CreateService();

            var showTimes = service.ShowTimes(1);

            Assert.Equal(2, showTimes.Count);
            Assert.Contains(showTimes, s => s.Time == "10:00");
            Assert.Contains(showTimes, s => s.Time == "14:00");
        }

        [Fact]
        public void GetShowTimes_InvalidMovieId_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ShowTimes(999));

            Assert.Equal("영화를 찾을 수 없습니다", ex.Message);
        }

        [Fact]
        public void GetShowTimes_MovieIdZero_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ShowTimes(0));

            Assert.Equal("영화를 찾을 수 없습니다", ex.Message);
        }

        [Fact]
        public void GetSeats_ValidShowTimeId_ReturnsSeats()
        {
            var service = CreateService();

            var seats = service.Seats(1);

            Assert.Equal(10, seats.Count);
            Assert.All(seats, seat => Assert.False(seat.IsReserved));
        }

        [Fact]
        public void GetSeats_InvalidShowTimeId_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.Seats(999));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void GetSeats_ShowTimeIdZero_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.Seats(0));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_ValidSeat_ReservesSuccessfully()
        {
            var service = CreateService();

            var result = service.ReserveSeat(1, 3);
            var seats = service.Seats(1);
            var seat = seats.First(s => s.Id == 3);

            Assert.Equal("예약 완료", result);
            Assert.True(seat.IsReserved);
        }

        [Fact]
        public void ReserveSeat_AfterReservation_GetSeatsReflectsUpdatedState()
        {
            var service = CreateService();

            service.ReserveSeat(1, 2);
            var seats = service.Seats(1);

            Assert.True(seats.First(s => s.Id == 2).IsReserved);
        }

        [Fact]
        public void ReserveSeat_InvalidShowTimeId_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(999, 1));

            Assert.Equal("상영시간 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_InvalidSeatId_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, 999));

            Assert.Equal("좌석 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_SeatIdZero_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, 0));

            Assert.Equal("좌석 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_SeatIdGreaterThanMax_ThrowsException()
        {
            var service = CreateService();

            var ex = Assert.Throws<InvalidOperationException>(() => service.ReserveSeat(1, 11));

            Assert.Equal("좌석 없음", ex.Message);
        }

        [Fact]
        public void ReserveSeat_LastSeatBoundary_ReservesSuccessfully()
        {
            var service = CreateService();

            var result = service.ReserveSeat(1, 10);

            Assert.Equal("예약 완료", result);
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
    }
}




