using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NFCAccessSystem.Data;
using OtpNet;
using QRCoder;

namespace NFCAccessSystem.Controllers
{
    [Authorize]
    public class Home : Controller
    {
        private readonly AccessSystemContext _context;

        public Home(AccessSystemContext context)
        {
            _context = context;
        }

        // GET: Home
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return _context.Users != null
                ? View(await _context.Users.ToListAsync())
                : Problem("Entity set 'AccessSystemContext.Users'  is null.");
        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Home/Create
        public IActionResult Create()
        {
            // generate the TOTP key
            var user = new User()
            {
                TotpSecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20))
            };

            const string label = "";
            const string issuer = "NFCAccessSystem";
            var qrCodeUri =
                $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={user.TotpSecret}&issuer={Uri.EscapeDataString(issuer)}";
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPngByteArr = qrCode.GetGraphic(5);
            ViewBag.QrCode = Convert.ToBase64String(qrCodeAsPngByteArr);

            return View(user);
        }

        // POST: Home/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("UserId,TagUid,Name,TotpSecret,Admin,OfflineAuth,MostRecentTotp")]
            User user)
        {
            if (ModelState.IsValid)
            {
                var totp = new Totp(Base32Encoding.ToBytes(user.TotpSecret));
                long timeWindowUsed;
                if (totp.VerifyTotp(user.MostRecentTotp, out timeWindowUsed,
                        VerificationWindow.RfcSpecifiedNetworkDelay))
                {
                    user.Authorized = true;
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // If code verification fails, set a global warning message
                // https://stackoverflow.com/questions/5739362/modelstate-addmodelerror-how-can-i-add-an-error-that-isnt-for-a-property/5740852#5740852
                ModelState.AddModelError(string.Empty, "The code you entered is incorrect, please try again.");
            }


            // return the same QR code again
            const string label = "";
            const string issuer = "NFCAccessSystem";
            var qrCodeUri =
                $"otpauth://totp/{Uri.EscapeDataString(label)}?secret={user.TotpSecret}&issuer={Uri.EscapeDataString(issuer)}";
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeAsPngByteArr = qrCode.GetGraphic(5);
            ViewBag.QrCode = Convert.ToBase64String(qrCodeAsPngByteArr);

            return View(user);
        }

        // GET: Home/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Home/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("UserId,TagUid,Name,TotpSecret,Authorized,Admin,OfflineAuth")]
            User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: Home/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Home/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'AccessSystemContext.Users'  is null.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }
    }
}