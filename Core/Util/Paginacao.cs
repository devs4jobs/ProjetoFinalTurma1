namespace Core.Util
{
    public class Paginacao
    {
        public int PaginaAtual { get; set; }
        public int TotalPaginas { get; set; }
        public int RegistroPorPagina { get; set; }
        public int TotalDeRegistros { get; set; }


        //Método para realizar o calculo da paginação
        public void Paginar(int numeroPagina, int quantidadeRegistros, int qtdTicket)
        {

            TotalPaginas = qtdTicket/ quantidadeRegistros;
            if (qtdTicket % quantidadeRegistros != 0) { TotalPaginas += 1; }
            TotalDeRegistros = qtdTicket;
            RegistroPorPagina = quantidadeRegistros;
            PaginaAtual = numeroPagina;
        }
    }
}
