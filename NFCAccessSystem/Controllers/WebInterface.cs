using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFCAccessSystem.Data;
using OtpNet;
using QRCoder;
using NFCHelper;

namespace NFCAccessSystem.Controllers
{
    [Authorize]
    public class WebInterface : Controller
    {
        private readonly ILogger _logger;
        private readonly AccessSystemContext _context;
        private const string SessionUserId = "_UserId";

        public WebInterface(AccessSystemContext context, ILogger<WebInterface> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: WebInterface
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return _context.Users != null
                ? View(await _context.Users.ToListAsync())
                : Problem("Entity set 'AccessSystemContext.Users'  is null.");
        }

        // GET: WebInterface/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            // authenticated action: create a session if one does not exist
            CreateSession();

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: WebInterface/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            // authenticated action: create a session if one does not exist
            CreateSession();

            // attempt to read UID from NFC tag
            var myNFCHelper = new NFCHelper.NFCHelper();
            string uidFromTagStr = myNFCHelper.ReadUid();

            if (uidFromTagStr == null)
            {
                _logger.LogInformation("NFC Helper returned null, did not find a tag.");
                ViewBag.NFCHint =
                    "Did not find a valid tag, please place a tag on the reader and refresh the page to fill it in automatically. Alternatively, you can also type in the UID manually if you wish.";
            }
            else
            {
                ViewBag.NFCHint = "UID successfully read from the NFC tag.";
            }

            // generate the TOTP key
            var user = new User()
            {
                TotpSecret = Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20)),
                TagUid = uidFromTagStr
            };

            const string label = "User";
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

        // POST: WebInterface/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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
            const string label = "User";
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

        // GET: WebInterface/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            // authenticated action: create a session if one does not exist
            CreateSession();

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

        // POST: WebInterface/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id,
            [Bind("UserId,TagUid,Name,TotpSecret,Authorized,Admin,OfflineAuth, MostRecentTotp")]
            User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors);

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

        // GET: WebInterface/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            // authenticated action: create a session if one does not exist
            CreateSession();

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

        // POST: WebInterface/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
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

        // GET: LogOut
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> LogOut()
        {
            ClearSession();
            return RedirectToAction(nameof(Index));
        }


        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }

        private void CreateSession()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserId)))
            {
                ClaimsPrincipal principal = HttpContext.User;

                if (null != principal) // If principal is not empty
                {
                    foreach (Claim claim in principal.Claims)
                    {
                        Console.WriteLine("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value);
                    }

                    // Name here is TagUid
                    var userTagUidFromAuth = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)
                        .Value.ToString();
                    // Look this up and map to UserId
                    var userIdFromAuth = _context.Users.FirstOrDefault(e => e.TagUid == userTagUidFromAuth).UserId;

                    HttpContext.Session.SetString(SessionUserId, userIdFromAuth.ToString());
                    _logger.LogInformation("Session created for user {0}, Tag UID {1}", userIdFromAuth,
                        userTagUidFromAuth);
                }
            }
            else
            {
                _logger.LogInformation("Session already exists, not creating a new one.");
            }
        }

        private void ClearSession()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString(SessionUserId)))
            {
                ClaimsPrincipal principal = HttpContext.User;

                if (null != principal) // If principal is not empty
                {
                    foreach (Claim claim in principal.Claims)
                    {
                        Console.WriteLine("CLAIM TYPE: " + claim.Type + "; CLAIM VALUE: " + claim.Value);
                    }

                    // Name here is TagUid
                    var userTagUidFromAuth = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)
                        .Value.ToString();
                    // Look this up and map to UserId
                    var userIdFromAuth = _context.Users.FirstOrDefault(e => e.TagUid == userTagUidFromAuth).UserId;

                    HttpContext.Session.Clear();
                    _logger.LogInformation("Session cleared for user {0}, Tag UID {1}", userIdFromAuth,
                        userTagUidFromAuth);
                }
            }
            else
            {
                _logger.LogInformation("There is no session to clear.");
            }
        }
    }
}