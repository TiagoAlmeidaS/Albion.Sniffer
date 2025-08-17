using AlbionOnlineSniffer.Web.Interfaces;

namespace AlbionOnlineSniffer.Web.Repositories
{
    /// <summary>
    /// Repositório em memória com limite configurável e comportamento FIFO
    /// </summary>
    /// <typeparam name="T">Tipo do item armazenado</typeparam>
    public class BoundedInMemoryRepository<T> : IInMemoryRepository<T>
    {
        private readonly int _maxItems;
        private readonly LinkedList<T> _items = new();
        private readonly object _lock = new();
        private readonly RepositoryStats _stats;

        public BoundedInMemoryRepository(int maxItems = 5000)
        {
            _maxItems = maxItems;
            _stats = new RepositoryStats
            {
                MaxItems = maxItems,
                CurrentItems = 0,
                TotalItems = 0,
                ItemsDiscarded = 0,
                FirstItemAdded = DateTime.MinValue,
                LastItemAdded = DateTime.MinValue
            };
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public void Add(T item)
        {
            if (item == null)
                return;

            lock (_lock)
            {
                var now = DateTime.UtcNow;
                
                // Se já atingiu o limite, remove o item mais antigo
                if (_items.Count >= _maxItems)
                {
                    _items.RemoveFirst();
                    _stats.ItemsDiscarded++;
                }

                // Adiciona o novo item
                _items.AddLast(item);
                _stats.TotalItems++;
                _stats.CurrentItems = _items.Count;
                _stats.LastItemAdded = now;

                // Define o primeiro item se for o primeiro
                if (_stats.FirstItemAdded == DateTime.MinValue)
                {
                    _stats.FirstItemAdded = now;
                }
            }
        }

        public IReadOnlyList<T> GetAll()
        {
            lock (_lock)
            {
                return _items.ToList();
            }
        }

        public IReadOnlyList<T> GetPaged(int skip, int take)
        {
            lock (_lock)
            {
                return _items.Skip(skip).Take(take).ToList();
            }
        }

        public IReadOnlyList<T> GetWhere(Func<T, bool> predicate)
        {
            if (predicate == null)
                return GetAll();

            lock (_lock)
            {
                return _items.Where(predicate).ToList();
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                _stats.CurrentItems = 0;
                _stats.FirstItemAdded = DateTime.MinValue;
                _stats.LastItemAdded = DateTime.MinValue;
            }
        }

        public RepositoryStats GetStats()
        {
            lock (_lock)
            {
                return new RepositoryStats
                {
                    TotalItems = _stats.TotalItems,
                    MaxItems = _stats.MaxItems,
                    CurrentItems = _stats.CurrentItems,
                    LastItemAdded = _stats.LastItemAdded,
                    FirstItemAdded = _stats.FirstItemAdded,
                    ItemsDiscarded = _stats.ItemsDiscarded
                };
            }
        }
    }
}