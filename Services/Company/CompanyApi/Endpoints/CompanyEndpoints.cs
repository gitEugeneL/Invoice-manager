using CompanyApi.Models.Dto;
using CompanyApi.Models.Entities;
using CompanyApi.Repositories.Interfaces;
using CompanyApi.Utils;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace CompanyApi.Endpoints;

public static class CompanyEndpoints
{
    public static void MapCompanyEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("api/v1/company")
            .WithTags("Company")
            .RequireAuthorization("base-policy");
        
        group.MapPost("", CreateCompany)
            .WithValidator<CreateCompanyDto>()
            .Produces<CompanyResponseDto>(StatusCodes.Status201Created);
        
        group.MapPut("", UpdateCompany)
            .WithValidator<UpdateCompanyDto>()
            .Produces<CompanyResponseDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("{companyId:guid}", GetCompanyById)
            .Produces<CompanyResponseDto>()
            .Produces(StatusCodes.Status404NotFound);
        
        group.MapGet("", GetAllCompanies)
            .Produces<PaginatedResponse<CompanyResponseDto>>();
        
        group.MapDelete("{companyId:guid}", DeleteCompanyById)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
    private static async Task<IResult> CreateCompany(
        [FromBody] CreateCompanyDto dto,
        HttpContext httpContext,
        ICompanyRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var company = await repository.CreateCompany(
            new Company
            {
                CreatedByUserId = userId,
                Name = dto.Name,
                TaxNumber = dto.TaxNumber,
                City = dto.City,
                Street = dto.Street,
                HouseNumber = dto.HouseNumber,
                PostalCode = dto.PostalCode
            }
        );
        return TypedResults
            .Created(company.Id.ToString() , new CompanyResponseDto(company));
    }

    private static async Task<Results<NotFound<string>, ForbidHttpResult, Ok<CompanyResponseDto>>> UpdateCompany(
        [FromBody] UpdateCompanyDto dto,
        HttpContext httpContext,
        ICompanyRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var company = await repository.FindCompanyById(dto.CompanyId);
        
        if (company is null)
            return TypedResults.NotFound($"Company: {dto.CompanyId} not found");
        
        if (company.CreatedByUserId != userId)
            return TypedResults.Forbid();
    
        company.Name = dto.Name ?? company.Name;
        company.TaxNumber = dto.TaxNumber ?? company.TaxNumber;
        company.City = dto.City ?? company.City;
        company.Street = dto.Street ?? company.Street;
        company.HouseNumber = dto.HouseNumber ?? company.HouseNumber;
        company.PostalCode = dto.PostalCode ?? company.PostalCode;
        await repository.UpdateCompany(company);
    
        return TypedResults
            .Ok(new CompanyResponseDto(company));
    }
    
    private static async Task<Results<Ok<CompanyResponseDto>, NotFound<string>>> GetCompanyById(
        Guid companyId,
        HttpContext httpContext,
        ICompanyRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var company = await repository.FindCompanyById(companyId);
        
        if (company is null || company.CreatedByUserId != userId)
            return TypedResults.NotFound($"Company: {companyId} not found");
    
        return TypedResults
            .Ok(new CompanyResponseDto(company));
    }
    
    private static async Task<IResult> GetAllCompanies(
        [AsParameters] QueryParameters parameters, 
        HttpContext httpContext,
        ICompanyRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var (companies, count) = 
            await repository.FindCompaniesByOwnerId(userId, parameters.PageNumber, parameters.PageSize);
    
        return TypedResults
            .Ok(new PaginatedResponse<CompanyResponseDto>(
                companies
                    .Select(c => new CompanyResponseDto(c))
                    .ToList(),
                count,
                parameters.PageNumber,
                parameters.PageSize));
    }
    
    private static async Task<Results<NoContent, NotFound<string>>> DeleteCompanyById(
        Guid companyId,
        HttpContext httpContext,
        ICompanyRepository repository)
    {
        var userId = BaseService.ReadUserIdFromToken(httpContext);
        var company = await repository.FindCompanyById(companyId);
    
        if (company is null || company.CreatedByUserId != userId)
            return TypedResults.NotFound($"Company: {companyId} not found");
    
        await repository.DeleteCompany(company);
        return TypedResults.NoContent();
    }
}