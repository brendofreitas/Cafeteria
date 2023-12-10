using Cafeteria.Modelos;

namespace Cafeteria.Repositorio
{
    public interface IRepositorio <T> where T : class
    {
        Task<List<T>> ObterTodos();
        T ObterPorId(int id);
        T Adicionar(T entity);
        Task<T> Atualizar(T entity);
        Task<bool> Remover(int id);
    }
}
