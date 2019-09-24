namespace Model
{
    public enum Status : int
    {
        ABERTO = 1,
        AGUARDANDO_RESPOSTA_DO_CLIENTE = 2,
        AGUARDANDO_RESPOSTA_DO_ATENDENTE = 3,
        FECHADO = 4
    }
}