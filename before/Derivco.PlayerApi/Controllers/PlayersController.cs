namespace Derivco.PlayerApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Derivco.PlayerApi.Models;
using Derivco.PlayerApi.Options;
using Derivco.PlayerApi.Services;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly DatabaseOptions _dbOptions;

    public PlayersController(IPlayerService playerService, IOptions<DatabaseOptions> dbOptions)
    {
        _playerService = playerService;
        _dbOptions = dbOptions.Value;
    }

    // GET api/players
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerDto>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var players = _playerService.GetAllPlayers();
        return Ok(players);
    }

    // GET api/players/{id}
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetById(int id)
    {
        var player = _playerService.GetPlayer(id);
        if (player is null)
            return NotFound();
        return Ok(player);
    }

    // POST api/players
    // [ApiController] automatically returns 400 if model validation fails — no explicit check needed.
    [HttpPost]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Create([FromBody] CreatePlayerRequest request)
    {
        var created = _playerService.CreatePlayer(request);
        return CreatedAtAction(nameof(GetById), new { id = created.PlayerId }, created);
    }

    // PUT api/players/{id}/balance
    [HttpPut("{id:int}/balance")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateBalance(int id, [FromBody] UpdateBalanceRequest request)
    {
        var updated = _playerService.UpdateBalance(id, request.NewBalance);
        if (updated is null)
            return NotFound();
        return Ok(updated);
    }
    // GET api/players/{id}/transactions
    // Returns all transactions for a player, or 404 if the player does not exist.
    [HttpGet("{id:int}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPlayerTransactions(int id)
    {
        var transactions = _playerService.GetPlayerTransactions(id);
        if (transactions is null)
            return NotFound();
        return Ok(transactions);
    }
    
    // POST api/players/{fromId}/transfer
    [HttpPost("{fromId:int}/transfer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Transfer(int fromId, [FromBody] TransferFundsRequest request)
    {
        if (fromId != request.FromPlayerId)
            return BadRequest(new { error = "Route fromId must match body FromPlayerId" });

        try
        {
            _playerService.TransferFunds(request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    // GET api/players/config
    // Diagnostic endpoint — demonstrates reading configuration via IOptions<T>.
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig()
    {
        return Ok(new { commandTimeoutSeconds = _dbOptions.CommandTimeoutSeconds });
    }

    public object GetTransactions(int v)
    {
        throw new NotImplementedException();
    }
}
