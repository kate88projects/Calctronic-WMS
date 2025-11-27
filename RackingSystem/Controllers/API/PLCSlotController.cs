using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RackingSystem.Data;
using RackingSystem.Services.LoaderServices;
using RackingSystem.Services.SlotServices;

namespace RackingSystem.Controllers.API
{
    [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]")]
    public class PLCSlotController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly ISlotService _slotService;
        private readonly IMapper _mapper;
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public PLCSlotController(AppDbContext dbContext, ISlotService slotService, IMapper mapper, IDbContextFactory<AppDbContext> contextFactory)
        {
            _dbContext = dbContext;
            _slotService = slotService;
            _mapper = mapper;
            _contextFactory = contextFactory;
        }

    }
}
