namespace AlbionOnlineSniffer.Web.Interfaces
{
    /// <summary>
    /// Interface base para repositórios em memória com limite configurável
    /// </summary>
    /// <typeparam name="T">Tipo do item armazenado</typeparam>
    public interface IInMemoryRepository<T>
    {
        /// <summary>
        /// Adiciona um item ao repositório
        /// </summary>
        /// <param name="item">Item a ser adicionado</param>
        void Add(T item);

        /// <summary>
        /// Obtém todos os itens do repositório
        /// </summary>
        /// <returns>Lista somente leitura dos itens</returns>
        IReadOnlyList<T> GetAll();

        /// <summary>
        /// Obtém itens com paginação
        /// </summary>
        /// <param name="skip">Quantidade de itens para pular</param>
        /// <param name="take">Quantidade de itens para retornar</param>
        /// <returns>Lista paginada dos itens</returns>
        IReadOnlyList<T> GetPaged(int skip, int take);

        /// <summary>
        /// Obtém itens filtrados por uma condição
        /// </summary>
        /// <param name="predicate">Predicado para filtrar os itens</param>
        /// <returns>Lista filtrada dos itens</returns>
        IReadOnlyList<T> GetWhere(Func<T, bool> predicate);

        /// <summary>
        /// Obtém a quantidade total de itens
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Limpa todos os itens do repositório
        /// </summary>
        void Clear();

        /// <summary>
        /// Obtém estatísticas do repositório
        /// </summary>
        /// <returns>Estatísticas de uso</returns>
        RepositoryStats GetStats();
    }

    /// <summary>
    /// Estatísticas de uso do repositório
    /// </summary>
    public class RepositoryStats
    {
        public int TotalItems { get; set; }
        public int MaxItems { get; set; }
        public int CurrentItems { get; set; }
        public DateTime LastItemAdded { get; set; }
        public DateTime FirstItemAdded { get; set; }
        public int ItemsDiscarded { get; set; }
    }
}