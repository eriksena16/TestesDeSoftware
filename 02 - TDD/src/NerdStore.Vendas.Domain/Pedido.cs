using FluentValidation.Results;
using NerdStore.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerdStore.Vendas.Domain
{
    public class Pedido
    {
        protected Pedido()
        {
            _pedidoItems = new List<PedidoItem>();
        }

        public static int MAX_UNIDADES_ITEM => 15;
        public static int MIN_UNIDADE_ITEM => 1;
        public Guid ClienteId { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal Desconto { get; private set; }
        public bool VoucherUtilizado { get; set; }
        public Voucher Voucher { get; set; }
        public PedidosStatus Status { get; set; }
        private readonly List<PedidoItem> _pedidoItems;
        public IReadOnlyCollection<PedidoItem> PedidoItems => _pedidoItems;

        public void CalcularValorDoPedido()
        {
            ValorTotal = PedidoItems.Sum(i => i.CalcularValor());
        }

        private bool PedidoItemExistente(PedidoItem pedidoItem)
        {
            return _pedidoItems.Any(p => p.ProdutoId == pedidoItem.ProdutoId);
        }

        public void ValidarPedidoItemInexistente(PedidoItem pedidoItem)
        {
            if (!PedidoItemExistente(pedidoItem)) throw new DomainException($"O Item não existe no pedido.");
        }

        private void ValidarQuantidadeItemPermitida(PedidoItem pedidoItem)
        {
            var quantidadeItems = pedidoItem.Quantidade;
            if (PedidoItemExistente(pedidoItem))
            {
                var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);
                quantidadeItems += itemExistente.Quantidade;
            }
            if (quantidadeItems > MAX_UNIDADES_ITEM) throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades por produto.");
        }

        public void AdicionarItem(PedidoItem pedidoItem)
        {
            ValidarQuantidadeItemPermitida(pedidoItem);

            if (PedidoItemExistente(pedidoItem))
            {
                var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

                itemExistente.AdicionarUnidade(pedidoItem.Quantidade);
                pedidoItem = itemExistente;
                _pedidoItems.Remove(itemExistente);
            }
            _pedidoItems.Add(pedidoItem);
            CalcularValorDoPedido();
        }

        public void AtualizarItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);
            ValidarQuantidadeItemPermitida(pedidoItem);

            var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

            _pedidoItems.Remove(itemExistente);
            _pedidoItems.Add(pedidoItem);
            CalcularValorDoPedido();


        }
        public void RemoverItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);
            _pedidoItems.Remove(pedidoItem);
            CalcularValorDoPedido();
        }

        public void TornarRascunho()
        {
            Status = PedidosStatus.Rascunho;
        }

        public ValidationResult AplicarVoucher(Voucher voucher)
        {
            var result = voucher.ValidarSeAplicavel();
            if (!result.IsValid) return result;

            Voucher = voucher;
            VoucherUtilizado = true;
            CalcularValorTotalDesconto();

            return result;
        }

        public void CalcularValorTotalDesconto()
        {
            decimal desconto = 0;
            if (!VoucherUtilizado) return;

            if (Voucher.TipoDescontoVoucher == TipoDescontoVoucher.Valor)
            {
                if (Voucher.ValorDesconto.HasValue)
                    desconto = Voucher.ValorDesconto.Value;
            }
            else
            {
                if (Voucher.PercentualDesconto.HasValue)
                    desconto = (ValorTotal * Voucher.PercentualDesconto.Value) / 100;

            }
            ValorTotal -= desconto;
            Desconto = desconto;
        }

        public static class PedidoFactory
        {
            public static Pedido NovoPedidoRascunho(Guid clienteId)
            {
                var pedido = new Pedido
                {
                    ClienteId = clienteId
                };
                pedido.TornarRascunho();
                return pedido;
            }
        }

        public enum PedidosStatus
        {
            Rascunho = 0,
            Iniciado = 1,
            Pago = 2,
            Entregue = 3,
            Cancelado = 4
        }


    }

    public class PedidoItem
    {
        public PedidoItem(Guid produtoId, string produtoNome, int quantidade, decimal valorUnitario)
        {
            if (quantidade < Pedido.MIN_UNIDADE_ITEM) throw new DomainException($"Minimo de {Pedido.MIN_UNIDADE_ITEM} unidades por produto.");

            ProdutoId = produtoId;
            ProdutoNome = produtoNome;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
        }

        public Guid ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

        internal void AdicionarUnidade(int unidade)
        {
            Quantidade += unidade;
        }

        internal decimal CalcularValor()
        {
            return Quantidade * ValorUnitario;
        }
    }

}
