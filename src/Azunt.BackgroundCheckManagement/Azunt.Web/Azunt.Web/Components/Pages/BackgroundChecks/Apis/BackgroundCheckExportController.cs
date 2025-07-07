using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Azunt.BackgroundCheckManagement;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace Azunt.Apis.BackgroundChecks
{
    //[Authorize(Roles = "Administrators")]
    [Route("api/[controller]")]
    [ApiController]
    public class BackgroundCheckExportController : ControllerBase
    {
        private readonly IBackgroundCheckRepository _backgroundCheckRepository;

        public BackgroundCheckExportController(IBackgroundCheckRepository backgroundCheckRepository)
        {
            _backgroundCheckRepository = backgroundCheckRepository;
        }

        /// <summary>
        /// 백그라운드체크 목록 엑셀 다운로드
        /// GET /api/BackgroundCheckExport/Excel
        /// </summary>
        [HttpGet("Excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var items = await _backgroundCheckRepository.GetAllAsync();

            if (!items.Any())
                return NotFound("No background check records found.");

            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("BackgroundChecks");

            var range = sheet.Cells["B2"].LoadFromCollection(
                items.Select(m => new
                {
                    m.Id,
                    m.BackgroundCheckId,
                    m.BackgroundStatus,
                    m.CompletedAt,
                    m.CreatedAt,
                    m.CreatedBy,
                    m.EmployeeId,
                    m.Provider,
                    m.Score,
                    m.Status,
                    m.UpdatedAt
                }),
                PrintHeaders: true
            );

            var header = sheet.Cells["B2:L2"];
            sheet.DefaultColWidth = 20;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
            range.Style.Border.BorderAround(ExcelBorderStyle.Medium);

            header.Style.Font.Bold = true;
            header.Style.Font.Color.SetColor(Color.White);
            header.Style.Fill.BackgroundColor.SetColor(Color.DarkGreen);

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{DateTime.Now:yyyyMMddHHmmss}_BackgroundChecks.xlsx");
        }
    }
}
