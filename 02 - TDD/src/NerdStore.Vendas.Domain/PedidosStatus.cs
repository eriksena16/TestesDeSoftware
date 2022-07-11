namespace NerdStore.Vendas.Domain
{
    public partial class Pedido
    {
        public enum PedidosStatus
        {
            Rascunho = 0,
            Iniciado = 1,
            Pago = 2,
            Entregue = 3,
            Cancelado = 4
        }


    }

}
