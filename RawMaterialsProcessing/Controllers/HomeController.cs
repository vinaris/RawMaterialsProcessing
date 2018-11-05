using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using RawMaterialsProcessing.Data;
using RawMaterialsProcessing.Models;
using RawMaterialsProcessing.Services;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.Net.Http.Headers;

namespace RawMaterialsProcessing.Controllers
{
    public class HomeController : Controller
    {
        private readonly IRepository _repository;
        private List<Work> _works;
        private static int startHour = 8;

        public HomeController(IRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index(List<string> successMessages, List<string> dangerMessages)
        {
            ViewBag.Nomenclature = _repository.GetAllNomenclature().Result;
            ViewBag.MachineTools = _repository.GetAllMachineTools().Result;
            ViewBag.Operation = _repository.GetAllOperations().Result;
            ViewBag.Parties = _repository.GetAllParties().Result;
            ViewBag.SuccessMessages = successMessages;
            ViewBag.DangerMessages = dangerMessages;
            return View();
        }

        public async Task<IActionResult> UploadData(List<IFormFile> files)
        {
            var machineTools = new List<MachineTool>();
            var nomenclature = new List<Nomenclature>();
            var parties = new List<Party>();
            var operations = new List<Operation>();
            var successMessages = new List<string>();
            var dangerMessages = new List<string>();
            foreach (var formFile in files.OrderBy(f => f.FileName))
            {
                if (formFile.FileName.Contains("nomenclatures") 
                    || formFile.FileName.Contains("machine_tools") 
                    || formFile.FileName.Contains("parties")
                    || formFile.FileName.Contains("times"))
                {
                    var stream = formFile.OpenReadStream();
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        reader.Read(); //Пропуск первой строки
                        while (reader.Read())
                        {
                            try
                            {
                                if (formFile.FileName.Contains("nomenclatures"))
                                {
                                    nomenclature.Add(new Nomenclature
                                    {
                                        Id = reader[0].ToString(),
                                        Name = reader[1].ToString()
                                    });
                                }
                                if (formFile.FileName.Contains("machine_tools"))
                                {
                                    machineTools.Add(new MachineTool()
                                    {
                                        Id = reader[0].ToString(),
                                        Name = reader[1].ToString()
                                    });
                                }
                                if (formFile.FileName.Contains("parties"))
                                {
                                    parties.Add(new Party()
                                    {
                                        Id = reader[0].ToString(),
                                        NomenclatureId = reader[1].ToString()
                                    });
                                }
                                if (formFile.FileName.Contains("times"))
                                {
                                    operations.Add(new Operation()
                                    {
                                        MachineToolId = reader[0].ToString(),
                                        NomenclatureId = reader[1].ToString(),
                                        Duration = Convert.ToInt32(reader[2])
                                    });
                                }
                            }
                            catch (Exception)
                            {
                                dangerMessages.Add("Ошибка чтения файла " + formFile.FileName + "!");
                                break;
                            }
                        }
                    }
                }
                else
                {
                    dangerMessages.Add("Файл " + formFile.FileName + " не поддерживается!");
                }
            }

            if (dangerMessages.Count == 0)
            {
                if (machineTools.Count > 0)
                {

                        if (await _repository.AddRangeMachineTool(machineTools))
                        {
                            successMessages.Add("Оборудование загружено!");
                        }
                        else
                        {
                            dangerMessages.Add("Оборудование не загружено!");
                        }
 

                }

                if (nomenclature.Count > 0)
                {
  
                        if (await _repository.AddRangeNomenclature(nomenclature) && nomenclature.Count > 0)
                        {
                            successMessages.Add("Номенклатура загружена!");
                        }
                        else
                        {
                            dangerMessages.Add("Номенклатура не загружена!");
                        }

                }

                if (parties.Count > 0)
                {
                    if (await _repository.RemoveAllParties())
                    {
                        if (await _repository.AddRangeParty(parties) && parties.Count > 0)
                        {
                            successMessages.Add("Партии загружены!");
                        }
                        else
                        {
                            dangerMessages.Add("Партии не загружены!");
                        }
                    }
                    else
                    {
                        dangerMessages.Add("Очистка партий не выполнена!");
                    }
                }

                if (operations.Count > 0)
                {
                    if (await _repository.RemoveAllOperations())
                    {
                        if (await _repository.AddRangeOperation(operations) && operations.Count > 0)
                        {
                            successMessages.Add("Операции загружены!");
                        }
                        else
                        {
                            dangerMessages.Add("Операции не загружены!");
                        }
                    }
                    else
                    {
                        dangerMessages.Add("Очистка операций не выполнена!");
                    }
                }
            }

            return RedirectToAction("Index", new { successMessages, dangerMessages });
        }

        public async Task<IActionResult> ClearDatabase()
        {
            await _repository.RemoveAllData();
            return RedirectToAction("Index");
        }

        private static int GetProcessTime(MachineTool machineTool, Nomenclature nomenclature, ICollection<Operation> operations)
        {
            if (operations == null || machineTool == null || nomenclature == null) return 0;
            var operation = operations.SingleOrDefault(o => o.MachineTool == machineTool && o.Nomenclature == nomenclature);
            if (operation != null)
            {
                return operation.Duration;
            }
            return 0;
        }

        public IActionResult MakeASchedule(bool? optimized = true)
        {
            var machineTools = _repository.GetAllMachineTools().Result;
            var parties = _repository.GetAllParties().Result;
            var operations = _repository.GetAllOperations().Result;
            if (machineTools.Count > 0 && operations.Count > 0 && parties.Count > 0)
            {
                _works = new List<Work>();

                foreach (var machine in machineTools)
                {
                    _works.Add(new Work{MachineTool = machine});
                }

                if (optimized.HasValue && optimized.Value)
                {
                    var processedParties = new List<Party>(parties);
                    while (processedParties.Count > 0)
                    {
                        foreach (var work in _works.OrderBy(o => o.TotalTime))
                        {
                            var currentTool = work;
                            var flag = false;
                            var nomenclaturePriority = operations.Where(p => p.MachineTool == currentTool.MachineTool)
                                .OrderBy(d => d.Duration).Select(n => new
                                {
                                    n.Nomenclature,
                                    n.Duration,
                                    CountOfTools = operations.Count(c => c.Nomenclature == n.Nomenclature)
                                });
                            foreach (var nomenclature in nomenclaturePriority.OrderBy(n => n.CountOfTools))
                            {
                                var foundedParty = processedParties.FirstOrDefault(n => n.Nomenclature == nomenclature.Nomenclature);
                                if (foundedParty != null)
                                {
                                    currentTool.Parties.Add(new PartyProcessing
                                    {
                                        Party = foundedParty,
                                        StartTime = currentTool.TotalTime,
                                        EndTime = currentTool.TotalTime + nomenclature.Duration
                                    });
                                    currentTool.TotalTime += nomenclature.Duration;
                                    processedParties.Remove(foundedParty);
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag)
                            {
                                break;
                            }
                        }
                    }
                }
                else
                {
                    foreach (var party in parties)
                    {
                        foreach (var work in _works.OrderBy(w => w.TotalTime))
                        {
                            var processTime = GetProcessTime(work.MachineTool, party.Nomenclature, operations);
                            if (processTime > 0)
                            {
                                work.Parties.Add(new PartyProcessing { Party = party, StartTime = work.TotalTime, EndTime = work.TotalTime + processTime });
                                work.TotalTime += processTime;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", new { dangerMessages = new List<string>{"Нет данных для составления расписания!"} });
            }
            ViewBag.Optimized = optimized;
            ViewBag.StartHour = startHour;
            return View(_works);
        }

        [HttpPost]
        public FileStreamResult SaveToExcel(List<Work> model)
        {
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Расписание");
                worksheet.Cells.Style.Font.Size = 14;
                var rowCounter = 1;
                var curTime = DateTime.Today.AddHours(startHour);
                foreach (var item in model)
                {
                    worksheet.Cells[rowCounter, 1].Value = "Расписание на " + curTime.ToShortDateString();
                    worksheet.Cells[rowCounter, 1, rowCounter, 5].Merge = true;
                    worksheet.Cells[rowCounter, 1, rowCounter, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells[rowCounter, 1].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 1].Style.Font.Size = 15;
                    rowCounter++;
                    worksheet.Cells[rowCounter, 1].Value = item.MachineTool.Name;
                    worksheet.Cells[rowCounter, 1].Style.Font.Bold = true;
                    worksheet.Cells[rowCounter, 1].Style.Font.Size = 15;
                    worksheet.Cells[rowCounter, 2].Value = "Количество:";
                    worksheet.Cells[rowCounter, 3].Value = item.Parties.Count;
                    worksheet.Cells[rowCounter, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    worksheet.Cells[rowCounter, 4].Value = "Общее время:";
                    worksheet.Cells[rowCounter, 5].Value = item.TotalTime;
                    worksheet.Cells[rowCounter, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                    rowCounter++;
                    using (var range = worksheet.Cells[rowCounter, 1, rowCounter, 5])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Font.Size = 14;
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                        range.Style.Font.Color.SetColor(Color.White);
                    }
                    worksheet.Cells[rowCounter, 1].Value = "Начало";
                    worksheet.Cells[rowCounter, 2].Value = "Окончание";
                    worksheet.Cells[rowCounter, 3].Value = "Номер партии";
                    worksheet.Cells[rowCounter, 4].Value = "Номенклатура";
                    worksheet.Cells[rowCounter, 5].Value = "Время выполнения";
                    rowCounter++;
                    foreach (var partyProcessing in item.Parties)
                    {
                        worksheet.Cells[rowCounter, 1].Value = curTime.AddMinutes(partyProcessing.StartTime).ToShortTimeString();
                        worksheet.Cells[rowCounter, 2].Value = curTime.AddMinutes(partyProcessing.EndTime).ToShortTimeString();
                        worksheet.Cells[rowCounter, 3].Value = partyProcessing.Party.Id;
                        worksheet.Cells[rowCounter, 4].Value = partyProcessing.Party.Nomenclature.Name;
                        worksheet.Cells[rowCounter, 5].Value = partyProcessing.Duration;
                        rowCounter++;
                    }
                    worksheet.Row(rowCounter).PageBreak = true;
                    rowCounter++;
                }
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.Cells.AutoFitColumns(0);
                var stream = new MemoryStream(package.GetAsByteArray());
                return new FileStreamResult(stream, new MediaTypeHeaderValue("application/octet-stream"))
                {
                    FileDownloadName = "Расписание на " + DateTime.Now.ToShortDateString() + ".xlsx"
                };
            }
        }

        [HttpPost]
        public IActionResult CreateGraph(List<Work> model)
        {
            if (model.Count > 0)
            {
                ViewBag.MachineTools = model.Select(m => m.MachineTool.Name).ToArray();
                ViewBag.PartiesCount = model.Select(m => m.Parties.Count).ToArray();
                ViewBag.TotalTimes = model.Select(m => m.TotalTime).ToArray();

                DateTime starttTime = DateTime.Today.AddHours(startHour);
                List<object> items = new List<object>();
                List<object> groups = new List<object>();
                var groupId = 1;
                var itemId = 1;
                foreach (var group in model)
                {
                    groups.Add(new
                    {
                        id = groupId,
                        content = group.MachineTool.Name
                    });
                    foreach (var item in group.Parties)
                    {
                        var startWorkTime = starttTime.AddMinutes(item.StartTime);
                        var endWorkTime = starttTime.AddMinutes(item.EndTime);
                        items.Add(new
                        {
                            id = itemId,
                            content = item.Party.Nomenclature.Name + " (" + item.Party.Id + ")",
                            group = groupId,
                            start = startWorkTime,
                            end = endWorkTime
                        });
                        itemId++;
                    }
                    groupId++;
                }
                ViewBag.TimeLineItems = items;
                ViewBag.TimeLineGroups = groups;
                return View("Schedule");
            }
            return RedirectToAction("Index", new { dangerMessages = new List<string> { "Нет данных для построения графиков!" } });
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
