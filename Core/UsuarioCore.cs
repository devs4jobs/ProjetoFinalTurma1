using Core.Util;
using FluentValidation;
using Model;
using System.Collections.Generic;
using System.Linq;

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

            RuleFor(u => u.Nome).NotNull().MinimumLength(3).WithMessage("O Nome deve ter no minimo 3 letras!");
            RuleFor(u => u.Email).EmailAddress().NotNull().WithMessage("Email inválido.");
            RuleFor(u => u.Senha).NotNull().Length(8, 12).WithMessage("A senha deve ser entre 8 e 12 caracteres e nao pode ser nula");
            RuleFor(u => u.Senha).Matches(@"[a-z-A-Z].\d|\d.[a-z-A-Z]").WithMessage("A senha deve conter ao menos uma letra e um número");
            RuleFor(u => u.ConfirmaSenha).Equal(_usuario.Senha).WithMessage("As senhas devem ser iguais!");
            RuleFor(u => u.Tipo).NotNull().WithMessage("O tipo do Usuario deve ser informado");
            if(_usuario.Tipo!=null) RuleFor(u => u.Tipo).Must(u => u.ToUpper() == "CLIENTE" || u.ToUpper() == "ATENDENTE").WithMessage("Tipo deve ser cliente ou atendente");
        }

        //Método para cadastro de usuario
        public Retorno CadastrarUsuario()
        {
            var validar = Validate(_usuario);
            if (!validar.IsValid)
                return new Retorno { Status = false, Resultado = validar.Errors.Select(a => a.ErrorMessage).ToList() };

            if (_dbcontext.Usuarios.Any(e => e.Email == _usuario.Email))
                return new Retorno { Status = false, Resultado = new List<string> { "Email ja cadastrado!" } };

            _dbcontext.Usuarios.Add(_usuario);

            _dbcontext.SaveChanges();

            return new Retorno { Status = true, Resultado = new List<string> { "Usuário cadastrado com sucesso!" } };
        }

        //Método para logar o usuario na plataforma.
        public Retorno LogarUsuario()
        {
            //Vejo se o login esta correto, se nao ja retorno uma mensagem.
            var usuarioLogin = _dbcontext.Usuarios.FirstOrDefault(u => u.Email == _usuario.Email && u.Senha == _usuario.Senha);

            if (usuarioLogin == null)
                return new Retorno { Status = false, Resultado = new List<string> { "Email ou senha inválidos!" } };
            //  Crio o objeto a ser retornado 
            var Resultado = new
            {
                TokenUsuario = usuarioLogin.Id,
                usuarioLogin.Nome,
                usuarioLogin.Tipo
            };

            return new Retorno { Status = true, Resultado = Resultado };
        }
    }
}
