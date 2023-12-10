using Cafeteria.Data;
using Cafeteria.Modelos;

namespace Cafeteria.Repositorio
{
    public class CafeRepositorio : IRepositorio<Cafe>
    {
        private readonly ContextoBanco _contexto;
        private readonly ILogger<CafeRepositorio> _logger;

        public CafeRepositorio(ContextoBanco contexto, ILogger<CafeRepositorio> logger)
        {
            _contexto = contexto;
            _logger = logger;
        }

        public  Cafe Adicionar(Cafe entity)
        {
            try
            {
                _contexto.Cafes.Add(entity);
                 _contexto.SaveChanges();

                _logger.LogInformation(message: $"Café adicionado com sucesso: {entity.id}", entity);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar café: {@Cafe}", entity);
                throw;
            }
        }

        public async Task<Cafe> Atualizar(Cafe entity)
        {
            try
            {
                _contexto.Cafes.Update(entity);
                await _contexto.SaveChangesAsync();

                _logger.LogInformation("Café atualizado com sucesso: {@Cafe}", entity);

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar café: {@Cafe}", entity);
                throw;
            }
        }

        public Cafe ObterPorId(int id)
        {
            try
            {
                if (id > 0)
                {
                    var obter = _contexto.Cafes.Find(id);
                    _logger.LogInformation("Café encontrado por ID: {Cafe}", obter);
                    return obter;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter café por ID: {Id}", id);
                throw;
            }
        }

        public async Task<List<Cafe>> ObterTodos()
        {
            try
            {
                var todos = _contexto.Cafes.ToList();

                _logger.LogInformation("Lista de cafés obtida com sucesso: {@Cafes}", todos);

                return todos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os cafés");
                throw;
            }
        }

        public async Task<bool> Remover(int id)
        {
            try
            {
                var cafe = _contexto.Cafes.Find(id);
                if (cafe != null)
                {
                    _contexto.Cafes.Remove(cafe);
                    await _contexto.SaveChangesAsync();

                    _logger.LogInformation("Café removido com sucesso: {@Cafe}", cafe);

                    return true;
                }

                _logger.LogWarning("Café não encontrado para remoção. ID: {Id}", id);

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover café por ID: {Id}", id);
                throw;
            }
        }
    }
}
