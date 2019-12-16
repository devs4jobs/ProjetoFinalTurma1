using Core.Util;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Core
{
    public class UsuarioCore : AbstractValidator<Usuario>
    {
        private Usuario _usuario { get; set; }
        public ServiceContext _dbcontext { get; set; }

        public UsuarioCore(ServiceContext Context) => _dbcontext = Context;

        public UsuarioCore(Usuario Usuario, ServiceContext Context)
        {
            _dbcontext = Context;
            _usuario = Usuario;

            RuleFor(u => u.Nome).NotEmpty().WithMessage("Não há caracteres no nome");
            RuleFor(u => u.Nome).NotNull().WithMessage("Nome deve ser informado").MinimumLength(3).WithMessage("O nome deve ter no mínimo 3 letras!");
            RuleFor(u => u.Email).NotNull().WithMessage("Email não pode ser nulo").EmailAddress().WithMessage("Email inválido.");
            RuleFor(u => u.Email).Must(x => !_dbcontext.Usuarios.Any(e => e.Email == x)).WithMessage("Email ja Cadastrado");
            RuleFor(u => u.Senha).NotNull().WithMessage("Senha Não pode ser nula").Length(8, 12).WithMessage("A senha deve ser entre 8 e 12 caracteres e não pode ser nula");
            RuleFor(u => u.Senha).Matches(@"[a-z-A-Z].\d|\d.[a-z-A-Z]").WithMessage("A senha deve conter ao menos uma letra e um número");
            RuleFor(u => u.ConfirmaSenha).Equal(_usuario.Senha).WithMessage("As senhas devem ser iguais!");
            RuleFor(u => u.Tipo).NotNull().WithMessage("O tipo do Usuário deve ser informado");
            if (_usuario.Tipo != null) RuleFor(u => u.Tipo).Must(u => u.ToUpper() == "CLIENTE" || u.ToUpper() == "ATENDENTE").WithMessage("Tipo deve ser cliente ou atendente");
        }

        /// <summary>
        /// Método para realizar o cadastro de um usuario
        /// </summary>
        /// <returns></returns>
        public async Task<Retorno> CadastrarUsuario()
        {
            var validar = Validate(_usuario);
            if (!validar.IsValid)  return new Retorno { Resultado = validar.Errors.Select(a => a.ErrorMessage) };

            //Adiciona e salva no banco de dados
            await _dbcontext.Usuarios.AddAsync(_usuario);
            await _dbcontext.SaveChangesAsync();

            return new Retorno { Status = true, Resultado = new List<string> { "Usuário cadastrado com sucesso!" } };
        }

        /// <summary>
        /// Método para logar o usuario na plataforma.
        /// </summary>
        /// <param name="loginView"></param>
        /// <returns></returns>
        public async Task<Retorno> LogarUsuario(Usuario loginView)
        {
            try 
            {
                //Vejo se o login esta correto, se nao ja retorno uma mensagem.
                _usuario = await _dbcontext.Usuarios.SingleOrDefaultAsync(u => u.Email == loginView.Email);
           
                return _usuario.Senha != loginView.Senha?  new Retorno { Resultado = new List<string> { "Senha inválida!" } }
                : new Retorno { Status = true, Resultado = new { TokenUsuario = _usuario.Id, _usuario.Nome, _usuario.Tipo } };
            }
            catch (Exception)
            {
                return new Retorno { Resultado = new List<string> { "Email não encontrado!" } };
            }
        }
    }
}
