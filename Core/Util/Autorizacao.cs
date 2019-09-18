using Model;
using System;
using System.Linq;

namespace Core.Util
{
    public static class Autorizacao
    {
        public static bool ValidarUsuario(string Usertoken, ServiceContext servicecontext)
        {

            if (!Guid.TryParse(Usertoken, out Guid token) || !servicecontext.Usuarios.Any(e => e.Id == token))
                return false;

            return true;
        }
    }
}
