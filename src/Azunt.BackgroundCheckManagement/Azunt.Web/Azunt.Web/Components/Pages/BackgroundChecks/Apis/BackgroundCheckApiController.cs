using Azunt.BackgroundCheckManagement;
using Microsoft.AspNetCore.Mvc;

namespace Azunt.Web.Components.Pages.BackgroundChecks.Apis;

[ApiController]
[Route("api/[controller]")]
public class BackgroundCheckApiController : ControllerBase
{
    private readonly IBackgroundCheckRepository _backgroundCheckRepository;

    public BackgroundCheckApiController(IBackgroundCheckRepository backgroundCheckRepository)
    {
        _backgroundCheckRepository = backgroundCheckRepository;
    }

    // GET: api/BackgroundCheckApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BackgroundCheckDto>>> GetBackgroundChecks()
    {
        var backgroundChecks = await _backgroundCheckRepository.GetAllAsync();

        var result = backgroundChecks.Select(b => new BackgroundCheckDto
        {
            Id = b.Id,
            BackgroundStatus = b.BackgroundStatus,
            CompletedAt = b.CompletedAt,
            CreatedBy = b.CreatedBy,
            CreatedAt = b.CreatedAt,
            Provider = b.Provider,
            Score = b.Score,
            Status = b.Status
        });

        return Ok(result);
    }

    // GET: api/BackgroundCheckApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BackgroundCheckDto>> GetBackgroundCheck(long id)
    {
        var b = await _backgroundCheckRepository.GetByIdAsync(id);
        if (b == null)
        {
            return NotFound();
        }

        var dto = new BackgroundCheckDto
        {
            Id = b.Id,
            BackgroundStatus = b.BackgroundStatus,
            CompletedAt = b.CompletedAt,
            CreatedBy = b.CreatedBy,
            CreatedAt = b.CreatedAt,
            Provider = b.Provider,
            Score = b.Score,
            Status = b.Status
        };

        return Ok(dto);
    }

    // POST: api/BackgroundCheckApi
    [HttpPost]
    public async Task<ActionResult<BackgroundCheckDto>> PostBackgroundCheck(BackgroundCheckDto dto)
    {
        var model = new BackgroundCheck
        {
            BackgroundStatus = dto.BackgroundStatus,
            CompletedAt = dto.CompletedAt,
            CreatedBy = dto.CreatedBy,
            Provider = dto.Provider,
            Score = dto.Score,
            Status = dto.Status,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var result = await _backgroundCheckRepository.AddAsync(model);

        var resultDto = new BackgroundCheckDto
        {
            Id = result.Id,
            BackgroundStatus = result.BackgroundStatus,
            CompletedAt = result.CompletedAt,
            CreatedBy = result.CreatedBy,
            CreatedAt = result.CreatedAt,
            Provider = result.Provider,
            Score = result.Score,
            Status = result.Status
        };

        return CreatedAtAction(nameof(GetBackgroundCheck), new { id = result.Id }, resultDto);
    }

    // PUT: api/BackgroundCheckApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutBackgroundCheck(long id, BackgroundCheckDto dto)
    {
        if (id != dto.Id) return BadRequest();

        var model = await _backgroundCheckRepository.GetByIdAsync(id);
        if (model == null) return NotFound();

        model.BackgroundStatus = dto.BackgroundStatus;
        model.CompletedAt = dto.CompletedAt;
        model.CreatedBy = dto.CreatedBy;
        model.Provider = dto.Provider;
        model.Score = dto.Score;
        model.Status = dto.Status;
        model.UpdatedAt = DateTimeOffset.UtcNow;

        await _backgroundCheckRepository.UpdateAsync(model);
        return NoContent();
    }

    // DELETE: api/BackgroundCheckApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBackgroundCheck(long id)
    {
        var success = await _backgroundCheckRepository.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
