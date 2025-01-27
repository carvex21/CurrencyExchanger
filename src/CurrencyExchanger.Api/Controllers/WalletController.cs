using CurrencyExchanger.Api.Services;
using CurrencyExchanger.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyExchanger.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WalletsController : ControllerBase
    {
        private readonly IWalletService _walletService;

        public WalletsController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateWallet()
        {
            var wallet = await _walletService.CreateWalletAsync();
            return Ok(wallet);
        }

        [HttpGet("{walletId}")]
        public async Task<IActionResult> GetWalletBalance(long walletId, [FromQuery] GetBalanceRequest request)
        {
            var balance = await _walletService.GetConvertedBalanceAsync(walletId, request.Currency);
            return Ok(new { Balance = balance, Currency = request.Currency ?? "EUR" });
        }

        [HttpPost("{walletId}/adjustbalance")]
        public async Task<IActionResult> AdjustWalletBalance(long walletId, [FromQuery] AdjustBalanceRequest request)
        {
            try
            {
                await _walletService.AdjustBalanceAsync(walletId, request.Amount, request.Currency, request.Strategy);
                return Ok("++Wallet balance adjusted++");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ">>An internal error occurred - Please try again later<<" });
            }
        }
    }
}