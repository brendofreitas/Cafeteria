﻿using Cafeteria.Modelos;
using Cafeteria.Repositorio;
using Microsoft.AspNetCore.Mvc;

namespace Cafeteria.Controllers
{
    [Route("[controller]")]
    public class CafeController : Controller
    {
        private IRepositorio<Cafe> _repositorio;
        private Instrumentor _instrumentor;

        private ILogger<CafeController> _logger;
        public CafeController(IRepositorio<Cafe> repositorio, ILogger<CafeController> logger, Instrumentor instrumentor)
        {
            _repositorio = repositorio;
            _logger = logger;
            _instrumentor = instrumentor;
        }


        [HttpGet("ObterPorId")]
        public IActionResult ObterPorId(int id)
        {
            try
            {
                if (id > 0)
                {
                    var cafe = _repositorio.ObterPorId(id);
                    _logger.LogError("Erro testes", "erro");
                    _logger.LogInformation("teste informacao", "erro");

                    _instrumentor.IncomingRequestCounter.Add(1,
                       new KeyValuePair<string, object?>("operation", "ObterPorId"),
                       new KeyValuePair<string, object?>("controller", nameof(CafeController)));

                    return Ok(cafe);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Falha ao obter todos os cafés: {e}");
                return BadRequest();
            }
        }

        [HttpPost("Criar")]
        public IActionResult Criar([FromBody] Cafe cafe)
        {
            try
            {
                if (cafe == null)
                {
                    return BadRequest();
                }
                else
                {
                    _repositorio.Adicionar(cafe);

                    _logger.LogInformation("Café adicionado com sucesso: {@Cafe}", cafe);

                    _instrumentor.IncomingRequestCounter.Add(1,
                     new KeyValuePair<string, object?>("operation", "Criar"),
                     new KeyValuePair<string, object?>("controller", nameof(CafeController)));

                    return Created($"/cafe/{cafe.id}", cafe);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Falha ao adicionar café: {e}");
                return BadRequest();
            }
        }

        [HttpPost("Atualizar")]
        public IActionResult Atualizar([FromBody] Cafe cafe)
        {
            try
            {
                if (cafe == null)
                {
                    return BadRequest();
                }
                else
                {
                    _repositorio.Atualizar(cafe);
                    return Created($"/cafe/{cafe.id}", cafe);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Falha ao atualizar café: {e}");
                return BadRequest();
            }
        }


        [HttpDelete("RemoverCafePorId")]
        public IActionResult Remover(int id)
        {
            try
            {
                if (id > 0)
                {
                    _repositorio.Remover(id);
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Falha ao remover café: {e}");
                return BadRequest();
            }
        }

    }
}
