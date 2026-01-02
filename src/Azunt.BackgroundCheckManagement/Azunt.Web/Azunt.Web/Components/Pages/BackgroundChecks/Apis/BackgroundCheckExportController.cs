using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Globalization;

// Open XML SDK
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using Azunt.BackgroundCheckManagement;

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

            if (items == null || !items.Any())
                return NotFound("No background check records found.");

            byte[] bytes;

            using (var ms = new System.IO.MemoryStream())
            {
                using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
                {
                    var wbPart = doc.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();

                    var wsPart = wbPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    wsPart.Worksheet = new Worksheet(sheetData);

                    var sheets = wbPart.Workbook.AppendChild(new Sheets());
                    sheets.Append(new Sheet
                    {
                        Id = wbPart.GetIdOfPart(wsPart),
                        SheetId = 1U,
                        Name = "BackgroundChecks"
                    });

                    //-------------------------------------------------
                    //  Header
                    //-------------------------------------------------
                    uint headerRowIndex = 1;
                    var headerRow = new Row { RowIndex = headerRowIndex };
                    sheetData.Append(headerRow);

                    string[] headers =
                    {
                        "Id",
                        "BackgroundCheckId",
                        "BackgroundStatus",
                        "CompletedAt",
                        "CreatedAt",
                        "CreatedBy",
                        "EmployeeId",
                        "Provider",
                        "Score",
                        "Status",
                        "UpdatedAt"
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        headerRow.Append(TextCell(Ref(i + 1, (int)headerRowIndex), headers[i]));
                    }

                    //-------------------------------------------------
                    //  Rows
                    //-------------------------------------------------
                    uint rowIndex = 2;

                    foreach (var m in items)
                    {
                        var row = new Row { RowIndex = rowIndex };
                        sheetData.Append(row);

                        var values = new[]
                        {
                            m.Id.ToString(),
                            m.BackgroundCheckId?.ToString() ?? string.Empty,
                            m.BackgroundStatus ?? string.Empty,
                            ConvertDate(m.CompletedAt),
                            ConvertDate(m.CreatedAt),
                            m.CreatedBy ?? string.Empty,
                            m.EmployeeId?.ToString() ?? string.Empty,
                            m.Provider ?? string.Empty,
                            m.Score?.ToString() ?? string.Empty,
                            m.Status ?? string.Empty,
                            ConvertDate(m.UpdatedAt)
                        };

                        for (int i = 0; i < values.Length; i++)
                        {
                            row.Append(TextCell(Ref(i + 1, (int)rowIndex), values[i]));
                        }

                        rowIndex++;
                    }

                    wsPart.Worksheet.Save();
                    wbPart.Workbook.Save();
                }

                bytes = ms.ToArray();
            }

            var fileName = $"{DateTime.Now:yyyyMMddHHmmss}_BackgroundChecks.xlsx";

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }

        // ============================================
        // Helpers
        // ============================================
        private static string ConvertDate(DateTimeOffset? dto)
        {
            return dto.HasValue
                ? dto.Value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
                : string.Empty;
        }

        private static Cell TextCell(string cellRef, string text) =>
            new Cell
            {
                CellReference = cellRef,
                DataType = CellValues.String,
                CellValue = new CellValue(text ?? string.Empty)
            };

        private static string Ref(int col1Based, int row) => $"{ColName(col1Based)}{row}";

        private static string ColName(int index)
        {
            var dividend = index;
            string col = string.Empty;

            while (dividend > 0)
            {
                var modulo = (dividend - 1) % 26;
                col = (char)('A' + modulo) + col;
                dividend = (dividend - modulo) / 26;
            }

            return col;
        }
    }
}
