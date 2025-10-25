using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinkCare_IT15.Data;
using LinkCare_IT15.Models.AdminModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkCare_IT15.Controllers
{
    [Route("Admin/[controller]")]
    public class EquipmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EquipmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ================================================================
        // INDEX (VIEW)
        // ================================================================
        [HttpGet("")]
        public IActionResult Index()
        {
            var model = new AdminInventoryModel
            {
                Equipments = _context.Equipments
                    .Include(e => e.MaintenanceLogs)
                    .Where(e => !e.IsArchived)
                    .ToList(),
                Consumables = _context.Consumables.ToList()
            };

            return View("~/Views/Admin/Equipments.cshtml", model);
        }

        public IActionResult IndexConsumables()
        {
            var model = new AdminInventoryModel
            {
                Consumables = _context.Consumables.Where(c => !c.IsArchived).ToList()
            };
            return View("~/Views/Admin/Equipments.cshtml", model);
        }

        // ================================================================
        // ADD EQUIPMENT / CONSUMABLE
        // ================================================================
        [HttpPost("")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AdminInventoryModel model)
        {
            try
            {
                // -------------------------------
                // ADD NEW EQUIPMENT
                // -------------------------------
                if (model.NewEquipment != null && !string.IsNullOrEmpty(model.NewEquipment.EquipmentName))
                {
                    var eqFile = Request.Form.Files["EquipmentImageFile"];
                    if (eqFile != null && eqFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await eqFile.CopyToAsync(ms);
                        model.NewEquipment.ImageData = ms.ToArray();
                    }

                    if (model.NewEquipment.Quantity < 1)
                        model.NewEquipment.Quantity = 1;

                    if (model.NewEquipment.AcquiredDate == default)
                        model.NewEquipment.AcquiredDate = DateTime.UtcNow;

                    model.NewEquipment.IsArchived = false;
                    _context.Equipments.Add(model.NewEquipment);
                }

                // -------------------------------
                // ADD NEW CONSUMABLE
                // -------------------------------
                if (model.NewConsumable != null && !string.IsNullOrEmpty(model.NewConsumable.ConsumableName))
                {
                    var conFile = Request.Form.Files["ConsumableImageFile"];
                    if (conFile != null && conFile.Length > 0)
                    {
                        using var ms = new MemoryStream();
                        await conFile.CopyToAsync(ms);
                        model.NewConsumable.ImageData = ms.ToArray();
                    }

                    _context.Consumables.Add(model.NewConsumable);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving data: " + ex.Message);
                model.Equipments = _context.Equipments.Where(e => !e.IsArchived).ToList();
                model.Consumables = _context.Consumables.ToList();
                return View("~/Views/Admin/Equipments.cshtml", model);
            }
        }

        // ================================================================
        // EDIT EQUIPMENT (MODAL)
        // ================================================================
        [HttpPost("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EquipmentModel updatedEquipment)
        {
            if (updatedEquipment == null || updatedEquipment.EquipmentId <= 0)
                return BadRequest("Invalid equipment data.");

            try
            {
                var existing = await _context.Equipments.FindAsync(updatedEquipment.EquipmentId);
                if (existing == null) return NotFound();

                existing.EquipmentName = updatedEquipment.EquipmentName;
                existing.Category = updatedEquipment.Category;
                existing.Quantity = updatedEquipment.Quantity;
                existing.PurchaseCost = updatedEquipment.PurchaseCost;
                existing.Status = updatedEquipment.Status;
                existing.AcquiredDate = updatedEquipment.AcquiredDate;
                existing.DisposeDate = updatedEquipment.DisposeDate;
                existing.LastMaintenanceDate = updatedEquipment.LastMaintenanceDate;
                existing.NextMaintenanceDate = updatedEquipment.NextMaintenanceDate;

                var file = Request.Form.Files["EditEquipmentImageFile"];
                if (file != null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms);
                    existing.ImageData = ms.ToArray();
                }

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Equipment '{existing.EquipmentName}' updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating equipment: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // ================================================================
        // ARCHIVE EQUIPMENT
        // ================================================================
        [HttpPost("Archive/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var equipment = await _context.Equipments.FindAsync(id);
            if (equipment == null) return NotFound();

            equipment.IsArchived = true;
            await _context.SaveChangesAsync();

            TempData["Info"] = $"'{equipment.EquipmentName}' has been archived.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("AddMaintenanceLog")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaintenanceLog([FromForm] MaintenanceLog log)
        {
            if (log == null || log.EquipmentId <= 0)
                return Json(new { success = false, message = "Invalid maintenance log data." });

            try
            {
                var equipment = await _context.Equipments
                    .FirstOrDefaultAsync(e => e.EquipmentId == log.EquipmentId);

                if (equipment == null)
                    return Json(new { success = false, message = "Equipment not found." });

                // ✅ Automatically get current logged-in user
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not logged in." });

                // ✅ Create a new MaintenanceLog entry
                var maintenanceLog = new MaintenanceLog
                {
                    EquipmentId = equipment.EquipmentId,
                    EquipmentName = equipment.EquipmentName,
                    Remarks = log.Remarks,
                    MaintenanceCost = log.MaintenanceCost,
                    MaintenanceDate = log.MaintenanceDate == default ? DateTime.Now : log.MaintenanceDate,
                    UserId = userId,
                    IsArchived = false
                };

                // ✅ Update Equipment’s last maintenance date
                equipment.LastMaintenanceDate = maintenanceLog.MaintenanceDate;

                _context.MaintenanceLogs.Add(maintenanceLog);
                _context.Equipments.Update(equipment);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Maintenance log added for '{equipment.EquipmentName}'.",
                    data = new
                    {
                        maintenanceLog.MaintenanceLogId,
                        maintenanceLog.MaintenanceDate,
                        maintenanceLog.MaintenanceCost,
                        maintenanceLog.Remarks,
                        maintenanceLog.EquipmentName
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An unexpected error occurred while saving: " + ex.Message
                });
            }
        }
        // ================================================================
        // EDIT CONSUMABLES (MODAL)
        // ================================================================
        [HttpPost("EditConsumable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConsumable(ConsumableModel updatedConsumable)
        {
            if (updatedConsumable == null || updatedConsumable.ConsumableId <= 0)
                return BadRequest("Invalid consumable data.");

            var existing = await _context.Consumables.FindAsync(updatedConsumable.ConsumableId);
            if (existing == null) return NotFound();

            existing.ConsumableName = updatedConsumable.ConsumableName;
            existing.Category = updatedConsumable.Category;
            existing.UnitCost = updatedConsumable.UnitCost;
            existing.Status = updatedConsumable.Status;

            var file = Request.Form.Files["EditConsumableImageFile"];
            if (file != null && file.Length > 0)
            {
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                existing.ImageData = ms.ToArray();
            }

            _context.Update(existing);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Consumable '{existing.ConsumableName}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // ARCHIVE CONSUMABLES
        // ================================================================
        [HttpPost("ArchiveConsumables/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConsumables(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null)
            {
                return NotFound();
            }

            consumable.IsArchived = true;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{consumable.ConsumableName} has been archived.";
            return RedirectToAction("Index");
        }
        // ================================================================
        // ✅ GET MAINTENANCE LOGS (AJAX)
        // ================================================================
        [HttpGet("GetMaintenanceLogs/{equipmentId}")]
        public async Task<IActionResult> GetMaintenanceLogs(int equipmentId)
        {
            try
            {
                var logs = await _context.MaintenanceLogs
                    .Include(l => l.User)
                    .Where(l => l.EquipmentId == equipmentId && !l.IsArchived)
                    .OrderByDescending(l => l.MaintenanceDate)
                    .Select(l => new
                    {
                        l.MaintenanceLogId,
                        l.EquipmentName,
                        l.Remarks,
                        l.MaintenanceCost,
                        MaintenanceDate = l.MaintenanceDate,
                        UserName = l.User != null ? l.User.UserName : "Unknown"
                    })
                    .ToListAsync();

                return Json(logs);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to load logs: " + ex.Message
                });
            }
        }
        // ================================================================
        // ✅ ADD BATCH (GET)
        // ================================================================

        [HttpGet("AddBatch")]
        public IActionResult AddBatch(int consumableId)
        {
            var consumable = _context.Consumables.FirstOrDefault(c => c.ConsumableId == consumableId);
            if (consumable == null)
                return Json(new { success = false, message = "Consumable not found." });

            var today = DateTime.UtcNow.Date;
            var countToday = _context.ConsumableBatches
                .Where(b => b.ConsumableId == consumableId && b.DateReceived.Date == today)
                .Count() + 1;

            var batchNumber = $"CON-{consumableId}-{today:yyyyMMdd}-{countToday:D4}";

            return Json(new
            {
                success = true,
                consumableId = consumable.ConsumableId,
                consumableName = consumable.ConsumableName,
                batchNumber
            });
        }

        // ================================================================
        // ✅ ADD BATCH (POST)
        // ================================================================
        [HttpPost("AddBatch")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBatch([FromForm] ConsumableBatch batch)
        {
            if (batch == null || batch.ConsumableId <= 0)
                return Json(new { success = false, message = "Invalid batch data." });

            try
            {
                var consumable = await _context.Consumables
                    .FirstOrDefaultAsync(c => c.ConsumableId == batch.ConsumableId);

                if (consumable == null)
                    return Json(new { success = false, message = "Consumable not found." });

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                    return Json(new { success = false, message = "User not logged in." });

                var today = DateTime.UtcNow.Date;
                var countToday = await _context.ConsumableBatches
                    .Where(b => b.ConsumableId == batch.ConsumableId && b.DateReceived.Date == today)
                    .CountAsync();

                batch.BatchNumber = $"CON-{batch.ConsumableId}-{today:yyyyMMdd}-{(countToday + 1):D4}";
                batch.UserId = userId;
                batch.DateReceived = DateTime.UtcNow;

                if (batch.ShelfLifeDays > 0)
                    batch.ExpirationDate = batch.DateReceived.AddDays(batch.ShelfLifeDays);

                _context.ConsumableBatches.Add(batch);
                await _context.SaveChangesAsync();

                // ✅ Include Consumable Image Path in response
                return Json(new
                {
                    success = true,
                    message = $"Batch {batch.BatchNumber} added for {consumable.ConsumableName}.",
                    data = new
                    {
                        batch.BatchNumber,
                        batch.Quantity,
                        batch.UnitCost,
                        ExpirationDate = batch.ExpirationDate?.ToString("yyyy-MM-dd"),
                        ConsumableImage = consumable.ImageData != null && consumable.ImageData.Length > 0
                        ? $"data:image/png;base64,{Convert.ToBase64String(consumable.ImageData)}"
                        : "/images/default-placeholder.png"
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding batch: " + ex.Message });
            }
        }


        [HttpGet("GetBatches")]
        public async Task<IActionResult> GetBatches()
        {
            try
            {
                var batches = await _context.ConsumableBatches
                    .Include(b => b.Consumable)
                    .Include(b => b.User)
                    .Where(b => !b.IsArchived)
                    .OrderByDescending(b => b.DateReceived)
                    .Select(b => new
                    {
                        b.BatchId,
                        b.BatchNumber,
                        ConsumableName = b.Consumable.ConsumableName,
                        b.Quantity,
                        b.UnitCost,
                        DateReceived = b.DateReceived.ToString("yyyy-MM-dd"),
                        ExpirationDate = b.ExpirationDate.HasValue
                            ? b.ExpirationDate.Value.ToString("yyyy-MM-dd")
                            : "N/A",
                        AddedBy = b.User.UserName
                    })
                    .ToListAsync();

                return Json(new { success = true, data = batches });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error loading batches: " + ex.Message });
            }
        }


    }
}
