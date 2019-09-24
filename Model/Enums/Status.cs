namespace Model
{
    public enum Status : int
    {
        ABERTO = 1,
        EM_ANDAMENTO = 2,
        AGUARDANDO_RESPOSTA_DO_CLIENTE = 3,
        AGUARDANDO_RESPOSTA_DO_ATENDENTE = 4,
        FECHADO = 5
    }
}