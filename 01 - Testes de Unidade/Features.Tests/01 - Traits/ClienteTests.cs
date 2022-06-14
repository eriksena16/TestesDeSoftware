using Features.Clientes;
using System;
using Xunit;

namespace Features.Tests
{
    public class ClienteTests
    {
        [Fact(DisplayName ="Novo Cliente Válido")]
        [Trait("Categoria", "Cliente Trait Testes")]
        public void Cliente_NovoCliente_DeveEstarValido()
        {
            //Arrange
            var cliente = new Cliente(
               Guid.NewGuid(),
                "Erik",
                "Sena",
                DateTime.Now.AddYears(-30),
                "eriksena16@gmail",
                true,
                DateTime.Now);
            //Act

            var result = cliente.EhValido();

            //Assert
            Assert.True(result);
            Assert.Equal(0, cliente.ValidationResult.Errors.Count);
        }

        [Fact(DisplayName = "Novo Cliente Inválido")]
        [Trait("Categoria", "Cliente Trait Testes")]
        public void Cliente_NovoCliente_DeveEstarInvalidoValido()
        {
            //Arrange
            var cliente = new Cliente(
               Guid.NewGuid(),
                "",
                "",
                DateTime.Now,
                "eriksena12gmail",
                true,
                DateTime.Now);

            //Act

            var result = cliente.EhValido();

            //Assert
            Assert.False(result);
            Assert.NotEqual(0, cliente.ValidationResult.Errors.Count);

        }
    }
}
