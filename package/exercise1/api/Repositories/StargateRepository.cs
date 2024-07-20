using MediatR;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;
using System;

namespace StargateAPI.Repositories;

public interface IStargateRepository
{
    #region [ Readers ]

    Task<Person?> GetAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<List<PersonAstronaut>> GetAllAstronautDetailsAsync(
        CancellationToken cancellationToken = default);

    Task<PersonAstronaut?> GetAstronautDetailsByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<List<AstronautDuty>> GetAstronautDutiesByAstronautIdAsync(
        int personId,
        CancellationToken cancellationToken = default);

    Task<Person?> GetUntrackedAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<AstronautDetail?> GetAstronautDetailByIdAsync(
        int id,
        CancellationToken cancellationToken= default);

    #endregion

    #region [ Writers ]

    Task<int> AddAstronautAsync(
        Person astronaut,
        CancellationToken cancellationToken = default);

    Task<int> AddAstronautDetailAsync(
        AstronautDetail detail,
        CancellationToken cancellationToken = default);

    Task<int> UpdateAstronautNameAsync(
        Person astronaut,
        string newName,
        CancellationToken cancellationToken = default);

    Task<int> UpdateAstronautDetailAsync(
        AstronautDetail currentDetail,
        CreateAstronautDuty newDetail,
        CancellationToken cancellationToken = default);

    #endregion
}

public class StargateRepository : IStargateRepository
{
    /// <summary>Represents the context for interacting with the Stargate database.</summary>
    private readonly StargateContext _context;

    public StargateRepository(StargateContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region [ Readers ]

    public async Task<Person?> GetAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.People
            .FirstOrDefaultAsync(x => x.Name.ToUpper().Equals(name.ToUpper()));
    }

    public async Task<List<PersonAstronaut>> GetAllAstronautDetailsAsync(
        CancellationToken cancellationToken = default)
    {
        return await (from p in _context.People
                      from ad in _context.AstronautDetails.Where(x => x.PersonId == p.Id)
                          .DefaultIfEmpty()
                      select new PersonAstronaut
                      {
                          PersonId = p.Id,
                          Name = p.Name,
                          CurrentRank = ad.CurrentRank,
                          CurrentDutyTitle = ad.CurrentDutyTitle,
                          CareerStartDate = ad.CareerStartDate,
                          CareerEndDate = ad.CareerEndDate
                      }).ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<PersonAstronaut?> GetAstronautDetailsByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        return await (from p in _context.People
                      where p.Name.ToUpper().Equals(name.ToUpper())
                      from ad in _context.AstronautDetails.Where(x => x.PersonId == p.Id)
                         .DefaultIfEmpty()
                      select new PersonAstronaut
                      {
                          PersonId = p.Id,
                          Name = p.Name,
                          CurrentRank = ad.CurrentRank,
                          CurrentDutyTitle = ad.CurrentDutyTitle,
                          CareerStartDate = ad.CareerStartDate,
                          CareerEndDate = ad.CareerEndDate
                      }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AstronautDuty>> GetAstronautDutiesByAstronautIdAsync(
        int personId,
        CancellationToken cancellationToken = default)
    {
        if (personId > 0)
        {
            throw new ArgumentNullException(nameof(personId));
        }

        return await _context.AstronautDuties
            .OrderByDescending(x => x.DutyStartDate)
            .Where(x => x.PersonId == personId)
            .ToListAsync();
    }

    public async Task<Person?> GetUntrackedAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _context.People
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name.ToUpper().Equals(name.ToUpper()));
    }

    public async Task<AstronautDetail?> GetAstronautDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        return await _context.AstronautDetails.FirstOrDefaultAsync(x => x.PersonId == id);
    }

    #endregion

    #region [ Writers ]

    public async Task<int> AddAstronautAsync(
        Person astronaut,
        CancellationToken cancellationToken = default)
    {
        if (astronaut is null)
        {
            throw new ArgumentNullException(nameof(astronaut));
        }

        _ = await _context.People.AddAsync(astronaut, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddAstronautDetailAsync(
        AstronautDetail detail,
        CancellationToken cancellationToken = default)
    {
        if (detail is null)
        {
            throw new ArgumentNullException(nameof(detail));
        }

        _ = await _context.AstronautDetails.AddAsync(detail, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAstronautNameAsync(
        Person astronaut,
        string newName,
        CancellationToken cancellationToken = default)
    {
        if (astronaut is null)
        {
            throw new ArgumentNullException(nameof(astronaut));
        }
        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentNullException(nameof(newName));
        }

        astronaut.Name = newName;
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAstronautDetailAsync(
        AstronautDetail currentDetail,
        CreateAstronautDuty newDetail,
        CancellationToken cancellationToken = default)
    {
        if (currentDetail is null)
        {
            throw new ArgumentNullException(nameof(currentDetail));
        }
        if (newDetail is null)
        {
            throw new ArgumentNullException(nameof(newDetail));
        }

        currentDetail.CurrentDutyTitle = newDetail.DutyTitle;
        currentDetail.CurrentRank = newDetail.Rank;
        return await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}