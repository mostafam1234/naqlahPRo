using Application.Features.AdminSection.SystemUsers.Dtos;
using Application.Features.AdminSection.SystemUsers.Queries;
using Domain.Constants;
using Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Presentaion.Authorization;
using Presentaion.Reponse;
using System.Security.Claims;

namespace Presentaion.Controllers.Admin
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [RequirePermission(PermissionNames.CanManageRolePermissions)]
    public class RolePermissionsAdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly RoleManager<Role> _roleManager;

        public RolePermissionsAdminController(IMediator mediator, RoleManager<Role> roleManager)
        {
            _mediator = mediator;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("GetAllPermissionDefinitions")]
        [ProducesResponseType(typeof(List<PermissionDefinitionDto>), StatusCodes.Status200OK)]
        public IActionResult GetAllPermissionDefinitions()
        {
            var list = PermissionNames.All.Select(p => new PermissionDefinitionDto
            {
                Name = p.Name,
                Description = p.Description,
                Module = p.Module
            }).ToList();
            return Ok(list);
        }

        [HttpGet]
        [Route("GetRolePermissions")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRolePermissions([FromQuery] int roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                return BadRequest(ProblemDetail.CreateProblemDetail("Role not found"));

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissions = claims
                .Where(c => string.Equals(c.Type, PermissionNames.ClaimType, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .ToList();
            return Ok(permissions);
        }

        [HttpPost]
        [Route("UpdateRolePermissions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionsRequest request)
        {
            var role = await _roleManager.FindByIdAsync(request.RoleId.ToString());
            if (role == null)
                return BadRequest(ProblemDetail.CreateProblemDetail("Role not found"));

            var existingClaims = (await _roleManager.GetClaimsAsync(role))
                .Where(c => string.Equals(c.Type, PermissionNames.ClaimType, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var claim in existingClaims)
                await _roleManager.RemoveClaimAsync(role, claim);

            var permissionSet = request.PermissionNames?.ToHashSet() ?? new HashSet<string>();
            foreach (var name in PermissionNames.All.Select(p => p.Name))
            {
                if (permissionSet.Contains(name))
                    await _roleManager.AddClaimAsync(role, new Claim(PermissionNames.ClaimType, name));
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetRolesLookup")]
        [ProducesResponseType(typeof(List<RoleLookupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRolesLookup()
        {
            var result = await _mediator.Send(new GetAllRolesLookupQuery());
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
        }

        [HttpPost]
        [Route("CreateRole")]
        [ProducesResponseType(typeof(CreateRoleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(ProblemDetail.CreateProblemDetail("Role name is required."));

            var normalizedName = request.Name.Trim().ToUpperInvariant();
            var existing = await _roleManager.FindByNameAsync(request.Name.Trim());
            if (existing != null)
                return BadRequest(ProblemDetail.CreateProblemDetail("A role with this name already exists."));

            var role = new Role
            {
                Name = request.Name.Trim(),
                NormalizedName = normalizedName,
                ArabicName = request.ArabicName?.Trim() ?? string.Empty
            };

            var result = await _roleManager.CreateAsync(role);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return BadRequest(ProblemDetail.CreateProblemDetail(errors));
            }

            return CreatedAtAction(nameof(GetRolesLookup), new CreateRoleResponse
            {
                Id = role.Id,
                Name = role.Name ?? string.Empty,
                ArabicName = role.ArabicName ?? string.Empty
            });
        }
    }

    public class CreateRoleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? ArabicName { get; set; }
    }

    public class CreateRoleResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ArabicName { get; set; } = string.Empty;
    }

    public class PermissionDefinitionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
    }

    public class UpdateRolePermissionsRequest
    {
        public int RoleId { get; set; }
        public List<string>? PermissionNames { get; set; }
    }
}
