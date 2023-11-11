using AutoMapper;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;
using WebApplication1.Dto;
using WebApplication1.Interface;
using WebApplication1.Models;

namespace ProkemonReviewApp.Tests.Services.Controllers
{
    public class PokemonControllerTests
    {
        private readonly IPokemonRepository _pokemonRepository;
        private readonly IReviewRepository _reviewRepository;
        private readonly IMapper _mapper;
        public PokemonControllerTests()
        {
            _pokemonRepository = A.Fake<IPokemonRepository>();
            _mapper = A.Fake<IMapper>();
            _reviewRepository = A.Fake<IReviewRepository>();    
        }

        [Fact]
        public void PokemonController_GetPokemons_ReturnOK(){
            var pokemons = A.Fake<ICollection<PokemonDto>>();
            var pokemonList = A.Fake<List<PokemonDto>>();
            A.CallTo(() => _mapper.Map<List<PokemonDto>>(pokemons)).Returns(pokemonList);
            var pokemonController = new PokemonController(_pokemonRepository, _reviewRepository,_mapper);
            var result = pokemonController.GetPokemons();
            result.Should().NotBeNull();
            result.Should().BeOfType(typeof(OkObjectResult));
        }

        [Fact]
        public void PokemonController_CreatePokemon_ReturnOK()
        {
            int ownerId = 1;
            int categoryId = 2;
            var pokemon = A.Fake<Pokemon>();
            var pokemonCreate = A.Fake<PokemonDto>();
            var pokemons = A.Fake<ICollection<PokemonDto>>();
            var pokemonList = A.Fake<List<PokemonDto>>();
            A.CallTo(() => _pokemonRepository.GetPokemonTrimToUpper(pokemonCreate)).Returns(pokemon);
            A.CallTo(() => _mapper.Map<Pokemon>(pokemonCreate)).Returns(pokemon);
            A.CallTo(() => _pokemonRepository.CreatePokemon(ownerId, categoryId, pokemon)).Returns(true);
            var pokemonController = new PokemonController(_pokemonRepository, _reviewRepository, _mapper);
            var result = pokemonController.CreatePokemon(ownerId, categoryId, pokemonCreate);
            result.Should().NotBeNull();
        }
    }
}
