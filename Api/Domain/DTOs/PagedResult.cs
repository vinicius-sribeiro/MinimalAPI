namespace MinimalAPI.Domain.DTOs
{
    public class PagedResult<T>
    {
        public int PaginaAtual { get; init; }
        public int PageSize { get; init; }
        public int TotalItens { get; init; }
        public int TotalPaginas { get; init; }
        public bool TemPaginaAnterior => PaginaAtual > 1;
        public bool TemProximaPagina => PaginaAtual < TotalPaginas;
        public List<T> Itens { get; init; } = [];        
    }
}
