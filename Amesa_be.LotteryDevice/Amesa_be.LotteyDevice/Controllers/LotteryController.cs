using AMESA_BE.Filters;
using AMESA_be.LotteryDevice.BL.Interfaces;
using AMESA_be.LotteryDevice.DTOs;
using AMESA_BE.Models;
using AMESA_BELotteryDevice.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration; // Add this using directive

namespace AMESA_BE.LotteryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [TypeFilter(typeof(GeneralActionResponseFilter))]
    [TypeFilter(typeof(ModelDtoStateFeatureFilter))]
    public class LotteryController : ControllerBase
    {
        private readonly ILotteryManager _lotteryManager;
        private readonly bool _isDryRun;

        public LotteryController(ILotteryManager lotteryManager, IConfiguration configuration)
        {
            _lotteryManager = lotteryManager ?? throw new ArgumentNullException(nameof(lotteryManager));
            _isDryRun = configuration.GetValue<bool>("DryRunSettings:IsDryRun");
        }

        [HttpPut("prepare")]
        public async Task<ActionResult<LotteryDto>> PrepareLottery([FromBody] PrepareLotteryRequestDto request, CancellationToken cancellationToken)
        {
            var lottery = await _lotteryManager.PrepareLotteryAsync(request.LotteryId, request.AutomateLottery,
                _isDryRun, cancellationToken);
            if(lottery == null) 
                return NotFound($"Lottery {request.LotteryId} does not exists");
            return  Ok(lottery);
        }

        [HttpPost("execute")]
        public async Task<ActionResult<LotteryWinnersDto>> ExecuteLottery([FromBody] ExecuteLotteryRequestDto request, CancellationToken cancellationToken)
        {
            var lotteryWinners = await _lotteryManager.ExecuteLotteryAsync(request.LotteryId, _isDryRun, cancellationToken);
            if (lotteryWinners == null)
                return NotFound($"Lottery {request.LotteryId} does not exists");

            return Ok(lotteryWinners);
        }
    }
}