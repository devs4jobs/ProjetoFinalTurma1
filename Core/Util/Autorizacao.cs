using Model;
using System;
using System.Linq;
namespace Core.Util
{
    public static class Autorizacao
    {
        public static bool ValidarUsuario(string Usertoken, ServiceContext serviceContext)
        {

            if (!Guid.TryParse(Usertoken, out Guid token) || !serviceContext.Usuarios.Any(e => e.Id == token))

                return false;

            return true;
        }
    }
}
