using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Dtos;

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

    Task<AstronautDuty?> GetAstronautDutyByIdTitleStartDateAsync(
        int id,
        string title,
        DateTime startDate,
        CancellationToken cancellationToken = default);

    Task<AstronautDuty?> GetMostRecentAstronautDutyByAstronautIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<Person?> GetUntrackedAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<AstronautDetail?> GetAstronautDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    #endregion

    #region [ Writers ]

    Task<int> AddAstronautAsync(
        Person astronaut,
        CancellationToken cancellationToken = default);

    Task<int> AddAstronautDetailAsync(
        AstronautDetail detail,
        CancellationToken cancellationToken = default);

    Task<int> AddAstronautDutyAsync(
        AstronautDuty duty,
        CancellationToken cancellationToken = default);

    Task<int> UpdateAstronautNameAsync(
        Person astronaut,
        string newName,
        CancellationToken cancellationToken = default);

    Task<int> UpdateAstronautDetailAsync(
        AstronautDetail currentDetail,
        CreateAstronautDuty newDetail,
        CancellationToken cancellationToken = default);

    Task<int> UpdateAstronautDutyAsync(
        AstronautDuty currentDuty,
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
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

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
        if (personId <= 0)
        {
            throw new ArgumentNullException(nameof(personId));
        }

        // Since I'm not validating the dates in any way, they can be entered all out of order.
        // Therefore, I'm sorting by id instead of duty start date.
        return await _context.AstronautDuties
            .OrderByDescending(x => x.Id)
            .Where(x => x.PersonId == personId)
            .ToListAsync(cancellationToken);
    }

    public async Task<AstronautDuty?> GetAstronautDutyByIdTitleStartDateAsync(
        int id,
        string title,
        DateTime startDate,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentNullException(nameof(id));
        }
        if (string.IsNullOrEmpty(title))
        {
            throw new ArgumentNullException(nameof(title));
        }

        return await _context.AstronautDuties
            .FirstOrDefaultAsync(
                x => x.PersonId == id && x.DutyTitle == title && x.DutyStartDate == startDate,
                cancellationToken);
    }
    public async Task<AstronautDuty?> GetMostRecentAstronautDutyByAstronautIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentNullException(nameof(id));
        }

        // Since I'm not validating the dates in any way, they can be entered all out of order.
        // Therefore, I'm sorting by id instead of duty start date.
        return await _context.AstronautDuties
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.PersonId == id, cancellationToken);
    }

    public async Task<Person?> GetUntrackedAstronautByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        return await _context.People
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Name.ToUpper().Equals(name.ToUpper()),
                cancellationToken);
    }

    public async Task<AstronautDetail?> GetAstronautDetailByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            throw new ArgumentNullException(nameof(id));
        }

        return await _context.AstronautDetails
            .FirstOrDefaultAsync(x => x.PersonId == id, cancellationToken: cancellationToken);
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

    public async Task<int> AddAstronautDutyAsync(
        AstronautDuty duty,
        CancellationToken cancellationToken = default)
    {
        if (duty is null)
        {
            throw new ArgumentNullException(nameof(duty));
        }

        _ = await _context.AstronautDuties.AddAsync(duty, cancellationToken);
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

        currentDetail.CurrentRank = newDetail.Rank;
        currentDetail.CurrentDutyTitle = newDetail.DutyTitle;
        currentDetail.CareerStartDate = newDetail.DutyStartDate;
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> UpdateAstronautDutyAsync(
        AstronautDuty duty,
        CancellationToken cancellationToken = default)
    {
        if (duty is null)
        {
            throw new ArgumentNullException(nameof(duty));
        }

        _ = _context.AstronautDuties.Update(duty);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}