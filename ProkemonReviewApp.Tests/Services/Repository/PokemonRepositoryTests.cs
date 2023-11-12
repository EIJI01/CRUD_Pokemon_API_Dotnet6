using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repository;

namespace ProkemonReviewApp.Tests.Services.Repository
{
    public class PokemonRepositoryTests
    {
        private async Task<DataContext> GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new DataContext(options);
            databaseContext.Database.EnsureCreated(); ;
            if(await databaseContext.Pokemons.CountAsync() <= 0)
            {
                for(int i =0; i< 10; i++)
                {
                    databaseContext.Pokemons.Add(
                   new Pokemon()
                   {
                       Name = "Pikachu",
                       BirthDate = new DateTime(1903, 1, 1),
                       PokemonCategories = new List<PokemonCategory>()
                           {
                                new PokemonCategory { Category = new Category() { Name = "Electric"}}
                           },
                       Reviews = new List<Review>()
                           {
                                new Review { Title="Pikachu",Text = "Pickahu is the best pokemon, because it is electric", Rating = 2,
                                Reviewer = new Reviewer(){ FirstName = "Teddy", LastName = "Smith" } },
                                new Review { Title="Pikachu", Text = "Pickachu is the best a killing rocks", Rating = 2,
                                Reviewer = new Reviewer(){ FirstName = "Taylor", LastName = "Jones" } },
                                new Review { Title="Pikachu",Text = "Pickchu, pickachu, pikachu", Rating = 1,
                                Reviewer = new Reviewer(){ FirstName = "Jessica", LastName = "McGregor" } },
                           }
                    });
                    await databaseContext.SaveChangesAsync();
                }
            }
            return databaseContext;
        }

        [Fact]
        public async void PokemonRepository_GetPokemon_ReturnPokemon()
        {
            var name = "Pikachu";
            var dbContext = await GetDatabaseContext();
            var pokemonRepository = new PokemonRepository(dbContext);
            var result = pokemonRepository.GetPokemon(name);
            result.Should().NotBeNull();
            result.Should().BeOfType<Pokemon>();
        }
        [Fact]
        public async void PokemonRepository_GetPokemonRating_ReturnDecimalBetweenOneAndten()
        {
            var pokemonId = 1;
            var dbContext = await GetDatabaseContext();
            var pokemonRepository = new PokemonRepository(dbContext);
            var result = pokemonRepository.GetPokemonRating(pokemonId);

            result.Should().NotBe(0);
            result.Should().BeInRange(1, 10);
            result.Should().BeOfType(typeof(decimal));        
        }
    }
}
