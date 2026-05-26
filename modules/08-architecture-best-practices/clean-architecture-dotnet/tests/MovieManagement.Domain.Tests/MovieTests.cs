using MovieManagement.Domain.Common;
using MovieManagement.Domain.Movies;

namespace MovieManagement.Domain.Tests;

public class MovieTests
{
    // A small helper so each test starts from a valid movie.
    private static Movie CreateValidMovie() => Movie.Create(
        title: "Inception",
        director: "Christopher Nolan",
        releaseDate: new DateOnly(2010, 7, 16),
        genre: Genre.SciFi,
        synopsis: "A thief who steals secrets through dreams.");

    [Fact]
    public void Create_WithValidData_SetsTheProperties()
    {
        var movie = CreateValidMovie();

        Assert.Equal("Inception", movie.Title);
        Assert.Equal("Christopher Nolan", movie.Director);
        Assert.Equal(Genre.SciFi, movie.Genre);
        Assert.NotEqual(Guid.Empty, movie.Id);
    }

    [Fact]
    public void Create_TrimsExtraSpacesFromText()
    {
        var movie = Movie.Create("  Dune  ", "  Denis Villeneuve  ", new DateOnly(2021, 10, 22), Genre.SciFi, "  Spice.  ");

        Assert.Equal("Dune", movie.Title);
        Assert.Equal("Denis Villeneuve", movie.Director);
        Assert.Equal("Spice.", movie.Synopsis);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyTitle_Throws(string badTitle)
    {
        var error = Assert.Throws<DomainException>(() =>
            Movie.Create(badTitle, "Some Director", new DateOnly(2020, 1, 1), Genre.Drama, "Plot."));

        Assert.Equal("A movie must have a title.", error.Message);
    }

    [Fact]
    public void Create_WithEmptyDirector_Throws()
    {
        Assert.Throws<DomainException>(() =>
            Movie.Create("A Title", "", new DateOnly(2020, 1, 1), Genre.Drama, "Plot."));
    }

    [Fact]
    public void AddRating_WithFirstScore_SetsAverageAndCount()
    {
        var movie = CreateValidMovie();

        movie.AddRating(8);

        Assert.Equal(8, movie.AverageRating);
        Assert.Equal(1, movie.RatingCount);
    }

    [Fact]
    public void AddRating_WithThreeScores_KeepsARunningAverage()
    {
        var movie = CreateValidMovie();

        movie.AddRating(10);
        movie.AddRating(8);
        movie.AddRating(6);

        Assert.Equal(8, movie.AverageRating);
        Assert.Equal(3, movie.RatingCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    [InlineData(-5)]
    public void AddRating_OutsideOneToTen_Throws(int badScore)
    {
        var movie = CreateValidMovie();

        Assert.Throws<DomainException>(() => movie.AddRating(badScore));
    }
}
