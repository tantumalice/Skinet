using Core.Entites;
using Microsoft.AspNetCore.Mvc;
using Core.Interfaces;
using Core.Specifications;
using AutoMapper;
using API.Errors;
using API.Helpers;
using API.DTO;

namespace API.Controllers;

public class ProductsController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductsController(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    [HttpGet]
    [Cached(600)]
    public async Task<ActionResult<List<Pagination<ProductToReturnDto>>>> GetProducts(
        [FromQuery] ProductSpecParams productParams)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(productParams);
        var countSpec = new ProductWithFiltersForCountSpecification(productParams);

        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpec);
        var products = await _unitOfWork.Repository<Product>().ListAsync(spec);

        var data = _mapper
            .Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

        return Ok(new Pagination<ProductToReturnDto>(productParams.PageIndex,
            productParams.PageSize, totalItems, data));
    }

    [HttpGet("{id}")]
    [Cached(600)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
        var spec = new ProductsWithTypesAndBrandsSpecification(id);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpec(spec);

        if (product == null)
        {
            return NotFound(new ApiResponse(404));
        }

        return _mapper.Map<Product, ProductToReturnDto>(product);
    }

    [HttpGet("brands")]
    [Cached(600)]
    public async Task<ActionResult<List<ProductBrand>>> GetBrands()
    {
        return Ok(await _unitOfWork.Repository<ProductBrand>().ListAllAsync());
    }

    [HttpGet("types")]
    [Cached(600)]
    public async Task<ActionResult<List<ProductType>>> GetTypes()
    {
        return Ok(await _unitOfWork.Repository<ProductType>().ListAllAsync());
    }
}
