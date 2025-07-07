using Microsoft.AspNetCore.Mvc;
using Azunt.BackgroundCheckManagement;
using System.Threading.Tasks;
using System.IO;
using System;

namespace Azunt.Apis.BackgroundChecks
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundCheckFileController : ControllerBase
    {
        private readonly IBackgroundCheckStorageService _storageService;
        private readonly IBackgroundCheckRepository _backgroundCheckRepository;

        public BackgroundCheckFileController(IBackgroundCheckStorageService storageService, IBackgroundCheckRepository backgroundCheckRepository)
        {
            _storageService = storageService;
            _backgroundCheckRepository = backgroundCheckRepository;
        }

        /// <summary>
        /// 파일명을 이용한 직접 다운로드
        /// GET /api/BackgroundCheckFile/{fileName}
        /// </summary>
        [HttpGet("{fileName}")]
        public async Task<IActionResult> DownloadByFileName(string fileName)
        {
            try
            {
                var stream = await _storageService.DownloadAsync(fileName);
                return File(stream, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound($"File not found: {fileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Download error: {ex.Message}");
            }
        }

        /// <summary>
        /// BackgroundCheck ID를 통해 파일명을 조회하여 다운로드
        /// GET /api/BackgroundCheckFile/ById/{id}
        /// </summary>
        [HttpGet("ById/{id}")]
        public async Task<IActionResult> DownloadById(long id)
        {
            var backgroundCheck = await _backgroundCheckRepository.GetByIdAsync(id);
            if (backgroundCheck == null)
            {
                return NotFound($"BackgroundCheck with Id {id} not found.");
            }

            if (string.IsNullOrEmpty(backgroundCheck.FileName))
            {
                return NotFound($"No file attached to backgroundCheck with Id {id}.");
            }

            try
            {
                var stream = await _storageService.DownloadAsync(backgroundCheck.FileName);
                return File(stream, "application/octet-stream", backgroundCheck.FileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound($"File not found: {backgroundCheck.FileName}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Download error: {ex.Message}");
            }
        }
    }
}